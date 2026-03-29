using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using System;
using proto.CollectMsg;

public class DataCollector : MonoBehaviour
{
    [SerializeField]
    private GameObject targetObject;
    [SerializeField]
    private GameObject mianCamera;
    [SerializeField]
    private GameObject handCon;

    [Tooltip("The time interval for collecting data")]
    [SerializeField]
    private float interval = 0.05f;
    private string collectType = "Simple";
    private List<XRNode> nodeListSimple = new List<XRNode> { XRNode.Head, XRNode.LeftHand, XRNode.RightHand };
    private List<XRNode> nodeListComplete = new List<XRNode> { XRNode.Head, XRNode.LeftHand, XRNode.RightHand, XRNode.LeftEye, XRNode.RightEye };

    //test
    public XRNode node;
    public XRNode node2;
    public XRNode node3;

    public bool tracked = false; // 数据取得可能否
    public Vector3 position; // 位置
    public Quaternion rotation; // 朝向
    public Vector3 eulerAngles;
    public Vector3 velocity; // 速度
    public Vector3 acceleration; // 加速度
    public Vector3 angularVelocity; // 角速度
    public Vector3 angularAcceleration; // 角加速度

    private string id = PhotonNetwork.LocalPlayer.NickName;

    private Dictionary<int, string> SensorDataType = new Dictionary<int, string>();
    private Dictionary<string, int> NodeTypeDic = new Dictionary<string, int>();

    void Awake()
    {
        //初始化传感器字典
        SensorDataType.Add(0, "position");
        SensorDataType.Add(1, "rotation");
        SensorDataType.Add(2, "velocity");
        SensorDataType.Add(3, "acceleration");
        SensorDataType.Add(4, "angularvelocity");
        SensorDataType.Add(5, "angularacceleration");

        //初始化节点字典
        NodeTypeDic.Add("LeftEye", 0);
        NodeTypeDic.Add("RightEye", 1);
        NodeTypeDic.Add("CenterEye", 2);
        NodeTypeDic.Add("Head", 3);
        NodeTypeDic.Add("LeftHand", 4);
        NodeTypeDic.Add("RightHand", 5);
        NodeTypeDic.Add("Unity", 6);
    }

