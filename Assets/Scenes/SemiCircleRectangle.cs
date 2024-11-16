using UnityEngine;

public class SemiCircleRectangle : MonoBehaviour
{
    public float width = 200f;  // 宽度
    public float height = 50f;  // 高度
    public int segments = 50;   // 圆的分段数
    public Color color = Color.red;

    private LineRenderer lineRenderer;

    void Start()
    {
        // 添加 LineRenderer 组件
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 2f;
        lineRenderer.endWidth = 2f;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        // 绘制带半圆的矩形
        DrawSemiCircleRectangle();
    }

    void DrawSemiCircleRectangle()
    {
        // 计算每个圆的角度步长
        float angleStep = 180f / segments;

        // 点的数组，记录所有点的位置
        Vector3[] points = new Vector3[segments * 2 + 2];

        // 先绘制左端半圆
        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            points[i] = new Vector3(Mathf.Cos(angle) * height, Mathf.Sin(angle) * height, 0);
        }

        // 绘制中间直线部分
        points[segments] = new Vector3(-width / 2, 0, 0);
        points[segments + 1] = new Vector3(width / 2, 0, 0);

        // 先绘制右端半圆
        for (int i = 0; i < segments; i++)
        {
            float angle = (i + 180f) * Mathf.Deg2Rad;
            points[segments + 2 + i] = new Vector3(Mathf.Cos(angle) * height, Mathf.Sin(angle) * height, 0);
        }

        // 将计算的所有点应用到 LineRenderer 上
        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }
}