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

        EnsureKnownRoleIdentities(scene);
        GameObject localRig = PrepareLocalXrRig(scene);
        DisableKnownRoleCameras(scene);

        GameObject avatar = FindAvatarById(scene, LoginSession.AvatarName);
        if (avatar == null)
        {
            Debug.LogWarning($"Login role '{LoginSession.Role}' could not find avatar '{LoginSession.AvatarName}' in scene '{scene.name}'.");
            return;
        }

        avatar.SetActive(true);
        TakePhotonOwnershipIfPossible(avatar);
        DisableExistingAvatarControllers(scene, avatar);
        DisableUnusedLocalRigs(scene, localRig);

        RoleAvatarController controller = avatar.GetComponent<RoleAvatarController>();
        if (controller == null)
        {
            controller = avatar.AddComponent<RoleAvatarController>();
        }

        controller.enabled = true;
        controller.SetLocalControlEnabled(true);
    }

    private static GameObject PrepareLocalXrRig(Scene scene)
    {
        GameObject localRig = FindInScene(scene, "VRRigDeviceBased");
        if (localRig == null)
        {
            localRig = FindInScene(scene, "VrRigActionBased");
        }

        Camera localCamera = null;
        if (localRig != null)
        {
            localRig.SetActive(true);
            localCamera = localRig.GetComponentInChildren<Camera>(true);
        }

        if (localCamera == null)
        {
            localCamera = FindBestSceneCamera(scene);
        }

        if (localCamera != null)
        {
            EnableCameraAsMain(localCamera);
            DisableSceneCamerasExcept(scene, localCamera);
            SetTrackedPoseDriversEnabled(localCamera, true);
        }

        return localRig;
    }

    private static void DisableSceneCamerasExcept(Scene scene, Camera cameraToKeep)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Camera[] cameras = roots[i].GetComponentsInChildren<Camera>(true);
            for (int j = 0; j < cameras.Length; j++)
            {
                if (cameras[j] != cameraToKeep)
                {
                    DisableCamera(cameras[j]);
                }
            }
        }
    }

    private static void DisableKnownRoleCameras(Scene scene)
    {
        string[] avatarNames = RoleAvatarIdentity.KnownAvatarIds;
        for (int i = 0; i < avatarNames.Length; i++)
        {
            GameObject avatar = FindAvatarById(scene, avatarNames[i]);
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

    private static void EnsureKnownRoleIdentities(Scene scene)
    {
        string[] avatarIds = RoleAvatarIdentity.KnownAvatarIds;
        for (int i = 0; i < avatarIds.Length; i++)
        {
            GameObject avatar = FindInScene(scene, avatarIds[i]);
            if (avatar == null)
            {
                continue;
            }

            RoleAvatarIdentity identity = avatar.GetComponent<RoleAvatarIdentity>();
            if (identity == null)
            {
                identity = avatar.AddComponent<RoleAvatarIdentity>();
            }

            identity.InitializeIfEmpty(avatarIds[i]);
        }
    }

    private static Camera FindBestSceneCamera(Scene scene)
    {
        GameObject rig = FindInScene(scene, "VRRigDeviceBased");
        if (rig == null)
        {
            rig = FindInScene(scene, "VrRigActionBased");
        }

        if (rig != null)
        {
            Camera rigCamera = rig.GetComponentInChildren<Camera>(true);
            if (rigCamera != null)
            {
                return rigCamera;
            }
        }

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Camera[] cameras = roots[i].GetComponentsInChildren<Camera>(true);
            for (int j = 0; j < cameras.Length; j++)
            {
                if (cameras[j].gameObject.name == "RoleCamera")
                {
                    continue;
                }

                return cameras[j];
            }
        }

        return null;
    }

    private static void TakePhotonOwnershipIfPossible(GameObject avatar)
    {
        PhotonView photonView = avatar.GetComponent<PhotonView>();
        if (photonView == null || !PhotonNetwork.InRoom)
        {
            return;
        }

        if (photonView.OwnershipTransfer == OwnershipOption.Fixed)
        {
            photonView.OwnershipTransfer = OwnershipOption.Takeover;
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

    private static void DisableUnusedLocalRigs(Scene scene, GameObject localRig)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i] == localRig)
            {
                continue;
            }

            if (roots[i].name == "VrRigActionBased" || roots[i].name == "ScreenRig")
            {
                roots[i].SetActive(false);
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

    private static void DisableCamera(Camera camera)
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

    private static GameObject FindAvatarById(Scene scene, string avatarId)
    {
        if (string.IsNullOrWhiteSpace(avatarId))
        {
            return null;
        }

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            RoleAvatarIdentity[] identities = roots[i].GetComponentsInChildren<RoleAvatarIdentity>(true);
            for (int j = 0; j < identities.Length; j++)
            {
                if (identities[j].Matches(avatarId))
                {
                    return identities[j].gameObject;
                }
            }
        }

        return FindInScene(scene, avatarId);
    }
}
