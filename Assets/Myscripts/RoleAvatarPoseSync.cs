using Photon.Pun;
using UnityEngine;

public class RoleAvatarPoseSync : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private float remoteLerpSpeed = 18f;
    [SerializeField] private bool syncRootRotation = true;

    private Transform headTransform;
    private Transform leftHandTransform;
    private Transform rightHandTransform;
    private bool hasTargets;

    private Quaternion targetRootRotation;
    private Vector3 targetHeadLocalPosition;
    private Quaternion targetHeadLocalRotation;
    private Vector3 targetLeftHandLocalPosition;
    private Quaternion targetLeftHandLocalRotation;
    private Vector3 targetRightHandLocalPosition;
    private Quaternion targetRightHandLocalRotation;
    private bool hasRemotePose;

    private void Awake()
    {
        BindAvatarParts();
        CaptureCurrentPoseAsTarget();
    }

    private void OnEnable()
    {
        EnsureObservedByPhotonView();
    }

    private void LateUpdate()
    {
        if (!hasRemotePose || photonView == null || photonView.IsMine)
        {
            return;
        }

        if (!hasTargets)
        {
            BindAvatarParts();
        }

        float t = 1f - Mathf.Exp(-remoteLerpSpeed * Time.deltaTime);

        if (syncRootRotation)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRootRotation, t);
        }

        ApplyRemoteTransform(headTransform, targetHeadLocalPosition, targetHeadLocalRotation, t);
        ApplyRemoteTransform(leftHandTransform, targetLeftHandLocalPosition, targetLeftHandLocalRotation, t);
        ApplyRemoteTransform(rightHandTransform, targetRightHandLocalPosition, targetRightHandLocalRotation, t);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (!hasTargets)
        {
            BindAvatarParts();
        }

        if (stream.IsWriting)
        {
            stream.SendNext(transform.rotation);
            WriteTransform(stream, headTransform);
            WriteTransform(stream, leftHandTransform);
            WriteTransform(stream, rightHandTransform);
            return;
        }

        targetRootRotation = (Quaternion)stream.ReceiveNext();
        ReadTransform(stream, out targetHeadLocalPosition, out targetHeadLocalRotation);
        ReadTransform(stream, out targetLeftHandLocalPosition, out targetLeftHandLocalRotation);
        ReadTransform(stream, out targetRightHandLocalPosition, out targetRightHandLocalRotation);
        hasRemotePose = true;
    }

    private void EnsureObservedByPhotonView()
    {
        if (photonView == null || photonView.ObservedComponents.Contains(this))
        {
            return;
        }

        photonView.ObservedComponents.Add(this);
    }

    private void CaptureCurrentPoseAsTarget()
    {
        targetRootRotation = transform.rotation;
        CaptureTransform(headTransform, out targetHeadLocalPosition, out targetHeadLocalRotation);
        CaptureTransform(leftHandTransform, out targetLeftHandLocalPosition, out targetLeftHandLocalRotation);
        CaptureTransform(rightHandTransform, out targetRightHandLocalPosition, out targetRightHandLocalRotation);
    }

    private void BindAvatarParts()
    {
        headTransform = FindHeadTransform();
        leftHandTransform = FindFirstChildStartingWith("LeftHand_");
        rightHandTransform = FindFirstChildStartingWith("RightHand_");
        hasTargets = headTransform != null && leftHandTransform != null && rightHandTransform != null;
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

    private static void WriteTransform(PhotonStream stream, Transform target)
    {
        if (target == null)
        {
            stream.SendNext(Vector3.zero);
            stream.SendNext(Quaternion.identity);
            return;
        }

        stream.SendNext(target.localPosition);
        stream.SendNext(target.localRotation);
    }

    private static void ReadTransform(PhotonStream stream, out Vector3 localPosition, out Quaternion localRotation)
    {
        localPosition = (Vector3)stream.ReceiveNext();
        localRotation = (Quaternion)stream.ReceiveNext();
    }

    private static void CaptureTransform(Transform target, out Vector3 localPosition, out Quaternion localRotation)
    {
        if (target == null)
        {
            localPosition = Vector3.zero;
            localRotation = Quaternion.identity;
            return;
        }

        localPosition = target.localPosition;
        localRotation = target.localRotation;
    }

    private static void ApplyRemoteTransform(Transform target, Vector3 localPosition, Quaternion localRotation, float t)
    {
        if (target == null)
        {
            return;
        }

        target.localPosition = Vector3.Lerp(target.localPosition, localPosition, t);
        target.localRotation = Quaternion.Slerp(target.localRotation, localRotation, t);
    }
}
