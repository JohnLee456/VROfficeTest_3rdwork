using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimetionTest : MonoBehaviour
{
    public Animator animator;
    public float updateRate = 0.2f; // 5 Hz，即每0.2秒更新一次
    private float timer;
    private List<int> values = new List<int> { 0, 1, 2, 2, 0, 1 };  // 你的值列表
    private int currentIndex = 0;  // 当前索引

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateRate)
        {
            timer = 0;
            UpdateIconState();
        }
    }

    private void UpdateIconState()
    {
        int value = ReceiveValue(); // 获取列表中的值
        animator.SetInteger("State", value); // 更新Animator的参数
        animator.SetFloat("SpeedMulitolity", 10.0f);
    }

    private int ReceiveValue()
    {
        // 按顺序返回列表中的值
        int value = values[currentIndex]; // 获取当前索引的值
        currentIndex = (currentIndex + 1) % values.Count; // 更新索引，如果到达列表末尾，则重新开始
        return value; // 返回当前值
    }
}