    // Start is called before the first frame update
    void Start()
    {
        //targetObject = GameObject.Find("/VRRigDeviceBased");
        //该函数应在update中调用，做到断线重连发生时，重新开启线程
        //其实在netmanager中已经在send之前检测了socket是否连接，因此推荐检测是否断连只依靠心跳机制，当心跳超时则主动关闭socket并在close事件的回调中实现短线再连的功能
        if(PhotonNetwork.LocalPlayer.NickName == "observer")
        {
            return;
        }
        StartCoroutine(WaitAndSendData());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdataUnityData(GameObject obj, GameObject camera)
    {
        MsgTranRoPo msg = new MsgTranRoPo();
        Vector3 playerPostion = obj.transform.position;
        Vector3 playerRotation = obj.transform.rotation.eulerAngles;
        //Vector3 cameraRotation = camera.transform.rotation.eulerAngles;
        long timestamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;

        //Position
        msg.unityRoPoList.Add(CreatVector3Info(6, 0, id,timestamp, playerPostion.x, playerPostion.y, playerPostion.z));
        msg.unityRoPoList.Add(CreatVector3Info(6, 1, id, timestamp, playerRotation.x, playerRotation.y, playerRotation.z));

        //msg.unityRoPoList[0].sensorDataType = 0;
        ////Unity
        //msg.unityRoPoList[0].nodeType = 6;
        //msg.unityRoPoList[0].id = id;
        //msg.unityRoPoList[0].timestamp = timestamp;
        //msg.unityRoPoList[0].x = playerPostion.x;
        //msg.unityRoPoList[0].y = playerPostion.y;
        //msg.unityRoPoList[0].z = playerPostion.z;


        ////Position
        //msg.unityRoPoList[1] = new Vector3Info();
        //msg.unityRoPoList[1].sensorDataType = 1;
        ////Unity
        //msg.unityRoPoList[1].nodeType = 6;
        //msg.unityRoPoList[1].id = id;
        //msg.unityRoPoList[1].timestamp = timestamp;
        //msg.unityRoPoList[1].x = playerRotation.x;
        //msg.unityRoPoList[1].y = playerRotation.y;
        //msg.unityRoPoList[1].z = playerRotation.z;

        NetManager.Send(msg);
    }

    IEnumerator WaitAndSendData()
    {
        //内部应检查是否与服务器保持连接
        //断连时结束协程 yield break;（弹出提示窗，提供重连机能已经由包含netmanager.update（）的脚本实现）
        while (true)
        {
            UpdataUnityData(targetObject, mianCamera);
            UpdataVector(collectType);
            yield return new WaitForSeconds(interval);
        }
    }

    public void UpdataVector(string collectType)
    {
        MsgUploadVector msg = new MsgUploadVector();
        long timestamp = 0;

        // 获得全部的跟踪节点数据

        // 准备节点用的列表
        List<XRNodeState> states = new List<XRNodeState>();

        // 获取全部的node数据(所有的节点种类)
        InputTracking.GetNodeStates(states);

        //Debug.Log("Node states: " + states.Count);

        if(collectType == "Simple")
        {
            // 确认获得的节点数据
            foreach (XRNodeState s in states)
            {
                if (nodeListSimple.Contains(s.nodeType))
                {
                    // 尝试取得数据
                    tracked = s.tracked;
                    int nodeKey = NodeTypeDic[s.nodeType.ToString()];
                    //计算时间戳
                    timestamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
                    //Debug.Log("Timestamp: " + timestamp);


                    s.TryGetPosition(out position);
                    msg.vector3InfoList.Add(CreatVector3Info(nodeKey, 0, id, timestamp, position.x, position.y, position.z));

                    /*if(s.nodeType == XRNode.Head)
                    {
                        Vector3 temp = mianCamera.transform.InverseTransformPoint(targetObject.transform.position);
                        Vector3 temp_local = targetObject.transform.InverseTransformPoint(targetObject.transform.position);
                        Debug.Log("temp_local: " + temp_local);
                        Vector3 temp_local_2 = temp_local + position;
                        Vector3 temp_1 = targetObject.transform.TransformPoint(position);

                        Vector3 temp_add = targetObject.transform.position + position;

                        Vector3 ob_rot = targetObject.transform.rotation.eulerAngles;
                        //float tempy = position.y - targetObject.transform.position.y ;
                        //float tempz = position.z - targetObject.transform.position.z;
                        Debug.Log("target.x: " + targetObject.transform.position.x + "   target.y: " + targetObject.transform.position.y + "   target.z: " + targetObject.transform.position.z + "  camera.x: " + mianCamera.transform.position.x + "  camera.y: " + mianCamera.transform.position.y + "  camera.z: " + mianCamera.transform.position.z + "  XRNode.x: " + position.x + "  XRNode.y: " + position.y + "  XRNode.z: " + position.z);
                        Debug.Log("x: " + temp_1.x + "   y: " + temp_1.y + "   z: " + temp_1.z + "  camera.x: " + mianCamera.transform.position.x + "  camera.y: " + mianCamera.transform.position.y + "  camera.z: " + mianCamera.transform.position.z);
                        Debug.Log("x_add: " +  temp_add.x + "   y_add: " + temp_add.y + "   z_add: " + temp_add.z + "  camera.x: " + mianCamera.transform.position.x + "  camera.y: " + mianCamera.transform.position.y + "  camera.z: " + mianCamera.transform.position.z);
                        Debug.Log("rot " + ob_rot);
                        //Debug.Log("camera.x: " + mianCamera.transform.position.x + "  camera.y: " + mianCamera.transform.position.y + "  camera.z: " + mianCamera.transform.position.z);
                        //Debug.Log("XRNode.x: " + position.x + "  XRNode.y: " + position.y + "  XRNode.z: " + position.z);
                    }*/
                    //test
                    //Debug.Log(msg.vector3InfoList[0]);

                    /*if (s.nodeType == XRNode.RightHand)
                    {

                        Vector3 temp_1 = transform.InverseTransformPoint(handCon.transform.position);
                        Debug.Log("target.x: " + targetObject.transform.position.x + "   target.y: " + targetObject.transform.position.y + "   target.z: " + targetObject.transform.position.z + "  rot.y: " + targetObject.transform.rotation.eulerAngles.y);
                        Debug.Log("target.x: " + handCon.transform.position.x + "   target.y: " + handCon.transform.position.y + "   target.z: " + handCon.transform.position.z  + "  XRNode.x: " + position.x + "  XRNode.y: " + position.y + "  XRNode.z: " + position.z);
                        Debug.Log("NodeRoy.x: " + eulerAngles.x + "  NodeRot.y: " + eulerAngles.y + "  NodeRot.z: " + eulerAngles.z);
                        Debug.Log("targetRoy.x: " + targetObject.transform.rotation.eulerAngles.x + "  targetRot.y: " + targetObject.transform.rotation.eulerAngles.y + "  targetRot.z: " + targetObject.transform.rotation.eulerAngles.z);
                        Debug.Log(mianCamera.transform.rotation.eulerAngles.x + " == " + (eulerAngles.x + targetObject.transform.rotation.eulerAngles.x) % 360 + "  " +  mianCamera.transform.rotation.eulerAngles.y + " == " + (eulerAngles.y + targetObject.transform.rotation.eulerAngles.y) % 360 + "    " + mianCamera.transform.rotation.eulerAngles.z + " == " + (eulerAngles.z + targetObject.transform.rotation.eulerAngles.z) % 360);
                        
                    }*/

                    s.TryGetVelocity(out velocity);
                    msg.vector3InfoList.Add(CreatVector3Info(nodeKey, 2, id, timestamp, velocity.x, velocity.y, velocity.z));

                    s.TryGetAcceleration(out acceleration);
                    msg.vector3InfoList.Add(CreatVector3Info(nodeKey, 3, id, timestamp, acceleration.x, acceleration.y, acceleration.z));

                    s.TryGetAngularVelocity(out angularVelocity);
                    msg.vector3InfoList.Add(CreatVector3Info(nodeKey, 4, id, timestamp, angularVelocity.x, angularVelocity.y, angularVelocity.z));

                    s.TryGetAngularAcceleration(out angularAcceleration);
                    msg.vector3InfoList.Add(CreatVector3Info(nodeKey, 5, id, timestamp, angularAcceleration.x, angularAcceleration.y, angularAcceleration.z));

                    s.TryGetRotation(out rotation);
                    eulerAngles = rotation.eulerAngles;
                    msg.vector3InfoList.Add(CreatVector3Info(nodeKey, 1, id, timestamp, eulerAngles.x, eulerAngles.y, eulerAngles.z));

                    /*if (s.nodeType == XRNode.RightHand)
                    {
                        Debug.Log("Rotation target.x: " + targetObject.transform.rotation.eulerAngles.x + "   target.y: " + targetObject.transform.rotation.eulerAngles.y + "   target.z: " + targetObject.transform.rotation.eulerAngles.z);
                        Debug.Log("Rotation hand.x: " + mianCamera.transform.rotation.eulerAngles.x + "  hand.y: " + mianCamera.transform.rotation.eulerAngles.y + "  hand.z: " + mianCamera.transform.rotation.eulerAngles.z);
                        Debug.Log("Rotation XRNode.x: " + rotation.eulerAngles.x + "  XRNode.y: " + rotation.eulerAngles.y + "  XRNode.z: " + rotation.eulerAngles.z);
                    }*/

                    //Debug.Log("Position ---> X: " + position.x + "  Y: " + position.y + "  Z: " + position.z);
                    //Debug.Log("Velocity ---> X: " + velocity.x + "  Y: " + velocity.y + "  Z: " + velocity.z);
                    //Debug.Log("Acceleration ---> X: " + acceleration.x + "  Y: " + acceleration.y + "  Z: " + acceleration.z);
                    //Debug.Log("AngularVelocity ---> X: " + angularVelocity.x + "  Y: " + angularVelocity.y + "  Z: " + angularVelocity.z);
                    //Debug.Log("AngularAcceleration ---> X: " + angularAcceleration.x + "  Y: " + angularAcceleration.y + "  Z: " + angularAcceleration.z);
                    //Debug.Log(s.nodeType);
                }
            }
            NetManager.Send(msg);
        }
        else if(collectType == "Complete")
        {
            // 确认获得的节点数据
            foreach (XRNodeState s in states)
            {
                if (nodeListComplete.Contains(s.nodeType))
                {
                    // 尝试取得数据
                    tracked = s.tracked;
                    int nodeKey = NodeTypeDic[s.nodeType.ToString()];
                    //计算时间戳
                    timestamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
                    //Debug.Log("Timestamp: " + timestamp);


                    s.TryGetPosition(out position);
                    msg.vector3InfoList.Add(CreatVector3Info(nodeKey, 0, id, timestamp, position.x, position.y, position.z));

                    //test
                    //Debug.Log(msg.vector3InfoList[0].nodeType);

                    s.TryGetRotation(out rotation);
                    eulerAngles = rotation.eulerAngles;
                    msg.vector3InfoList.Add(CreatVector3Info(nodeKey, 1, id, timestamp, eulerAngles.x, eulerAngles.y, eulerAngles.z));

                    s.TryGetVelocity(out velocity);
                    msg.vector3InfoList.Add(CreatVector3Info(nodeKey, 2, id, timestamp, velocity.x, velocity.y, velocity.z));

                    s.TryGetAcceleration(out acceleration);
                    msg.vector3InfoList.Add(CreatVector3Info(nodeKey, 3, id, timestamp, acceleration.x, acceleration.y, acceleration.z));

                    s.TryGetAngularVelocity(out angularVelocity);
                    msg.vector3InfoList.Add(CreatVector3Info(nodeKey, 4, id, timestamp, angularVelocity.x, angularVelocity.y, angularVelocity.z));

                    s.TryGetAngularAcceleration(out angularAcceleration);
                    msg.vector3InfoList.Add(CreatVector3Info(nodeKey, 5, id, timestamp, angularAcceleration.x, angularAcceleration.y, angularAcceleration.z));

                    s.TryGetRotation(out rotation);
                    eulerAngles = rotation.eulerAngles;
                    msg.vector3InfoList.Add(CreatVector3Info(nodeKey, 1, id, timestamp, eulerAngles.x, eulerAngles.y, eulerAngles.z));

                    //Debug.Log("Position ---> X: " + position.x + "  Y: " + position.y + "  Z: " + position.z);
                    //Debug.Log("Velocity ---> X: " + velocity.x + "  Y: " + velocity.y + "  Z: " + velocity.z);
                    //Debug.Log("Acceleration ---> X: " + acceleration.x + "  Y: " + acceleration.y + "  Z: " + acceleration.z);
                    //Debug.Log("AngularVelocity ---> X: " + angularVelocity.x + "  Y: " + angularVelocity.y + "  Z: " + angularVelocity.z);
                    //Debug.Log("AngularAcceleration ---> X: " + angularAcceleration.x + "  Y: " + angularAcceleration.y + "  Z: " + angularAcceleration.z);
                    NetManager.Send(msg);
                }
            }
            NetManager.Send(msg);
        }
  
    }

    public Vector3Info CreatVector3Info(int nodeType, int sensorDataType, string id, long timestamp, float x, float y, float z)
    {
        Vector3Info info = new Vector3Info();
        info.nodeType = nodeType;
        info.sensorDataType = sensorDataType;
        info.id = id;
        info.timestamp = timestamp;
        info.x = x;
        info.y = y;
        info.z = z;
        return info;
    }

    public float GetInterval()
    {
        //test
        Debug.Log("Current Interval " + interval);

        return interval;
    }

    public string GetCollectType()
    {
        //test
        Debug.Log("Current CollectType " + collectType);

        return collectType;
    }

    public void ChangeInterval(float newInterval)
    {
        interval = newInterval;
        
        //test
        Debug.Log("In DataCollector Interval " + interval);
    }
    public void ChangeCollectType(string Type)
    {
        collectType = Type;

        //test
        Debug.Log("In DataCollector CollectType " + collectType);
    }
}
