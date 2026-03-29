using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRUiKits.Utils;
using Photon.Pun;
using Photon.Realtime;

public class GenTextPanel : BasePanel
{

    private Button CloseBtn;
    private Button GenTextBtn;

    private UIKitInputField GenText;


    public override void OnInit()
    {
        skinPath = "GenTextPanel";
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] args)
    {
        CloseBtn = skin.transform.Find("Contents/CloseButton").GetComponent<Button>();
        GenTextBtn = skin.transform.Find("Contents/GenerateButton").GetComponent<Button>();
        GenText = skin.transform.Find("Contents/FormKeyboard-L1/Form/GenText").GetComponent<UIKitInputField>();

        //注册按钮事件
        CloseBtn.onClick.AddListener(OnCloseClick);
        GenTextBtn.onClick.AddListener(OnGenTextClick);
    }


    public void OnCloseClick()
    {
        Close();
    }

    public void OnGenTextClick()
    {
        string text = GenText.text;
        if (text != "")
        {
            object[] myCustomInitData = new object[] { text };
            PhotonNetwork.Instantiate("Simple Helvetica Test", new Vector3(0f, 1.2f, 1.7f), Quaternion.identity, 0, myCustomInitData);
        }
        else
        {
            PanelManager.Open<TipPanel>("Text is Null");
        }

    }

    public override void OnClose()
    {
        
    }
}
