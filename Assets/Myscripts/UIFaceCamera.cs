using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // 获取主摄像机
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 获取摄像机到对象的方向
        Vector3 direction = transform.position - mainCamera.transform.position;

        // 只在水平轴上调整方向
        direction.y = 0;

        // 如果方向向量不为零，则旋转对象
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
