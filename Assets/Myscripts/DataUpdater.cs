using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using proto.PlayerMsg;

public class DataUpdater : MonoBehaviour
{
    //[SerializeField]
    //private TextMeshProUGUI textMeshPro;

    private string data = "";

    public float interval = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        //NetManager.AddMsgListener("MsgGetPlayerTranInfo", OnMsgGetPlayerTranInfo);
        StartCoroutine(WaitAndQueryTran());
    }

    // Update is called once per frame
    //void LateUpdate()
    //{
    //    if(textMeshPro != null)
    //    {
    //        textMeshPro.text = data;
    //    }
    //}

    IEnumerator WaitAndQueryTran()
    {
        //内部应检查是否与服务器保持连接
        //断连时结束协程 yield break;（弹出提示窗，提供重连机能已经由包含netmanager.update（）的脚本实现）
        while (true)
        {
            QueryTran();
            yield return new WaitForSeconds(interval);
        }
    }

    public void QueryTran()
    {
        MsgGetPlayerTranInfo msg = new MsgGetPlayerTranInfo();
        NetManager.Send(msg);
    }

}
