using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Graphics2D
{
    // Graphics2D Info
    public List<Vector2> NodePoints = new List<Vector2>();
    public List<Vector2[]> Segments = new List<Vector2[]>();
    public Vector2 Pivot = Vector2.zero;

    public int BrushSize = 1;
    public Color Color = Color.black;
    public bool CanBeSelected = false;

    private bool selected = false;
    private Graphics2D highlightGraphics;  // 高亮图形的引用

    public Graphics2D() { }

    public Graphics2D(List<Vector2> nodePoints, Color color, int brushSize, bool canBeSelected)
    {
        NodePoints = nodePoints;
        BrushSize = brushSize;
        Color = color;
        CanBeSelected = canBeSelected;

        if (nodePoints is { Count: > 0 })
            Pivot = nodePoints[0];
    }

    public Graphics2D(List<Vector2> nodePoints, List<Vector2[]> segments, Color color, int brushSize,
        bool canBeSelected, Vector2 pivot)
    {
        NodePoints = nodePoints;
        Segments = segments;
        Color = color;
        CanBeSelected = canBeSelected;
        Pivot = pivot;
        BrushSize = brushSize;
    }

    public void Reset()
    {
        NodePoints.Clear();
        BrushSize = 1;
        Color = Color.black;

        // 可选：通知画板清理这个图形
        highlightGraphics = null;

    }

    public void AddPoint(Vector2 point)
    {
        NodePoints.Add(point);
    }

    public void AddRange(List<Vector2> points)
    {
        NodePoints.AddRange(points);
    }

    public void AddSegment(Vector2[] segment)
    {
        Segments.Add(segment);
    }

    public void Select()
    {
        if (selected)
            return;

        selected = true;

        // // 创建高亮颜色
        // Color highlightColor = GetHighlightColor(Color);

        // 创建高亮图形（画笔加粗）
        highlightGraphics = new Graphics2D(
            NodePoints,
            Segments,
            Color.yellow,
            BrushSize + 2,  // 加粗更多，更明显
            false,
            Pivot
        );

        // 绘制高亮图形
        CanvasPixelPainter.Instance?.PaintGraphics2D(highlightGraphics);
        CanvasPixelPainter.Instance?.PaintGraphics2D(this);
    }

    public void Deselect()
    {
        if (!selected)
            return;

        selected = false;

        // 如果有高亮图形，先清除它
        if (highlightGraphics != null)
        {
            // 用白色覆盖高亮部分（清除）
            Graphics2D clearGraphics = new Graphics2D(
                NodePoints,
                Segments,
                Color.white,
                highlightGraphics.BrushSize,  // 使用相同的画笔大小
                false,
                Pivot
            );
            CanvasPixelPainter.Instance?.PaintGraphics2D(clearGraphics);

            // 重新绘制原始图形
            CanvasPixelPainter.Instance?.PaintGraphics2D(this);

            highlightGraphics = null;
        }
    }

    // // 获取高亮颜色的辅助方法
    // private Color GetHighlightColor(Color originalColor)
    // {
    //     // 方法1：反转颜色（更通用）
    //     if (originalColor.r + originalColor.g + originalColor.b < 1.5f)
    //     {
    //         // 深色变浅色
    //         return new Color(
    //             1f - originalColor.r * 0.7f,
    //             1f - originalColor.g * 0.7f,
    //             1f - originalColor.b * 0.7f,
    //             Mathf.Clamp01(originalColor.a + 0.3f)
    //         );
    //     }
    //     else
    //     {
    //         // 浅色变深色
    //         return new Color(
    //             originalColor.r * 0.3f,
    //             originalColor.g * 0.3f,
    //             originalColor.b * 0.3f,
    //             Mathf.Clamp01(originalColor.a + 0.3f)
    //         );
    //     }
    //
    //     // 方法2：使用固定高亮颜色（如黄色）
    //     // return Color.yellow;
    //
    //     // 方法3：增加饱和度
    //     // Color.RGBToHSV(originalColor, out float h, out float s, out float v);
    //     // return Color.HSVToRGB(h, Mathf.Min(s * 1.5f, 1f), Mathf.Min(v * 1.2f, 1f));
    // }
}

public partial class CanvasPixelPainter
{
    public void PaintGraphics2D(Graphics2D graphics2D)
    {
        Vector2[][] segments = graphics2D.Segments.ToArray();
        int brushSize = graphics2D.BrushSize;
        Color color = graphics2D.Color;

        foreach (var segment in segments)
        {
            foreach (var point in segment)
            {
                PointPaint(point, color, brushSize);
            }
        }
        texture.Apply();
        Debug.Log("Completed Paint graphics 2D");
    }
}
