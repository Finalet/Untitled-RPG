using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AwesomeTechnologies.Utility;

namespace AwesomeTechnologies
{
    [CustomEditor(typeof(VegetationMaskArea))]
    public class VegetationMaskAreaEditor : VegetationMaskEditor
    {
        public VegetationMaskArea VegetationMaskArea;

        public void Awake()
        {
           
            //base.Awake();
            VegetationMaskArea = (VegetationMaskArea)target;



        }

        public override void OnInspectorGUI()
        {
            HelpTopic = "home/vegetation-studio/components/vegetation-masks/vegetation-mask-area";
            base.OnInspectorGUI();

            GUILayout.BeginVertical("box");
            VegetationMaskArea.ReductionTolerance = EditorGUILayout.Slider("Accuracy", VegetationMaskArea.ReductionTolerance,0.1f,2f);

            if (GUILayout.Button("Calculate hull outline"))
            {
                VegetationMaskArea.GenerateHullNodes(VegetationMaskArea.ReductionTolerance);
                SceneView.RepaintAll();
            }
            EditorGUILayout.HelpBox("This will analyze the meshes in the GameObject and children and calculate an outline.", MessageType.Info);
            GUILayout.EndVertical();          
        }

        Vector3 GetLabelPosition(List<Vector3> positionList, int idealNumber)
        {
            if (positionList.Count > idealNumber) return positionList[idealNumber];
            return positionList.Count > 0 ? positionList[0] : Vector3.zero;
        }

