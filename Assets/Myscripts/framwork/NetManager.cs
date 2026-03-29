using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Linq;
using proto.SysMsg;

public static class NetManager
{
    static Socket socket;
    static ByteArray readBuff;
    static Queue<ByteArray> writeQueue;

    //事件
    public enum NetEvent
    {
        ConnectSucc = 1,
        ConnectFail = 2,
        Close = 3,
        DisConnected = 4
    }

    //事件委托类型
    public delegate void EventListener(String err);
    //事件监听列表
    private static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();

    //消息委托类型
    public delegate void MsgListener(ProtoBuf.IExtensible msgBase);
    //消息监听列表
    private static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();

    //消息列表
    static List<ProtoBuf.IExtensible> msgList = new List<ProtoBuf.IExtensible>();
    //消息列表长度
    static int msgCount = 0;
    //每一次Update处理的消息量
    readonly static int MAX_MESSAGE_FIRE = 10;

    //是否启用心跳
    public static bool isUsePing = true;
    //心跳间隔时间
    public static int pingInterval = 30;
    //上一次发送PING的时间
    static float lastPingTime = 0;
    //上一次收到PONG的时间
    static float lastPongTime = 0;

    //是否正在连接
    static bool isConnecting = false;
    //是否正在关闭
    static bool isClosing = false;

    //是否启用数据收集
    //static bool isEnableCollection = false;
    
    //是否与服务器连接着(TODO)
    //static bool isConnected = false;

