using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyMain : MonoBehaviour
{
    //TODO
    //需要在主函数里监听被顶下线的信息
    //需要响应Close事件的方法

    //保持一个玩家ID
    public static string playerId  = "";
    //指向场景中的键盘
    public GameObject keyboard;

    //为了处理无法在主线程之外创建模板的问题 这里主要用于OnClose()
    private delegate void action(params object[] para);

    struct ToDoAction
    {
        public action actionFun;
        public object param;
    }

    List<ToDoAction> toDoActionList = new List<ToDoAction>();
    List<ToDoAction> currentAvtionList = new List<ToDoAction>();


    // Start is called before the first frame update
    void Start()
    {
        PanelManager.Init();
        PanelManager.Open<StartPanel>();
        //PanelManager.Open<TipPanel>("User Name is wrong!");

        //注册Close事件
        NetManager.AddEventListener(NetManager.NetEvent.Close, OnConnectClose);
    }

    // Update is called once per frame
    void Update()
    {
        if (toDoActionList.Count > 0)
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


        NetManager.Update();
    }

    private void OnDisable()
    {
        PanelManager.DeInit();

        //移除Close事件
        NetManager.RemoveEventListener(NetManager.NetEvent.Close, OnConnectClose);
    }


    public void OnConnectClose(string err)
    {
        string info = "Lost connection to data server!";
        lock (toDoActionList)
        {
            toDoActionList.Add(new ToDoAction { actionFun = PanelManager.Open<TipPanel>, param = info });
        }
    }
}