        public override void OnSceneGUI()
        {
            base.OnSceneGUI();

            if (VegetationMaskArea.ShowArea)
            {
                Vector3[] worldPoints = new Vector3[VegetationMask.Nodes.Count];
                for (int i = 0; i <= VegetationMask.Nodes.Count - 1; i++)
                {
                    worldPoints[i] = VegetationMask.transform.TransformPoint(VegetationMask.Nodes[i].Position);
                }

                List<Vector3> worldPointsClosedList = new List<Vector3>(worldPoints);
                worldPointsClosedList.Add(worldPointsClosedList[0]);
                Handles.color = new Color(1, 1, 1, 0.8f);
                Handles.DrawAAPolyLine(3, worldPointsClosedList.ToArray());
                List<Vector3> worldPointsList = new List<Vector3>(worldPoints);

                GUIStyle stLabel = new GUIStyle(EditorStyles.boldLabel);

                if ((VegetationMask.AdditionalGrassPerimiter > 0.1f || VegetationMask.AdditionalGrassPerimiterMax > 0.1f) && VegetationMask.RemoveGrass)
                {
                    List<Vector3> inflatedGrassList = PolygonUtility.InflatePolygon(worldPointsList, VegetationMask.AdditionalGrassPerimiter, true);
                    PolygonUtility.AlignPointsWithTerrain(inflatedGrassList, true, VegetationMaskArea.GroundLayerMask);

                   
                    Handles.Label(GetLabelPosition(inflatedGrassList,0), "Grass", stLabel);

                    Handles.color = new Color(0, 1, 0, 0.8f);
                    Handles.DrawAAPolyLine(3, inflatedGrassList.ToArray());

                    if (VegetationMask.AdditionalGrassPerimiterMax > VegetationMask.AdditionalGrassPerimiter)
                    {
                        List<Vector3> inflatedGrassListMax = PolygonUtility.InflatePolygon(worldPointsList, VegetationMask.AdditionalGrassPerimiterMax, true);
                        PolygonUtility.AlignPointsWithTerrain(inflatedGrassListMax, true, VegetationMaskArea.GroundLayerMask);
                        Handles.DrawAAPolyLine(3, inflatedGrassListMax.ToArray());
                    }
                }

                if ((VegetationMask.AdditionalPlantPerimiter > 0.1f || VegetationMask.AdditionalPlantPerimiterMax > 0.1f) && VegetationMask.RemovePlants)
                {
                    List<Vector3> inflatedPlantList = PolygonUtility.InflatePolygon(worldPointsList, VegetationMask.AdditionalPlantPerimiter, true);
                    PolygonUtility.AlignPointsWithTerrain(inflatedPlantList, true, VegetationMaskArea.GroundLayerMask);
                    Handles.Label(GetLabelPosition(inflatedPlantList,1), "Plants", stLabel);
                    Handles.color = new Color(0, 0, 1, 0.8f);
                    Handles.DrawAAPolyLine(3, inflatedPlantList.ToArray());

                    if (VegetationMask.AdditionalPlantPerimiterMax > VegetationMask.AdditionalPlantPerimiter)
                    {
                        List<Vector3> inflatedPlantListMax = PolygonUtility.InflatePolygon(worldPointsList, VegetationMask.AdditionalPlantPerimiterMax, true);
                        PolygonUtility.AlignPointsWithTerrain(inflatedPlantListMax, true, VegetationMaskArea.GroundLayerMask);                       
                        Handles.DrawAAPolyLine(3, inflatedPlantListMax.ToArray());
                    }

                }

                if ((VegetationMask.AdditionalTreePerimiter > 0.1f || VegetationMask.AdditionalTreePerimiterMax > 0.1f) && VegetationMask.RemoveTrees)
                {
                    List<Vector3> inflatedTreeList = PolygonUtility.InflatePolygon(worldPointsList, VegetationMask.AdditionalTreePerimiter, true);
                    PolygonUtility.AlignPointsWithTerrain(inflatedTreeList, true, VegetationMaskArea.GroundLayerMask);
                    Handles.Label(GetLabelPosition(inflatedTreeList,2), "Trees", stLabel);
                    Handles.color = new Color(1, 0, 0, 0.8f);
                    Handles.DrawAAPolyLine(3, inflatedTreeList.ToArray());

                    if (VegetationMask.AdditionalTreePerimiterMax > VegetationMask.AdditionalTreePerimiter)
                    {
                        List<Vector3> inflatedTreeListMax = PolygonUtility.InflatePolygon(worldPointsList, -VegetationMask.AdditionalTreePerimiterMax, true);
                        PolygonUtility.AlignPointsWithTerrain(inflatedTreeListMax, true, VegetationMaskArea.GroundLayerMask);
                        Handles.DrawAAPolyLine(3, inflatedTreeListMax.ToArray());
                    }
                }

                if ((VegetationMask.AdditionalObjectPerimiter > 0.1f || VegetationMask.AdditionalObjectPerimiterMax > 0.1f) && VegetationMask.RemoveObjects)
                {
                    List<Vector3> inflatedObjectList = PolygonUtility.InflatePolygon(worldPointsList, VegetationMask.AdditionalObjectPerimiter, true);
                    PolygonUtility.AlignPointsWithTerrain(inflatedObjectList, true, VegetationMaskArea.GroundLayerMask);
                    Handles.Label(GetLabelPosition(inflatedObjectList,3), "Objects", stLabel);
                    Handles.color = new Color(1, 1, 0, 0.8f);
                    Handles.DrawAAPolyLine(3, inflatedObjectList.ToArray());


                    if (VegetationMask.AdditionalObjectPerimiterMax > VegetationMask.AdditionalObjectPerimiter)
                    {
                        List<Vector3> inflatedObjectListMax = PolygonUtility.InflatePolygon(worldPointsList, VegetationMask.AdditionalObjectPerimiterMax, true);
                        PolygonUtility.AlignPointsWithTerrain(inflatedObjectListMax, true, VegetationMaskArea.GroundLayerMask);
                        Handles.DrawAAPolyLine(3, inflatedObjectListMax.ToArray());
                    }
                }

                if ((VegetationMask.AdditionalLargeObjectPerimiter > 0.1f || VegetationMask.AdditionalLargeObjectPerimiterMax > 0.1f) && VegetationMask.RemoveLargeObjects)
                {
                    List<Vector3> inflatedLargeObjectList = PolygonUtility.InflatePolygon(worldPointsList, VegetationMask.AdditionalLargeObjectPerimiter, true);
                    PolygonUtility.AlignPointsWithTerrain(inflatedLargeObjectList, true, VegetationMaskArea.GroundLayerMask);
                    Handles.Label(GetLabelPosition(inflatedLargeObjectList,4), "Large Objects", stLabel);
                    Handles.color = new Color(1, 1, 1, 0.8f);
                    Handles.DrawAAPolyLine(3, inflatedLargeObjectList.ToArray());

                    if (VegetationMask.AdditionalLargeObjectPerimiterMax > VegetationMask.AdditionalLargeObjectPerimiter)
                    {
                        List<Vector3> inflatedLargeObjectListMax = PolygonUtility.InflatePolygon(worldPointsList, VegetationMask.AdditionalLargeObjectPerimiterMax, true);
                        PolygonUtility.AlignPointsWithTerrain(inflatedLargeObjectListMax, true, VegetationMaskArea.GroundLayerMask);
                        Handles.DrawAAPolyLine(3, inflatedLargeObjectListMax.ToArray());
                    }
                }



                //List<Vector3> _inflatedList2 = PolygonUtility.InflatePolygon(_worldPointsList, 4d, true);
                //PolygonUtility.AlignPointsWithTerrain(_inflatedList, true);
                //PolygonUtility.AlignPointsWithTerrain(_inflatedList2, true);

                //Handles.color = new Color(1, 0, 0, 0.8f);
                //Handles.DrawAAPolyLine(2, _inflatedList.ToArray());
                //Handles.color = new Color(0, 1, 0, 0.8f);
                //Handles.DrawAAPolyLine(2, _inflatedList2.ToArray());
            }
        }
    }
}
