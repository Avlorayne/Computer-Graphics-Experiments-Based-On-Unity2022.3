using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Exp3_Controller : Control
{
    [Header("图形绘制")]
    public LinePainter linePainter = new LinePainter();
    public ClosedFigure closedFigure = new ClosedFigure();
    public Rectangle rectangle = new Rectangle();

    [Header("交互按键")] public GameObject TransformScroll;
    void Start()
    {
        Init();

        Button btn  = CreateButton("OverPaint");
        btn.onClick.AddListener(() =>
        {
            if (SelectedGraphics == null)
                return;

            SelectedGraphics.ClearOnCanvas();
            Debug.Log($"[Exp3_Controller.OverPaint]\nHave Cleared Selected Graphics");

            IEnumerator Wait(float duration)
            {
                yield return new WaitForSeconds(duration);
                SelectedGraphics.OverPaint();
                SelectGraphicByGraphics(SelectedGraphics);
            }

            StartCoroutine(Wait(1));
            // SelectedGraphics.OverPaint();
        });

        AddToScrollRect(TransformScroll);
        SelectGraphicAction += BindNewGraphic;

        AddPainter(linePainter);
        AddPainter(closedFigure);
        AddPainter(rectangle);
    }

    void BindNewGraphic()
    {
        Debug.Log($"[Exp3_Controller.BindNewGraphic]\nStart to Bind New Graphic");
        TransformBind2D.Instance.SetTransformListen(SelectedGraphics);
    }
}
