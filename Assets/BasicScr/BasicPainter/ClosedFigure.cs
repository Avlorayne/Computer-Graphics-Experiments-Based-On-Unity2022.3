using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public partial class ClosedFigure : Painter
{
    private List<Vector2> points = new List<Vector2>();

    protected override bool SetPointRealize(Vector2 point)
    {
        if (point0 == Vector2.zero)
        {
            point0 = point;
        }
        else if(point1 == Vector2.zero)
        {
            point1 = point;

            PaintLine(point0, point1);

            // 连续点击，保持首尾连续
            point0 = point1;
            point1 = Vector2.zero;
        }
        else
        {
            Debug.LogError($"[ClosedFigure.SetPointRealize] 画线结点赋值顺序出错。Point0 {point0}, Point1 {point1}");
        }
        points.Add(point);

        return false;
    }

    protected override void EndPaintRealize()
    {
        Vector2 first = points.First();
        Vector2 last = points.Last();

        if (first == default(Vector2) || last == default(Vector2))
        {
            Debug.LogWarning($"[ClosedFigure.EndPaintRealize] {first}, {last}");
            return;
        }
        PaintLine(first, last);

        points.Clear();
    }
}
