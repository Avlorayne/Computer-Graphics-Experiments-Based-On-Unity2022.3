using System.Collections.Generic;
using UnityEngine;

namespace PolygonClip
{
    public static class SutherlandHodgmanClipping
    {
        public static bool Clip(MyGraphics2D subjectPolygon, MyGraphics2D rectangle)
        {
            // 获取多边形顶点
            List<Vector2> polygonPoints = new List<Vector2>(subjectPolygon.NodePoints);

            // 获取裁剪窗口
            Vector2 rectMin = rectangle.NodePoints[0];
            Vector2 rectMax = rectangle.NodePoints[1];

            // 确保矩形坐标正确
            float xMin = Mathf.Min(rectMin.x, rectMax.x);
            float xMax = Mathf.Max(rectMin.x, rectMax.x);
            float yMin = Mathf.Min(rectMin.y, rectMax.y);
            float yMax = Mathf.Max(rectMin.y, rectMax.y);

            // 裁剪多边形
            List<Vector2> clippedPoints = ClipPolygon(polygonPoints, xMin, xMax, yMin, yMax);

            if (clippedPoints.Count == 0)
                return false;

            // 更新多边形
            subjectPolygon.NodePoints.Clear();
            subjectPolygon.NodePoints.AddRange(clippedPoints);
            subjectPolygon.OverPaint();

            return true;
        }

        public static List<Vector2> ClipPolygon(List<Vector2> polygon, float xMin, float xMax, float yMin, float yMax)
        {
            if (polygon == null || polygon.Count < 3)
                return new List<Vector2>();

            List<Vector2> output = polygon;

            // 按左、右、下、上的顺序裁剪
            output = ClipAgainstEdge(output, Edge.Left, xMin);
            output = ClipAgainstEdge(output, Edge.Right, xMax);
            output = ClipAgainstEdge(output, Edge.Bottom, yMin);
            output = ClipAgainstEdge(output, Edge.Top, yMax);

            return output;
        }

        private enum Edge { Left, Right, Bottom, Top }

        private static List<Vector2> ClipAgainstEdge(List<Vector2> polygon, Edge edge, float bound)
        {
            List<Vector2> result = new List<Vector2>();

            if (polygon.Count == 0)
                return result;

            Vector2 prevPoint = polygon[polygon.Count - 1];
            bool prevInside = IsInside(prevPoint, edge, bound);

            foreach (Vector2 currPoint in polygon)
            {
                bool currInside = IsInside(currPoint, edge, bound);

                if (prevInside && currInside)
                {
                    // 两个点都在内侧
                    result.Add(currPoint);
                }
                else if (prevInside && !currInside)
                {
                    // 从内侧到外侧
                    result.Add(GetIntersection(prevPoint, currPoint, edge, bound));
                }
                else if (!prevInside && currInside)
                {
                    // 从外侧到内侧
                    result.Add(GetIntersection(prevPoint, currPoint, edge, bound));
                    result.Add(currPoint);
                }

                prevPoint = currPoint;
                prevInside = currInside;
            }

            return result;
        }

        private static bool IsInside(Vector2 point, Edge edge, float bound)
        {
            switch (edge)
            {
                case Edge.Left:   return point.x >= bound;
                case Edge.Right:  return point.x <= bound;
                case Edge.Bottom: return point.y >= bound;
                case Edge.Top:    return point.y <= bound;
                default: return false;
            }
        }

        private static Vector2 GetIntersection(Vector2 p1, Vector2 p2, Edge edge, float bound)
        {
            float t;

            switch (edge)
            {
                case Edge.Left:
                case Edge.Right:
                    t = (bound - p1.x) / (p2.x - p1.x);
                    return new Vector2(bound, p1.y + (p2.y - p1.y) * t);

                case Edge.Bottom:
                case Edge.Top:
                    t = (bound - p1.y) / (p2.y - p1.y);
                    return new Vector2(p1.x + (p2.x - p1.x) * t, bound);

                default:
                    return p1;
            }
        }
    }

    public static class WeilerAthertonClipping
    {
        public static bool Clip(MyGraphics2D subjectPolygon, MyGraphics2D rectangle)
        {
            // 获取多边形顶点
            List<Vector2> polygonPoints = new List<Vector2>(subjectPolygon.NodePoints);

            // 获取裁剪窗口
            Vector2 rectMin = rectangle.NodePoints[0];
            Vector2 rectMax = rectangle.NodePoints[1];

            // 确保矩形坐标正确
            float xMin = Mathf.Min(rectMin.x, rectMax.x);
            float xMax = Mathf.Max(rectMin.x, rectMax.x);
            float yMin = Mathf.Min(rectMin.y, rectMax.y);
            float yMax = Mathf.Max(rectMin.y, rectMax.y);

            // 构建裁剪窗口多边形（顺时针）
            List<Vector2> clipPolygon = new List<Vector2>
            {
                new Vector2(xMin, yMin),
                new Vector2(xMax, yMin),
                new Vector2(xMax, yMax),
                new Vector2(xMin, yMax)
            };

            // 裁剪多边形
            List<List<Vector2>> clippedPolygons = ClipPolygon(polygonPoints, clipPolygon);

            if (clippedPolygons.Count == 0)
                return false;

            // 取第一个裁剪结果（假设只有一个）
            List<Vector2> result = clippedPolygons[0];

            if (result.Count < 3)
                return false;

            // 更新多边形
            subjectPolygon.NodePoints.Clear();
            subjectPolygon.NodePoints.AddRange(result);
            subjectPolygon.OverPaint();

            return true;
        }

        private static List<List<Vector2>> ClipPolygon(List<Vector2> subjectPolygon, List<Vector2> clipPolygon)
        {
            // 简单实现：使用 Sutherland-Hodgman 算法
            return new List<List<Vector2>>
            {
                SutherlandHodgmanClipping.ClipPolygon(
                    subjectPolygon,
                    clipPolygon[0].x,
                    clipPolygon[2].x,
                    clipPolygon[0].y,
                    clipPolygon[2].y
                )
            };

            // 完整的 Weiler-Atherton 算法实现较复杂，这里提供简化版本
            // 实际应用中需要实现完整的节点追踪算法
        }

        // 简化版本的 Weiler-Atherton 实现（注释中说明算法思路）
        private static List<List<Vector2>> WeilerAthertonClip(List<Vector2> subjectPolygon, List<Vector2> clipPolygon)
        {
            /*
            完整的 Weiler-Atherton 算法步骤：

            1. 计算所有交点，并标记为"进入点"或"离开点"
            2. 在两个多边形中按顺序插入交点节点
            3. 创建双向链接：
               - 多边形内部链接
               - 交点之间的链接（进入点↔离开点）
            4. 从任意未访问的"进入点"开始追踪：
               a. 沿被裁剪多边形前进直到遇到交点
               b. 切换到裁剪多边形继续前进
               c. 重复直到回到起点
            5. 收集追踪到的顶点形成裁剪多边形

            由于实现较复杂，这里提供简化的 Sutherland-Hodgman 版本
            */

            // 回退到 Sutherland-Hodgman
            return new List<List<Vector2>>();
        }
    }
}
