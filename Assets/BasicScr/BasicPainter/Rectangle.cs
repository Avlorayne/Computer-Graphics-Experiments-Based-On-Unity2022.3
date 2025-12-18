using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Rectangle : Painter
{
    protected override bool SetPointRealize(Vector2 point)
    {
        if (point0 == Vector2.zero)
        {
            point0 = point;
        }
        else if(point1 == Vector2.zero)
        {
            point1 = point;
            return true;
        }
        else
        {
            Debug.LogError($"[Rectangle.SetPointRealize] 画线结点赋值顺序出错。Point0 {point0}, Point1 {point1}");
        }
        return false;
    }

    protected override void EndPaintRealize()
    {
        Vector2[] points = {point0, new (point0.x, point1.y), point1, new (point1.x, point0.y)}; // 按照绘制连接顺序
        Debug.Log($"[Rectangle.EndPaintRealize] Rectangle NodePoints {points[0]}, {points[1]}, {points[2]}, {points[3]}");

        for(int i = 0; i < points.Length-1; i++)
        {
            PaintLine(points[i], points[i+1]);
        }
        PaintLine(points[3], points[0]);
    }
}
