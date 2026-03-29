using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Linq;
using System;



public class FixedSizeQueue<T>
{
    private Queue<T> queue = new Queue<T>();
    private int limit = 15;

    public FixedSizeQueue(int limit)
    {
        this.limit = limit;
    }

    public void Enqueue(T item)
    {
        if (queue.Count >= limit)
        {
            queue.Dequeue();  // 移除队列中最早加入的元素
        }
        queue.Enqueue(item);
    }

    public T Dequeue()
    {
        if (queue.Count > 0)
        {
            return queue.Dequeue();
        }
        throw new InvalidOperationException("队列为空");
    }

    public int Count => queue.Count;

    public void Clear()
    {
        queue.Clear();
    }

    // 添加一个方法来计算队列中的元素之和
    public float Sum()
    {
        if (queue.Count == 0)
        {
            return 0;  // 如果队列为空，则返回0
        }

        if (typeof(T) == typeof(float))  // 确保T是int类型
        {
            return queue.Sum(item => (float)(object)item);
        }
        else
        {
            throw new InvalidOperationException("Sum方法仅支持整数类型的元素");
        }
    }
}


public class IconControlVer2 : MonoBehaviour
{
    private List<string[]> talkData = new List<string[]>(); // 存储是否在说话的数据
    private List<long[]> talkTimestampData = new List<long[]>(); // 存储是否在说话的时间戳数据
    private List<float[]> probabilityData = new List<float[]>(); // 存储概率数据
    private List<long[]> probabilityTimestampData = new List<long[]>(); // 存储概率数据
    private int[] talkIndexList = { 0, 0, 0, 0 }; // 当前每个人的说话数据索引
    private int[] probIndexList = { 0, 0, 0, 0 }; // 当前每个人的说话数据索引
    private float[] talkUpdateTimeList = { 0f, 0f, 0f, 0f }; // 跟踪说话更新时间
    private float[] probUpdateTimeList = { 0f, 0f, 0f, 0f }; // 跟踪概率更新时间
    private float[] talkIntervalList = { 0f, 0f, 0f, 0f }; // 说话更新间隔
    private float[] probIntervalList = { 0f, 0f, 0f, 0f }; // 概率更新间隔
/*    private float talkUpdateTime = 0f; // 跟踪说话更新时间
    private float probUpdateTime = 0f; // 跟踪概率更新时间
    private float talkInterval = 1f / 25f; // 说话更新间隔
    private float probInterval = 1f / 1f; // 概率更新间隔*/
    public Animator[] animators;
    private int[] lastStatus = new int[4];
    private float[] activeUpdateTimes = new float[] { 0f, 0f, 0f, 0f }; // 跟踪活跃维持时间
    private float activeKeepTime = 1f; // 跟踪活跃维持时间
    private bool[] actives = new bool[4];
    public Image[] faceColorImage;
    private float[] colorTransitionTimes = new float[] { 0f, 0f, 0f, 0f };
    private float speedMulitolity = 10.0f;

    private float[] targetFillAmounts = new float[] { 0f, 0f, 0f, 0f };
    public Image[] targetImages;
    private float lerpRate = 5f;


    public GameObject[] talkingIcons; // 指定第一个图标
    public GameObject[] faceIcons; // 指定第二个图标
    public GameObject[] tethers;//指示连线
    public float showLineThreshold = 0.5f;

    public Text testText;

    public string isSpeakingFilePath;
    public string probabilityFilePath;
    private int count = 0;

    public FixedSizeQueue<float>[] queues = new FixedSizeQueue<float>[4];  //四个队列用于存储一段时间的说话意图结果
    int maxQueueSize = 15; //设置队列的上限，存储多久时间内的模型结果

    Color myColor = new Color(1f, 0f, 0f, 0f);

    private int peopleCount = 4;

    int temp = -20;

    void Start()
    {
        ReadSpeakCSV();
        ReadProbabilityCSV();
        /*        StartCoroutine(UpdateIcons());*/
        for (int i = 0; i < talkingIcons.Length; i++)
        {
            ShowFace(i);
        }

        for (int i = 0; i < talkingIcons.Length; i++)
        {
            queues[i] = new FixedSizeQueue<float>(maxQueueSize);  // 每个队列的元素上限为15
        }

    }

