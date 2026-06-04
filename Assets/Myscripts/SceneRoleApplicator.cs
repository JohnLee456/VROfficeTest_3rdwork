using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public static class SceneRoleApplicator
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyToScene(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToScene(scene);
    }

    private static void ApplyToScene(Scene scene)
    {
        if (!LoginSession.HasRoute || !scene.IsValid())
        {
            return;
        }

        if (scene.name != LoginSession.SceneName)
        {
            return;
        }

        DisableSceneCameras(scene);
        PrepareKnownRoleCameras(scene);

        GameObject avatar = FindInScene(scene, LoginSession.AvatarName);
        if (avatar == null)
        {
            Debug.LogWarning($"Login role '{LoginSession.Role}' could not find avatar '{LoginSession.AvatarName}' in scene '{scene.name}'.");
            return;
        }

        avatar.SetActive(true);
        TakePhotonOwnershipIfPossible(avatar);
        DisableExistingAvatarControllers(scene, avatar);
        DisableUnusedLocalRigs(scene);

        RoleAvatarController controller = avatar.GetComponent<RoleAvatarController>();
        if (controller == null)
        {
            controller = avatar.AddComponent<RoleAvatarController>();
        }

        controller.enabled = true;
        controller.SetLocalControlEnabled(true);
    }

    private static void DisableSceneCameras(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Camera[] cameras = roots[i].GetComponentsInChildren<Camera>(true);
            for (int j = 0; j < cameras.Length; j++)
            {
                cameras[j].enabled = false;
                cameras[j].tag = "Untagged";

                AudioListener listener = cameras[j].GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = false;
                }
            }
        }
    }

    private static void PrepareKnownRoleCameras(Scene scene)
    {
        string[] avatarNames = { "GCHbot", "ZJR", "ZHZ", "DCY" };
        for (int i = 0; i < avatarNames.Length; i++)
        {
            GameObject avatar = FindInScene(scene, avatarNames[i]);
            if (avatar == null)
            {
                continue;
            }

            RoleAvatarController controller = avatar.GetComponent<RoleAvatarController>();
            if (controller == null)
            {
                controller = avatar.AddComponent<RoleAvatarController>();
            }

            controller.PrepareRoleCamera();
            controller.SetLocalControlEnabled(false);
            controller.enabled = false;
        }
    }

    private static void TakePhotonOwnershipIfPossible(GameObject avatar)
    {
        PhotonView photonView = avatar.GetComponent<PhotonView>();
        if (photonView == null || !PhotonNetwork.InRoom)
        {
            return;
        }

        if (!photonView.IsMine)
        {
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }
    }

    private static void DisableExistingAvatarControllers(Scene scene, GameObject selectedAvatar)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            GCHBotController[] gchControllers = roots[i].GetComponentsInChildren<GCHBotController>(true);
            for (int j = 0; j < gchControllers.Length; j++)
            {
                gchControllers[j].enabled = false;
            }

            RoleAvatarController[] roleControllers = roots[i].GetComponentsInChildren<RoleAvatarController>(true);
            for (int j = 0; j < roleControllers.Length; j++)
            {
                if (roleControllers[j].gameObject != selectedAvatar)
                {
                    roleControllers[j].SetLocalControlEnabled(false);
                    roleControllers[j].enabled = false;
                }
            }
        }
    }

    private static void DisableUnusedLocalRigs(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i].name == "VRRigDeviceBased" || roots[i].name == "VrRigActionBased" || roots[i].name == "ScreenRig")
            {
                roots[i].SetActive(false);
            }
        }
    }

    private static GameObject FindInScene(Scene scene, string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
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
}
