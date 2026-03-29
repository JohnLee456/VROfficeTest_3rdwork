using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineConnector : MonoBehaviour
{
    public Transform modelTransform;  // 模型的位置
    public RectTransform uiIconTransform;  // UI图标的位置
    private LineRenderer lineRenderer;
    public int segmentCount = 20;  // 分段数量
    public float baseCurveHeight = 0.2f;  // 弧度高度
    public float heightDistanceMultiplier = 1f;  // 距离对高度的影响系数
    public float baseHorizontalOffset = 0.5f;  // 基础的横向偏移量
    public float offsetDistanceMultiplier = 0.1f;  // 距离对横向偏移量的影响系数
 

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segmentCount + 1;  // 设置LineRenderer的点数量

        // 设置线条的宽度
        lineRenderer.startWidth = 0.002f;
        lineRenderer.endWidth = 0.002f;

        // 设置线条的颜色和透明度
/*        Color lineColor = new Color(1f, 1f, 1f, 0.3f);  // 红色，50%透明度
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;*/
    }

    void Update()
    {
        Vector3 modelPosition = modelTransform.position;
        Vector3 uiWorldPosition = uiIconTransform.position;


        // 计算模型和UI图标之间的距离
        float distance = Vector3.Distance(modelPosition, uiWorldPosition);

        // 根据距离调整弧度高度
        float curveHeight = baseCurveHeight + distance * heightDistanceMultiplier;

        // 根据距离调整横向偏移量
        float horizontalOffset = baseHorizontalOffset + distance * offsetDistanceMultiplier;

        // 计算线的中心点，并添加弧度高度和偏移量
        Vector3 midPoint = (modelPosition + uiWorldPosition) / 2;
        midPoint.y += curveHeight;  // 提升弧线的中间部分以避开视线

        // 添加横向偏移，确保线条靠近视野边缘
        Vector3 direction = (uiWorldPosition - modelPosition).normalized;
        Vector3 perpendicularOffset = Vector3.Cross(direction, Vector3.down).normalized * horizontalOffset;

        for (int i = 0; i <= segmentCount; i++)
        {
            float t = (float)i / segmentCount;  // 计算t值，范围从0到1
            //Vector3 point = Vector3.Lerp(Vector3.Lerp(modelPosition, midPoint + perpendicularOffset, t), Vector3.Lerp(midPoint + perpendicularOffset, uiWorldPosition, t), t);
            Vector3 point = Vector3.Lerp(Vector3.Lerp(modelPosition, midPoint, t), Vector3.Lerp(midPoint, uiWorldPosition, t), t);
            lineRenderer.SetPosition(i, point);  // 设置LineRenderer的点位置
        }
    }
}
