﻿#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace FIMSpace.FEditor
{

    public static class FGUI_Handles
    {

        public static void DrawArrow(Vector3 position, Quaternion direction, float scale = 1f, float width = 5f, float stripeLength = 1f)
        {
            Vector3[] points = new Vector3[8];

            // Low base dots
            points[0] = new Vector3(-0.12f, 0f, 0f);
            points[1] = new Vector3(0.12f, 0f, 0f);

            // Pre tip right triangle dot
            points[2] = new Vector3(0.12f, 0f, 0.4f + 1 * stripeLength);
            // Tip right side
            points[3] = new Vector3(0.4f, 0f, 0.32f + 1 * stripeLength);
            // Tip
            points[4] = new Vector3(0.0f, 0f, 1f + 1 * stripeLength);
            // Tip left side
            points[5] = new Vector3(-0.4f, 0f, 0.32f + 1 * stripeLength);
            // Pre tip left triangle dot
            points[6] = new Vector3(-0.12f, 0f, 0.4f + 1 * stripeLength);
            points[7] = points[0];


            Matrix4x4 rotation = Matrix4x4.TRS(Vector3.zero, direction, Vector3.one * scale);

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = rotation.MultiplyPoint(points[i]);
                points[i] += position;
            }

            Handles.DrawAAPolyLine(width, points);
        }

        public static void DrawBoneHandle(Vector3 from, Vector3 to, Vector3 forward, float fatness = 1f)
        {
            Vector3 dir = (to - from);
            float ratio = dir.magnitude / 7f; ratio *= fatness;
            float baseRatio = ratio * 0.75f;
            Quaternion rot = (dir == Vector3.zero ? rot = Quaternion.identity : rot = Quaternion.LookRotation(dir, forward));
            dir.Normalize();
            Handles.DrawLine(from, to);

            Vector3 p = from + dir * baseRatio;
            Handles.DrawLine(to, p + rot * Vector3.right * ratio);
            Handles.DrawLine(from, p + rot * Vector3.right * ratio);
            Handles.DrawLine(to, p - rot * Vector3.right * ratio);
            Handles.DrawLine(from, p - rot * Vector3.right * ratio);
        }

        public static void DrawBoneHandle(Vector3 from, Vector3 to, float fatness = 1f)
        {
            DrawBoneHandle(from, to, (to - from).normalized, fatness);
        }

        public static void DrawRay(Vector3 pos, Vector3 dir)
        {
            Handles.DrawLine(pos, pos + dir);
        }

        public static void DrawDottedRay(Vector3 pos, Vector3 dir, float scale = 2f)
        {
            Handles.DrawDottedLine(pos, pos + dir, scale);
        }

    }

}

#endif
