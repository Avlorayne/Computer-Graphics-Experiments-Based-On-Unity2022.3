using System;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class MyTransform2D
{
    public Vector2 AnchorPosition;
    public Vector2 Scalation;
    public float Rotation;

    #region Constructors
    public MyTransform2D()
    {
        AnchorPosition =  Vector2.zero;
        Scalation = Vector2.one;
        Rotation = 0;
    }

    public MyTransform2D(Vector2 anchorPosition, Vector2 scalation = default, float rotation = 0)
    {
        AnchorPosition = anchorPosition;
        Scalation = scalation == default ? Vector2.one : scalation;
        Rotation = rotation;
    }
    #endregion

    public string MyDebug
    {
        get
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[MyTransform2D]-------------------------------------".Truncate(25));
            sb.AppendLine($"AnchorPosition: {AnchorPosition}");
            sb.AppendLine($"Scalation: {Scalation}");
            sb.AppendLine($"Rotation: {Rotation}");

            return sb.ToString();
        }
    }

    #region Graphic Transform

    public static void Transform(MyGraphics2D graphics, MyTransform2D newTransform)
    {
        MyTransform2D originTransform = graphics.Transform;
        Debug.Log($"[MyTransform2D.Transform]\nGraphics Before Transformed:\n{graphics.MyDebug}");

        MyTransform2D delta = new MyTransform2D(
            newTransform.AnchorPosition - originTransform.AnchorPosition,
            newTransform.Scalation / originTransform.Scalation,
            newTransform.Rotation - originTransform.Rotation);

        for (int i = 0; i < graphics.Offsets.Count; i++)
        {
            // 以锚点为变换中心
            Vector2 offset = graphics.Offsets[i];
            // 缩放 → 旋转 → 平移
            Vector2 scaled = ScaleOffset(offset, delta.Scalation);
            Vector2 rotated = RotateOffset(scaled, delta.Rotation);
            Vector2 final = TranslateOffset(rotated, delta.AnchorPosition);

            graphics.NodePoints[i] = originTransform.AnchorPosition + final;
        }

        graphics.Transform = newTransform;
        Debug.Log($"[MyTransform2D.Transform]\nGraphics Transformed:\n{graphics.MyDebug}");

        graphics.OverPaint();
        graphics.Select();
    }
    #endregion


    #region Point Trasform

    static Vector2 TranslateOffset(Vector2 offset, Vector2 transDelta)
    {
        return new Vector2(offset.x + transDelta.x, offset.y + transDelta.y);
    }

    static Vector2 ScaleOffset(Vector2 offset, Vector2 scalDelta)
    {
        return new Vector2(offset.x * scalDelta.x, offset.y * scalDelta.y);
    }

    static Vector2 RotateOffset(Vector2 offset, float rotDelta)
    {
        float angle = rotDelta * Mathf.Deg2Rad;
        float x = offset.x * Mathf.Cos(angle) - offset.y * Mathf.Sin(angle);
        float y = offset.x * Mathf.Sin(angle) + offset.y * Mathf.Cos(angle);
        return new Vector2(x, y);
    }

    static Matrix4x4 CreateTransformMatrix(Vector2 anchor, Vector2 scale, float rad, Vector2 newPos)
    {
        // 顺序：移回原点 → 缩放 → 旋转 → 移到新位置
        var T1 = CreateTranslationMatrix(-anchor);
        var S = CreateScaleMatrix(scale);
        var R = CreateRotationMatrix_Z(rad);
        var T2 = CreateTranslationMatrix(newPos);

        return T2 * R * S * T1;
    }

    static Vector2 TranslateByMatrix(Vector2 originPoint, Vector2 translationDelta)
    {
        Matrix4x4 translationMatrix = CreateTranslationMatrix(translationDelta);
        Vector4 homogeneousPoint = new Vector4(originPoint.x, originPoint.y,  1, 1);
        Vector4 transformedPoint = translationMatrix * homogeneousPoint;

        return new Vector2(transformedPoint.x, transformedPoint.y);
    }

    static Vector2 RotateByMatrix(Vector2 originPoint, float rotationDelta)
    {
        Matrix4x4 rotationMatrix = CreateRotationMatrix_Z(rotationDelta);
        Vector4 homogeneousPoint = new Vector4(originPoint.x, originPoint.y,  1, 1);
        Vector4 rotatedPoint = rotationMatrix * homogeneousPoint;
        return new Vector2(rotatedPoint.x, rotatedPoint.y);
    }


    static Vector2 ScaleByMatrix(Vector2 originPoint, Vector2 scale)
    {
        Matrix4x4 scaleMatrix = CreateScaleMatrix(scale);
        Vector4 homogeneousPoint = new Vector4(originPoint.x, originPoint.y, 1, 1);
        Vector4 scaledPoint = scaleMatrix * homogeneousPoint;
        return new Vector2(scaledPoint.x, scaledPoint.y);
    }
    #endregion

    #region Get Marix4x4

    static Matrix4x4 CreateTranslationMatrix(Vector2 translationDelta)
    {
        // 齐次平移矩阵：
        // [1, 0, 0, tx]
        // [0, 1, 0, ty]
        // [0, 0, 1, 0]
        // [0, 0, 0, 1]

        Matrix4x4 matrix = Matrix4x4.identity;
        matrix.m03 = translationDelta.x;
        matrix.m13 = translationDelta.y;
        return matrix;
    }

    static Matrix4x4 CreateScaleMatrix(Vector2 scalationDealta)
    {
        // 齐次缩放矩阵：
        // [sx, 0, 0, 0]
        // [0, sy, 0, 0]
        // [0, 0, 1, 0]
        // [0, 0, 0, 1]
        Matrix4x4 matrix = Matrix4x4.identity;
        matrix.m00 = scalationDealta.x;
        matrix.m11 = scalationDealta.y;
        return matrix;
    }

    static Matrix4x4 CreateRotationMatrix_Z(float rad) // 参数已经是弧度
    {
        Matrix4x4 matrix = Matrix4x4.identity;
        matrix.m00 = Mathf.Cos(rad);
        matrix.m01 = -Mathf.Sin(rad);
        matrix.m10 = Mathf.Sin(rad);
        matrix.m11 = Mathf.Cos(rad);
        return matrix;
    }
    #endregion
}
