using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public interface IPainter
{
    Color PaintColor { get; set; }
    int BrushSize { get; set; }

    void SetPoint(Vector2 point);
    MyGraphics2D[] PaintResult { get; }

    // 允许重置状态（用于复用Painter实例）
    void Reset();
}

// [Serializable]
public abstract class Painter
{
    public Color PaintColor = Color.black;
    public int BrushSize = 1;

    protected Vector2 point0 = Vector2.zero;
    protected Vector2 point1 = Vector2.zero;

    private List<MyGraphics2D> GraphicsList { get; set; } =  new List<MyGraphics2D>();
    protected MyGraphics2D MyGraphics2D = null;

    public MyGraphics2D[] PaintResult
    {
        get
        {
            EndPaint();
            MyGraphics2D[] result = GraphicsList.ToArray();
            GraphicsList.Clear();
            if(result.Length != 0)
                Debug.Log($"[{this.GetType().Name}.PaintResult]\nResult MyGraphics2D Count: {result.Length}"
                          + $"\n is Can Be Selected:{string.Join(",", result.Select(g => g.CanBeSelected))}"
                          + $"\nColor: {result[0].Color}"
                          + $"\nBrushSize: {result[0].BrushSize}");

            return result.Length > 0 ? result : null;
        }
    }

    protected void PaintLine(Vector2 start, Vector2 end)
    {
        Vector2[] segment = CanvasPixelPainter.Instance.LinePaint(start, end, PaintColor, BrushSize);
        MyGraphics2D.AddSegment(segment);
        // Debug.Log($"[{this.GetType().Name}.PaintLine]\nSegments Count: {MyGraphics2D.Segments.Count}"
        //           // + $"\n Add Segment:\n{string.Join(",", segment.Select(s => s.ToString()))}"
        //           );
    }

    public void SetPoint(Vector2 point)
    {
        if (MyGraphics2D == null)
        {
            MyGraphics2D = new MyGraphics2D(new List<Vector2>(), new List<Vector2[]>(), this.GetType(),
                PaintColor, BrushSize, true, new MyTransform2D(point));
        }

        bool isEnd = SetPointRealize(point);

        MyGraphics2D.AddPoint(point);
        CanvasPixelPainter.Instance.PointPaint(point, PaintColor, BrushSize, true);

        if (isEnd)
            EndPaint();
    }

    public void EndPaint()
    {
        if (MyGraphics2D == null)
            return;

        EndPaintRealize();

        GraphicsList.Add(MyGraphics2D);
        Debug.Log($"[{this.GetType().Name}.EndPaint]\nEndPaint, Add a new MyGraphics2D to List!");

        point0 = point1 = Vector2.zero;
        MyGraphics2D = null;
    }

    protected abstract bool SetPointRealize(Vector2 point);

    protected virtual void EndPaintRealize(){}
}
