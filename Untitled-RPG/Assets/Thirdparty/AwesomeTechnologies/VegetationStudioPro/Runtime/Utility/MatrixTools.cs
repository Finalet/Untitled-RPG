using UnityEngine;

namespace AwesomeTechnologies.Utility
{


    public class MatrixTools
    {
        public static Vector3 ExtractTranslationFromMatrix(Matrix4x4 matrix)
        {
            Vector3 translate;
            translate.x = matrix.m03;
            translate.y = matrix.m13;
            translate.z = matrix.m23;
            return translate;
        }

        public static Quaternion ExtractRotationFromMatrix(Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            if (forward == Vector3.zero) return Quaternion.identity;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return Quaternion.LookRotation(forward, upwards);
        }

        public static Vector3 ExtractScaleFromMatrix(Matrix4x4 matrix)
        {
            return new Vector3(matrix.GetColumn(0).magnitude, matrix.GetColumn(1).magnitude, matrix.GetColumn(2).magnitude);
        }

        public static Matrix4x4 ScaleMatrix4X4(Matrix4x4 lhs, ref Matrix4x4 rhs)
        {
            return new Matrix4x4
            {
                m00 = lhs.m00 * rhs.m00,
                m01 = lhs.m01 * rhs.m11,
                m02 = lhs.m02 * rhs.m22,
                m03 = lhs.m03 * rhs.m33,
                m10 = lhs.m10 * rhs.m00,
                m11 = lhs.m11 * rhs.m11,
                m12 = lhs.m12 * rhs.m22,
                m13 = lhs.m13 * rhs.m33,
                m20 = lhs.m20 * rhs.m00,
                m21 = lhs.m21 * rhs.m11,
                m22 = lhs.m22 * rhs.m22,
                m23 = lhs.m23 * rhs.m33,
                m30 = lhs.m30 * rhs.m00,
                m31 = lhs.m31 * rhs.m11,
                m32 = lhs.m32 * rhs.m22,
                m33 = lhs.m33 * rhs.m33
            };
        }
        public static Matrix4x4 TranslateMatrix4X4(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            return new Matrix4x4
            {
                m00 = lhs.m00,
                m01 = lhs.m01,
                m02 = lhs.m02,
                m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03,
                m10 = lhs.m10,
                m11 = lhs.m11,
                m12 = lhs.m12,
                m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13,
                m20 = lhs.m20,
                m21 = lhs.m21,
                m22 = lhs.m22,
                m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23,
                m30 = lhs.m30,
                m31 = lhs.m31,
                m32 = lhs.m32,
                m33 = lhs.m30 * rhs.m03 + lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33
            };
        }

        public static void FastTranslateMatrix4X4(ref Matrix4x4 lhs, ref Matrix4x4 rhs)
        {
            lhs.m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03;
            lhs.m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13;
            lhs.m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23;
            lhs.m33 = lhs.m30 * rhs.m03 + lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33;
        }
    }
}