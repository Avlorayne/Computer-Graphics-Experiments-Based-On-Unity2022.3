using System;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

public class TransformBind2D : MonoBehaviour
{
    private static TransformBind2D instance;
    public static TransformBind2D Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<TransformBind2D>();

            return instance;
        }
    }

    public Slider[] sliderList;
    private Slider PosX;
    private Slider PosY;
    private Slider SclX;
    private Slider SclY;
    private Slider RotZ;

    private MyGraphics2D Graphics;
    private MyTransform2D Transform;
    private MyTransform2D transformInBuffer =  new MyTransform2D();
    private Texture2D texture;

    public string MyDebug
    {
        get
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[TransformBind2D]--------------------------".Truncate(25));
            sb.AppendLine($"AnchorPosition: {new Vector2(PosX.value, PosY.value)}");
            sb.AppendLine($"ScaleByMatrix: {new Vector2(SclX.value, SclY.value)}");
            sb.AppendLine($"Rotation: {RotZ.value}");
            return sb.ToString();
        }
    }

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        sliderList = GetComponentsInChildren<Slider>();
        if (sliderList.Length < 5)
        {
            Debug.LogError($"[TransformBind2D.Start]\nCan't Find All Sliders. Slider Count: {sliderList.Length}");
            return;
        }

        PosX = sliderList[0];
        PosY = sliderList[1];
        SclX = sliderList[2];
        SclY = sliderList[3];
        RotZ = sliderList[4];

        texture = CanvasPixelPainter.Instance.canvas.texture as Texture2D;

        PosX.onValueChanged.AddListener(value =>
        {
            Debug.Log($"[TransformBind2D.PosX]\nvalue: {value}");

            ApplyTransform();
        });

        PosY.onValueChanged.AddListener(value =>
        {
            Debug.Log($"[TransformBind2D.PosY]\nvalue: {value}");

            ApplyTransform();
        });

        SclX.onValueChanged.AddListener(value =>
        {
            Debug.Log($"[TransformBind2D.SclX]\nvalue: {value}");

            ApplyTransform();
        });

        SclY.onValueChanged.AddListener(value =>
        {
            Debug.Log($"[TransformBind2D.SclY]\nvalue: {value}");

            ApplyTransform();
        });

        RotZ.onValueChanged.AddListener(value =>
        {
            Debug.Log($"[TransformBind2D.RotZ]\nvalue: {value}");

            ApplyTransform();
        });
    }

    // 核心改造：Slider直接控制绝对值
    void ApplyTransform()
    {
        if (Graphics == null) return;

        // 直接从Slider计算目标变换（绝对值）
        Vector2 newPos = new Vector2(PosX.value * texture.width, PosY.value * texture.height);
        Vector2 newScl = new Vector2(SclX.value * 3, SclY.value * 3);
        float newRot = RotZ.value * 360;

        // 创建新变换（Anchor设为0，不再捣乱）
        MyTransform2D newTransform = new MyTransform2D(newPos, newScl, newRot);

        // 应用变换（基于原始点重新计算）
        MyTransform2D.Transform(Graphics, newTransform);
    }

    public void SetTransformListen(MyGraphics2D graphics)
    {
        if (graphics == null)
        {
            Debug.Log("[TransformBind2D.SetTransformListen]\nFailed to set graphics");
            return;
        }
        Graphics = graphics;
        Transform = graphics.Transform;
        Debug.Log($"[TransformBind2D.SetTransformListen]\nOrigin Graphics, Transform:\n{Transform.MyDebug}");

        transformInBuffer = new MyTransform2D(Transform.AnchorPosition, Transform.Scalation, Transform.Rotation);
        // Debug.Log($"[TransformBind2D.SetTransformListen]\nTransform In Buffer:\n{Transform.MyDebug}");

        // 临时禁用事件监听，避免触发变换操作
        SetSlidersWithoutNotify(transformInBuffer.AnchorPosition.x, transformInBuffer.AnchorPosition.y,
                               transformInBuffer.Scalation.x, transformInBuffer.Scalation.y, 
                               transformInBuffer.Rotation);

        Debug.Log($"[TransformBind2D.SetTransformListen]\nSet New Graphics, Transform Bind:\n{MyDebug}");
    }

    private void SetSlidersWithoutNotify(float posX, float posY, float sclX, float sclY, float rotZ)
    {
        // 映射到 0-1 范围
        PosX.SetValueWithoutNotify(posX / texture.width);
        PosY.SetValueWithoutNotify(posY / texture.height);
        SclX.SetValueWithoutNotify(sclX / 3f);
        SclY.SetValueWithoutNotify(sclY / 3f);
        RotZ.SetValueWithoutNotify(rotZ / 360f);
    }

}
