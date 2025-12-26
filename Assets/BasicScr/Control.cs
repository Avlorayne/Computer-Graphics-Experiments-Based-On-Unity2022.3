using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Control : MonoBehaviour
{
    [Header("使用图形组件")]
    public RawImage rawImage;
    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;

    [Header("交互按键")]
    public GameObject button;
    public Vector2 buttonSize = new Vector2(100, -100);
    public ScrollRect ScrollRect;
    public List<Button> buttons = new List<Button>();

    private List<Painter> painters = new List<Painter>();
    protected Action<Vector2> DrawMode; // 当前的绘制模式

    protected List<MyGraphics2D> graphicsList = new List<MyGraphics2D>();

    protected MyGraphics2D SelectedGraphics;
    protected Action SelectGraphicAction;

    bool isMouseBlocked
    {
        get
        {
            // 进行射线检测
            PointerEventData pointerData = new PointerEventData(eventSystem);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);

            bool isBlocked = false;

            // 检查是否有 UI 元素阻挡
            foreach (RaycastResult result in results)
            {
                // 如果点击到了按钮或其他 UI 元素（除了目标 RawImage）
                if (result.gameObject != rawImage.gameObject &&
                    result.gameObject != this.gameObject) // 排除自身
                {
                    isBlocked = true;
                    Debug.Log($"[Control.isMouseBlocked]\n鼠标射线被 {result.gameObject.name} 阻挡");
                    break;
                }
            }

            return isBlocked;
        }
    }

    void Start()
    {
        // Init();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isMouseBlocked)
        {
            if (DrawMode != null)
                Draw();
        }
    }

    void Draw()
    {
        Vector2 mousePos =Input.mousePosition;
        DrawMode(mousePos);
        // Debug.Log($"[Control.Draw]\nSet Point {mousePos}");
    }

    protected void Init()
    {
        // 一次性注册所有Painter子类
        PainterFactory.AutoRegisterAllPainters();

        Debug.Log(CanvasPixelPainter.Instance != null
            ? "[Control.Init]\nManaged to Find CanvasPixelPainter"
            : "[Control.Init]\nFailed to Find CanvasPixelPainter");

        if (rawImage == null)
        {
            rawImage = GetComponentInChildren<RawImage>();
        }
        if(raycaster == null)
        {
            raycaster = GetComponent<GraphicRaycaster>();
        }
        if (eventSystem == null)
        {
            eventSystem = FindAnyObjectByType<EventSystem>();
        }

        if (ScrollRect == null)
        {
            ScrollRect = GetComponentInChildren<ScrollRect>();
        }

        buttons.Clear();

            // Button btn1 = CreateButton("End Paint");
            // // 增添当前完成的图形

        Button btn2 = CreateButton("Clear");
        btn2.onClick.AddListener(() =>
        {
            SelectedGraphics = null;
            graphicsList.Clear();
            CanvasPixelPainter.Instance.Clear();
            Debug.Log($"[Control.Button(Clear)]\nthe MyGraphics2D Clear. Existing MyGraphics2D count: {graphicsList.Count}");
        });

        // Button btn3 = CreateButton("Null");
        // btn3.onClick.AddListener(() =>
        // {
        //     DrawMode =  null;
        //     SelectedGraphics = null;
        //     Debug.Log($"[Control.Button(Null)]\nDrawMode is Null");
        // });

        SelectGraphicAction += SelectGraphic;
        Button btn4 = CreateButton("Select Graphic");
        btn4.onClick.AddListener(() =>
        {
            Debug.Log($"[Control.Button(Select Graphic)]\n");
            SelectGraphicAction();
        });
    }

    protected void SelectGraphic(MyGraphics2D graphics)
    {
        if (graphics == null)
        {
            Debug.LogError("[Control.SelectGraphic(null)]");
            return;
        }
        if(graphicsList.Count == 0)
            return;
        if (!graphicsList.Contains(graphics))
        {
            Debug.LogError("[Control.SelectGraphic]\nThe graphics list doesn't contain graphics");
            return;
        }
        DrawMode = null;
        SelectedGraphics?.Deselect();

        SelectedGraphics = graphics;
        SelectedGraphics?.Select();
        Debug.Log($"[Control.SelectGraphic]\nSelected graphic2D");
    }

    protected void SelectGraphic()
    {
        DrawMode = null;

        SelectedGraphics?.Deselect();
        // Texture2D texture = GetComponentInChildren<RawImage>().texture as Texture2D;
        // if (texture == null)
        // {
        //     Debug.Log($"[Control.SelectGraphic] Texture is not found");
        // }
        // SelectedGraphics = GraphicsSelector.Select(Input.mousePosition, texture, graphicsList);

        if(graphicsList.Count == 0)
            return;

        if (SelectedGraphics != null)
        {
            if (graphicsList.Contains(SelectedGraphics))
            {
                int i = graphicsList.IndexOf(SelectedGraphics) + 1;
                if (i >= 0 && i < graphicsList.Count)
                {
                    SelectedGraphics = graphicsList[i];
                    Debug.Log($"[Control.SelectGraphic]\nselected graphic2D[{i}]");
                }
                else
                {
                    SelectedGraphics = null;
                    Debug.Log($"[Control.SelectGraphic]\nselected graphic2D to Null");
                }
            }
        }
        else
        {
            SelectedGraphics = graphicsList[0];
            Debug.Log($"[Control.SelectGraphic]\nselected graphic2D[0]");
        }
        SelectedGraphics?.Select();
    }

    protected void AddPainter(Painter painter)
    {
        painters.Add(painter);
        Button btn = CreateButton(painter.GetType().Name);

        // 绑定按钮点击事件
        btn.onClick.AddListener(() =>
        {
            DrawMode = painter.SetPoint;
            Debug.Log($"[Control.Button]\nDrawMode Changed: {DrawMode.Target.GetType().Name}->{DrawMode.Method.Name}");
        });
        Debug.Log($"[Control.Button]\nAdd Button onClick Event: {painter.GetType().Name}->{nameof(painter.SetPoint)}");
    }

    // 修改 CreateButton 函数
    protected Button CreateButton(string buttonName)
    {
        GameObject btnGo = AddToScrollRect(button);

        Button btn = btnGo.GetComponent<Button>();
        btnGo.GetComponentInChildren<TextMeshProUGUI>().text = buttonName;

        buttons.Add(btn);
        btn.onClick.AddListener(EndPaint);
        Debug.Log($"[Control.CreateButton]\nAdd Button Bind with {buttonName}");

        return btn;
    }

    protected void EndPaint()
    {
        if(DrawMode?.Target is Painter painter &&
           graphicsList != null &&
           painter.PaintResult is { } result)
        {
            var resultList = result.ToList();
            if (resultList.Count > 0)
            {
                graphicsList.AddRange(resultList);
                Debug.Log($"[Control.Button(End Paint)]\n{painter.GetType().Name}'s MyGraphics2D Added, count: {resultList.Count}" +
                          $"\nNow the Existing MyGraphics2D count: {graphicsList.Count}");
            }
            else
            {
                Debug.LogWarning($"[Control.Button(End Paint)]\nNo myGraphics2D to add from {painter.GetType().Name}");
            }
        }
    }

    protected GameObject AddToScrollRect(GameObject go)
    {
        RectTransform rect = ScrollRect.content;
        GameObject instantiate = Instantiate(go, rect);
        return instantiate;
    }
}
