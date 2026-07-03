using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PanelManager
{
    //Layer
    public enum Layer
    {
        Panel,
        Tip,
    }
    //层级列表
    private static Dictionary<Layer, Transform> layers = new Dictionary<Layer, Transform>();
    //面板列表
    public static Dictionary<string, BasePanel> panels = new Dictionary<string, BasePanel>();
    //结构
    public static Transform root;
    public static Transform canvas;

    //初始化 获得场景中的游戏对象
    public static void Init()
    {
        root = GameObject.Find("Root").transform;
        canvas = root.Find("Canvas");
        Transform panel = canvas.Find("Panel");
        Transform tip = canvas.Find("Tip");
        layers.Add(Layer.Panel, panel);
        layers.Add(Layer.Tip, tip);
    }

    public static void DeInit()
    {
        root = null;
        canvas = null;
        panels.Clear();
        layers.Remove(Layer.Panel);
        layers.Remove(Layer.Tip);
    }

    //打开面板
    public static void Open<T>(params object[] para) where T : BasePanel
    {
        if (typeof(T) == typeof(TipPanel))
        {
            return;
        }

        //已经打开
        string name = typeof(T).ToString();
        if (panels.ContainsKey(name))
        {
            return;
        }

        //组件
        BasePanel panel = root.gameObject.AddComponent<T>();

        panel.OnInit();//这里是由basepanel调用的loginpanel的OnInit，体现了运行时多态
        panel.Init();

        //父容器
        Transform layer = layers[panel.layer];
        panel.skin.transform.SetParent(layer, false);//这个和之前的transform.parent = this.transform 有什么区别？答：this.transform是将自己设置为了父对象
        //列表
        panels.Add(name, panel);
        //onshow
        panel.OnShow(para);
    }



    //关闭面板
    public static void Close(string name)
    {
        //没有打开
        if (!panels.ContainsKey(name))
        {
            return;
        }

        BasePanel panel = panels[name];

        //OnClose
        panel.OnClose();

        //列表
        panels.Remove(name);
        //销毁
        GameObject.Destroy(panel.skin);//skin是游戏对象
        Component.Destroy(panel);//销毁绑在root上的BasePanel组件
    }
}
