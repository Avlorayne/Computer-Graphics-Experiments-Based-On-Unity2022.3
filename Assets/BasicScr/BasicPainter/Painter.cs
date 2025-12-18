using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// [Serializable]
public abstract class Painter
{
    public Color PaintColor = Color.black;
    public int BrushSize = 1;

    protected Vector2 point0 = Vector2.zero;
    protected Vector2 point1 = Vector2.zero;

    private List<Graphics2D> GraphicsList { get; set; } =  new List<Graphics2D>();
    protected Graphics2D Graphics2D = null;

    public Graphics2D[] PaintResult
    {
        get
        {
            EndPaint();
            Graphics2D[] result = GraphicsList.ToArray();
            GraphicsList.Clear();
            if(result.Length != 0)
                Debug.Log($"[{this.GetType().Name}.PaintResult] Result Graphics2D Count: {result.Length}"
                          + $"\n is Can Be Selected:{string.Join(",", result.Select(g => g.CanBeSelected))}"
                          + $"\nColor: {result[0].Color}"
                          + $"\nBrushSize: {result[0].BrushSize}");

            return result.Length > 0 ? result : null;
        }
    }

    protected void PaintLine(Vector2 start, Vector2 end)
    {
        Vector2[] segment = CanvasPixelPainter.Instance.LinePaint(start, end, PaintColor, BrushSize);
        Graphics2D.AddSegment(segment);
        Debug.Log($"[{this.GetType().Name}.PaintLine] Segments Count: {Graphics2D.Segments.Count}"
                  // + $"\n Add Segment:\n{string.Join(",", segment.Select(s => s.ToString()))}"
                  );
    }

    public void SetPoint(Vector2 point)
    {
        if (Graphics2D == null)
        {
            Graphics2D = new Graphics2D(new List<Vector2>(), new List<Vector2[]>(),
                PaintColor, BrushSize, true, Vector2.zero);
        }

        bool isEnd = SetPointRealize(point);

        Graphics2D.AddPoint(point);
        CanvasPixelPainter.Instance.PointPaint(point, PaintColor, BrushSize, true);

        if (isEnd)
            EndPaint();
    }

    public void EndPaint()
    {
        if (Graphics2D == null)
            return;

        EndPaintRealize();

        GraphicsList.Add(Graphics2D);
        Debug.Log($"[{this.GetType().Name}.EndPaint] EndPaint, Add a new Graphics2D to List!");

        point0 = point1 = Vector2.zero;
        Graphics2D = null;
    }

    protected abstract bool SetPointRealize(Vector2 point);

    protected virtual void EndPaintRealize(){}
}
