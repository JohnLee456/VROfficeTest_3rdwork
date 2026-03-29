using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartPanel : BasePanel
{
    private Button startBtn;
    private Toggle allowCheck;


    public override void OnInit()
    {
        skinPath = "StartPanel";
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] args)
    {
        //寻找组件
        startBtn = skin.transform.Find("Contents/StartButton").GetComponent<Button>();
        allowCheck = skin.transform.Find("Contents/Toggle").GetComponent<Toggle>();
        
        //初始化checkbox
        allowCheck.isOn = false;

        //注册事件
        startBtn.onClick.AddListener(OnStartClick);
    }

    public void OnStartClick()
    {
        if(allowCheck.isOn == true)
        {
            //TODO 是否需要将一个记录是否启用收集数据模式的全局变量的更改？ 有待商讨...
            PanelManager.Open<ConnectPanel>();
            Close();
        }
        else
        {
            PanelManager.Open<JoinRoomPanel>();
            Close();
        }
    }

    public override void OnClose()
    {

    }
}
