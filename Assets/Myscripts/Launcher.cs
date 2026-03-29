using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;


public class Launcher : MonoBehaviourPunCallbacks
{
    #region Private Fields

    /// <summary>
    /// This client's version number. Users are separated from each other by gameVersion(which allows you to make breaking changes)
    /// </summary>
    string gameVersion = "1";

    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    [SerializeField]
    private byte maxPlayersPerRoom = 10;

    /// <summary>
    /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
    /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
    /// Typically this is used for the OnConnectedToMaster() callback.
    /// </summary>
    bool isConnecting;

    #endregion



    #region Public Field

    [Tooltip("The UI Panel to let the user enter name, connect and play")]
    [SerializeField]
    private GameObject controlPanel;

    [Tooltip("The UI Label to inform the user that the connection is in progress")]
    [SerializeField]
    private GameObject progressLabel;

    #endregion

    #region MonoBehaviours CallBacks

    // Start is called before the first frame update
    void Start()
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
        //Connect();
    }

    #endregion

    #region Public Methods

    public void Connect()
    {
        progressLabel.SetActive(true);
        controlPanel.SetActive(false);

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.UseRpcMonoBehaviourCache = true;

            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "jp";
            //keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected,
            //so we need to know what to do then
            isConnecting = PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    #endregion



    #region MonoBehaviourPunCallbacks Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("Luncher: OnConnectedToMaster was called by PUN");

        //为photon设置用户id
        PhotonNetwork.LocalPlayer.NickName = LobbyMain.playerId;
        
        
        // we don't want to do anything if we are not attempting to join a room.
        // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
        // we don't want to do anything.
        if (isConnecting)
        {
            // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
            PhotonNetwork.JoinRandomRoom();
            isConnecting = false;
        }

    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);

        Debug.LogWarningFormat("Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {

        base.OnJoinedRoom();
        Debug.Log("Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
        ////for test
        //Debug.Log("After creatRoom " + PhotonNetwork.CurrentRoom.Name);
        //Debug.Log(PhotonNetwork.CurrentRoom.Players.Count);
        //foreach (var entry in PhotonNetwork.CurrentRoom.Players)
        //{
        //    Debug.Log("Key: "+ entry.Key + "    Value: " + entry.Value.NickName);
        //}

        SceneManager.LoadScene("Office");

        //if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        //{
        //    // #Critical: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
        //    Debug.Log("We load the 'Office'");

        //    SceneManager.LoadScene("Office");
        //}
    }

    #endregion

}