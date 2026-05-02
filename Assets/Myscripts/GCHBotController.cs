using UnityEngine;

public class GCHBotController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private bool attachMainCameraToHead = true;
    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0f, 0.03f, 0.08f);
    [SerializeField] private Vector3 cameraLocalEulerAngles = Vector3.zero;

    private Transform headTransform;

    private void Start()
    {
        if (attachMainCameraToHead)
        {
            AttachMainCameraToHead();
        }
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

    private void AttachMainCameraToHead()
    {
        headTransform = FindHeadTransform();
        if (headTransform == null)
        {
            Debug.LogWarning("GCHbot camera attach skipped: head transform was not found.", this);
            return;
        }

        Camera targetCamera = Camera.main;
        if (targetCamera == null)
        {
            targetCamera = FindObjectOfType<Camera>();
        }

        if (targetCamera == null)
        {
            Debug.LogWarning("GCHbot camera attach skipped: no camera was found.", this);
            return;
        }

        DisableTrackedPoseDrivers(targetCamera);

        Transform cameraTransform = targetCamera.transform;
        cameraTransform.SetParent(headTransform, false);
        cameraTransform.localPosition = cameraLocalPosition;
        cameraTransform.localEulerAngles = cameraLocalEulerAngles;
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
