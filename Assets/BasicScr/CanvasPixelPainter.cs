using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class CanvasPixelPainter : MonoBehaviour
{
    private static CanvasPixelPainter _instance;

    public static CanvasPixelPainter Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<CanvasPixelPainter>();
                if (_instance == null)
                {
                    _instance = new GameObject("CanvasPixelPainter").AddComponent<CanvasPixelPainter>();
                }
            }
            return _instance;
        }
    }

    public RawImage canvas;
    public Color PaintColorDefault = Color.black;
    public int BrushSizeDefault = 1;

    List<Vector2> Segment = new List<Vector2>();

    int width;
    int height;

    private Texture2D texture;
    private Color[] clearPixels;
    private RectTransform canvasRect;

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        if(canvas == null)
            canvas = gameObject.GetComponent<RawImage>();

        canvasRect = canvas.rectTransform;
        width = (int)canvas.rectTransform.rect.width;
        height = (int)canvas.rectTransform.rect.height;
        // 创建纹理
        texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;

        // 初始化透明背景
        clearPixels = new Color[width * height];
        for (int i = 0; i < clearPixels.Length; i++)
            clearPixels[i] = Color.white;

        Debug.Log($"[CanvasPixelPainter.Start] Create Texture2D: width {width} height {height}");

        Clear();
        canvas.texture = texture;
    }

    void Update()
    {
        // if (Input.GetMouseButtonDown(0))
        // {
        //     Vector2  mousePos = Input.mousePosition;
        //     PointPaint(mousePos, Color.black, 5);
        // }
    }

    public void PointPaint(Vector2 localPos, Color color, int brushSize = 5,  bool applyImmediately = false)
    {
        int x = Mathf.RoundToInt(localPos.x);
        int y = Mathf.RoundToInt(localPos.y);
        // Debug.Log($"Draw ({x},{y})");

        // 绘制点
        if (!(x >= 0 && x < width && y >= 0 && y < height))
            return;

        for (int i = -brushSize; i <= brushSize; i++)
        {
            for (int j = -brushSize; j <= brushSize; j++)
            {
                int px = x + i;
                int py = y + j;
                if (px >= 0 && px < width && py >= 0 && py < height)
                {
                    texture.SetPixel(px, py, color);
                }
            }
        }

        if (applyImmediately)
            texture.Apply();
        else
            Segment.Add(localPos);
    }

    public Vector2[] LinePaint(Vector2 point0, Vector2 point1, Color newColor = default, int brushSize = 5)
    {
        Segment.Clear();

        Color currentColor = newColor == default ? PaintColorDefault : newColor;
        int currentSize = brushSize == BrushSizeDefault ? BrushSizeDefault : brushSize;

        // 取整坐标
        int x0 = Mathf.RoundToInt(point0.x);
        int y0 = Mathf.RoundToInt(point0.y);
        int x1 = Mathf.RoundToInt(point1.x);
        int y1 = Mathf.RoundToInt(point1.y);

        // Debug.Log($"Draw Line From ({x0},{y0}) to ({x1},{y1})");

        // 计算dx和dy（绝对值）
        int dx = x1 - x0;
        int dy = y1 - y0;

        // 确定步进方向
        int stepX = (dx > 0 ? 1 : -1) * 2 * currentSize;
        int stepY = (dy > 0 ? 1 : -1) * 2 * currentSize;

        // 取绝对值
        dx = Mathf.Abs(dx);
        dy = Mathf.Abs(dy);

        // 判断斜率，决定主方向
        if (dx > dy)
        {
            // 斜率 |k| < 1，x为主方向
            int d = 2 * dy - dx;
            int incrE = 2 * dy;        // 选择E点（x+1, y）
            int incrNE = 2 * (dy - dx); // 选择NE点（x+1, y+1）

            int x = x0;
            int y = y0;

            // 绘制起点
            PointPaint(new Vector2(x0, y0), currentColor, currentSize, false);

            // 沿x方向步进
            for (int i = 0; i + currentSize < dx; i += 2 * currentSize)
            {
                x += stepX;

                if (d < 0)
                {
                    // 选择E点
                    d += incrE;
                }
                else
                {
                    // 选择NE点
                    y += stepY;
                    d += incrNE;
                }

                PointPaint(new Vector2(x, y), currentColor, currentSize);
            }

            // 绘制终点
            PointPaint(new Vector2(x1, y1), currentColor,  currentSize);
        }
        else
        {
            // 斜率 |k| >= 1，y为主方向
            int d = 2 * dx - dy;
            int incrN = 2 * dx;        // 选择N点（x, y+1）
            int incrNE = 2 * (dx - dy); // 选择NE点（x+1, y+1）

            int x = x0;
            int y = y0;

            // 绘制起点
            PointPaint(new Vector2(x0, y0), currentColor, currentSize);

            // 沿y方向步进
            for (int i = 0; i + currentSize < dy; i += 2 * currentSize)
            {
                y += stepY;

                if (d < 0)
                {
                    // 选择N点
                    d += incrN;
                }
                else
                {
                    // 选择NE点
                    x += stepX;
                    d += incrNE;
                }

                PointPaint(new Vector2(x, y), currentColor, currentSize);
            }

            // 绘制终点
            PointPaint(new Vector2(x1, y1), currentColor, currentSize);
        }

        texture.Apply();
        return Segment.ToArray();
    }

    public void Clear()
    {
        texture.SetPixels(clearPixels);
        texture.Apply();
    }
}
