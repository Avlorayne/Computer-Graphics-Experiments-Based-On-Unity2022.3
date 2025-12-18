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

    protected List<Graphics2D> graphicsList = new List<Graphics2D>();

    protected Graphics2D selectedGraphic2D;

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
                    Debug.Log($"[Control.isMouseBlocked] 鼠标射线被 {result.gameObject.name} 阻挡");
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
        Debug.Log($"[Control.Draw] Set Point {mousePos}");
    }

    protected void Init()
    {
        if (CanvasPixelPainter.Instance != null)
        {
            Debug.Log("[Control.Init] Managed to Find CanvasPixelPainter");
        }
        else
        {
            Debug.Log("[Control.Init] Failed to Find CanvasPixelPainter");
        }

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

        Button btn1 = CreateButton("End Paint");
        // 增添当前完成的图形
        btn1.onClick.AddListener(() =>
        {
            if(DrawMode?.Target is Painter painter &&
               graphicsList != null &&
               painter.PaintResult is { } result)
            {
                var resultList = result.ToList();
                if (resultList.Count > 0)
                {
                    graphicsList.AddRange(resultList);
                    Debug.Log($"[Control.Button(End Paint)] {painter.GetType().Name}'s Graphics2D Added, count: {resultList.Count}" +
                              $"\nNow the Existing Graphics2D count: {graphicsList.Count}");
                }
                else
                {
                    Debug.LogWarning($"[Control.Button(End Paint)] No graphics2D to add from {painter.GetType().Name}");
                }
            }
        });

        Button btn2 = CreateButton("Clear");
        btn2.onClick.AddListener(() =>
        {
            graphicsList.Clear();
            CanvasPixelPainter.Instance.Clear();
            Debug.Log($"[Control.Button(Clear)] the Graphics2D Clear. Existing Graphics2D count: {graphicsList.Count}");
        });

        Button btn3 = CreateButton("Null");
        btn3.onClick.AddListener(() =>
        {
            DrawMode =  null;
            selectedGraphic2D = null;
            Debug.Log($"[Control.Button(Null)] DrawMode is Null");
        });

        Button btn4 = CreateButton("Select Graphic");
        btn4.onClick.AddListener(() =>
        {
            DrawMode = null;
            SelectGraphic();
            Debug.Log($"[Control.Button(Select Graphic)]");
        });
    }

    void SelectGraphic()
    {
        selectedGraphic2D?.Deselect();
        // Texture2D texture = GetComponentInChildren<RawImage>().texture as Texture2D;
        // if (texture == null)
        // {
        //     Debug.Log($"[Control.SelectGraphic] Texture is not found");
        // }
        // selectedGraphic2D = GraphicsSelector.Select(Input.mousePosition, texture, graphicsList);

        if(graphicsList.Count == 0)
            return;

        if (selectedGraphic2D != null)
        {
            if (graphicsList.Contains(selectedGraphic2D))
            {
                int index = graphicsList.IndexOf(selectedGraphic2D);
                var i = ++index;
                if (i >= 0 && i < graphicsList.Count)
                {
                    selectedGraphic2D = graphicsList[i];
                    Debug.Log($"[Control.SelectGraphic] selected graphic2D[{i}]");
                }
                else
                {
                    selectedGraphic2D = null;
                    Debug.Log($"[Control.SelectGraphic] selected graphic2D to Null");
                }
            }
        }
        else
        {
            selectedGraphic2D = graphicsList[0];
            Debug.Log($"[Control.SelectGraphic] selected graphic2D[0]");
        }
        selectedGraphic2D?.Select();
    }

    protected void AddPainter(Painter painter)
    {
        painters.Add(painter);
        Button btn = CreateButton(painter.GetType().Name);

        // 绑定按钮点击事件
        btn.onClick.AddListener(() =>
        {
            DrawMode = painter.SetPoint;
            Debug.Log($"[Control.Button] DrawMode Changed: {DrawMode.Target.GetType().Name}->{DrawMode.Method.Name}");
        });
        Debug.Log($"[Control.Button] Button onClick Event: {painter.GetType().Name}->{nameof(painter.SetPoint)}");
    }

    // 修改 CreateButton 函数
    protected Button CreateButton(string buttonName)
    {
        RectTransform rect = ScrollRect.content;
        GameObject btnGo = Instantiate(button, rect);

        Button btn = btnGo.GetComponent<Button>();
        btnGo.GetComponentInChildren<TextMeshProUGUI>().text = buttonName;

        buttons.Add(btn);
        Debug.Log($"[Control.CreateButton] Add Button Bind with {buttonName}");

        return btn;
    }
}