    void Update()
    {
        /*        // 可以在这里根据需要添加逻辑来更改颜色或图标
                talkUpdateTime += Time.deltaTime;
                probUpdateTime += Time.deltaTime;

                if (talkUpdateTime >= talkInterval)
                {
                    if (talkIndex < talkData.Count)
                    {
                        UpdateTalkStatus(talkIndex);
                        talkIndex++;
                    }
                    talkUpdateTime = 0f;
                }*/

        /*        for (int i = 0; i < talkingIcons.Length; i++)
                {
                    //检测冷却时间
                    activeUpdateTimes[i] += Time.deltaTime;
                    if ((activeUpdateTimes[i] > activeKeepTime) && (actives[i] == true))
                    {
        *//*                animators[i].ResetTrigger("ActiveTrigger");
                        animators[i].ResetTrigger("CalmTrigger");
                        animators[i].ResetTrigger("LowTrigger");
                        animators[i].ResetTrigger("MiddleTrigger");
                        animators[i].ResetTrigger("HighTrigger");*//*
                        animators[i].SetTrigger("LowTrigger");
                        animators[i].SetFloat("SpeedMulitolity", speedMulitolity);
                        actives[i] = false;
                        //colorTransitionTimes[i] = 0f;
                        //lastStatus[i] = -2;
                    }
                }*/

        /*        if (probUpdateTime >= probInterval)
                {
                    if (probIndex < probabilityData.Count)
                    {
                        probIndex++; // Increment probability index only
                        UpdateFaceState();
                        UpdateTargetFillAmounts();
                    }
                    probUpdateTime = 0f;
                }*/

        for (int i = 0; i < peopleCount; i++)
        {
            talkUpdateTimeList[i] += Time.deltaTime;
            probUpdateTimeList[i] += Time.deltaTime;

            if (talkIndexList[i] < talkData[i].Length - 1)
                talkIntervalList[i] = (talkTimestampData[i][talkIndexList[i] + 1] - talkTimestampData[i][talkIndexList[i]]) / 1000f;

            if (probIndexList[i] < probabilityData[i].Length - 1)
                probIntervalList[i] = (probabilityTimestampData[i][probIndexList[i] + 1] - probabilityTimestampData[i][probIndexList[i]]) / 1000f;

            //Debug.Log("probIndexList[i] " + probIndexList[i]);

            if ((talkUpdateTimeList[i] > talkIntervalList[i]) && (talkIndexList[i] < talkData[i].Length - 1))
            {
                talkIndexList[i]++;
                UpdateTalkStatus(i);
                talkUpdateTimeList[i] -= talkIntervalList[i];
            }

            if ((probUpdateTimeList[i] > probIntervalList[i]) && (probIndexList[i] < probabilityData[i].Length - 1))
            {
                probIndexList[i]++;
                UpdateFaceState(i);
                UpdateTargetFillAmounts(i);
                probUpdateTimeList[i] -= probIntervalList[i];
            }
        }



        for (int i = 0; i < talkingIcons.Length; i++)
        {

/*            if (colorTransitionTimes[i] < 0.2f && actives[i] == false)
            {
                // 使用Lerp函数在红色和蓝色之间过渡
                faceColorImage[i].color = Color.Lerp(myColor, Color.yellow, colorTransitionTimes[i] / 0.2f);
                colorTransitionTimes[i] += Time.deltaTime;
            }*/

            //跟新进度条
            if (targetImages[i].fillAmount != targetFillAmounts[i])
            {
                targetImages[i].fillAmount = Mathf.Lerp(targetImages[i].fillAmount, targetFillAmounts[i], Time.deltaTime * lerpRate);
            }

/*            if (i == 0)
                testText.text = actives[i] + " " + lastStatus[i].ToString() + " talkdata " + talkData[talkIndex][i] + " probabilityData " + probabilityData[probIndex][i];*/
        }
    }

