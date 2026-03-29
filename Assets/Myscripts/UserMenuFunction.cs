using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

using proto.RoomMsg;

public class UserMenuFunction : MonoBehaviourPunCallbacks
{
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
        Debug.Log("Done OnLeftRoom");
    }

    public void LeaveRoom()
    {
        MsgLeaveRoom msg = new MsgLeaveRoom();
        NetManager.Send(msg);
        PhotonNetwork.LeaveRoom();
    }

    public void OpenPosListPanel()
    {
        PanelManager.Open<PosListPanel>();
        gameObject.SetActive(false);
    }

    public void OpenGenTextPanel()
    {
        PanelManager.Open<GenTextPanel>();
        gameObject.SetActive(false);
    }

    public void OpenSettingPanel()
    {
        PanelManager.Open<SettingPanel>();
        gameObject.SetActive(false);
    }

    public void OpenTimePanel()
    {
        PanelManager.Open<TimePanel>();
        gameObject.SetActive(false);
    }

    public void OpenItemsPanel()
    {
        PanelManager.Open<ItemsPanel>();
        gameObject.SetActive(false);
    }
}
