using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class MyTransform3D
{
    public Vector3 Position;
    public Vector3 Scalation;
    public Vector3 Rotation;

    public Vector3 Anchor;
    // public Vector3 Pivot;

    public MyTransform3D()
    {
        Position =  Vector3.zero;
        Scalation = Vector3.one;
        Rotation = Vector3.zero;
        Anchor = Vector3.zero;
    }

    public MyTransform3D(Vector3 position, Vector3 scalation = default, Vector3 rotation = default, Vector3 anchor = default/*, Vector3 pivot = default*/)
    {
        Position = position;
        Scalation = scalation == default ? Vector3.one : scalation;
        Rotation = rotation == default ? Vector3.zero : rotation;
        Anchor = anchor ==  default ? Vector3.zero : anchor;
        // Pivot = pivot ==  default ? new Vector3(0.5f, 0.5f, 0.5f) : pivot;
    }


    public static Vector3 Translate(Vector3 originPoint ,Vector3 translationDelta)
    {
        Matrix4x4 translationMatrix = CreateTranslationMatrix(translationDelta);
        Vector4 homogeneousPoint = new Vector4(originPoint.x, originPoint.y, originPoint.z, 1);
        Vector4 transformedPoint = translationMatrix * homogeneousPoint;

        return new Vector3(transformedPoint.x, transformedPoint.y , transformedPoint.z);
    }

    static Matrix4x4 CreateTranslationMatrix(Vector3 translationDelta)
    {
        // 齐次平移矩阵：
        // [1, 0, 0, tx]
        // [0, 1, 0, ty]
        // [0, 0, 1, tz]
        // [0, 0, 0, 1]

        Matrix4x4 matrix = Matrix4x4.identity;
        matrix.m03 = translationDelta.x;
        matrix.m13 = translationDelta.y;
        matrix.m23 = translationDelta.z;

        return matrix;
    }

    static Matrix4x4 CreateScaleMatrix(Vector3 scalationDealta)
    {
        // 齐次缩放矩阵：
        // [sx, 0, 0, 0]
        // [0, sy, 0, 0]
        // [0, 0, sz, 0]
        // [0, 0, 0, 1]
        Matrix4x4 matrix = Matrix4x4.identity;
        matrix.m00 = scalationDealta.x;
        matrix.m11 = scalationDealta.y;
        matrix.m22 = scalationDealta.z;
        return matrix;
    }

    static Matrix4x4 CreateRotationMatrix_Z(float angle)
    {
        // 齐次缩放矩阵：
        // [cos a   , sin a , 0, 0]
        // [-sin a  , cos a , 0, 0]
        // [0       , 0     , 1, 0]
        // [0       , 0     , 0, 1]
        Matrix4x4 matrix = Matrix4x4.identity;
        matrix.m00 = Mathf.Cos(angle);
        matrix.m01 = Mathf.Sin(angle);
        matrix.m11 = -Mathf.Sin(angle);
        matrix.m22 = Mathf.Cos(angle);
        return matrix;
    }



    public static void Rotate(Vector3 originPoint, Quaternion rotationDelta)
    {

    }


    public static Vector3 Scale(Vector3 originPoint, Vector3 scalationDelta)
    {
        Matrix4x4 scaleMatrix = CreateScaleMatrix(scalationDelta);
        Vector4 homogeneousPoint = new Vector4(originPoint.x, originPoint.y, originPoint.z, 1);
        Vector4 scaledPoint = scaleMatrix * homogeneousPoint;

        return new Vector3(scaledPoint.x, scaledPoint.y, scaledPoint.z);
    }

}
