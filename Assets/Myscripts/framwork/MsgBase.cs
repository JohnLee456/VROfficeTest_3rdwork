using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MsgBase
{
    //协议名
    //public string protoName = "";

    //编码
    /*public static byte[] Encode(MsgBase msgBase)
    {
        string s = JsonUtility.ToJson(msgBase);
        //Debug.Log("Encode " + s);
        return System.Text.Encoding.UTF8.GetBytes(s);
    }*/

    public static byte[] Encode(ProtoBuf.IExtensible msgBase)
    {
        using (var memory = new System.IO.MemoryStream())
        {
            ProtoBuf.Serializer.Serialize(memory, msgBase);
            return memory.ToArray();
        }
    }

    //解码
    /*public static MsgBase Decode(string protoName,byte[] bytes, int offset,int count)
    {
        string s = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
        MsgBase msgBase = (MsgBase)JsonUtility.FromJson(s, Type.GetType(protoName));
        return msgBase;
    }*/
    public static ProtoBuf.IExtensible Decode(string protoName, byte[] bytes, int offset, int count)
    {
        using (var memory = new System.IO.MemoryStream(bytes, offset, count))
        {
            System.Type t = System.Type.GetType(protoName);
            return (ProtoBuf.IExtensible)ProtoBuf.Serializer.NonGeneric.Deserialize(t, memory);
        }
    }

    //编码协议名（2字节长度+字符串）
    /*public static byte[] EncodeName(MsgBase msgBase)
    {

        //test
        Debug.Log(msgBase.protoName);

        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.protoName);
        Int16 len = (Int16)nameBytes.Length;
        byte[] bytes = new byte[len + 2];
        bytes[0] = (byte)(len % 256);
        bytes[1] = (byte)(len / 256);

        Array.Copy(nameBytes, 0, bytes, 2, len);

        return bytes;
    }*/
    public static byte[] EncodeName(ProtoBuf.IExtensible msgBase)
    {
        //test
        //Debug.Log(msgBase.ToString());

        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.ToString());
        Int16 len = (Int16)nameBytes.Length;
        byte[] bytes = new byte[len + 2];
        bytes[0] = (byte)(len % 256);
        bytes[1] = (byte)(len / 256);

        Array.Copy(nameBytes, 0, bytes, 2, len);

        return bytes;
    }



    //解码协议名（返回协议名和长度）
    public static string DecodeName(byte[] bytes, int offset, out int count)
    {
        count = 0;

        //判断字符串是否长于2字节
        if(offset + 2 > bytes.Length)
        {
            return "";
        }

        Int16 len = (Int16)((bytes[offset + 1] << 8) | bytes[offset]);
        if (offset+2 + len > bytes.Length)
        {
            return "";
        }

        count = 2 + len;
        string name = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);
        return name;
    }
}
