using System.Linq;
using UnityEngine;

namespace LineClip
{
public static class CohenSutherland
{
    private const int INSIDE = 0; // 0000
    private const int LEFT = 1;   // 0001
    private const int RIGHT = 2;  // 0010
    private const int BOTTOM = 4; // 0100
    private const int TOP = 8;    // 1000

    // 计算点相对于裁剪窗口的区域码
    private static int ComputeOutCode(Vector2 point, Vector2 windowMin, Vector2 windowMax)
    {
        int code = INSIDE;

        if (point.x < windowMin.x) code |= LEFT;
        else if (point.x > windowMax.x) code |= RIGHT;
        if (point.y < windowMin.y) code |= BOTTOM;
        else if (point.y > windowMax.y) code |= TOP;

        return code;
    }

    // Cohen-Sutherland 裁剪算法：裁剪线段并返回是否可见及裁剪后端点
    public static bool Clip(MyGraphics2D line, MyGraphics2D rectangle)
    {
        Vector2 p0 = line.NodePoints.First();
        Vector2 p1 = line.NodePoints.Last();
        if (p1 == p0 || p1 == Vector2.zero || p0 == Vector2.zero)
        {
            Debug.LogError($"[Exp3_Controller.CohenSutherlandClip]\nPoints of Selected Line are the same or Error: \n{p0} {p1}");
            return false;
        }
        Vector2 windowMin = rectangle.NodePoints.First();
        Vector2 windowMax = rectangle.NodePoints.Last();
        if (windowMin == windowMax || windowMax == Vector2.zero || windowMin == Vector2.zero)
        {
            Debug.LogError($"[Exp3_Controller.CohenSutherlandClip]\nPoints of Selected Rect are the same or Error: \n{windowMin} {windowMax}");
            return false;
        }

        if (windowMin.x > windowMax.x)
        {
            (windowMin.x, windowMax.x) = (windowMax.x, windowMin.x);
        }

        if (windowMin.y > windowMax.y)
        {
            (windowMin.y, windowMax.y) = (windowMax.y, windowMin.y);
        }

        int outcode0 = ComputeOutCode(p0, windowMin, windowMax);
        int outcode1 = ComputeOutCode(p1, windowMin, windowMax);
        float k = (p1.x - p0.x) / (p1.y - p0.y);
        float m = (p1.y - p0.y) / (p1.x - p0.x);
        bool accept = false;

        while (true)
        {
            if ((outcode0 | outcode1) == 0) // 完全在窗口内
            {
                accept = true;
                break;
            }
            else if ((outcode0 & outcode1) != 0) // 完全在窗口外
            {
                break;
            }
            else
            {
                // 至少有一个端点在窗口外，选一个来裁剪
                int outcodeOut = (outcode0 != 0) ? outcode0 : outcode1;
                Vector2 p = new Vector2();

                if ((outcodeOut & TOP) != 0)
                {
                    p.x = p0.x +  k * (windowMax.y - p0.y);
                    p.y = windowMax.y;
                }
                else if ((outcodeOut & BOTTOM) != 0)
                {
                    p.x = p0.x + k * (windowMin.y - p0.y);
                    p.y = windowMin.y;
                }
                else if ((outcodeOut & RIGHT) != 0)
                {
                    p.y = p0.y + m * (windowMax.x - p0.x);
                    p.x = windowMax.x;
                }
                else if ((outcodeOut & LEFT) != 0)
                {
                    p.y = p0.y + m * (windowMin.x - p0.x);
                    p.x = windowMin.x;
                }

                if (outcodeOut == outcode0)
                {
                    p0 = p;
                    outcode0 = ComputeOutCode(p0, windowMin, windowMax);
                }
                else
                {
                    p1 = p;
                    outcode1 = ComputeOutCode(p1, windowMin, windowMax);
                }
            }
        }
        Debug.Log($"[CohenSutherland.Clip]\nLine Points after Clip: {p0} {p1}");
        line.NodePoints.Clear();
        line.NodePoints.Add(p0);
        line.NodePoints.Add(p1);
        line.OverPaint();
        return accept;
    }
}

public static class MidpointSubdivision
{
    // 区域码定义
    private const int INSIDE = 0;
    private const int LEFT = 1;
    private const int RIGHT = 2;
    private const int BOTTOM = 4;
    private const int TOP = 8;

