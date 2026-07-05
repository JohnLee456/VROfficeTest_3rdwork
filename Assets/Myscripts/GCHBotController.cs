using UnityEngine;

/// <summary>
/// Backwards-compatible scene component for GCHbot.
/// GCHbot now uses the shared RoleAvatarController binding path, like ZHZ/ZJR/DCY.
/// </summary>
public class GCHBotController : MonoBehaviour
{
    private const string GchAvatarId = "GCHbot";

    [SerializeField] private bool handOffToSharedRoleBinding = true;
    [SerializeField] private bool alignBodyYawToCameraOnEnable = true;
    [SerializeField] private float cameraAlignedBodyYawOffset;

    private void Awake()
    {
        EnsureIdentity();
        ConfigureSharedRoleBinding();
    }

    private void Start()
    {
        HandOffToSharedRoleBinding();
    }

    private void HandOffToSharedRoleBinding()
    {
        if (!handOffToSharedRoleBinding)
        {
            return;
        }

        RoleAvatarController controller = EnsureSharedRoleController();

        bool shouldControlLocally =
            LoginSession.HasRoute &&
            gameObject.scene.name == LoginSession.SceneName &&
            RoleAvatarIdentity.MatchesAvatarId(LoginSession.AvatarName, GchAvatarId);

        controller.enabled = shouldControlLocally;
        controller.SetLocalControlEnabled(shouldControlLocally);

        enabled = false;
    }

    private void ConfigureSharedRoleBinding()
    {
        RoleAvatarController controller = EnsureSharedRoleController();
        controller.ConfigureInitialBodyYawAlignment(alignBodyYawToCameraOnEnable, cameraAlignedBodyYawOffset);
    }

    private RoleAvatarController EnsureSharedRoleController()
    {
        RoleAvatarController controller = GetComponent<RoleAvatarController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<RoleAvatarController>();
        }

        return controller;
    }

    private void EnsureIdentity()
    {
        RoleAvatarIdentity identity = GetComponent<RoleAvatarIdentity>();
        if (identity == null)
        {
            identity = gameObject.AddComponent<RoleAvatarIdentity>();
        }

        identity.InitializeIfEmpty(GchAvatarId);
    }
}
