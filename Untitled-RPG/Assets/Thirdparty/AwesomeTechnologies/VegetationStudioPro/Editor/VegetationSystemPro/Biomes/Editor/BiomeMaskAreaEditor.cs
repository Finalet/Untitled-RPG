using System.Collections.Generic;
using AwesomeTechnologies.External.CurveEditor;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationStudio;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem.Biomes
{
    [CustomEditor(typeof(BiomeMaskArea))]
    public class VegetationMaskAreaEditor : VegetationStudioProBaseEditor
    {
        private BiomeMaskArea _biomeMaskArea;
        private static readonly List<int> LayerNumbers = new List<int>();
        private InspectorCurveEditor _distanceCurveEditor;

        public void OnEnable()
        {
            var settings = InspectorCurveEditor.Settings.DefaultSettings;
            _distanceCurveEditor = new InspectorCurveEditor(settings) { CurveType = InspectorCurveEditor.InspectorCurveType.Distance };
        }

        public void OnDisable()
        {
            _distanceCurveEditor.RemoveAll();
        }

        public override void OnInspectorGUI()
        {
            _biomeMaskArea = (BiomeMaskArea)target;
            OverrideLogoTextureName = "Banner_BiomeMaskArea";
            base.OnInspectorGUI();

            EditorGUIUtility.labelWidth = 200;

            GUILayout.BeginVertical("box");        
            EditorGUILayout.HelpBox("Create the area where you want to modify the vegetation, you can remove and/or include vegetation types", MessageType.Info);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            if (_biomeMaskArea.enabled)
            {
                EditorGUILayout.LabelField("Insert Node: Ctrl-Click", LabelStyle);
                EditorGUILayout.LabelField("Delete Node: Ctrl-Shift-Click", LabelStyle);
                EditorGUILayout.LabelField("Toggle edge: Ctrl-Alt-Click", LabelStyle);
                EditorGUILayout.HelpBox("Edges betwee 2 disabled edge nodes will not be included when calculating edge distance in rules and blending.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Enable mask to edit nodes", MessageType.Info);
            }

            if (_biomeMaskArea.Nodes.Count < 4) EditorGUILayout.HelpBox("There has to be at least 3 nodes to define the area", MessageType.Warning);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            _biomeMaskArea.ShowArea = EditorGUILayout.Toggle("Show Area", _biomeMaskArea.ShowArea);
            _biomeMaskArea.ShowHandles = EditorGUILayout.Toggle("Show Handles", _biomeMaskArea.ShowHandles);

            if (EditorGUI.EndChangeCheck())
            {
                SetMaskDirty();
            }

            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Select ground layers that will be used for selection when adding and moving masks. These will be used in addition to unity terrains.", MessageType.Info);
            _biomeMaskArea.GroundLayerMask = LayerMaskField("Ground Layers", _biomeMaskArea.GroundLayerMask);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Generate splatmap"))
            {
                VegetationStudioManager.GenerateSplatMap();
            }
            EditorGUILayout.HelpBox("This will generate the splatmaps with biomes for all Terrains based on current rules in the vegetation packages.", MessageType.Info);

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Vegetation Blend settings", LabelStyle);

            if (_distanceCurveEditor.EditCurves(_biomeMaskArea.BlendCurve,_biomeMaskArea.InverseBlendCurve, this))
            {
                SetMaskDirty();
            }

            EditorGUILayout.LabelField("Texture Blend settings", LabelStyle);

            if (_distanceCurveEditor.EditCurve(_biomeMaskArea.TextureBlendCurve, this))
            {
                SetMaskDirty();
            }

            EditorGUILayout.HelpBox("The blend curve defines how the edge area(within distance) will blend against the main biome. Green is for the selected biome. Red the main biome.", MessageType.Info);

            EditorGUI.BeginChangeCheck();
            _biomeMaskArea.BlendDistance =
                EditorGUILayout.Slider("Blend distance", _biomeMaskArea.BlendDistance, 0, 300f);
            _biomeMaskArea.UseNoise = EditorGUILayout.Toggle("Use noise", _biomeMaskArea.UseNoise);
            _biomeMaskArea.NoiseScale = EditorGUILayout.Slider("Noise scale", _biomeMaskArea.NoiseScale, 1, 500);
            EditorGUILayout.HelpBox("When enabled noise will be used in addition to the fallout curve to create the edge blend values.", MessageType.Info);
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                SetMaskDirty();
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Mask settings", LabelStyle);
            EditorGUI.BeginChangeCheck();
            _biomeMaskArea.MaskName = EditorGUILayout.TextField("Mask Name", _biomeMaskArea.MaskName);
            _biomeMaskArea.BiomeType =
                (BiomeType)EditorGUILayout.EnumPopup("Select biome", _biomeMaskArea.BiomeType);
            if (EditorGUI.EndChangeCheck())
            {
                SetMaskDirty();
            }
            GUILayout.EndVertical();
        }

        void SetMaskDirty()
        {
            _biomeMaskArea.UpdateBiomeMask();
            EditorUtility.SetDirty(_biomeMaskArea);
            if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(_biomeMaskArea.gameObject.scene);
        }

        // ReSharper disable once InconsistentNaming
        public virtual void OnSceneGUI()
        {
            _biomeMaskArea = (BiomeMaskArea)target;
            if (_biomeMaskArea.ShowHandles && _biomeMaskArea.enabled)
            {
                Event currentEvent = Event.current;

                if (currentEvent.shift || currentEvent.control)
                {
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                }

                if (currentEvent.shift && currentEvent.control)
                {
                    for (int i = 0; i <= _biomeMaskArea.Nodes.Count - 1; i++)
                    {
                        Vector3 cameraPosition = SceneView.currentDrawingSceneView.camera.transform.position;
                        Vector3 worldSpaceNode =
                            _biomeMaskArea.transform.TransformPoint(_biomeMaskArea.Nodes[i].Position);
                        float distance = Vector3.Distance(cameraPosition, worldSpaceNode);

                        Handles.color = Color.red;
                        if (Handles.Button(worldSpaceNode,
                            Quaternion.LookRotation(worldSpaceNode - cameraPosition, Vector3.up), 0.030f * distance,
                            0.015f * distance, Handles.CircleHandleCap))
                        {
                            _biomeMaskArea.DeleteNode(_biomeMaskArea.Nodes[i]);
                            SetMaskDirty();
                        }
                    }
                }
                
                if (currentEvent.alt && currentEvent.control)
                {
                    for (int i = 0; i <= _biomeMaskArea.Nodes.Count - 1; i++)
                    {
                        Vector3 cameraPosition = SceneView.currentDrawingSceneView.camera.transform.position;
                        Vector3 worldSpaceNode =
                            _biomeMaskArea.transform.TransformPoint(_biomeMaskArea.Nodes[i].Position);
                        float distance = Vector3.Distance(cameraPosition, worldSpaceNode);

                        Handles.color = Color.yellow;
                        if (Handles.Button(worldSpaceNode,
                            Quaternion.LookRotation(worldSpaceNode - cameraPosition, Vector3.up), 0.030f * distance,
                            0.015f * distance, Handles.CircleHandleCap))
                        {
                            _biomeMaskArea.Nodes[i].DisableEdge = !_biomeMaskArea.Nodes[i].DisableEdge;
                            SetMaskDirty();
                        }
                    }
                }

                if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && currentEvent.control &&
                    !currentEvent.shift && !currentEvent.alt)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
                    var hits = Physics.RaycastAll(ray, 10000f);
                    for (int i = 0; i <= hits.Length - 1; i++)
                    {
                        if (hits[i].collider is TerrainCollider || _biomeMaskArea.GroundLayerMask.Contains(hits[i].collider.gameObject.layer))
                        {
                            _biomeMaskArea.AddNode(hits[i].point);
                            currentEvent.Use();
                            SetMaskDirty();
                            break;
                        }
                    }
                }

                if (!currentEvent.shift && !currentEvent.alt)
                {
                    bool nodeChange = false;
                    Vector3 cameraPosition = SceneView.currentDrawingSceneView.camera.transform.position;

                    for (int i = 0; i <= _biomeMaskArea.Nodes.Count - 1; i++)
                    {
                        Vector3 worldSpaceNode =
                            _biomeMaskArea.transform.TransformPoint(_biomeMaskArea.Nodes[i].Position);
                        float distance = Vector3.Distance(cameraPosition, worldSpaceNode);
                        if (distance > 200 && _biomeMaskArea.Nodes.Count > 50) continue;

                        Vector3 newWorldSpaceNode = Handles.PositionHandle(worldSpaceNode, Quaternion.identity);
                        _biomeMaskArea.Nodes[i].Position =
                            _biomeMaskArea.transform.InverseTransformPoint(newWorldSpaceNode);
                        if (worldSpaceNode != newWorldSpaceNode) nodeChange = true;
                    }

                    if (nodeChange)
                    {
                        _biomeMaskArea.PositionNodes();
                        SetMaskDirty();
                    }
                }
            }

            if (_biomeMaskArea.ShowArea)
            {
                Vector3[] worldPoints = new Vector3[_biomeMaskArea.Nodes.Count];
                for (int i = 0; i <= _biomeMaskArea.Nodes.Count - 1; i++)
                {
                    worldPoints[i] = _biomeMaskArea.transform.TransformPoint(_biomeMaskArea.Nodes[i].Position);
                }

                List<Vector3> worldPointsClosedList = new List<Vector3>(worldPoints);
                worldPointsClosedList.Add(worldPointsClosedList[0]);
                Handles.color = new Color(1, 1, 1, 0.8f);
                Handles.DrawAAPolyLine(3, worldPointsClosedList.ToArray());
                
                

                if (_biomeMaskArea.BlendDistance > 0.01f)
                {
                    List<Vector3> inflatedTreeListMax = PolygonUtility.InflatePolygon(worldPointsClosedList, -_biomeMaskArea.BlendDistance, true);
                    PolygonUtility.AlignPointsWithTerrain(inflatedTreeListMax, true, _biomeMaskArea.GroundLayerMask);
                    Handles.color = new Color(1, 0, 0, 0.8f);
                    Handles.DrawAAPolyLine(3, inflatedTreeListMax.ToArray());
                }
            }
        }

        static LayerMask LayerMaskField(string label, LayerMask layerMask)
        {
            var layers = InternalEditorUtility.layers;

            LayerNumbers.Clear();

            for (int i = 0; i < layers.Length; i++)
                LayerNumbers.Add(LayerMask.NameToLayer(layers[i]));

            int maskWithoutEmpty = 0;
            for (int i = 0; i < LayerNumbers.Count; i++)
            {
                if (((1 << LayerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }

            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers);

            int mask = 0;
            for (int i = 0; i < LayerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << LayerNumbers[i]);
            }
            layerMask.value = mask;

            return layerMask;
        }
    }
}
