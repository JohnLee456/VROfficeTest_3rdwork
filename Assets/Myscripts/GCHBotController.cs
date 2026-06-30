using UnityEngine;
using Unity.XR.CoreUtils;

public class GCHBotController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private bool useMainCameraAsHeadTracker = true;
    [SerializeField] private bool alignCameraToAvatarOnStart = true;
    [SerializeField] private float fallbackEyeHeight = 1.6f;
    [SerializeField] private bool rotateBodyWithHeadsetYaw = true;
    [SerializeField] private bool useEyeCenterForCameraTarget = true;
    [SerializeField] private float bodyYawSyncDeadZone = 0.25f;
    [SerializeField] private bool lockEyeCenterToCameraPosition = true;

    private Transform headTransform;
    private Transform leftEyeTransform;
    private Transform rightEyeTransform;
    private Camera xrCamera;
    private XROrigin xrOrigin;
    private Transform xrRigRoot;
    private bool cameraAligned;
    private bool loggedMissingCamera;
    private bool headTrackingCalibrated;
    private bool bodyYawTrackingCalibrated;
    private Quaternion referenceCameraWorldRotation = Quaternion.identity;
    private Quaternion referenceHeadWorldRotation = Quaternion.identity;
    private Quaternion lastSyncedCameraYawRotation = Quaternion.identity;

    private void Start()
    {
        BindAvatarParts();
        PrepareMainCamera();
        AlignCameraToAvatar();
    }

    private void Update()
    {
        float turn = 0f;
        if (Input.GetKey(KeyCode.Q))
        {
            turn -= 1f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            turn += 1f;
        }

        transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime, Space.World);

        Quaternion horizontalRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        Vector3 forward = horizontalRotation * Vector3.forward;
        Vector3 right = horizontalRotation * Vector3.right;
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            move += forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            move -= forward;
        }
        if (Input.GetKey(KeyCode.D))
        {
            move += right;
        }
        if (Input.GetKey(KeyCode.A))
        {
            move -= right;
        }

        if (move.sqrMagnitude <= 0f)
        {
            return;
        }

        float speed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
            ? moveSpeed * sprintMultiplier
            : moveSpeed;
        transform.position += move.normalized * speed * Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (!useMainCameraAsHeadTracker)
        {
            return;
        }

        PrepareMainCamera();
        AlignCameraToAvatar();
        ApplyCameraPoseToAvatar();
    }

    private void PrepareMainCamera()
    {
        if (xrCamera == null)
        {
            xrCamera = Camera.main;
        }

        if (xrCamera == null)
        {
            xrCamera = FindObjectOfType<Camera>(true);
        }

        if (xrCamera == null)
        {
            if (!loggedMissingCamera)
            {
                Debug.LogWarning("GCHbot XR camera follow skipped: no camera was found.", this);
                loggedMissingCamera = true;
            }

            return;
        }

        loggedMissingCamera = false;
        xrCamera.gameObject.SetActive(true);
        xrCamera.enabled = true;
        xrCamera.tag = "MainCamera";
        SetTrackedPoseDriversEnabled(xrCamera, true);
        xrRigRoot = FindContainingRigRoot(xrCamera.transform);
        xrOrigin = xrRigRoot != null ? xrRigRoot.GetComponent<XROrigin>() : null;

        AudioListener listener = xrCamera.GetComponent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = true;
        }
    }

    private void ApplyCameraPoseToAvatar()
    {
        if (xrCamera == null)
        {
            return;
        }

        if (headTransform == null)
        {
            BindAvatarParts();
        }

        if (rotateBodyWithHeadsetYaw)
        {
            FollowCameraYawWithBody();
        }

        if (headTransform != null)
        {
            if (!headTrackingCalibrated)
            {
                CalibrateHeadTracking();
            }

            Quaternion cameraDelta = xrCamera.transform.rotation * Quaternion.Inverse(referenceCameraWorldRotation);
            headTransform.rotation = cameraDelta * referenceHeadWorldRotation;
        }

        LockEyeCenterToCameraPosition();
    }

    private void AlignCameraToAvatar()
    {
        if (!alignCameraToAvatarOnStart || cameraAligned || xrCamera == null)
        {
            return;
        }

        if (headTransform == null)
        {
            BindAvatarParts();
        }

        Transform rigRoot = xrRigRoot != null ? xrRigRoot : FindContainingRigRoot(xrCamera.transform);
        if (!IsSafeXrMoveRoot(rigRoot))
        {
            return;
        }

        xrRigRoot = rigRoot;
        LevelXrRigRoot(rigRoot);

        Vector3 targetEyePosition = GetTargetEyePosition();
        if (!TryMoveXrOriginCameraToWorldLocation(targetEyePosition))
        {
            rigRoot.position += targetEyePosition - xrCamera.transform.position;
        }

        cameraAligned = true;
        headTrackingCalibrated = false;
        bodyYawTrackingCalibrated = false;
    }

    private Vector3 GetTargetEyePosition()
    {
        if (useEyeCenterForCameraTarget && leftEyeTransform != null && rightEyeTransform != null)
        {
            return (leftEyeTransform.position + rightEyeTransform.position) * 0.5f;
        }

        if (headTransform != null)
        {
            return headTransform.position;
        }

        return transform.position + Vector3.up * fallbackEyeHeight;
    }

    private bool TryMoveXrOriginCameraToWorldLocation(Vector3 targetEyePosition)
    {
        if (xrOrigin == null)
        {
            Transform containingRig = xrRigRoot != null ? xrRigRoot : FindContainingRigRoot(xrCamera.transform);
            xrOrigin = containingRig != null ? containingRig.GetComponent<XROrigin>() : null;
        }

        if (xrOrigin == null || xrOrigin.Camera == null || !IsSafeXrMoveRoot(xrOrigin.transform))
        {
            return false;
        }

        return xrOrigin.MoveCameraToWorldLocation(targetEyePosition);
    }

    private void CalibrateHeadTracking()
    {
        referenceCameraWorldRotation = xrCamera.transform.rotation;
        referenceHeadWorldRotation = headTransform.rotation;
        headTrackingCalibrated = true;
    }

    private void FollowCameraYawWithBody()
    {
        if (!TryExtractYawRotation(xrCamera.transform.rotation, out Quaternion cameraYaw))
        {
            return;
        }

        if (!bodyYawTrackingCalibrated)
        {
            lastSyncedCameraYawRotation = cameraYaw;
            bodyYawTrackingCalibrated = true;
            return;
        }

        Vector3 previousForward = lastSyncedCameraYawRotation * Vector3.forward;
        Vector3 currentForward = cameraYaw * Vector3.forward;
        float yawDelta = Vector3.SignedAngle(previousForward, currentForward, Vector3.up);

        if (Mathf.Abs(yawDelta) <= bodyYawSyncDeadZone)
        {
            return;
        }

        RotateBodyAroundCameraPosition(yawDelta);
        lastSyncedCameraYawRotation = cameraYaw;
    }

    private void RotateBodyAroundCameraPosition(float deltaYaw)
    {
        Quaternion deltaRotation = Quaternion.AngleAxis(deltaYaw, Vector3.up);
        Vector3 pivot = xrCamera != null ? xrCamera.transform.position : GetTargetEyePosition();

        transform.position = pivot + deltaRotation * (transform.position - pivot);
        transform.rotation = deltaRotation * transform.rotation;
    }

    private void LockEyeCenterToCameraPosition()
    {
        if (!lockEyeCenterToCameraPosition || xrCamera == null)
        {
            return;
        }

        Vector3 eyePosition = GetTargetEyePosition();
        Vector3 correction = xrCamera.transform.position - eyePosition;
        if (correction.sqrMagnitude <= 0.0000001f)
        {
            return;
        }

        transform.position += correction;
    }

    private void BindAvatarParts()
    {
        headTransform = FindHeadTransform();
        leftEyeTransform = FindFirstChildStartingWith("LeftEye_");
        rightEyeTransform = FindFirstChildStartingWith("RightEye_");
    }

    private Transform FindHeadTransform()
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name == "Head_4" || children[i].name == "Head")
            {
                return children[i];
            }
        }

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name.ToLowerInvariant().Contains("head"))
            {
                return children[i];
            }
        }

        return null;
    }

    private Transform FindFirstChildStartingWith(string prefix)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name.StartsWith(prefix))
            {
                return children[i];
            }
        }

        return null;
    }

    private bool IsSafeXrMoveRoot(Transform candidate)
    {
        return candidate != null &&
            candidate != transform &&
            !candidate.IsChildOf(transform) &&
            !transform.IsChildOf(candidate);
    }

    private static void LevelXrRigRoot(Transform rigRoot)
    {
        if (rigRoot == null || (rigRoot.name != "VRRigDeviceBased" && rigRoot.name != "VrRigActionBased"))
        {
            return;
        }

        Vector3 eulerAngles = rigRoot.eulerAngles;
        rigRoot.rotation = Quaternion.Euler(0f, eulerAngles.y, 0f);
    }

    private static bool TryExtractYawRotation(Quaternion rotation, out Quaternion yawRotation)
    {
        Vector3 forward = rotation * Vector3.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude <= 0.0001f)
        {
            yawRotation = Quaternion.identity;
            return false;
        }

        yawRotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
        return true;
    }

    private static void SetTrackedPoseDriversEnabled(Camera targetCamera, bool enabled)
    {
        if (targetCamera == null)
        {
            return;
        }

        MonoBehaviour[] behaviours = targetCamera.GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour != null && behaviour.GetType().Name.Contains("TrackedPoseDriver"))
            {
                behaviour.enabled = enabled;
            }
        }
    }

    private static Transform FindContainingRigRoot(Transform start)
    {
        Transform current = start;
        while (current != null)
        {
            if (current.name == "VRRigDeviceBased" || current.name == "VrRigActionBased")
            {
                return current;
            }

            current = current.parent;
        }

        return start.root;
    }
}
