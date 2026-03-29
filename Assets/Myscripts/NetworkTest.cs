using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //应在更新前确保与服务器连结，并在断连时提供提示与重连功能。
        NetManager.Update();
    }
}
