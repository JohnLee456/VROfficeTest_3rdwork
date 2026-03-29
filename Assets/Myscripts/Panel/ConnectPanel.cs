using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRUiKits.Utils;
using System;


public class ConnectPanel : BasePanel
{
    private GameObject keyboard;
    private UIKitInputField ipInput;
    private UIKitInputField portInput;
    private InvokeKeyboard ipInputKeyboard;
    private InvokeKeyboard portInputKeyboard;
    private Button connectBtn;
    private Button hideBtn;

    private GameObject connectingPanel;
    private GameObject controlPanel;

    //为了解决无法在主线程之外创建模板的问题
    private delegate void action(params object[] para);

    struct ToDoAction
    {
        public action actionFun;
        public object param;
    }

    List<ToDoAction> toDoActionList = new List<ToDoAction>();
    List<ToDoAction> currentAvtionList = new List<ToDoAction>();

    //初始化
    public override void OnInit()
    {
        skinPath = "ConnectPanel";
        layer = PanelManager.Layer.Panel;
        keyboard = GameObject.Find("Manager").GetComponent<LobbyMain>().keyboard;
    }

    public override void OnShow(params object[] args)
    {
        //寻找组件
        ipInput = skin.transform.Find("Contents/IPInputField").GetComponent<UIKitInputField>();
        portInput = skin.transform.Find("Contents/PortInputField").GetComponent<UIKitInputField>();
        ipInputKeyboard = skin.transform.Find("Contents/IPInputField").GetComponent<InvokeKeyboard>();
        portInputKeyboard = skin.transform.Find("Contents/PortInputField").GetComponent<InvokeKeyboard>();
        connectBtn = skin.transform.Find("Contents/ConnectButton").GetComponent<Button>();
        hideBtn = skin.transform.Find("Contents/HideButton").GetComponent<Button>();
        connectingPanel = skin.transform.Find("Contents(Connecting)").gameObject;
        controlPanel = skin.transform.Find("Contents").gameObject;

        ipInput.text = "192.168.16.32";
        portInput.text = "8848";

        //设置提示面板不显示
        controlPanel.SetActive(true);
        connectingPanel.SetActive(false);

        //指定keyboard
        ipInputKeyboard.keyboard = keyboard;
        portInputKeyboard.keyboard = keyboard;

        //监听
        connectBtn.onClick.AddListener(OnConnectClick);
        hideBtn.onClick.AddListener(OnHideClick);

        //网络事件监听
        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.AddEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
    }

    public override void OnClose()
    {
        //移除网络事件监听
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
    }

    //连接成功回调
    void OnConnectSucc(string err)
    {
        Debug.Log("OnConnectSucc");
        //Todo 将isconnected这设置为1
        lock (toDoActionList)
        {
            toDoActionList.Add(new ToDoAction { actionFun = PanelManager.Open<LoginPanel>, param = null }) ;
            toDoActionList.Add(new ToDoAction { actionFun = this.Close, param = null });
        }
    }

    //连接失败回调
    void OnConnectFail(string err)
    {
        lock (toDoActionList)
        {
            toDoActionList.Add(new ToDoAction { actionFun = ChangePanel, param = null });
            toDoActionList.Add(new ToDoAction { actionFun = PanelManager.Open<TipPanel>, param = err });
        }
    }

    public void OnConnectClick()
    {

#if UNITY_EDITOR
        //test
        ipInput.text = "192.168.16.11";
        portInput.text = "8848";
#endif

        //ip或port为空
        if (ipInput.text == "" || portInput.text == "")
        {
            PanelManager.Open<TipPanel>("UserName or Password cannot be empty!");
            return;
        }

        if (!int.TryParse(portInput.text, out int port))
        {
            PanelManager.Open<TipPanel>("Port name unavailable!");
            return;
        }

        keyboard.SetActive(false);
        controlPanel.SetActive(false);
        connectingPanel.SetActive(true);

        NetManager.Connect(ipInput.text, port);
    }

    public void OnHideClick()
    {
        keyboard.SetActive(false);
    }

    //用来更改连接等待时的面板
    public void ChangePanel(params object[] para)
    {
        controlPanel.SetActive(true);
        connectingPanel.SetActive(false);
    }

    private void Update()
    {
        if(toDoActionList.Count > 0)
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
        
    }

}