    void UpdateTargetFillAmounts(int i)
    {
        if (talkData[i][talkIndexList[i]] == "0")
        {
            queues[i].Enqueue(probabilityData[i][probIndexList[i]]);

            targetFillAmounts[i] = queues[i].Sum()/maxQueueSize;
            if(i==0)
                Debug.Log("targetFillAmounts 0 = " + targetFillAmounts[i]);
            //Debug.Log((i).ToString() + "targetFillAmounts" + (targetFillAmounts[i]).ToString());
        }
        else
        {
            queues[i].Clear();
            targetFillAmounts[i] = queues[i].Count;
            //Debug.Log((i).ToString() + "targetFillAmounts" + (targetFillAmounts[i]).ToString());
        }

        if(targetFillAmounts[i] >= showLineThreshold)
        {
            tethers[i].SetActive(true);
/*                LineRenderer lineRenderer = tethers[i].GetComponent<LineRenderer>();
            Color lineColor = new Color(1f, 0f, 0f, 1f);  // 红色，50%透明度
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;*/

        }
        else
        {
            tethers[i].SetActive(false);
        }

    }

    /*    void ReadSpeakCSV()
        {
            string path = isSpeakingFilePath; // 替换为你的CSV文件名
            string[] lines = File.ReadAllLines(path);

            foreach (var line in lines)
            {
                string[] entries = line.Split(',');
                if (entries.Length == 4) // 确保每行有4个数据
                {
                    talkData.Add(entries);
                }
            }
        }

        void ReadProbabilityCSV()
        {
            string path = probabilityFilePath; // 替换为你的CSV文件名
            string[] lines = File.ReadAllLines(path);

            foreach (var line in lines)
            {
                string[] entries = line.Split(',');
                float[] probabilities = new float[entries.Length];
                for (int i = 0; i < entries.Length; i++)
                {
                    if (float.TryParse(entries[i], out float value))
                    {
                        probabilities[i] = value;
                    }
                }
                probabilityData.Add(probabilities);
            }
        }*/

    void ReadSpeakCSV()
    {
        List<string> aliceSpeakList = new List<string>();
        List<string> bobSpeakList = new List<string>();
        List<string> carolSpeakList = new List<string>();
        List<string> daveSpeakList = new List<string>();

        List<long> aliceTimestampList = new List<long>();
        List<long> bobTimestampList = new List<long>();
        List<long> carolTimestampList = new List<long>();
        List<long> daveTimestampList = new List<long>();

        string path = isSpeakingFilePath; // 替换为你的CSV文件名
        string[] lines = File.ReadAllLines(path);

        foreach (var line in lines)
        {
            string[] entries = line.Split(',');

            if (entries.Length == 8) // 确保每行有4个数据
            {

                aliceSpeakList.Add(entries[0]);
                bobSpeakList.Add(entries[2]);
                carolSpeakList.Add(entries[4]);
                daveSpeakList.Add(entries[6]);

                aliceTimestampList.Add(long.Parse(entries[1]));
                bobTimestampList.Add(long.Parse(entries[3]));
                carolTimestampList.Add(long.Parse(entries[5]));
                daveTimestampList.Add(long.Parse(entries[7]));
            }
        }
        talkData.Add(aliceSpeakList.ToArray());
        talkData.Add(bobSpeakList.ToArray());
        talkData.Add(carolSpeakList.ToArray());
        talkData.Add(daveSpeakList.ToArray());


        talkTimestampData.Add(aliceTimestampList.ToArray());
        talkTimestampData.Add(bobTimestampList.ToArray());
        talkTimestampData.Add(carolTimestampList.ToArray());
        talkTimestampData.Add(daveTimestampList.ToArray());

        Debug.Log("talkData.count " + talkData.Count);
        Debug.Log("talkTimestampData.count " + talkTimestampData.Count);
    }

