using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using proto.RoomMsg;
using UnityEngine.SceneManagement;

public class OfficeMain : MonoBehaviour
{

    public GameObject userMenu;
    private bool localLoginMode;
    private bool netListenerRegistered;


    //为了处理无法在主线程之外创建模板的问题 这里主要用于OnClose()
    private delegate void action(params object[] para);

    struct ToDoAction
    {
        public action actionFun;
        public object param;
    }

    List<ToDoAction> toDoActionList = new List<ToDoAction>();
    List<ToDoAction> currentAvtionList = new List<ToDoAction>();


    // Start is called before the first frame update
    void Start()
    {
        localLoginMode = SceneManager.GetActiveScene().name == "OfficeLoggedIn" && !PhotonNetwork.IsConnected;
        if (localLoginMode)
        {
            Debug.Log("OfficeMain running in OfficeLoggedIn local test mode.");
            return;
        }

        PanelManager.Init();
        PanelManager.Open<UserMenuPanel>();

        //进入房间之后在数据收集服务端也要进入房间
        MsgEnterRoom msg = new MsgEnterRoom();
/*        msg.id = PhotonNetwork.CurrentRoom.Name;*/
        NetManager.Send(msg);

        //注册Close事件，用来监听掉线或服务器关闭
        NetManager.AddEventListener(NetManager.NetEvent.Close, OnConnectClose);
        netListenerRegistered = true;

        ////test
        //string text = "Set from Photon";
        //object[] myCustomInitData = new object[] { text };
        //PhotonNetwork.Instantiate("Simple Helvetica Test", new Vector3(0f, 2f, 1.7f), Quaternion.identity, 0, myCustomInitData);
    }

    // Update is called once per frame
    void Update()
    {
        if (localLoginMode)
        {
            return;
        }

        if (toDoActionList.Count > 0)
        {
            lock (toDoActionList)
            {
                currentAvtionList.Clear();
                currentAvtionList.AddRange(toDoActionList);
                toDoActionList.Clear();
            }
            for (int i = 0; i < currentAvtionList.Count; i++)
            {
                currentAvtionList[i].actionFun(currentAvtionList[i].param);
            }
        }

        NetManager.Update();
    }

    private void OnDisable()
    {
        if (!localLoginMode)
        {
            PanelManager.DeInit();
        }

        if (netListenerRegistered)
        {
            NetManager.RemoveEventListener(NetManager.NetEvent.Close, OnConnectClose);
        }
    }


    private void OnConnectClose(string err)
    {
        string info = "Lost connection to data server!";
        lock (toDoActionList)
        {
            toDoActionList.Add(new ToDoAction { actionFun = PanelManager.Open<TipPanel>, param = info });
        }
    }

}
