using Photon.Pun;
using proto.RoomMsg;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToLoginAfterLeavingRoom : MonoBehaviourPunCallbacks
{
    private const string LoginSceneName = "Lobby";

    private static ReturnToLoginAfterLeavingRoom instance;
    private bool returnRequested;

    public static void StartReturn()
    {
        Instance.BeginReturn();
    }

    private static ReturnToLoginAfterLeavingRoom Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindObjectOfType<ReturnToLoginAfterLeavingRoom>();
            if (instance != null)
            {
                return instance;
            }

            GameObject runner = new GameObject("Return To Login After Leaving Room");
            instance = runner.AddComponent<ReturnToLoginAfterLeavingRoom>();
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

    private void BeginReturn()
    {
        if (returnRequested)
        {
            return;
        }

        returnRequested = true;
        SendLeaveRoomMessage();

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            return;
        }

        LoadLoginScene();
    }

    public override void OnLeftRoom()
    {
        if (!returnRequested)
        {
            return;
        }

        LoadLoginScene();
    }

    private static void SendLeaveRoomMessage()
    {
        MsgLeaveRoom msg = new MsgLeaveRoom();
        NetManager.Send(msg);
    }

    private void LoadLoginScene()
    {
        returnRequested = false;
        SceneManager.LoadScene(LoginSceneName);
    }
}