    void ReadProbabilityCSV()
    {
        List<float> aliceProbabilityList = new List<float>();
        List<float> bobProbabilityList = new List<float>();
        List<float> carolProbabilityList = new List<float>();
        List<float> daveProbabilityList = new List<float>();

        List<long> aliceProbabilityTimestampList = new List<long>();
        List<long> bobProbabilityTimestampList = new List<long>();
        List<long> carolProbabilityTimestampList = new List<long>();
        List<long> daveProbabilityTimestampList = new List<long>();


        string path = probabilityFilePath; // 替换为你的CSV文件名
        string[] lines = File.ReadAllLines(path);

        foreach (var line in lines)
        {
            string[] entries = line.Split(',');

            if (entries.Length == 8) // 确保每行有4个数据
            {

                aliceProbabilityList.Add(float.Parse(entries[0]));
                bobProbabilityList.Add(float.Parse(entries[2]));
                carolProbabilityList.Add(float.Parse(entries[4]));
                daveProbabilityList.Add(float.Parse(entries[6]));

                aliceProbabilityTimestampList.Add(long.Parse(entries[1]));
                bobProbabilityTimestampList.Add(long.Parse(entries[3]));
                carolProbabilityTimestampList.Add(long.Parse(entries[5]));
                daveProbabilityTimestampList.Add(long.Parse(entries[7]));
            }

            /*            float[] probabilities = new float[entries.Length];
                        for (int i = 0; i < entries.Length; i++)
                        {
                            if (float.TryParse(entries[i], out float value))
                            {
                                probabilities[i] = value;
                            }
                        }
                        probabilityData.Add(probabilities);*/
        }
        probabilityData.Add(aliceProbabilityList.ToArray());
        probabilityData.Add(bobProbabilityList.ToArray());
        probabilityData.Add(carolProbabilityList.ToArray());
        probabilityData.Add(daveProbabilityList.ToArray());


        probabilityTimestampData.Add(aliceProbabilityTimestampList.ToArray());
        probabilityTimestampData.Add(bobProbabilityTimestampList.ToArray());
        probabilityTimestampData.Add(carolProbabilityTimestampList.ToArray());
        probabilityTimestampData.Add(daveProbabilityTimestampList.ToArray());
    }



    public void ShowFace(int i)
    {
        talkingIcons[i].SetActive(false);
        faceIcons[i].SetActive(true);
        InitShowFace(i);
        animators[i].SetTrigger("CalmTrigger");
    }


    public void InitShowFace(int i)
    {
        //actives[i] = true;
        //activeUpdateTimes[i] = 0f;
        //faceColorImage[i].color = myColor;
        animators[i].ResetTrigger("ActiveTrigger");
        animators[i].ResetTrigger("CalmTrigger");
        animators[i].ResetTrigger("LowTrigger");
        animators[i].ResetTrigger("MiddleTrigger");
        animators[i].ResetTrigger("HighTrigger");
        lastStatus[i] = -2;
    }



    void UpdateTalkStatus(int i)
    {
        count += 1;
        if (talkData[i][talkIndexList[i]] == "1")
        {
            if (!talkingIcons[i].activeSelf)
            {
                InitShowFace(i);
                talkingIcons[i].SetActive(true);
                animators[i].SetTrigger("ActiveTrigger");
            }
        }
        else
        {
            if (talkingIcons[i].activeSelf)
            {
                talkingIcons[i].SetActive(false);
                animators[i].ResetTrigger("ActiveTrigger");
                animators[i].SetTrigger("CalmTrigger");
                animators[i].SetFloat("SpeedMulitolity", speedMulitolity);
            }
        }
    }


    void UpdateFaceState(int i)
    {
        if (!talkingIcons[i].activeSelf)
        {
            int value;
            // 使用最新的概率数据更新颜色
            //float prob = probIndex < probabilityData.Count ? probabilityData[probIndex][i] : 0f;
            float prob = probIndexList[i] < probabilityData[i].Length ? probabilityData[i][probIndexList[i]] : 0f;
            if (prob > 0.5f)
                value = 1;
            else
                value = 0;
            if ((value == 1) && (lastStatus[i] != 1))
            {
                animators[i].SetFloat("SpeedMulitolity", speedMulitolity);
                animators[i].SetTrigger("MiddleTrigger");
/*                    actives[i] = true;
                activeUpdateTimes[i] = 0;*/

            }
            if ((value == 0) && (lastStatus[i] != 0))
            {
                animators[i].SetFloat("SpeedMulitolity", speedMulitolity);
                animators[i].SetTrigger("LowTrigger");
            }
            lastStatus[i] = value;
        }
    }
}
