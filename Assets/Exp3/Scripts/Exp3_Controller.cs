using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LineClip;
using PolygonClip;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Exp3_Controller : Control
{
    [Header("图形绘制")]
    public LinePainter linePainter = new LinePainter();
    public Polygon polygon = new Polygon();
    public Rectangle rectangle = new Rectangle();

    [Header("交互按键")] public GameObject TransformScroll;

    [Header("Clip")]
    private MyGraphics2D selected0;
    private MyGraphics2D selected1;

    void Start()
    {
        Init();

        // Button btn  = CreateButton("OverPaint");
        // btn.onClick.AddListener(() =>
        // {
        //     if (SelectedGraphics == null)
        //         return;
        //
        //     SelectedGraphics.ClearOnCanvas();
        //     Debug.Log($"[Exp3_Controller.OverPaint]\nHave Cleared Selected Graphics");
        //
        //     IEnumerator Wait(float duration)
        //     {
        //         yield return new WaitForSeconds(duration);
        //         SelectedGraphics.OverPaint();
        //         SelectGraphic(SelectedGraphics);
        //     }
        //
        //     StartCoroutine(Wait(1));
        //     // SelectedGraphics.OverPaint();
        // });

        AddToScrollRect(TransformScroll);
        SelectGraphicAction += BindNewGraphic;

        AddPainter(linePainter);
        AddPainter(polygon);
        AddPainter(rectangle);

        Button btn1 = CreateButton("Cohen-Sutherland Line Clip");
        btn1.onClick.AddListener(() =>
        {
            LineClipping = CohenSutherland.Clip;
            LineClip();
        });

        Button btn2 = CreateButton("Midpoint Sub Line Clip");
        btn2.onClick.AddListener(() =>
        {
            LineClipping = MidpointSubdivision.Clip;
            LineClip();
        });

        Button btn3 = CreateButton("Sutherland Pol Clip");
        btn3.onClick.AddListener(() =>
        {
            PolygonClipping = SutherlandHodgmanClipping.Clip;
            PolygonClip();
        });

        Button btn4 = CreateButton("Weiler Atherton Pol Clip");
        btn4.onClick.AddListener(() =>
        {
            PolygonClipping = WeilerAthertonClipping.Clip;
            PolygonClip();
        });
    }

    void BindNewGraphic()
    {
        Debug.Log($"[Exp3_Controller.BindNewGraphic]\nStart to Bind New Graphic");
        TransformBind2D.Instance.SetTransformListen(SelectedGraphics);
    }

    private Func<MyGraphics2D, MyGraphics2D, bool> LineClipping = null;
    private Func<MyGraphics2D, MyGraphics2D, bool> PolygonClipping = null;

    void LineClip()
    {
        if (SelectedGraphics != null && selected0 == null)
        {
            selected0 = SelectedGraphics;
            Debug.Log($"[Exp3_Controller.LineClip]\nAdd Line");

            SelectGraphic();
            return;
        }

        if (SelectedGraphics != null && selected1 == null)
        {
            selected1 = SelectedGraphics;
            Debug.Log($"[Exp3_Controller.LineClip]\nAdd Rectangle");

            if(!LineClipping(selected0, selected1))
            {
                selected0.ClearOnCanvas();
                graphicsList.Remove(selected0);
            }

            SelectGraphic();
            selected0 = selected1 = null;
        }
    }

    void PolygonClip()
    {
        if (SelectedGraphics != null && selected0 == null)
        {
            selected0 = SelectedGraphics;
            Debug.Log($"[Exp3_Controller.PolygonClip]\nAdd Polygon");

            SelectGraphic();
            return;
        }

        if (SelectedGraphics != null && selected1 == null)
        {
            selected1 = SelectedGraphics;
            Debug.Log($"[Exp3_Controller.PolygonClip]\nAdd Rectangle");

            if(!PolygonClipping(selected0, selected1))
            {
                selected0.ClearOnCanvas();
                graphicsList.Remove(selected0);
            }

            SelectGraphic();
            selected0 = selected1 = null;
        }
    }
}
