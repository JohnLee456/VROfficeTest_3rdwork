using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRUiKits.Utils;
using proto.LoginMsg;


public class LoginPanel : BasePanel
{
    private GameObject keyboard;
    private UIKitInputField idInput;
    private UIKitInputField pwInput;
    private InvokeKeyboard idInputKeyboard;
    private InvokeKeyboard pwInputKeyboard;
    private Button loginBtn;
    private Button regBtn;
    private Button hideBtn;

    //初始化
    public override void OnInit()
    {
        skinPath = "LoginPanel";
        layer = PanelManager.Layer.Panel;
        keyboard = GameObject.Find("Manager").GetComponent<LobbyMain>().keyboard;
    }

    public override void OnShow(params object[] args)
    {
        //寻找组件
        idInput = skin.transform.Find("Contents/UserNameInputField").GetComponent<UIKitInputField>();
        pwInput = skin.transform.Find("Contents/PasswordInputField").GetComponent<UIKitInputField>();
        idInputKeyboard = skin.transform.Find("Contents/UserNameInputField").GetComponent<InvokeKeyboard>();
        pwInputKeyboard = skin.transform.Find("Contents/PasswordInputField").GetComponent<InvokeKeyboard>();
        loginBtn = skin.transform.Find("Contents/LoginButton").GetComponent<Button>();
        regBtn = skin.transform.Find("Contents/RegisterButton").GetComponent<Button>();
        hideBtn = skin.transform.Find("Contents/HideButton").GetComponent<Button>();

        //指定keyboard
        idInputKeyboard.keyboard = keyboard;
        pwInputKeyboard.keyboard = keyboard;

        idInput.text = "Ellen";
        pwInput.text = "123";

        //监听
        loginBtn.onClick.AddListener(OnLoginClick);
        regBtn.onClick.AddListener(OnRegClick);
        hideBtn.onClick.AddListener(OnHideClick);

        //网络协议监听
        NetManager.AddMsgListener("MsgLogin", OnMsgLogin);
    }

    public override void OnClose()
    {
        //移除网络协议监听
        NetManager.RemoveMsgListener("MsgLogin", OnMsgLogin);
    }

    //当按下成功按钮
    public void OnLoginClick()
    {

#if UNITY_EDITOR
        idInput.text = "observer";
        pwInput.text = "123456";
#endif

        //用户名密码为空
        if (idInput.text == "" || pwInput.text == "")
        {
            PanelManager.Open<TipPanel>("UserName or Password cannot be empty!");
            return;
        }

        keyboard.SetActive(false);

        //发送
        MsgLogin msgLogin = new MsgLogin();
        msgLogin.id = idInput.text;
        msgLogin.pw = pwInput.text;
        NetManager.Send(msgLogin);
    }

    //当按下注册按钮
    public void OnRegClick()
    {
        keyboard.SetActive(false);
        PanelManager.Open<RegisterPanel>();
        this.Close();
    }

    public void OnHideClick()
    {
        keyboard.SetActive(false);
    }


    //登陆成功回调函数
    public void OnMsgLogin(ProtoBuf.IExtensible msgBase)
    {
        MsgLogin msg = (MsgLogin)msgBase;
        if (msg.result == 0)
        {
            PanelManager.Open<JoinRoomPanel>();
            Debug.Log("Login Succ");
            LobbyMain.playerId = idInput.text;
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("Login fail");
        }

    }

}