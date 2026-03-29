using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using proto.LoginMsg;

public class AnalServerCon : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI stateText;
    
    public string ipAdd = "192.168.2.109";
    public int port = 8848;

    public string userId = "";
    public string password = "";

    private string curState = "";

    private bool isConnected = false;

    // Start is called before the first frame update
    void Start()
    {
        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.AddEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
        NetManager.AddEventListener(NetManager.NetEvent.Close, OnConnectClose);
        NetManager.AddMsgListener("MsgLogin", OnMsgLogin);
        OnConnectClick();
    }

    // Update is called once per frame
    void Update()
    {
        if (stateText != null)
        {
            stateText.text = curState;
        }
        NetManager.Update();
    }

    public void OnConnectClick()
    {
        NetManager.Connect(ipAdd, port);
        //TODO:开始转圈圈 显示连接中...
        curState = "Connecting";
    }

    public void OnLoginClick()
    {
        if (!isConnected)
        {
            curState = "No Connected";
            return;
        }
        if (userId == "" || password == "")
        {
            Debug.Log("userId or password is empty!");
            curState = "Empty";
            return;
        }
        MsgLogin msgLogin = new MsgLogin();
        msgLogin.id = userId;
        msgLogin.pw = password;
        NetManager.Send(msgLogin);
    }

    public void OnCloseClick()
    {
        NetManager.Close();
        //TODO:开始转圈圈 显示连接中...
    }

    //登陆成功回调函数
    public void OnMsgLogin(ProtoBuf.IExtensible msgBase)
    {
        MsgLogin msg = (MsgLogin)msgBase;
        if (msg.result == 0)
        {
            PhotonNetwork.LocalPlayer.NickName = userId;
            curState = "Login succ";
        }
        else
        {
            curState = "Login fail";
        }

    }


    //连接成功回调
    void OnConnectSucc(string err)
    {
        Debug.Log("OnConnectSucc");
        //TODO: Enter Game
        isConnected = true;
        curState = "Connect Succ";
    }

    void OnConnectFail(string err)
    {
        Debug.Log("OnConnectFail " + err);
        //TODO: SHOW Connect Fail in Text
        curState = "Connect Fail";
    }

    //关闭连接
    void OnConnectClose(string err)
    {
        Debug.Log("OnConnectClose");
        //TODO: 弹出提示框（网络断开）
        //TODO: 弹出按钮（重新连接）
    }

    private void OnDisable()
    {
        //保证注册的监听不影响下一个场景
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
        NetManager.RemoveEventListener(NetManager.NetEvent.Close, OnConnectClose);
        NetManager.RemoveMsgListener("MsgLogin", OnMsgLogin);
    }
}