    // 计算区域码
    private static int ComputeCode(Vector2 p, Rect rect)
    {
        int code = INSIDE;
        if (p.x < rect.xMin) code |= LEFT;
        if (p.x > rect.xMax) code |= RIGHT;
        if (p.y < rect.yMin) code |= BOTTOM;
        if (p.y > rect.yMax) code |= TOP;
        return code;
    }

    // 中点分割算法主函数
    public static bool Clip(MyGraphics2D line, MyGraphics2D rectObj)
    {
        // 获取线段端点
        Vector2 p1 = line.NodePoints[0];
        Vector2 p2 = line.NodePoints[1];

        // 获取裁剪窗口
        Vector2 rectMin = rectObj.NodePoints[0];
        Vector2 rectMax = rectObj.NodePoints[1];
        Rect clipRect = new Rect(
            Mathf.Min(rectMin.x, rectMax.x),
            Mathf.Min(rectMin.y, rectMax.y),
            Mathf.Abs(rectMax.x - rectMin.x),
            Mathf.Abs(rectMax.y - rectMin.y)
        );

        // 计算区域码
        int code1 = ComputeCode(p1, clipRect);
        int code2 = ComputeCode(p2, clipRect);

        // 完全在窗口外
        if ((code1 & code2) != 0)
        {
            Debug.Log("线段完全在窗口外");
            return false;
        }

        // 完全在窗口内
        if ((code1 | code2) == 0)
        {
            Debug.Log("线段完全在窗口内");
            return true;
        }

        if (code1 != 0)
        {
            // 中点分割：找到离p1最近的可见点
            Vector2 visibleP1 = FindVisiblePoint(p1, p2, clipRect, code1, true);
            // 更新线段
            line.NodePoints[0] = visibleP1;
        }

        if (code2 != 0)
        {
            // 中点分割：找到离p2最近的可见点
            Vector2 visibleP2 = FindVisiblePoint(p1, p2, clipRect, code2, false);
            // 更新线段
            line.NodePoints[1] = visibleP2;
        }
        line.OverPaint();

        Debug.Log($"裁剪后：{line.NodePoints[0]} -> {line.NodePoints[1]}");
        return true;
    }

    // 寻找可见点（核心算法）
    private static Vector2 FindVisiblePoint(Vector2 p1, Vector2 p2, Rect rect, int outcode, bool findFromP1)
    {
        Vector2 start = findFromP1 ? p1 : p2;
        Vector2 end = findFromP1 ? p2 : p1;

        int depth = 0;
        const int maxDepth = 16; // 防止无限递归

        while (depth++ < maxDepth)
        {
            Vector2 mid = (start + end) / 2;

            // 检查是否足够接近
            if (Vector2.Distance(start, end) < 0.5f)
                return mid;

            int midCode = ComputeCode(mid, rect);

            // 中点在窗口内
            if (midCode == 0)
            {
                end = mid;
                // if (findFromP1)
                //     end = mid;    // 向p1方向靠近
                // else
                //     start = mid;   // 向p2方向靠近
            }
            // 中点和起点在同一外侧区域
            else if ((outcode & midCode) != 0)
            {
                start = mid;
                // if (findFromP1)
                //     start = mid;   // 向中点移动
                // else
                //     end = mid;     // 向中点移动
            }
            // 中点和起点在不同外侧区域
            else
            {
                end = mid;
                // if (findFromP1)
                //     end = mid;     // 向窗口方向移动
                // else
                //     start = mid;    // 向窗口方向移动
            }
        }

        Debug.LogWarning($"达到最大递归深度 {maxDepth}，返回中点");
        return (start + end) / 2;
    }
}
}
