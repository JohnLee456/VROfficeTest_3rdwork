using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePanel : MonoBehaviour
{
    //皮肤路径
    public string skinPath;
    //皮肤
    public GameObject skin;
    //层级
    public PanelManager.Layer layer = PanelManager.Layer.Panel;

    //初始化
    public void Init()
    {
        //皮肤
        GameObject skinPrefab = ResManager.LoadPrefab(skinPath);
        skin = (GameObject)Instantiate(skinPrefab);
        OfficeXrUiSupport.ConfigureCanvasesIn(skin, false);
    }
    //关闭
    public void Close(params object[] para)
    {
        string name = this.GetType().ToString();
        PanelManager.Close(name);
    }
    //
    public virtual void OnInit()
    {

    }
    //
    public virtual void OnShow(params object[] para)//params关键字是用来传递不定长度的关键字的，可以向里面传递1个参数，也可以是n个参数，使用第几个参数用para[n（n是数组index）]调用
    {

    }
    //
    public virtual void OnClose()
    {

    }
}
