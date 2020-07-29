using System.Collections.Generic;
using AwesomeTechnologies.Common;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationSystem.Biomes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace AwesomeTechnologies
{
    public class VegetationMaskEditor : VegetationStudioProBaseEditor
    {
        public VegetationMask VegetationMask;
        private int _vegetationTypeIndex;

        public override void OnInspectorGUI()
        {
            VegetationMask = (VegetationMask)target;

            base.OnInspectorGUI();
            EditorGUIUtility.labelWidth = 200;

            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Create the area where you want to modify the vegetation, you can remove and/or include vegetation types", MessageType.Info);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            if (VegetationMask.enabled)
            {
                EditorGUILayout.LabelField("Insert Node: Ctrl-Click", LabelStyle);
                EditorGUILayout.LabelField("Delete Node: Ctrl-Shift-Click", LabelStyle);
            }
            else
            {
                EditorGUILayout.HelpBox("Enable mask to edit nodes", MessageType.Info);
            }
          
            if (VegetationMask.Nodes.Count < 4) EditorGUILayout.HelpBox("There has to be at least 3 nodes to define the area", MessageType.Warning);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Select ground layers that will be used for selection when adding and moving masks. These will be used in addition to unity terrains.", MessageType.Info);
            VegetationMask.GroundLayerMask = LayerMaskField("Ground Layers", VegetationMask.GroundLayerMask);
            GUILayout.EndVertical();


            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            VegetationMask.ShowArea = EditorGUILayout.Toggle("Show Area", VegetationMask.ShowArea);
            VegetationMask.ShowHandles = EditorGUILayout.Toggle("Show Handles", VegetationMask.ShowHandles);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(VegetationMask);
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Mask settings", LabelStyle);
            EditorGUI.BeginChangeCheck();
            VegetationMask.MaskName = EditorGUILayout.TextField("Mask Name", VegetationMask.MaskName);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(VegetationMask);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Global vegetation removal", LabelStyle);
            EditorGUILayout.HelpBox("The area within the additional perimeter range will be filtered by a noise texture. To get a 100% removal in this area set min and max range to the same distance.", MessageType.Info);

            VegetationMask.RemoveGrass = EditorGUILayout.Toggle("Remove Grass", VegetationMask.RemoveGrass);
            if (VegetationMask.RemoveGrass)
            {
                //vegetationMask.AdditionalGrassPerimiter = EditorGUILayout.Slider("Additional perimeter distance", vegetationMask.AdditionalGrassPerimiter, 0f, 40f);
                EditorFunctions.FloatRangeField("Additional perimeter range", ref VegetationMask.AdditionalGrassPerimiter,
                    ref VegetationMask.AdditionalGrassPerimiterMax, 0, 40);
                VegetationMask.NoiseScaleGrass = EditorGUILayout.Slider("Noise scale", VegetationMask.NoiseScaleGrass, 1f, 40f);
            }
            EditorGUILayout.Space();
            VegetationMask.RemovePlants = EditorGUILayout.Toggle("Remove Plants", VegetationMask.RemovePlants);
            if (VegetationMask.RemovePlants)
            {
                //   vegetationMask.AdditionalPlantPerimiter = EditorGUILayout.Slider("Additional perimeter distance", vegetationMask.AdditionalPlantPerimiter, 0f, 40f);
                EditorFunctions.FloatRangeField("Additional perimeter range", ref VegetationMask.AdditionalPlantPerimiter,
                    ref VegetationMask.AdditionalPlantPerimiterMax, 0, 40);
                VegetationMask.NoiseScalePlant = EditorGUILayout.Slider("Noise scale", VegetationMask.NoiseScalePlant, 1f, 40f);

            }
            EditorGUILayout.Space();
            VegetationMask.RemoveTrees = EditorGUILayout.Toggle("Remove Trees", VegetationMask.RemoveTrees);
            if (VegetationMask.RemoveTrees)
            {
                //vegetationMask.AdditionalTreePerimiter = EditorGUILayout.Slider("Additional perimeter distance", vegetationMask.AdditionalTreePerimiter, 0f, 40f);
                EditorFunctions.FloatRangeField("Additional perimeter range", ref VegetationMask.AdditionalTreePerimiter,
                    ref VegetationMask.AdditionalTreePerimiterMax, 0, 40);
                VegetationMask.NoiseScaleTree = EditorGUILayout.Slider("Noise scale", VegetationMask.NoiseScaleTree, 1f, 40f);
            }
           

            EditorGUILayout.Space();
            VegetationMask.RemoveObjects = EditorGUILayout.Toggle("Remove Objects", VegetationMask.RemoveObjects);
            if (VegetationMask.RemoveObjects)
            {
                // vegetationMask.AdditionalObjectPerimiter = EditorGUILayout.Slider("Additional perimeter distance", vegetationMask.AdditionalObjectPerimiter, 0f, 40f);
                EditorFunctions.FloatRangeField("Additional perimeter range", ref VegetationMask.AdditionalObjectPerimiter,
                    ref VegetationMask.AdditionalObjectPerimiterMax, 0, 40);
                VegetationMask.NoiseScaleObject = EditorGUILayout.Slider("Noise scale", VegetationMask.NoiseScaleObject, 1f, 40f);
            }
    

            EditorGUILayout.Space();
            VegetationMask.RemoveLargeObjects = EditorGUILayout.Toggle("Remove Large Objects", VegetationMask.RemoveLargeObjects);
            if (VegetationMask.RemoveLargeObjects)
            {
                //vegetationMask.AdditionalLargeObjectPerimiter = EditorGUILayout.Slider("Additional perimeter distance", vegetationMask.AdditionalLargeObjectPerimiter, 0f, 40f);
                EditorFunctions.FloatRangeField("Additional perimeter range", ref VegetationMask.AdditionalLargeObjectPerimiter,
                    ref VegetationMask.AdditionalLargeObjectPerimiterMax, 0, 40);
                VegetationMask.NoiseScaleLargeObject = EditorGUILayout.Slider("Noise scale", VegetationMask.NoiseScaleLargeObject, 1f, 40f);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Localized vegetation placement", LabelStyle);
            VegetationMask.IncludeVegetationType = EditorGUILayout.Toggle("Include vegetation", VegetationMask.IncludeVegetationType);

            if (EditorGUI.EndChangeCheck())
            {
                SetMaskDirty();
            }

            if (VegetationMask.IncludeVegetationType)
            {

                if (GUILayout.Button("Add vegetation type"))
                {
                    VegetationTypeSettings newVegetationTypeSettings = new VegetationTypeSettings();
                    VegetationMask.VegetationTypeList.Add(newVegetationTypeSettings);
                    _vegetationTypeIndex = VegetationMask.VegetationTypeList.Count - 1;
                    SetMaskDirty();
                }

                string[] packageNameList = new string[VegetationMask.VegetationTypeList.Count];
                for (int i = 0; i <= VegetationMask.VegetationTypeList.Count - 1; i++)
                {
                    packageNameList[i] = (i + 1).ToString() + ". Item";
                }

              
                if (VegetationMask.VegetationTypeList.Count > 0)
                {
                    if (_vegetationTypeIndex > VegetationMask.VegetationTypeList.Count - 1) _vegetationTypeIndex = VegetationMask.VegetationTypeList.Count - 1;
                    _vegetationTypeIndex = EditorGUILayout.Popup("Selected item", _vegetationTypeIndex, packageNameList);

                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginVertical("box");

                    VegetationMask.VegetationTypeList[_vegetationTypeIndex].Index = (VegetationTypeIndex)EditorGUILayout.EnumPopup("Vegetation type", VegetationMask.VegetationTypeList[_vegetationTypeIndex].Index);
                    VegetationMask.VegetationTypeList[_vegetationTypeIndex].Density = EditorGUILayout.Slider("Density", VegetationMask.VegetationTypeList[_vegetationTypeIndex].Density, 0f, 1f);
                    VegetationMask.VegetationTypeList[_vegetationTypeIndex].Size = EditorGUILayout.Slider("Size", VegetationMask.VegetationTypeList[_vegetationTypeIndex].Size, 0f, 2f);

                    if (GUILayout.Button("Delete selected item"))
                    {
                        VegetationMask.VegetationTypeList.RemoveAt(_vegetationTypeIndex);
                    }

                    GUILayout.EndVertical();

                    if (EditorGUI.EndChangeCheck())
                    {
                        SetMaskDirty();
                    }
                }                          
            }

          
            GUILayout.EndVertical();
        }

        void SetMaskDirty()
        {
            VegetationMask.UpdateVegetationMask();
            EditorUtility.SetDirty(VegetationMask);
            if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(VegetationMask.gameObject.scene);
        }

        public virtual void OnSceneGUI()
        {
            VegetationMask = (VegetationMask)target;

            if (VegetationMask.ShowHandles && VegetationMask.enabled)
            {
                Event currentEvent = Event.current;

                if (currentEvent.shift || currentEvent.control)
                {
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                }
                else
                {
                    //HandleUtility.Repaint();
                }

                if ( currentEvent.shift && currentEvent.control)
                {
                    for (int i = 0; i <= VegetationMask.Nodes.Count - 1; i++)
                    {
                        Vector3 cameraPosition = SceneView.currentDrawingSceneView.camera.transform.position;
                        Vector3 worldSpaceNode = VegetationMask.transform.TransformPoint(VegetationMask.Nodes[i].Position);
                        float distance = Vector3.Distance(cameraPosition, worldSpaceNode);

                        Handles.color = Color.red;
                        if (Handles.Button(worldSpaceNode,Quaternion.LookRotation(worldSpaceNode - cameraPosition,Vector3.up),0.015f* distance, 0.015f * distance, Handles.CircleHandleCap))
                        {
                            VegetationMask.DeleteNode(VegetationMask.Nodes[i]);
                            SetMaskDirty();
                        }
                    }
                }

                if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && currentEvent.control && !currentEvent.shift && !currentEvent.alt)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
                    var hits = Physics.RaycastAll(ray, 10000f);
                    for (int i = 0; i <= hits.Length - 1; i++)
                    {
                        if (hits[i].collider is TerrainCollider || VegetationMask.GroundLayerMask.Contains(hits[i].collider.gameObject.layer))
                        {
                            VegetationMask.AddNode(hits[i].point);
                            currentEvent.Use();
                            SetMaskDirty();
                            break;
                        }
                    }
                }

                if (!currentEvent.shift)
                {
                    bool nodeChange = false;
                    Vector3 cameraPosition = SceneView.currentDrawingSceneView.camera.transform.position;

                    for (int i = 0; i <= VegetationMask.Nodes.Count - 1; i++)
                    {
                        Vector3 worldSpaceNode = VegetationMask.transform.TransformPoint(VegetationMask.Nodes[i].Position);
                        float distance = Vector3.Distance(cameraPosition, worldSpaceNode);
                        if (distance > 200 && VegetationMask.Nodes.Count > 50) continue;
                        //{
                            Vector3 newWorldSpaceNode = Handles.PositionHandle(worldSpaceNode, Quaternion.identity);
                            VegetationMask.Nodes[i].Position = VegetationMask.transform.InverseTransformPoint(newWorldSpaceNode);
                            if (worldSpaceNode != newWorldSpaceNode) nodeChange = true;
                        //}
                    }

                    if (nodeChange)
                    {
                        VegetationMask.PositionNodes();
                        SetMaskDirty();
                    }
                }               
            }
        }

        static List<int> layerNumbers = new List<int>();
        static LayerMask LayerMaskField(string label, LayerMask layerMask)
        {
            var layers = InternalEditorUtility.layers;

            layerNumbers.Clear();

            for (int i = 0; i < layers.Length; i++)
                layerNumbers.Add(LayerMask.NameToLayer(layers[i]));

            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }

            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers);

            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }
            layerMask.value = mask;

            return layerMask;
        }
    }
}
