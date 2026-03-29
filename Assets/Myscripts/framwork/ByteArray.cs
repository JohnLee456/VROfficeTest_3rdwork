using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ByteArray
{
    const int DEFAULT_SIZE = 128*1024;
    //初始长度
    int initSize = 0;
    public byte[] bytes;
    public int readIdx = 0;
    public int writeIdx = 0;
    //容量
    private int capacity = 0;
    //剩余长度
    public int remain { get { return capacity - writeIdx; } }
    public int length { get { return writeIdx - readIdx; } }

    public ByteArray(byte[] defaultBytes)
    {
        bytes = defaultBytes;
        capacity = defaultBytes.Length;
        initSize = defaultBytes.Length;
        readIdx = 0;
        writeIdx = defaultBytes.Length;
    }

    public ByteArray(int size = DEFAULT_SIZE)
    {
        bytes = new byte[size];
        capacity = size;
        initSize = size;
        readIdx = 0;
        writeIdx = 0;
    }

    public void ReSize(int size)
    {
        if (size < length) return;
        if (size < initSize) return;
        int n = 1;
        while (n < size) n *= 2;
        capacity = n;
        byte[] newBytes = new byte[capacity];
        Array.Copy(bytes, readIdx, newBytes, 0, writeIdx - readIdx);
        bytes = newBytes;
        writeIdx = length;
        readIdx = 0;

    }

    public void CheckAndMoveBytes()
    {
        if (length < 8)
        {
            MoveBytes();
        }
    }

    public void MoveBytes()
    {
        Array.Copy(bytes, readIdx, bytes, 0, length);
        writeIdx = length;
        readIdx = 0;
    }

    //弃用write
    public int Write(byte[] bs, int offset, int count)
    {
        if (remain < count)
        {
            ReSize(length + count);//存在疑问？答：确实会出现超出列表的bug
        }
        Array.Copy(bs, offset, bytes, writeIdx, count);
        writeIdx += count;
        return count;
    }

    public int Read(byte[] bs, int offset, int count)
    {
        count = Math.Min(count, length);
        Array.Copy(bytes, readIdx, bs, offset, count); //0是不是要改成readIdx？答：是的！
        readIdx += count;
        CheckAndMoveBytes();
        return count;
    }

    public Int16 ReadInt16()
    {
        if (length < 2) return 0;
        Int16 ret = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
        readIdx += 2;
        CheckAndMoveBytes();
        return ret;
    }

    public Int32 ReadInt32()
    {
        if (length < 4) return 0;
        Int32 ret = (Int32)((bytes[readIdx + 3] << 24) | (bytes[readIdx + 2] << 16) | (bytes[readIdx + 1] << 8) | bytes[readIdx]);
        readIdx += 4;
        CheckAndMoveBytes();
        return ret;
    }

    //调试用方法
    public override string ToString()
    {
        return BitConverter.ToString(bytes, readIdx, length);
    }

    public string Debug()
    {
        return string.Format("readIdx({0}) writeIdx({1}) bytes({2})", readIdx, writeIdx, BitConverter.ToString(bytes, 0, bytes.Length));
    }
}
