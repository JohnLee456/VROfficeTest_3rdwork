using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemsPanel : BasePanel
{
    //提示文本
    private Text text;
    //Ok按钮
    private Button closeBtn;
    //初始化
    public override void OnInit()
    {
        skinPath = "ItemsPanel";
        layer = PanelManager.Layer.Panel;
    }
    //显示
    public override void OnShow(params object[] args)
    {
        //寻找组件
        text = skin.transform.Find("Contents/Info").GetComponent<Text>();
        closeBtn = skin.transform.Find("Contents/CloseButton").GetComponent<Button>();
        //按钮点击事件监听
        closeBtn.onClick.AddListener(OnCloseClick);
    }
    //关闭
    public override void OnClose()
    {

    }
    //当按下确定按钮
    public void OnCloseClick()
    {
        this.Close();
    }

}
