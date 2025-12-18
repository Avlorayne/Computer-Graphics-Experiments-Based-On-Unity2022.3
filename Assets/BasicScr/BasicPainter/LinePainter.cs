using System;
using UnityEngine;

[Serializable]
public class LinePainter: Painter
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

            PaintLine(point0, point1);

            // 连续点击，保持首尾连续
            point0 = point1;
            point1 = Vector2.zero;
        }
        else
        {
            Debug.LogError($"[LinePainter.SetPointRealize] 画线结点赋值顺序出错。Point0 {point0}, Point1 {point1}");
        }

        return false;
    }
}
