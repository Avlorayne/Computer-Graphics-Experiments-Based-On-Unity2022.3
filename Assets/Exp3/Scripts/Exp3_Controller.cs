using System;
using UnityEngine;
using UnityEngine.UI;

public class Exp3_Controller : Control
{
    [Header("图形绘制")]
    public LinePainter linePainter = new LinePainter();
    public ClosedFigure closedFigure = new ClosedFigure();
    public Rectangle rectangle = new Rectangle();

    void Start()
    {
        Init();

        AddPainter(linePainter);
        AddPainter(closedFigure);
        AddPainter(rectangle);
    }
}
