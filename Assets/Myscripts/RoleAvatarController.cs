using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Photon.Pun;
using System.Collections.Generic;

public class RoleAvatarController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0f, 0.03f, 0.08f);
    [SerializeField] private Vector3 cameraLocalEulerAngles = Vector3.zero;
    [SerializeField] private bool controlHandsFromVrControllersInNoBot = true;
    [SerializeField] private float handPoseScale = 1f;
    [SerializeField] private Vector3 leftHandPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 rightHandPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 leftHandRotationOffsetEuler = Vector3.zero;
    [SerializeField] private Vector3 rightHandRotationOffsetEuler = Vector3.zero;

    private PhotonView photonView;
    private Camera roleCamera;
    private bool localControlEnabled;
    private Transform headTransform;
    private Transform leftHandTransform;
    private Transform rightHandTransform;
    private readonly List<XRNodeState> nodeStates = new List<XRNodeState>();
    private bool loggedMissingHandTargets;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        roleCamera = GetComponentInChildren<Camera>(true);
    }

    private void Start()
    {
        if (CanUseLocalControl())
        {
            SetRoleCameraEnabled(true);
        }

        BindAvatarParts();
    }

    public void SetLocalControlEnabled(bool enabled)
    {
        localControlEnabled = enabled;
        SetRoleCameraEnabled(enabled && CanUseLocalControl());
    }

    public void PrepareRoleCamera()
    {
        EnsureRoleCamera();
        SetRoleCameraEnabled(false);
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
        transform.position += move.normalized * speed * Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (!CanUseLocalControl() || !ShouldControlHandsFromVrControllers())
        {
            return;
        }

        ApplyVrControllerHandPoses();
    }

    private bool CanUseLocalControl()
    {
        if (!localControlEnabled)
        {
            return false;
        }

        if (photonView != null && PhotonNetwork.InRoom && !photonView.IsMine)
        {
            return false;
        }

        return true;
    }

    private void SetRoleCameraEnabled(bool enabled)
    {
        Camera targetCamera = enabled ? EnsureRoleCamera() : roleCamera;
        if (targetCamera == null)
        {
            return;
        }

        targetCamera.gameObject.SetActive(enabled);
        targetCamera.enabled = enabled;
        targetCamera.tag = enabled ? "MainCamera" : "Untagged";

        AudioListener listener = targetCamera.GetComponent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = enabled;
        }
    }

    private Camera EnsureRoleCamera()
    {
        if (roleCamera != null)
        {
            return roleCamera;
        }

        headTransform = FindHeadTransform();
        if (headTransform == null)
        {
            Debug.LogWarning($"Role avatar camera setup skipped for '{name}': head transform was not found.", this);
            return null;
        }

        Transform existingRoleCamera = headTransform.Find("RoleCamera");
        if (existingRoleCamera != null)
        {
            roleCamera = existingRoleCamera.GetComponent<Camera>();
        }

        if (roleCamera == null)
        {
            GameObject cameraObject = new GameObject("RoleCamera");
            cameraObject.transform.SetParent(headTransform, false);
            roleCamera = cameraObject.AddComponent<Camera>();
        }

        Transform cameraTransform = roleCamera.transform;
        cameraTransform.localPosition = cameraLocalPosition;
        cameraTransform.localEulerAngles = cameraLocalEulerAngles;

        DisableTrackedPoseDrivers(roleCamera);

        if (roleCamera.GetComponent<AudioListener>() == null)
        {
            roleCamera.gameObject.AddComponent<AudioListener>();
        }

        return roleCamera;
    }

    private bool ShouldControlHandsFromVrControllers()
    {
        if (!controlHandsFromVrControllersInNoBot)
        {
            return false;
        }

        if (!LoginSession.HasRoute || LoginSession.AvatarName != gameObject.name)
        {
            return false;
        }

        if (LoginSession.Role != LoginUserRole.Leader && LoginSession.Role != LoginUserRole.Member)
        {
            return false;
        }

        return SceneManager.GetActiveScene().name == OfficeSceneSupport.OfficeLoggedInNoBot;
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

        targetHand.position = headTransform.position + headTransform.rotation * (headRelativePosition * handPoseScale + positionOffset);
        targetHand.rotation = headTransform.rotation * headRelativeRotation * Quaternion.Euler(rotationOffsetEuler);
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

    private static void DisableTrackedPoseDrivers(Camera targetCamera)
    {
        MonoBehaviour[] behaviours = targetCamera.GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null)
            {
                continue;
            }

            if (behaviour.GetType().Name.Contains("TrackedPoseDriver"))
            {
                behaviour.enabled = false;
            }
        }
    }
}
