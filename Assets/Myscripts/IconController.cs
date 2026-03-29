using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class IconController : MonoBehaviour
{
    private List<string[]> talkData = new List<string[]>(); // 存储是否在说话的数据
    private List<long[]> talkTimestampData = new List<long[]>(); // 存储是否在说话的时间戳数据
    private List<float[]> probabilityData = new List<float[]>(); // 存储概率数据
    private List<long[]> probabilityTimestampData = new List<long[]>(); // 存储概率数据
/*    private int talkIndex = 0; // 当前说话数据索引
    private int probIndex = 0; // 当前概率数据索引*/
    private int[] talkIndexList = { 0, 0, 0, 0 }; // 当前每个人的说话数据索引
    private int[] probIndexList = { 0, 0, 0, 0 }; // 当前每个人的说话数据索引
    private float[] talkUpdateTimeList = { 0f, 0f, 0f, 0f }; // 跟踪说话更新时间
    private float[] probUpdateTimeList = { 0f, 0f, 0f, 0f }; // 跟踪概率更新时间
    private float[] talkIntervalList = { 0f, 0f, 0f, 0f }; // 说话更新间隔
    private float[] probIntervalList = { 0f, 0f, 0f, 0f }; // 概率更新间隔
    public Animator[] animators;
    private int[] lastStatus = new int[4];
    private float[] activeUpdateTimes = new float[] { 0f, 0f, 0f, 0f }; // 跟踪活跃维持时间
    private float activeKeepTime = 2f; // 跟踪活跃维持时间
    private bool[] actives = new bool[4];
    public Image[] faceColorImage;
    private float[] colorTransitionTimes = new float[] { 0f, 0f, 0f, 0f };
    private float speedMulitolity = 10.0f;

    private float[] targetFillAmounts = new float[] { 0f, 0f, 0f, 0f };
    public Image[] targetImages;
    private float lerpRate = 5f;


    public GameObject[] talkingIcons; // 指定第一个图标
    public GameObject[] faceIcons; // 指定第二个图标

    public Text testText;

    public string isSpeakingFilePath;
    public string probabilityFilePath;
    private int count = 0;

    private int peopleCount = 4;

    Color myColor = new Color(1f, 0f, 0f, 0f);

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
    }

    void Update()
    {
        // 可以在这里根据需要添加逻辑来更改颜色或图标
/*        talkUpdateTime += Time.deltaTime;
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


        for (int i = 0; i< peopleCount; i++)
        {
            talkUpdateTimeList[i] += Time.deltaTime;
            probUpdateTimeList[i] += Time.deltaTime;

            if(talkIndexList[i] < talkData[i].Length - 1)
                talkIntervalList[i] = (talkTimestampData[i][talkIndexList[i]+1] - talkTimestampData[i][talkIndexList[i]]) / 1000f;
            
            if(probIndexList[i] < probabilityData[i].Length - 1)
                probIntervalList[i] = (probabilityTimestampData[i][probIndexList[i]+1] - probabilityTimestampData[i][probIndexList[i]]) / 1000f;
            
            //Debug.Log("probIndexList[i] " + probIndexList[i]);

            if ((talkUpdateTimeList[i] > talkIntervalList[i]) && (talkIndexList[i]  < talkData[i].Length - 1))
            {
                talkIndexList[i]++;
                UpdateTalkStatus(i);
                talkUpdateTimeList[i] -= talkIntervalList[i];
            }

            if((probUpdateTimeList[i] > probIntervalList[i]) && (probIndexList[i] < probabilityData[i].Length -1))
            {
                probIndexList[i]++;
                temp = UpdateFaceState(i);
                UpdateTargetFillAmounts(i);
                probUpdateTimeList[i] -= probIntervalList[i];
            }
        }


/*        for (int i = 0; i < talkingIcons.Length; i++)
        {
            //检测冷却时间
            activeUpdateTimes[i] += Time.deltaTime;
            if ((activeUpdateTimes[i] > activeKeepTime) && (actives[i] == true))
            {
                animators[i].ResetTrigger("ActiveTrigger");
                animators[i].ResetTrigger("CalmTrigger");
                animators[i].ResetTrigger("LowTrigger");
                animators[i].ResetTrigger("MiddleTrigger");
                animators[i].ResetTrigger("HighTrigger");
                animators[i].SetTrigger("CalmTrigger");
                animators[i].SetFloat("SpeedMulitolity", speedMulitolity);
                actives[i] = false;
                colorTransitionTimes[i] = 0f;
                lastStatus[i] = -2;
            }
        }*/

/*        if (probUpdateTime >= probInterval)
        {
            if (probIndex < probabilityData.Count)
            {
                probIndex++; // Increment probability index only
                temp = UpdateFaceState();
                UpdateTargetFillAmounts();
            }
            probUpdateTime = 0f;
        }*/

        for (int i = 0; i < peopleCount; i++)
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
                testText.text = actives[i] + " " + lastStatus[i].ToString() + " talkdata " + talkData[talkIndex][i] + " probabilityData " + probabilityData[probIndex][i] + " value " + temp;*/
        }
    }

    void UpdateTargetFillAmounts(int i)
    {
        if (actives[i] == false)
            targetFillAmounts[i] = probIndexList[i] < probabilityData[i].Length ? probabilityData[i][probIndexList[i]] : 0f;
        else
            targetFillAmounts[i] = 0;   
    }

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

    public void ShowTalking(int i)
    {
        talkingIcons[i].SetActive(true);
        faceIcons[i].SetActive(false);
    }

    // 调用这个方法来切换显示图标2
    public void ShowFace(int i)
    {
        talkingIcons[i].SetActive(false);
        faceIcons[i].SetActive(true);
        InitShowFace(i);
    }

    public void InitShowFace(int i)
    {
        /*        actives[i] = true;
                activeUpdateTimes[i] = 0f;
                faceColorImage[i].color = myColor;*/
        animators[i].ResetTrigger("ActiveTrigger");
        animators[i].ResetTrigger("CalmTrigger");
        animators[i].ResetTrigger("LowTrigger");
        animators[i].ResetTrigger("MiddleTrigger");
        animators[i].ResetTrigger("HighTrigger");
        lastStatus[i] = -2;
    }



