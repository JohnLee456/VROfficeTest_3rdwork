using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonLoginFlow : MonoBehaviourPunCallbacks
{
    public const string DefaultRoomName = "VROffice";

    private const string GameVersion = "1";
    private const string FixedRegion = "jp";
    private const byte MaxPlayersPerRoom = 10;

    private static PhotonLoginFlow instance;

    private LoginRoute pendingRoute;
    private string pendingRoomName;
    private Action<string> failedCallback;
    private bool loginInProgress;

    public static PhotonLoginFlow Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindObjectOfType<PhotonLoginFlow>();
            if (instance != null)
            {
                return instance;
            }

            GameObject flowObject = new GameObject("Photon Login Flow");
            instance = flowObject.AddComponent<PhotonLoginFlow>();
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Login(LoginRoute route, string roomName, Action<string> onFailed)
    {
        if (loginInProgress)
        {
            onFailed?.Invoke("Photon login is already in progress.");
            return;
        }

        if (route == null)
        {
            onFailed?.Invoke("Login route is missing.");
            return;
        }

        pendingRoute = route;
        pendingRoomName = NormalizeRoomName(roomName);
        failedCallback = onFailed;
        loginInProgress = true;

        ApplyLoginIdentity();
        ConfigurePhoton();

        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.Name == pendingRoomName)
            {
                CompleteRoomJoin();
                return;
            }

            PhotonNetwork.LeaveRoom();
            return;
        }

        if (PhotonNetwork.IsConnectedAndReady)
        {
            JoinTargetRoom();
            return;
        }

        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log($"Connecting to Photon as '{pendingRoute.AvatarName}' for room '{pendingRoomName}'.");
            if (!PhotonNetwork.ConnectUsingSettings())
            {
                FailLogin("Photon connection could not be started.");
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        if (!loginInProgress)
        {
            return;
        }

        ApplyLoginIdentity();
        JoinTargetRoom();
    }

    public override void OnLeftRoom()
    {
        if (!loginInProgress)
        {
            return;
        }

        JoinTargetRoom();
    }

    public override void OnJoinedRoom()
    {
        if (!loginInProgress)
        {
            return;
        }

        CompleteRoomJoin();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (!loginInProgress)
        {
            return;
        }

        FailLogin($"Join room failed ({returnCode}): {message}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (!loginInProgress)
        {
            return;
        }

        FailLogin($"Create room failed ({returnCode}): {message}");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (!loginInProgress)
        {
            return;
        }

        FailLogin($"Photon disconnected: {cause}");
    }

    private void JoinTargetRoom()
    {
        ApplyLoginIdentity();

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = MaxPlayersPerRoom,
            IsOpen = true,
            IsVisible = true
        };

        Debug.Log($"Joining or creating Photon room '{pendingRoomName}' as '{pendingRoute.AvatarName}'.");
        PhotonNetwork.JoinOrCreateRoom(pendingRoomName, options, TypedLobby.Default);
    }

    private void CompleteRoomJoin()
    {
        ApplyLoginIdentity();
        Debug.Log($"Joined Photon room '{PhotonNetwork.CurrentRoom.Name}' as '{pendingRoute.AvatarName}'. Loading target scene.");

        string sceneName = LoginSceneTarget.SceneName;
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            FailLogin($"Cannot load scene '{sceneName}'. Please make sure it is enabled in Build Settings.");
            return;
        }

        loginInProgress = false;
        failedCallback = null;
        PhotonRoomChatManager.Instance.ConnectToCurrentRoomChannel();
        LoginSceneTarget.Load();
    }

    private void ApplyLoginIdentity()
    {
        LoginSession.Apply(pendingRoute);
        LobbyMain.playerId = pendingRoute.AvatarName;
    }

    private static void ConfigurePhoton()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
        PhotonNetwork.GameVersion = GameVersion;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = FixedRegion;
    }

    private void FailLogin(string message)
    {
        Debug.LogWarning(message);
        loginInProgress = false;

        Action<string> callback = failedCallback;
        failedCallback = null;
        callback?.Invoke(message);
    }

    private static string NormalizeRoomName(string roomName)
    {
        return string.IsNullOrWhiteSpace(roomName) ? DefaultRoomName : roomName.Trim();
    }
}
