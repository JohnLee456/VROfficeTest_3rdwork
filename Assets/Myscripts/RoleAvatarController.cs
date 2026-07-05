using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;
using UnityEngine.XR;

public class RoleAvatarController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private bool alignXrRigToAvatarOnEnable = true;
    [SerializeField] private float fallbackEyeHeight = 1.6f;
    [SerializeField] private bool controlHeadFromVrHeadset = true;
    [SerializeField] private bool rotateBodyWithHeadsetYaw = true;
    [SerializeField] private bool useEyeCenterForCameraTarget = true;
    [SerializeField] private int initialXrAlignmentFrames = 45;
    [SerializeField] private bool followXrMovementWithBody = true;
    [SerializeField] private bool followXrVerticalMovement = false;
    [SerializeField] private float bodyYawSyncDeadZone = 0.25f;
    [SerializeField] private bool lockEyeCenterToCameraPosition = true;
    [SerializeField] private bool controlHandsFromVrControllersInNoBot = true;
    [SerializeField] private float handPoseScale = 1f;
    [SerializeField] private Vector3 leftHandPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 rightHandPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 leftHandRotationOffsetEuler = Vector3.zero;
    [SerializeField] private Vector3 rightHandRotationOffsetEuler = Vector3.zero;
    [SerializeField] private bool alignBodyYawToXrCameraOnEnable;
    [SerializeField] private float cameraAlignedBodyYawOffset;

    private PhotonView photonView;
    private RoleAvatarIdentity avatarIdentity;
    private Camera roleCamera;
    private Camera xrCamera;
    private XROrigin xrOrigin;
    private Transform xrRigRoot;
    private bool localControlEnabled;
    private bool xrRigAligned;
    private int xrAlignmentFramesRemaining;
    private bool bodyFollowAnchorCalibrated;
    private Vector3 lastXrBodyAnchorPosition;
    private bool loggedMissingXrCamera;
    private Transform headTransform;
    private Transform leftEyeTransform;
    private Transform rightEyeTransform;
    private Transform leftHandTransform;
    private Transform rightHandTransform;
    private readonly List<XRNodeState> nodeStates = new List<XRNodeState>();
    private bool loggedMissingHandTargets;
    private bool headTrackingCalibrated;
    private bool bodyYawTrackingCalibrated;
    private int bodyYawAlignmentFramesRemaining;
    private Quaternion referenceCameraWorldRotation = Quaternion.identity;
    private Quaternion referenceHeadWorldRotation = Quaternion.identity;
    private Quaternion lastSyncedCameraYawRotation = Quaternion.identity;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        avatarIdentity = GetComponent<RoleAvatarIdentity>();
        if (avatarIdentity == null)
        {
            avatarIdentity = gameObject.AddComponent<RoleAvatarIdentity>();
        }

        avatarIdentity.InitializeIfEmpty(gameObject.name);
        roleCamera = GetComponentInChildren<Camera>(true);
    }

    public string AvatarId => avatarIdentity != null ? avatarIdentity.AvatarId : RoleAvatarIdentity.NormalizeAvatarId(gameObject.name);

    private void Start()
    {
        BindAvatarParts();
        DisableRoleCamera();

        if (CanUseLocalControl())
        {
            if (xrAlignmentFramesRemaining <= 0)
            {
                xrAlignmentFramesRemaining = Mathf.Max(1, initialXrAlignmentFrames);
            }

            PrepareXrView();
        }
    }

    public void SetLocalControlEnabled(bool enabled)
    {
        localControlEnabled = enabled;
        xrRigAligned = false;
        xrAlignmentFramesRemaining = enabled ? Mathf.Max(1, initialXrAlignmentFrames) : 0;
        bodyFollowAnchorCalibrated = false;
        headTrackingCalibrated = false;
        bodyYawTrackingCalibrated = false;
        bodyYawAlignmentFramesRemaining = enabled && alignBodyYawToXrCameraOnEnable
            ? Mathf.Max(1, initialXrAlignmentFrames)
            : 0;
        DisableRoleCamera();

        if (enabled && CanUseLocalControl())
        {
            PrepareXrView();
        }
    }

    public void ConfigureInitialBodyYawAlignment(bool alignToXrCamera, float yawOffset = 0f)
    {
        alignBodyYawToXrCameraOnEnable = alignToXrCamera;
        cameraAlignedBodyYawOffset = yawOffset;
        bodyYawAlignmentFramesRemaining = alignToXrCamera ? Mathf.Max(1, initialXrAlignmentFrames) : 0;
        bodyYawTrackingCalibrated = false;
        headTrackingCalibrated = false;
    }

    public void PrepareRoleCamera()
    {
        DisableRoleCamera();
    }

    private void Update()
    {
        if (!CanUseLocalControl())
        {
            return;
        }

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
        MovePlayer(move.normalized * speed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (!CanUseLocalControl())
        {
            return;
        }

        PrepareXrView();
        FollowXrMovementWithBody();
        ApplyXrCameraPoseToAvatar();

        if (ShouldControlHandsFromVrControllers())
        {
            ApplyVrControllerHandPoses();
        }
    }

    private bool CanUseLocalControl()
    {
        if (!localControlEnabled)
        {
            return false;
        }

        if (!LoginSession.HasRoute || !RoleAvatarIdentity.MatchesAvatarId(AvatarId, LoginSession.AvatarName))
        {
            return false;
        }

        if (photonView != null && PhotonNetwork.InRoom && !photonView.IsMine)
        {
            return false;
        }

        return true;
    }

    private void PrepareXrView()
    {
        DisableRoleCamera();

        if (!CacheXrCamera())
        {
            if (!loggedMissingXrCamera)
            {
                Debug.LogWarning($"Local XR camera was not found for avatar '{name}'.", this);
                loggedMissingXrCamera = true;
            }

            return;
        }

        loggedMissingXrCamera = false;
        EnableCameraAsMain(xrCamera);
        DisableOtherSceneCameras(xrCamera);
        SetTrackedPoseDriversEnabled(xrCamera, true);

        if (alignXrRigToAvatarOnEnable && (!xrRigAligned || xrAlignmentFramesRemaining > 0))
        {
            if (AlignXrRigToAvatar() && xrAlignmentFramesRemaining > 0)
            {
                xrAlignmentFramesRemaining--;
            }
        }

        if (alignBodyYawToXrCameraOnEnable && bodyYawAlignmentFramesRemaining > 0 && AlignBodyYawToXrCamera())
        {
            bodyYawAlignmentFramesRemaining--;
        }
    }

    private bool CacheXrCamera()
    {
        if (xrCamera != null)
        {
            return true;
        }

        GameObject rig = FindSceneObject("VRRigDeviceBased");
        if (rig == null)
        {
            rig = FindSceneObject("VrRigActionBased");
        }

        if (rig != null)
        {
            rig.SetActive(true);
            xrRigRoot = rig.transform;
            xrOrigin = rig.GetComponent<XROrigin>();
            xrCamera = xrOrigin != null && xrOrigin.Camera != null
                ? xrOrigin.Camera
                : rig.GetComponentInChildren<Camera>(true);
        }

        if (xrCamera == null && Camera.main != null && !Camera.main.transform.IsChildOf(transform))
        {
            xrCamera = Camera.main;
            xrRigRoot = FindContainingRigRoot(xrCamera.transform);
            xrOrigin = xrRigRoot != null ? xrRigRoot.GetComponent<XROrigin>() : null;
        }

        if (xrCamera == null)
        {
            Camera[] cameras = Object.FindObjectsOfType<Camera>(true);
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera candidate = cameras[i];
                if (candidate == null ||
                    candidate.transform.IsChildOf(transform) ||
                    candidate.gameObject.scene != SceneManager.GetActiveScene())
                {
                    continue;
                }

                xrCamera = candidate;
                xrRigRoot = FindContainingRigRoot(candidate.transform);
                xrOrigin = xrRigRoot != null ? xrRigRoot.GetComponent<XROrigin>() : null;
                break;
            }
        }

        return xrCamera != null;
    }

    private bool AlignXrRigToAvatar()
    {
        if (xrCamera == null)
        {
            return false;
        }

        if (headTransform == null)
        {
            BindAvatarParts();
        }

        Transform rigRoot = FindSafeXrMoveRoot();
        if (rigRoot == null)
        {
            return false;
        }

        xrRigRoot = rigRoot;
        LevelXrRigRoot(rigRoot);

        Vector3 targetEyePosition = GetTargetEyePosition();
        if (!TryMoveXrOriginCameraToWorldLocation(targetEyePosition))
        {
            rigRoot.position += targetEyePosition - xrCamera.transform.position;
        }

        xrRigAligned = true;
        ResetBodyFollowAnchor();
        headTrackingCalibrated = false;
        bodyYawTrackingCalibrated = false;
        return true;
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

    private void MovePlayer(Vector3 worldDelta)
    {
        Transform moveRoot = FindSafeXrMoveRoot();
        if (moveRoot != null)
        {
            moveRoot.position += worldDelta;
        }

        transform.position += worldDelta;
        ResetBodyFollowAnchor();
    }

    private void FollowXrMovementWithBody()
    {
        if (!followXrMovementWithBody)
        {
            return;
        }

        Transform anchor = GetXrBodyMovementAnchor();
        if (anchor == null)
        {
            bodyFollowAnchorCalibrated = false;
            return;
        }

        if (!bodyFollowAnchorCalibrated)
        {
            lastXrBodyAnchorPosition = anchor.position;
            bodyFollowAnchorCalibrated = true;
            return;
        }

        Vector3 worldDelta = anchor.position - lastXrBodyAnchorPosition;
        lastXrBodyAnchorPosition = anchor.position;

        if (!followXrVerticalMovement)
        {
            worldDelta.y = 0f;
        }

        if (worldDelta.sqrMagnitude <= 0.0000001f)
        {
            return;
        }

        transform.position += worldDelta;
    }

    private void ResetBodyFollowAnchor()
    {
        Transform anchor = GetXrBodyMovementAnchor();
        if (anchor == null)
        {
            bodyFollowAnchorCalibrated = false;
            return;
        }

        lastXrBodyAnchorPosition = anchor.position;
        bodyFollowAnchorCalibrated = true;
    }

    private Transform GetXrBodyMovementAnchor()
    {
        if (xrOrigin != null && IsSafeXrMoveRoot(xrOrigin.transform))
        {
            return xrOrigin.transform;
        }

        Transform anchor = xrRigRoot;
        if (anchor == null && xrCamera != null)
        {
            anchor = FindContainingRigRoot(xrCamera.transform);
        }

        return IsSafeXrMoveRoot(anchor) ? anchor : null;
    }

    private Transform FindSafeXrMoveRoot()
    {
        if (xrCamera == null)
        {
            return null;
        }

        Transform containingRig = xrRigRoot != null ? xrRigRoot : FindContainingRigRoot(xrCamera.transform);
        Transform current = xrCamera.transform;
        Transform best = null;

        while (current != null)
        {
            if (IsSafeXrMoveRoot(current))
            {
                best = current;
            }

            if (current == containingRig)
            {
                break;
            }

            current = current.parent;
        }

        return best;
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

    private void ApplyXrCameraPoseToAvatar()
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

        if (controlHeadFromVrHeadset && headTransform != null)
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

    private bool AlignBodyYawToXrCamera()
    {
        if (xrCamera == null || !TryExtractYawRotation(xrCamera.transform.rotation, out Quaternion cameraYaw))
        {
            return false;
        }

        float targetYaw = GetYawDegrees(cameraYaw) + cameraAlignedBodyYawOffset;
        float deltaYaw = Mathf.DeltaAngle(transform.eulerAngles.y, targetYaw);
        if (Mathf.Abs(deltaYaw) > 0.001f)
        {
            RotateBodyAroundCameraPosition(deltaYaw);
        }

        lastSyncedCameraYawRotation = cameraYaw;
        bodyYawTrackingCalibrated = true;
        headTrackingCalibrated = false;
        ResetBodyFollowAnchor();
        return true;
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

    private bool ShouldControlHandsFromVrControllers()
    {
        if (!controlHandsFromVrControllersInNoBot)
        {
            return false;
        }

        if (!LoginSession.HasRoute || !RoleAvatarIdentity.MatchesAvatarId(AvatarId, LoginSession.AvatarName))
        {
            return false;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == OfficeSceneSupport.OfficeLoggedIn)
        {
            return LoginSession.Role == LoginUserRole.Study ||
                LoginSession.Role == LoginUserRole.Leader ||
                LoginSession.Role == LoginUserRole.Member;
        }

        if (sceneName == OfficeSceneSupport.OfficeLoggedInNoBot)
        {
            return LoginSession.Role == LoginUserRole.Leader ||
                LoginSession.Role == LoginUserRole.Member;
        }

        return false;
    }

    private void ApplyVrControllerHandPoses()
    {
        if (!HasAvatarParts())
        {
            BindAvatarParts();
        }

        if (!HasAvatarParts())
        {
            if (!loggedMissingHandTargets)
            {
                Debug.LogWarning($"VR hand control skipped for '{name}': head or hand transforms were not found.", this);
                loggedMissingHandTargets = true;
            }

            return;
        }

        InputTracking.GetNodeStates(nodeStates);

        Pose headPose = Pose.identity;
        if (!TryGetNodePose(XRNode.Head, out headPose))
        {
            headPose.position = Vector3.zero;
            headPose.rotation = Quaternion.identity;
        }

        ApplyHandPose(XRNode.LeftHand, leftHandTransform, headPose, leftHandPositionOffset, leftHandRotationOffsetEuler);
        ApplyHandPose(XRNode.RightHand, rightHandTransform, headPose, rightHandPositionOffset, rightHandRotationOffsetEuler);
    }

    private void ApplyHandPose(XRNode node, Transform targetHand, Pose headPose, Vector3 positionOffset, Vector3 rotationOffsetEuler)
    {
        if (!TryGetNodePose(node, out Pose handPose))
        {
            return;
        }

        Vector3 headRelativePosition = Quaternion.Inverse(headPose.rotation) * (handPose.position - headPose.position);
        Quaternion headRelativeRotation = Quaternion.Inverse(headPose.rotation) * handPose.rotation;
        Transform handAnchor = GetHandWorldAnchor();
        if (handAnchor == null)
        {
            return;
        }

        targetHand.position = handAnchor.position + handAnchor.rotation * (headRelativePosition * handPoseScale + positionOffset);
        targetHand.rotation = handAnchor.rotation * headRelativeRotation * Quaternion.Euler(rotationOffsetEuler);
    }

    private Transform GetHandWorldAnchor()
    {
        if (xrCamera != null)
        {
            return xrCamera.transform;
        }

        return headTransform;
    }

    private bool TryGetNodePose(XRNode node, out Pose pose)
    {
        for (int i = 0; i < nodeStates.Count; i++)
        {
            XRNodeState state = nodeStates[i];
            if (state.nodeType != node)
            {
                continue;
            }

            bool hasPosition = state.TryGetPosition(out Vector3 position);
            bool hasRotation = state.TryGetRotation(out Quaternion rotation);
            if (hasPosition && hasRotation)
            {
                pose = new Pose(position, rotation);
                return true;
            }
        }

        pose = Pose.identity;
        return false;
    }

    private void BindAvatarParts()
    {
        headTransform = FindHeadTransform();
        leftEyeTransform = FindFirstChildStartingWith("LeftEye_");
        rightEyeTransform = FindFirstChildStartingWith("RightEye_");
        leftHandTransform = FindFirstChildStartingWith("LeftHand_");
        rightHandTransform = FindFirstChildStartingWith("RightHand_");
        loggedMissingHandTargets = false;
    }

    private bool HasAvatarParts()
    {
        return headTransform != null && leftHandTransform != null && rightHandTransform != null;
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

    private void DisableRoleCamera()
    {
        if (roleCamera == null)
        {
            roleCamera = GetComponentInChildren<Camera>(true);
        }

        Camera[] cameras = GetComponentsInChildren<Camera>(true);
        for (int i = 0; i < cameras.Length; i++)
        {
            DisableCamera(cameras[i], true);
        }
    }

    private static void DisableOtherSceneCameras(Camera keepCamera)
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Camera[] cameras = roots[i].GetComponentsInChildren<Camera>(true);
            for (int j = 0; j < cameras.Length; j++)
            {
                if (cameras[j] != keepCamera)
                {
                    DisableCamera(cameras[j], false);
                }
            }
        }
    }

    private static void EnableCameraAsMain(Camera camera)
    {
        if (camera == null)
        {
            return;
        }

        camera.gameObject.SetActive(true);
        camera.enabled = true;
        camera.tag = "MainCamera";

        AudioListener listener = camera.GetComponent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = true;
        }
    }

    private static void DisableCamera(Camera camera, bool deactivateObject)
    {
        if (camera == null)
        {
            return;
        }

        camera.enabled = false;
        camera.tag = "Untagged";

        AudioListener listener = camera.GetComponent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = false;
        }

        if (deactivateObject)
        {
            camera.gameObject.SetActive(false);
        }
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

    private static float GetYawDegrees(Quaternion yawRotation)
    {
        Vector3 forward = yawRotation * Vector3.forward;
        return Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
    }

    private static GameObject FindSceneObject(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            return null;
        }

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i].name == objectName)
            {
                return roots[i];
            }

            Transform[] children = roots[i].GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < children.Length; j++)
            {
                if (children[j].name == objectName)
                {
                    return children[j].gameObject;
                }
            }
        }

        return null;
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