/*    void UpdateTalkStatus(int index)
    {
        count += 1;
        for (int i = 0; i < talkingIcons.Length; i++)
        {
            if (talkData[index][i] == "1")
            {
                if (!talkingIcons[i].activeSelf)
                {
                    talkingIcons[i].SetActive(true);
                    animators[i].SetTrigger("ActiveTrigger");
                }
                InitShowFace(i);
            }
            else
            {
                if (talkingIcons[i].activeSelf)
                    talkingIcons[i].SetActive(false);
            }
        }
    }*/

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


    int UpdateFaceState(int i)
    {
        int result = -10;
        int value;
        //if (actives[i] == false)
        if (!talkingIcons[i].activeSelf)
        {
            // 使用最新的概率数据更新颜色
            float prob = probIndexList[i] < probabilityData[i].Length ? probabilityData[i][probIndexList[i]] : 0f;
            if (prob > 0.8f)
                value = 2;
            else if (prob < 0.2f)
                /*userIcons[i].color = Color.blue;*/
                value = 0;
            else
                /*userIcons[i].color = Color.yellow;*/
                value = 1;
            if (value != lastStatus[i])
            {
                animators[i].SetFloat("SpeedMulitolity", speedMulitolity);
                if (value == 0)
                    animators[i].SetTrigger("LowTrigger");
                else if (value == 1)
                    animators[i].SetTrigger("MiddleTrigger");
                else if (value == 2)
                    animators[i].SetTrigger("HighTrigger");
                lastStatus[i] = value;
            }
            if (i == 0)
                {
                    result = value;
                }
        }
        return result;
    }

    /*    IEnumerator UpdateIcons()
        {
            int index = 0;
            while (index < talkData.Count)
            {
                for (int i = 0; i < userIcons.Length; i++)
                {
                    // 只在状态发生变化时更新图标
                    if (talkData[index][i] != lastStatus[i])
                    {
                        userIcons[i].sprite = talkData[index][i] == "1" ? talkingSprite : notTalkingSprite;
                        lastStatus[i] = talkData[index][i]; // 更新最后的状态
                    }

                    // 更改图标颜色
                    if (userIcons[i].sprite == notTalkingSprite) // 确保当前是notTalkingSprite
                    {
                        float prob;
                        if (index < winSize)
                            prob = 0; 
                        else
                            prob = probabilityData[index - winSize][i];
                        if (prob > 0.8f)
                            userIcons[i].color = Color.red;
                        else if (prob < 0.2f)
                            userIcons[i].color = Color.blue;
                        else
                            userIcons[i].color = Color.yellow;
                    }

                }
                index++;
                yield return new WaitForSeconds(1f / 25f); // 每1/25秒更新一次
            }
        }*/

    /*    // 调用这个方法来改变图标的颜色
        public void ChangeIconColor(Color newColor)
        {
            iconImage.color = newColor;
        }

        // 调用这个方法来切换图标
        public void ChangeIcon()
        {
            currentIconIndex = (currentIconIndex + 1) % iconSprites.Length;
            iconImage.sprite = iconSprites[currentIconIndex];
        }*/


}
