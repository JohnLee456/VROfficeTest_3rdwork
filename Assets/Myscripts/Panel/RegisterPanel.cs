using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRUiKits.Utils;
using proto.LoginMsg;

public class RegisterPanel : BasePanel
{
    //指定键盘
    private GameObject keyboard;
    //账号输入框
    private UIKitInputField idInput;
    //密码输入框
    private UIKitInputField pwInput;
    //重复输入框
    private UIKitInputField repInput;


    private InvokeKeyboard idInputKeyboard;
    private InvokeKeyboard pwInputKeyboard;
    private InvokeKeyboard repInputKeyboard;

    //隐藏按钮
    private Button hideBtn;
    //注册按钮
    private Button regBtn;
    //关闭按钮
    private Button closeBtn;

    //初始化 皮肤（美术资源）
    public override void OnInit()
    {
        skinPath = "RegisterPanel";
        layer = PanelManager.Layer.Panel;
        keyboard = GameObject.Find("Manager").GetComponent<LobbyMain>().keyboard;
    }
    //显示
    public override void OnShow(params object[] args)
    {
        //寻找组件
        idInput = skin.transform.Find("Contents/UserNameInputField").GetComponent<UIKitInputField>();
        pwInput = skin.transform.Find("Contents/PasswordInputField").GetComponent<UIKitInputField>();
        repInput = skin.transform.Find("Contents/RePasswordInputField").GetComponent<UIKitInputField>();
        idInputKeyboard = skin.transform.Find("Contents/UserNameInputField").GetComponent<InvokeKeyboard>();
        pwInputKeyboard = skin.transform.Find("Contents/PasswordInputField").GetComponent<InvokeKeyboard>();
        repInputKeyboard = skin.transform.Find("Contents/RePasswordInputField").GetComponent<InvokeKeyboard>();
        regBtn = skin.transform.Find("Contents/RegisterButton").GetComponent<Button>();
        closeBtn = skin.transform.Find("Contents/CloseButton").GetComponent<Button>();
        hideBtn = skin.transform.Find("Contents/HideButton").GetComponent<Button>();

        //指定keyboard
        idInputKeyboard.keyboard = keyboard;
        pwInputKeyboard.keyboard = keyboard;
        repInputKeyboard.keyboard = keyboard;

        //按钮的监听
        regBtn.onClick.AddListener(OnRegClick);
        closeBtn.onClick.AddListener(OnCloseClick);
        hideBtn.onClick.AddListener(OnHideClick);
        
        //网络协议监听
        NetManager.AddMsgListener("MsgRegister", OnMsgRegister);//书上似乎打错了
    }
    //关闭
    public override void OnClose()
    {
        NetManager.RemoveMsgListener("MsgRegister", OnMsgRegister);
    }
    //当按下注册按钮
    public void OnRegClick()
    {
        //用户名密码为空
        if (idInput.text == "" || pwInput.text == "")
        {
            PanelManager.Open<TipPanel>("UserName or Password cannot be empty!");
            return;
        }
        //两次密码不同
        if (repInput.text != pwInput.text)
        {
            PanelManager.Open<TipPanel>("Two different passwords entered!");
            return;
        }
        //发送
        MsgRegister msgReg = new MsgRegister();
        msgReg.id = idInput.text;
        msgReg.pw = pwInput.text;
        NetManager.Send(msgReg);
    }
    //当按下关闭按钮
    public void OnCloseClick()
    {
        PanelManager.Open<LoginPanel>();
        this.Close();
    }

    public void OnHideClick()
    {
        keyboard.SetActive(false);
    }

    //收到注册协议
    public void OnMsgRegister(ProtoBuf.IExtensible msgBase)
    {
        MsgRegister msg = (MsgRegister)msgBase;
        if (msg.result == 0)
        {
            Debug.Log("Register Succ");
            //提示
            PanelManager.Open<TipPanel>("Register Success!");
        }
        else
        {
            PanelManager.Open<TipPanel>("Register Fail!");
        }
    }
}