    //添加事件监听
    public static void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] += listener;
        }
        else
        {
            eventListeners[netEvent] = listener;
        }
    }

    //删除事件监听
    public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] -= listener;
            if (eventListeners[netEvent] == null)
            {
                eventListeners.Remove(netEvent);
            }
        }
    }

    //分发事件
    public static void FireEvent(NetEvent netEvent, String err)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent](err);
        }
    }

    //添加消息监听
    public static void AddMsgListener(string msgName, MsgListener listener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] += listener;
        }
        else
        {
            msgListeners[msgName] = listener;
        }
    }

    //删除消息监听
    public static void RemoveMsgListener(string msgName, MsgListener listener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] -= listener;
            if (msgListeners[msgName] == null)
            {
                msgListeners.Remove(msgName);
            }
        }
    }

    //分发消息
    public static void FireMsg(string protoName, ProtoBuf.IExtensible msgBase)//需要改换基类
    {
        string[] parts = protoName.Split('.');
        string msgName = parts[parts.Length - 1];
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName](msgBase);
        }
    }

    public static void Connect(string ip,int port)
    {
        if(socket!=null && socket.Connected)
        {
            Debug.Log("Connect fail, already connected!");
            return;
        }
        if (isConnecting)
        {
            Debug.Log("Connect fail, isConnecting!");
            return;
        }
        //初始化成员
        InitState();
        socket.NoDelay = true;
        isConnecting = true;
        socket.BeginConnect(ip, port, ConnectCallback, socket);
    }

    //断开链接
    public static void Close()
    {
        if(socket==null || !socket.Connected)
        {
            return;
        }
        if (isConnecting)
        {
            return;
        }
        if (writeQueue.Count > 0)
        {
            isClosing = true;
        }
        else
        {
            socket.Close();
            FireEvent(NetEvent.Close, "");
        }
    }

    //初始化状态
    private static void InitState()
    {
        //Socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        readBuff = new ByteArray();
        writeQueue = new Queue<ByteArray>();
        isConnecting = false;
        isClosing = false;
        //初始化消息列表
        msgList = new List<ProtoBuf.IExtensible>();
        msgCount = 0;
        lastPingTime = Time.time;
        lastPongTime = Time.time;
        //监听PONG协议
        if (!msgListeners.ContainsKey("MsgPong"))
        {
            AddMsgListener("MsgPong", OnMsgPong);
        }
    }

    //Connect回调
    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Secc!");
            FireEvent(NetEvent.ConnectSucc, "");
            isConnecting = false;

            //开始接收
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
        }
        catch(SocketException ex)
        {
            Debug.Log("Socket Connect Fail! " + ex.ToString());
            FireEvent(NetEvent.ConnectFail, ex.ToString());
            isConnecting = false;
        }
    }

    //发送数据
    public static void Send(ProtoBuf.IExtensible msg)
    {
        if (socket == null || !socket.Connected)
        {
            //分发断连事件(可能没有必要，取决于当服务器离线时socket是否还被认为是连接着的。若客户端在服务器突然离线时socket还认为是连接着的话，那就只能通过长时间无回应来判断是否断连)
            //需要测试
            //FireEvent(NetEvent.DisConnected, "");

            return;
        }
        if (isConnecting)
        {
            return;
        }
        if (isClosing)
        {
            return;
        }

        //数据编码
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);

        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];

        //组装长度
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        //组装名字
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);

        //写入队列
        ByteArray ba = new ByteArray(sendBytes);

        //debug
        //Debug.Log("[Debug] Write into sendArray: " + msg.ToString());

        //writeQueue的长度
        int count = 0;
        lock (writeQueue)
        {
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }
        //send
        if (count == 1)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        }
    }

    public static void SendCallback(IAsyncResult ar)
    {

        //获取state,EndSend的处理
        Socket socket = (Socket)ar.AsyncState;
        //状态判断
        if (socket == null || !socket.Connected)
        {
            return;
        }
        int count = socket.EndSend(ar);
        //获取写入队列的第一条数据
        ByteArray ba;
        lock (writeQueue)
        {
            ba = writeQueue.First();
        }
        ba.readIdx += count;
        if (ba.length == 0)
        {
            lock (writeQueue)
            {
                writeQueue.Dequeue();
                if (writeQueue.Count != 0)
                {
                    ba = writeQueue.First();
                }
                else
                {
                    ba = null;
                }

            }
        }

        if (ba != null)
        {
            socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, socket);
        }
        else if (isClosing)
        {
            socket.Close();
        }
    }

    public static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            if (count == 0)
            {
                Close();
                return;
            }
            readBuff.writeIdx += count;

            //处理二进制消息
            OnReceiveData();

            //若空间不够则扩大缓冲区空间
            if (readBuff.remain < 8)
            {
                readBuff.MoveBytes();
                readBuff.ReSize(readBuff.length * 2);
            }

            //继续接受消息
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
        }
        catch(SocketException ex)
        {
            Debug.Log("Socket Receive fail " + ex.ToString());

            if(ex.GetType() == typeof(SocketException))
            {
                //当服务器意外关闭时，断开连接（自己加的，有待测试）
                //这时socket.Connected = false, socket != null
                FireEvent(NetEvent.Close, "");
                Debug.Log("Socket出现异常，执行了关闭");
            }
        }
    }

    public static void OnReceiveData()
    {
        if(readBuff.length <= 2)
        {
            return;
        }

        int readIdx = readBuff.readIdx;
        byte[] bytes = readBuff.bytes;
        Int16 bodyLenth = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
        if (readBuff.length < bodyLenth)
            return;
        readBuff.readIdx += 2;

        /*
        //Debug 测试int赋值是赋指针还是新开内存 测试结果：是新开内存
        Debug.Log("readIdx: "+readIdx);
        Debug.Log("readBuff.readIdx: "+readBuff.readIdx);

        //Debug 测试byte数组赋值是赋指针还是新开内存 测试结果：是赋指针，改变值的话会一起变
        Debug.Log("bytes: " + BitConverter.ToString(bytes,0,bytes.Length));
        Debug.Log("readBuff.Bytes: " + BitConverter.ToString(readBuff.bytes,0,readBuff.bytes.Length));*/

        int nameCount = 0;
        string protoName = MsgBase.DecodeName(readBuff.bytes, readBuff.readIdx, out nameCount);
        if (protoName == "")
        {
            Debug.Log("OnReceiveData MsgBase.DocodeName fail");
            return;
        }
        readBuff.readIdx += nameCount;

        //解析协议体
        int bodyCount = bodyLenth - nameCount;//namecount包含2字节长度
        ProtoBuf.IExtensible msgBase = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
        readBuff.readIdx += bodyCount;
        readBuff.CheckAndMoveBytes();

        //添加到消息队列
        lock (msgList)
        {
            msgList.Add(msgBase);
        }

        msgCount++;

        //继续读取消息
        if (readBuff.length > 2)
        {
            OnReceiveData();
        }

    }

    //更新消息
    public static void MsgUpdate()
    {
        if (msgCount == 0)
        {
            return;
        }
        for (int i = 0; i < MAX_MESSAGE_FIRE; i++)
        {
            ProtoBuf.IExtensible msgBase = null;
            lock (msgList)
            {
                if (msgList.Count > 0)
                {
                    msgBase = msgList[0];
                    msgList.RemoveAt(0);
                    msgCount--;
                }
            }
            if(msgBase != null)
            {
                FireMsg(msgBase.ToString(), msgBase);
            }
            else
            {
                break;
            }
        }
    }

    private static void PingUpdate()
    {
        if (!isUsePing)
        {
            return;
        }

        //发送PING
        if (Time.time - lastPingTime > pingInterval)
        {
            MsgPing msgPing = new MsgPing();
            Send(msgPing);
            lastPingTime = Time.time;
        }

        //检测PONG时间
        if (Time.time - lastPongTime > pingInterval * 4)
        {
            Close();
        }
    }

    private static void OnMsgPong(ProtoBuf.IExtensible msgBase)
    {
        lastPongTime = Time.time;
    }

    public static void Update()
    {
        MsgUpdate();
        PingUpdate();
    }

}
