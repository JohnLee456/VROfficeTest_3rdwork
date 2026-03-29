using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class JoinRoomPanel : BasePanel
{
    private Button joinBtn;


    //初始化 皮肤（美术资源）
    public override void OnInit()
    {
        skinPath = "JoinRoomPanel";
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] args)
    {
        //寻找组件
        joinBtn = skin.transform.Find("Contents/JoinButton").GetComponent<Button>();
        //注册事件
        joinBtn.onClick.AddListener(OnJoinClick);
    }

    public override void OnClose()
    {
        
    }

    void OnJoinClick()
    {
        skin.GetComponent<Launcher>().Connect();
    }
}
