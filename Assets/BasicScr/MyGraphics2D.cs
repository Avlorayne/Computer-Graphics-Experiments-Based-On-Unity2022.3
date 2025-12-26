using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class MyGraphics2D
{
    // MyGraphics2D Info
    public List<Vector2> NodePoints = new List<Vector2>();
    public List<Vector2[]> Segments = new List<Vector2[]>();

    public Type PainterType;
    public int BrushSize = 1;
    public Color Color = Color.black;
    public bool CanBeSelected = false;

    private bool selected = false;
    // public List<Vector2> OriginalNodePoints; // 原始点（只读备份）
    public List<Vector2> Offsets
    {
        get
        {
            List<Vector2> offsets = new List<Vector2>();
            for (int i = 0; i < NodePoints.Count; i++)
            {
                Vector2 offset = NodePoints[i] - Transform.AnchorPosition;
                offsets.Add(offset);
            }
            return offsets;
        }
    }
    private MyGraphics2D highlightGraphics;  // 高亮图形的引用

    public MyTransform2D Transform;

    #region Constructors

    public MyGraphics2D() { }

    public MyGraphics2D(List<Vector2> nodePoints, Type painterType, Color color, int brushSize, bool canBeSelected)
    {
        NodePoints = nodePoints;
        PainterType = painterType;
        BrushSize = brushSize;
        Color = color;
        CanBeSelected = canBeSelected;
        // OriginalNodePoints = new List<Vector2>(NodePoints);
    }

    public MyGraphics2D(List<Vector2> nodePoints, List<Vector2[]> segments, Type painterType, Color color, int brushSize,
        bool canBeSelected, MyTransform2D transform2D = null)
    {
        NodePoints = nodePoints;
        Segments = segments;
        PainterType = painterType;
        Color = color;
        BrushSize = brushSize;
        CanBeSelected = canBeSelected;
        Transform = transform2D ?? new MyTransform2D();
        // OriginalNodePoints = new List<Vector2>(NodePoints);
    }
    #endregion

    #region Basic Set

    public string MyDebug
    {
        get
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[MyGraphics2D]--------------------------------------".Truncate(25));
            sb.AppendLine($"Node Points {NodePoints.Count} in total:");
            sb.AppendLine($"{string.Join(", ", NodePoints)}");
            // sb.AppendLine($"Origin Nodes:");
            // sb.AppendLine($"{string.Join(", ", OriginalNodePoints)}");
            sb.AppendLine($"Segments {Segments.Count} in total");
            sb.AppendLine($"Painter Type {PainterType}");
            sb.AppendLine($"Color {Color}");
            sb.AppendLine($"Brush Size {BrushSize}");
            sb.AppendLine($"CanBeSelected {CanBeSelected}");
            sb.AppendLine($"Transform {Transform.MyDebug}");
            return sb.ToString();
        }
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
        // OriginalNodePoints.Add(point);
    }

    public void AddRange(List<Vector2> points)
    {
        NodePoints.AddRange(points);
        // OriginalNodePoints.AddRange(points);
    }

    public void AddSegment(Vector2[] segment)
    {
        Segments.Add(segment);
    }

    public void ClearOnCanvas()
    {
        MyGraphics2D clearGraphics = new MyGraphics2D(
            NodePoints,
            Segments,
            PainterType,
            Color.white,
            BrushSize,  // 使用相同的画笔大小
            false
        );

        CanvasPixelPainter.Instance?.PaintGraphics2D(clearGraphics);

        Debug.Log($"[MyGraphics2D.ClearOnCanvas]\nGraphics Cleared On Canvas]");
    }

    public void Highlight()
    {
        // 创建高亮图形（画笔加粗）
        highlightGraphics = new MyGraphics2D(
            NodePoints,
            Segments,
            PainterType,
            Color.yellow,
            BrushSize + 2,  // 加粗更多，更明显
            false
        );

        // 绘制高亮图形
        CanvasPixelPainter.Instance?.PaintGraphics2D(highlightGraphics);
        CanvasPixelPainter.Instance?.PaintGraphics2D(this);
        Debug.Log($"[MyGraphics2D.Highlight]\nGraphics Highlight On Canvas");
    }

    public void OverPaint()
    {
        if (PainterType == null)
        {
            Debug.LogError("[MyGraphics2D.OverPaint]\nPainterType is null! Ensure it's set during construction.");
            return;
        }

        try
        {
            // 使用工厂创建Painter（高性能 + 类型安全）
            Painter painter = PainterFactory.CreatePainter(PainterType);
            if (painter == null)
            {
                Debug.Log($"[MyGraphics2D.OverPaint]\nFailed to create painter! PainterType: {PainterType}");
                return;
            }
            Debug.Log($"[MyGraphics2D.OverPaint]\nFind Matched Painter: {painter.GetType()}]");
            
            // 配置绘制参数
            painter.PaintColor = this.Color;
            painter.BrushSize = this.BrushSize;

            // 清除高亮和当前图形
            highlightGraphics?.ClearOnCanvas();
            this.ClearOnCanvas();

            // 根据变换后的 NodePoints 重新生成 Segments
            foreach (var point in NodePoints)
            {
                painter.SetPoint(point);
            }

            // 获取重新生成的结果
            MyGraphics2D[] results = painter.PaintResult;
            if (results == null || results.Length == 0)
            {
                Debug.LogWarning($"[MyGraphics2D.OverPaint]\nPainter {PainterType.Name} produced no output.");
                return;
            }

            // 更新 Segments 数据（保持正确的点密度）
            MyGraphics2D newGraphics = results[0];
            this.Segments = newGraphics.Segments;

            // 将更新后的图形绘制到画布上
            CanvasPixelPainter.Instance?.PaintGraphics2D(this);

            Debug.Log($"[MyGraphics2D.OverPaint]\nSuccessfully overpainted with {PainterType.Name}. " +
                      $"Segments: {Segments.Count}, Points: {NodePoints.Count}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MyGraphics2D.OverPaint]\nOverPaint failed with {PainterType?.Name}: {ex.Message}\n" +
                           $"StackTrace: {ex.StackTrace}");
        }
    }
    #endregion

    #region Selection

    public void Select()
    {
        selected = true;

        // // 创建高亮颜色
        // Color highlightColor = GetHighlightColor(Color);

        Highlight();

    }

    public void Deselect()
    {
        if (!selected)
            return;

        selected = false;

        // 如果有高亮图形，先清除它
        highlightGraphics?.ClearOnCanvas();

        // 重新绘制原始图形
        CanvasPixelPainter.Instance?.PaintGraphics2D(this);
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
    #endregion
}

public partial class CanvasPixelPainter
{
    public void PaintGraphics2D(MyGraphics2D myGraphics2D)
    {
        Vector2[][] segments = myGraphics2D.Segments.ToArray();
        int brushSize = myGraphics2D.BrushSize;
        Color color = myGraphics2D.Color;

        foreach (var segment in segments)
        {
            foreach (var point in segment)
            {
                PointPaint(point, color, brushSize);
            }
        }
        texture.Apply();
        Debug.Log("[CanvasPixelPainter.PaintGraphics2D]\nCompleted Paint graphics 2D");
    }
}
