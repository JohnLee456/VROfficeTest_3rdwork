using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserMenuPanel : BasePanel
{
    private Button button;

    private GameObject controller;

    //初始化 皮肤（美术资源）
    public override void OnInit()
    {
        skinPath = "UserMenu";
        layer = PanelManager.Layer.Panel;
    }
    public override void OnShow(params object[] args)
    {
        //初始化后将菜单隐藏
        skin.gameObject.SetActive(false);

        //获取组件
        controller = GameObject.Find("VRRigDeviceBased");

        //注册按钮控制事件
        OnButtonPress onButtonPress = controller.GetComponent<OnButtonPress>();
        onButtonPress.OnPress.AddListener(OnOpenMenuClick);
    }

    public override void OnClose()
    {

    }

    public void OnOpenMenuClick()
    {
        if(skin != null && !skin.activeInHierarchy)
        {
            skin.SetActive(true);
        }
    }
}
