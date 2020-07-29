using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AwesomeTechnologies.Utility;

namespace AwesomeTechnologies
{
    [CustomEditor(typeof(VegetationMaskLine))]
    public class VegetationMaskLineEditor : VegetationMaskEditor
    {
        public VegetationMaskLine VegetationMaskLine;
        private bool _showLineSegmentSettings = false;

        public void Awake()
        {
            //base.Awake();
            VegetationMaskLine = (VegetationMaskLine)target;
        }

        public override void OnInspectorGUI()
        {
            HelpTopic = "home/vegetation-studio/components/vegetation-masks/vegetation-mask-line";
            base.OnInspectorGUI();

            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            VegetationMaskLine.LineWidth = EditorGUILayout.Slider("Width", VegetationMaskLine.LineWidth, 0.1f, 40f);
            if (EditorGUI.EndChangeCheck())
            {
                VegetationMaskLine.UpdateVegetationMask();
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.HelpBox("Set the width of the line segments in meters to define the mask area", MessageType.Info);
            GUILayout.EndVertical();


            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Show line segment settings", LabelStyle);

            _showLineSegmentSettings = EditorGUILayout.Toggle("Show settings", _showLineSegmentSettings);

            EditorGUILayout.HelpBox("Enable to edit width and status per line segment", MessageType.Info);

            GUILayout.EndVertical();

            if (_showLineSegmentSettings)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Nodes", LabelStyle);



                for (int i = 0; i <= VegetationMaskLine.Nodes.Count - 2; i++)
                {
                    GUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("Node #" + (i +1), LabelStyle, GUILayout.Width(63));
                    EditorGUIUtility.labelWidth = 50;
                    VegetationMaskLine.Nodes[i].Active =
                        EditorGUILayout.Toggle("Enabled", VegetationMaskLine.Nodes[i].Active,GUILayout.Width(65));
                    EditorGUIUtility.labelWidth = 85;
                    VegetationMaskLine.Nodes[i].OverrideWidth =
                    EditorGUILayout.Toggle("Custom width", VegetationMaskLine.Nodes[i].OverrideWidth, GUILayout.Width(105));
                    EditorGUIUtility.labelWidth = 50;
                    VegetationMaskLine.Nodes[i].CustomWidth = EditorGUILayout.Slider("", VegetationMaskLine.Nodes[i].CustomWidth, 0.1f, 40f);
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();

                EditorGUIUtility.labelWidth = 0;
            }

            if (EditorGUI.EndChangeCheck())
            {
                VegetationMaskLine.UpdateVegetationMask();
                EditorUtility.SetDirty(target);
            }
        }

        public override void OnSceneGUI()
        {
            base.OnSceneGUI();

            if (VegetationMaskLine.ShowArea)
            {
                Vector3[] worldPoints = new Vector3[VegetationMaskLine.Nodes.Count];
                for (int i = 0; i <= VegetationMaskLine.Nodes.Count - 1; i++)
                {
                    worldPoints[i] = VegetationMaskLine.transform.TransformPoint(VegetationMaskLine.Nodes[i].Position);
                }

                DrawLineOutline(worldPoints, VegetationMaskLine.Nodes, VegetationMaskLine.LineWidth / 2f ,0, new Color(1, 1, 1, 0.8f));

                if ((VegetationMask.AdditionalGrassPerimiter > 0.1f || VegetationMask.AdditionalGrassPerimiterMax > 0.1f) && VegetationMask.RemoveGrass)
                {
                    DrawLineOutline(worldPoints, VegetationMaskLine.Nodes, VegetationMaskLine.LineWidth / 2f , VegetationMask.AdditionalGrassPerimiter / 2f, new Color(0, 1, 0, 0.8f));
                    DrawLineOutline(worldPoints, VegetationMaskLine.Nodes, VegetationMaskLine.LineWidth / 2f , VegetationMask.AdditionalGrassPerimiterMax / 2f, new Color(0, 1, 0, 0.8f));

                }

                if ((VegetationMask.AdditionalPlantPerimiter > 0.1f || VegetationMask.AdditionalPlantPerimiterMax > 0.1f) && VegetationMask.RemovePlants)
                {
                    DrawLineOutline(worldPoints, VegetationMaskLine.Nodes, VegetationMaskLine.LineWidth / 2f , VegetationMask.AdditionalPlantPerimiter / 2f, new Color(0, 0, 1, 0.8f));
                    DrawLineOutline(worldPoints, VegetationMaskLine.Nodes, VegetationMaskLine.LineWidth / 2f , VegetationMask.AdditionalPlantPerimiterMax / 2f, new Color(0, 0, 1, 0.8f));

                }


                if ((VegetationMask.AdditionalTreePerimiter > 0.1f || VegetationMask.AdditionalTreePerimiterMax > 0.1f) && VegetationMask.RemoveTrees)
                {
                    DrawLineOutline(worldPoints, VegetationMaskLine.Nodes, VegetationMaskLine.LineWidth / 2f , VegetationMask.AdditionalTreePerimiter / 2f, new Color(1, 0, 0, 0.8f));
                    DrawLineOutline(worldPoints, VegetationMaskLine.Nodes, VegetationMaskLine.LineWidth / 2f , VegetationMask.AdditionalTreePerimiterMax / 2f, new Color(1, 0, 0, 0.8f));

                }

                if ((VegetationMask.AdditionalObjectPerimiter > 0.1f || VegetationMask.AdditionalObjectPerimiterMax > 0.1f) && VegetationMask.RemoveObjects)
                {
                    DrawLineOutline(worldPoints, VegetationMaskLine.Nodes, VegetationMaskLine.LineWidth / 2f , VegetationMask.AdditionalObjectPerimiter / 2f, new Color(1, 1, 0, 0.8f));
                    DrawLineOutline(worldPoints, VegetationMaskLine.Nodes, VegetationMaskLine.LineWidth / 2f , VegetationMask.AdditionalObjectPerimiterMax / 2f, new Color(1, 1, 0, 0.8f));
                }

                if ((VegetationMask.AdditionalLargeObjectPerimiter > 0.1f || VegetationMask.AdditionalLargeObjectPerimiterMax > 0.1f) && VegetationMask.RemoveLargeObjects)
                {
                    DrawLineOutline(worldPoints, VegetationMaskLine.Nodes, VegetationMaskLine.LineWidth / 2f , VegetationMask.AdditionalLargeObjectPerimiter / 2f, new Color(1, 1, 1, 0.8f));
                    DrawLineOutline(worldPoints, VegetationMaskLine.Nodes, VegetationMaskLine.LineWidth / 2f , VegetationMask.AdditionalLargeObjectPerimiterMax / 2f, new Color(1, 1, 1, 0.8f));
                }
            }
        }

        void DrawLineOutline(Vector3[] worldPoints, List<Node> nodeList,  float width, float additionalWidth, Color color)
        {
            Vector3 cameraPosition = SceneView.currentDrawingSceneView.camera.transform.position;

            for (int i = 0; i <= worldPoints.Length - 2; i++)
            {
                if (!nodeList[i].Active) continue;
                float lineWidth = width + additionalWidth;
                if (nodeList[i].OverrideWidth)
                {
                    lineWidth = nodeList[i].CustomWidth / 2f + additionalWidth;
                }

                float distance = Vector3.Distance(worldPoints[i], cameraPosition);
                if (distance < 200)
                {
                    List<Vector3> pointList = new List<Vector3> {worldPoints[i], worldPoints[i + 1]};
                    List<Vector3> newInflatedList = PolygonUtility.InflatePolygon(pointList, lineWidth, false);
                    PolygonUtility.AlignPointsWithTerrain(newInflatedList, true,VegetationMaskLine.GroundLayerMask);
                    Handles.color = color;
                    Handles.DrawAAPolyLine(3, newInflatedList.ToArray());
                }               
            }
        }
    }
}

