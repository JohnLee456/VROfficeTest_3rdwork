using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using proto.PlayerMsg;

public class PosListPanel : BasePanel
{
    //存放房间内已经生成了的用户的信息单元
    //当玩家数量变化时，将对应的信息展示单元删除，反之则初始化新的信息单元
    struct InfoItem
    {
        public GameObject infoObject;
        public Text infoText;
        public Button infoBtn;
    }

    private Dictionary<string, InfoItem> playerDic = new Dictionary<string, InfoItem>();
    
    //信息单元的预制件
    private GameObject infoPrefabs;

    //信息单元容器
    private Transform content;

    private Button CloseBtn;

    public override void OnInit()
    {
        skinPath = "PosListMenu";
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] para)
    {
        infoPrefabs = skin.transform.Find("Scroll View/Viewport/Content/Item-L1").gameObject;
        content = skin.transform.Find("Scroll View/Viewport/Content");
        CloseBtn = skin.transform.Find("Close").GetComponent<Button>();

        //取消显示该组件
        infoPrefabs.SetActive(false);

        CloseBtn.onClick.AddListener(OnCloseClick);

        //注册监听
        NetManager.AddMsgListener("MsgGetPlayerTranInfo", OnMsgGetPlayerTranInfo);
        NetManager.AddMsgListener("MsgPlayerLeave", OnMsgPlayerLeave);
    }

    public override void OnClose()
    {
        //移除监听
        NetManager.RemoveMsgListener("MsgGetPlayerTranInfo", OnMsgGetPlayerTranInfo);
        NetManager.RemoveMsgListener("MsgPlayerLeave", OnMsgPlayerLeave);
    }

    private InfoItem GenerateInfoItem()
    {
        //创建物体
        GameObject infoIns = Instantiate(infoPrefabs);
        infoIns.transform.SetParent(content);
        infoIns.SetActive(true);
        infoIns.transform.localScale = Vector3.one;
        infoIns.transform.localPosition = new Vector3(0,0,0);
        infoIns.transform.localRotation = new Quaternion(0,0,0,0);

        //获取组件
        Text infoInsText = infoIns.transform.Find("Title").GetComponent<Text>();
        Button infoInsBtn = infoIns.GetComponent<Button>();

        InfoItem infoItem = new InfoItem{ infoObject = infoIns, infoText = infoInsText, infoBtn = infoInsBtn };
 
        return infoItem;
    }

    private void RemoveInfoItem(string id)
    {
        InfoItem infoItem = playerDic[id];
        Destroy(infoItem.infoObject);
        playerDic.Remove(id);
    }

    public void OnMsgGetPlayerTranInfo(ProtoBuf.IExtensible msgBase)
    {
        MsgGetPlayerTranInfo msg = (MsgGetPlayerTranInfo)msgBase;
        if(msg.playerTrans == null)
        {
            return;
        }

        foreach(PlayerTranInfo player in msg.playerTrans)
        {
            string userid = player.userid;
            if (playerDic.ContainsKey(userid))
            {
                InfoItem infoItem = playerDic[userid];
                float x = player.x;
                float y = player.y;
                float z = player.z;
                infoItem.infoText.text = string.Format("Id:{0}\n X: {1:N2}, Y: {2:N2}, Z:{3:N2}\n", userid, x, y, z);
            }
            else
            {
                InfoItem infoItem = GenerateInfoItem();
                playerDic.Add(userid, infoItem);
                float x = player.x;
                float y = player.y;
                float z = player.z;
                infoItem.infoText.text = string.Format("Id:{0}\n X: {1:N2}, Y: {2:N2}, Z:{3:N2}\n", userid, x, y, z);
            }
        }
    }

    public void OnMsgPlayerLeave(ProtoBuf.IExtensible msgBase)
    {
        MsgPlayerLeave msg = (MsgPlayerLeave)msgBase;
        if (playerDic.ContainsKey(msg.id))
        {
            RemoveInfoItem(msg.id);
        }
    }


    public void OnCloseClick()
    {
        Close();
    }
}
