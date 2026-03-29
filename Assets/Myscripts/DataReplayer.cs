using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class FrameData
{
    public Vector3 headPosition;
    public Quaternion headRotation;
    public Vector3 rightPosition;
    public Quaternion rightRotation;
    public Vector3 leftPosition;
    public Quaternion leftRotation;
    public long timeStamp;


    public FrameData(long ts, Vector3 hp, Quaternion hr, Vector3 rp, Quaternion rr, Vector3 lp, Quaternion lr)
    {
        timeStamp = ts;
        headPosition = hp;
        headRotation = hr;
        rightPosition = rp;
        rightRotation = rr;
        leftPosition = lp;
        leftRotation = lr;
    }
}


public class DataReplayer : MonoBehaviour
{
    public string filePath;
    private List<FrameData> frameDataList = new List<FrameData>();
    private int currentFrame = 0;
    private float timeBetweenFrames = 1f / 25f; // 25Hz
    private float timer = 0f;

    public Transform headTransform;
    public Transform rootTransform;
    public Transform bodyTransform;
    public Transform rightHandTransform;
    public Transform leftHandTransform;

    Vector3 offset = Vector3.zero;
    Quaternion handOffsetRotation = Quaternion.Euler(new Vector3(90, -90, -90));  //设定手部偏转以纠正模型的不匹配

    void Start()
    {
        LoadData();

        Transform currentTransform = headTransform;

        while (currentTransform.parent != null)
        {
            offset = offset + currentTransform.localPosition;
            currentTransform = currentTransform.parent;
        }

        timeBetweenFrames = (frameDataList[currentFrame + 1].timeStamp - frameDataList[currentFrame].timeStamp) / 1000f;
    }

    void LoadData()
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] values = line.Split(',');

                long timeStamp = long.Parse(values[0]);

                float hPosX = float.Parse(values[1]);
                float hPosY = float.Parse(values[2]);
                float hPosZ = float.Parse(values[3]);
                float hRotX = float.Parse(values[4]);
                float hRotY = float.Parse(values[5]);
                float hRotZ = float.Parse(values[6]);

                float rPosX = float.Parse(values[7]);
                float rPosY = float.Parse(values[8]);
                float rPosZ = float.Parse(values[9]);
                float rRotX = float.Parse(values[10]);
                float rRotY = float.Parse(values[11]);
                float rRotZ = float.Parse(values[12]);

                float lPosX = float.Parse(values[13]);
                float lPosY = float.Parse(values[14]);
                float lPosZ = float.Parse(values[15]);
                float lRotX = float.Parse(values[16]);
                float lRotY = float.Parse(values[17]);
                float lRotZ = float.Parse(values[18]);

                frameDataList.Add(new FrameData(timeStamp, new Vector3(hPosX, hPosY, hPosZ), Quaternion.Euler(new Vector3(hRotX, hRotY, hRotZ)), new Vector3(rPosX, rPosY, rPosZ), Quaternion.Euler(new Vector3(rRotX, rRotY, rRotZ)), new Vector3(lPosX, lPosY, lPosZ), Quaternion.Euler(new Vector3(lRotX, lRotY, lRotZ))));
            }
        }
    }

    void Update()
    {
        if (currentFrame < frameDataList.Count - 1)
        {
            timer += Time.deltaTime;
            timeBetweenFrames = (frameDataList[currentFrame + 1].timeStamp - frameDataList[currentFrame].timeStamp) / 1000f;

            while (timer > timeBetweenFrames && currentFrame < frameDataList.Count - 1)
            {
                timer -= timeBetweenFrames;
                currentFrame++;
            }

            if (currentFrame < frameDataList.Count - 1)
            {
                // 进行线性插值
                float t = timer / timeBetweenFrames;
                headTransform.position = Vector3.Lerp(frameDataList[currentFrame].headPosition, frameDataList[currentFrame + 1].headPosition, t);
                headTransform.rotation = Quaternion.Slerp(frameDataList[currentFrame].headRotation, frameDataList[currentFrame + 1].headRotation, t);
                rootTransform.position = headTransform.position - offset;

                Vector3 headEulerAngles = headTransform.rotation.eulerAngles;
                Quaternion bodyTargetRotation = Quaternion.Euler(0, headEulerAngles.y, 0);
                bodyTransform.rotation = bodyTargetRotation;


                rightHandTransform.position = Vector3.Lerp(frameDataList[currentFrame].rightPosition, frameDataList[currentFrame + 1].rightPosition, t);
                rightHandTransform.rotation = Quaternion.Slerp(frameDataList[currentFrame].rightRotation, frameDataList[currentFrame + 1].rightRotation, t);
                rightHandTransform.localRotation = rightHandTransform.localRotation * handOffsetRotation;

                leftHandTransform.position = Vector3.Lerp(frameDataList[currentFrame].leftPosition, frameDataList[currentFrame + 1].leftPosition, t);
                leftHandTransform.rotation = Quaternion.Slerp(frameDataList[currentFrame].leftRotation, frameDataList[currentFrame + 1].leftRotation, t);
                leftHandTransform.localRotation = leftHandTransform.localRotation * handOffsetRotation;

            }
        }
    }
}