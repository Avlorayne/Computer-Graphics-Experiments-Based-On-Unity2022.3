
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphicsSelector
{
    public static Graphics2D Select(Vector2 localPos, Texture2D texture, List<Graphics2D> graphicsList, int tolerance = 2)
    {
        int x = Mathf.RoundToInt(localPos.x);
        int y = Mathf.RoundToInt(localPos.y);

        if (texture == null)
        {
            Debug.Log($"[GraphicsSelector.Select] Failed to Find Texture2D");
            return null;
        }

        if (!(x >= 0 && x < texture.width && y >= 0 && y < texture.height))
            return null;

        // 获取tolerance范围内的所有像素颜色
        Color[] pixels = (
            from i in Enumerable.Range(-tolerance, 2 * tolerance + 1)
            from j in Enumerable.Range(-tolerance, 2 * tolerance + 1)
            where i * i + j * j <= tolerance * tolerance  // 圆形区域
            select texture.GetPixel(
                Mathf.Clamp(x + i, 0, texture.width - 1),   // 确保不越界
                Mathf.Clamp(y + j, 0, texture.height - 1)   // 确保不越界
            )
        ).ToArray();

        Debug.Log($"[GraphicsSelector.Select] Pixels Count in Tolerance Area: {pixels.Length}");

        // 判断区域内是否都是白色（使用阈值判断）
        if (pixels.All(p => IsWhiteColor(p)))
        {
            Debug.Log($"[GraphicsSelector.Select] All pixels are white in tolerance area");
            return null;
        }

        // 从区域内像素中提取所有非白色的颜色
        var nonWhitePixels = pixels.Where(p => !IsWhiteColor(p)).Distinct().ToList();
        Debug.Log($"[GraphicsSelector.Select] Non-white colors found: {nonWhitePixels.Count}");

        // 找出可能匹配的图形
        var potentialGraphics = new List<Graphics2D>();
        foreach (var graphic in graphicsList)
        {
            // 检查区域内是否有像素颜色与图形颜色匹配
            bool hasMatchingColor = nonWhitePixels.Any(pixel =>
                ColorsAreSimilar(pixel, graphic.Color, 0.1f));

            if (hasMatchingColor)
            {
                potentialGraphics.Add(graphic);
            }
        }

        Debug.Log($"[GraphicsSelector.Select] Find Potential Graphics: {potentialGraphics.Count}");

        // 如果没有匹配颜色的图形，返回null
        if (potentialGraphics.Count == 0)
            return null;

        // 只在可能有匹配的图形中检测线段点击
        foreach (var graphic in potentialGraphics.Where(graphic => graphic.CanBeSelected))
        {
            // 检查是否点击在线段上
            for (int i = 0; i < graphic.NodePoints.Count - 1; i++)
            {
                if (IsPointOnLine(localPos,
                        graphic.NodePoints[i],
                        graphic.NodePoints[i + 1],
                        tolerance))
                {
                    Debug.Log($"[GraphicsSelector.Select] Select Graphics on Mouse Position: {localPos}");
                    return graphic;
                }
            }
        }

        return null;
    }

    // 辅助方法：判断是否为白色
    private static bool IsWhiteColor(Color color, float threshold = 0.95f)
    {
        return color.r >= threshold &&
               color.g >= threshold &&
               color.b >= threshold;
    }

    // 辅助方法：判断两个颜色是否相似
    private static bool ColorsAreSimilar(Color color1, Color color2, float threshold = 0.1f)
    {
        return Mathf.Abs(color1.r - color2.r) < threshold &&
               Mathf.Abs(color1.g - color2.g) < threshold &&
               Mathf.Abs(color1.b - color2.b) < threshold;
    }

    /// <summary>
    /// 判断点是否在直线上（考虑容差）
    /// </summary>
    /// <param name="point">要判断的点</param>
    /// <param name="lineStart">直线起点</param>
    /// <param name="lineEnd">直线终点</param>
    /// <param name="tolerance">容差范围</param>
    /// <returns>是否在线上</returns>
    public static bool IsPointOnLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd, int tolerance = 1)
    {
        // 计算线段方向
        Vector2 lineDirection = lineEnd - lineStart;
        float lineLength = lineDirection.magnitude;

        // 如果线段长度为零，检查点是否与端点重合
        if (lineLength < Mathf.Epsilon)
        {
            return Vector2.Distance(point, lineStart) < tolerance;
        }

        // 计算点到线段起点的向量
        Vector2 pointDirection = point - lineStart;

        // 计算投影长度（参数t）
        float t = Vector2.Dot(pointDirection, lineDirection.normalized);

        // 如果投影点在线段之外
        if (t < -tolerance || t > lineLength + tolerance)
        {
            return false;
        }

        // 计算投影点
        Vector2 projection = lineStart + lineDirection.normalized * t;

        // 计算点到投影点的距离
        float distance = Vector2.Distance(point, projection);

        // 判断距离是否在容差范围内
        return distance <= tolerance;
    }

    /// <summary>
    /// 使用射线法判断点是否在多边形内部
    /// </summary>
    public static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        if (polygon == null || polygon.Count < 3)
            return false;

        bool inside = false;
        int n = polygon.Count;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            Vector2 pi = polygon[i];
            Vector2 pj = polygon[j];

            // 检查点是否在边上
            if (IsPointOnLine(point, pi, pj))
            {
                return true; // 点在边上
            }

            // 射线法判断
            if (((pi.y > point.y) != (pj.y > point.y)) &&
                (point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y) + pi.x))
            {
                inside = !inside;
            }
        }

        return inside;
    }

    /// <summary>
    /// 改进的射线法，处理特殊情况
    /// </summary>
    public static bool IsPointInPolygonRobust(Vector2 point, List<Vector2> polygon, float epsilon = 1e-7f)
    {
        if (polygon == null || polygon.Count < 3)
            return false;

        bool inside = false;
        int n = polygon.Count;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[j];

            // 检查点是否在边上
            if (IsPointOnLine(point, a, b))
            {
                return true;
            }

            // 处理水平边的情况
            if (Mathf.Abs(a.y - b.y) < epsilon)
            {
                continue;
            }

            // 检查射线与边的交点
            float intersectX = (point.y - a.y) * (b.x - a.x) / (b.y - a.y) + a.x;

            // 检查交点是否在点的右侧
            if (point.x < intersectX - epsilon)
            {
                // 检查射线是否穿过顶点
                if ((a.y > point.y) != (b.y > point.y))
                {
                    inside = !inside;
                }
            }
        }
        return inside;
    }
}
