using AwesomeTechnologies.Common;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Grass
{

    [CustomEditor(typeof(Grass.GrassPatchGenerator))]
    public class GrassPatchGeneratorEditor : VegetationStudioProBaseEditor
    {
        [MenuItem("Window/Awesome Technologies/Add Grass Patch Generator")]
        static void AddPatchGenerator()
        {
            GameObject go = new GameObject {name = "Grass Patch Generator"};
            go.AddComponent<GrassPatchGenerator>();
        }

        private GrassPatchGenerator _grassPatchGenerator;

        public override void OnInspectorGUI()
        {
            HelpTopic = "grass-patch-generator";

            base.OnInspectorGUI();
            _grassPatchGenerator = (GrassPatchGenerator)target;

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Patch settings", LabelStyle);
            EditorGUI.BeginChangeCheck();

            _grassPatchGenerator.PlaneCount = EditorGUILayout.IntSlider("Plane count", _grassPatchGenerator.PlaneCount, 1, 30);
            _grassPatchGenerator.Size = EditorGUILayout.Slider("Size", _grassPatchGenerator.Size, 0.1f, 2f);


            EditorFunctions.FloatRangeField("Min/Max Scale", ref _grassPatchGenerator.MinScale,
                ref _grassPatchGenerator.MaxScale, 0.1f, 2f);

            _grassPatchGenerator.PlaneWidth = EditorGUILayout.Slider("Plane height", _grassPatchGenerator.PlaneWidth, 0f, 1f);
            _grassPatchGenerator.PlaneHeight = EditorGUILayout.Slider("Plane width", _grassPatchGenerator.PlaneHeight, 0f, 1f);


            //EditorFunctions.FloatRangeField("Min/Max Width", ref _grassPatchGenerator.PlaneWidth,
            //    ref _grassPatchGenerator.PlaneMaxWidth, 0.1f, 2f);

            //EditorFunctions.FloatRangeField("Min/Max Height", ref _grassPatchGenerator.PlaneHeight,
            //    ref _grassPatchGenerator.PlaneMaxHeight, 0.1f, 2f);

            //_grassPatchGenerator.PlaneWidth = EditorGUILayout.Slider("Width", _grassPatchGenerator.PlaneWidth, 0.1f, 2f);
            //_grassPatchGenerator.PlaneHeight = EditorGUILayout.Slider("Height", _grassPatchGenerator.PlaneHeight, 0.1f, 2f);
            EditorGUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Resolution", LabelStyle);
            _grassPatchGenerator.PlaneWidthSegments = EditorGUILayout.IntSlider("Width segments", _grassPatchGenerator.PlaneWidthSegments, 2, 8);
            _grassPatchGenerator.PlaneHeightSegments = EditorGUILayout.IntSlider("Height segments", _grassPatchGenerator.PlaneHeightSegments, 2, 8);
            EditorGUILayout.EndVertical();

            //GUILayout.BeginVertical("box");
            //EditorGUILayout.LabelField("View LOD of patch.", LabelStyle);
            //EditorGUI.BeginDisabledGroup(true);
            //_grassPatchGenerator.GrassPatchLod = (GrassPatchLod)EditorGUILayout.EnumPopup("Select LOD", _grassPatchGenerator.GrassPatchLod);            
            //EditorGUI.EndDisabledGroup();
            //EditorGUILayout.HelpBox("Not yet implemented", MessageType.Warning);
            //EditorGUILayout.EndVertical();


            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Bending", LabelStyle);
            _grassPatchGenerator.MinBendHeight = EditorGUILayout.Slider("Minimum bend height", _grassPatchGenerator.MinBendHeight, 0f, 1f);
            _grassPatchGenerator.MaxBendDistance = EditorGUILayout.Slider("Bend", _grassPatchGenerator.MaxBendDistance, 0.1f, 0.8f);
            _grassPatchGenerator.CurveOffset = EditorGUILayout.Slider("Curve", _grassPatchGenerator.CurveOffset, 0.1f, 0.8f);
            EditorGUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Mesh", LabelStyle);
            _grassPatchGenerator.GenerateBackside = EditorGUILayout.Toggle("Generate backside", _grassPatchGenerator.GenerateBackside);
            if (_grassPatchGenerator.GenerateBackside)
            {
                EditorGUILayout.HelpBox("With the Vegetation Studio custom grass shader backside is not needed. Will give unnecessary extra polygons", MessageType.Warning);
            }
            EditorGUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Grass texture", LabelStyle);
            _grassPatchGenerator.GrassTexture = (Texture2D)EditorGUILayout.ObjectField(_grassPatchGenerator.GrassTexture, typeof(Texture2D), false, GUILayout.Height(64), GUILayout.Width(64));
            EditorGUILayout.EndVertical();

            

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Shader settings", LabelStyle);
            _grassPatchGenerator.ColorTint1 = EditorGUILayout.ColorField("Tint color 1", _grassPatchGenerator.ColorTint1);
            _grassPatchGenerator.ColorTint2 = EditorGUILayout.ColorField("Tint color 2", _grassPatchGenerator.ColorTint2);
            _grassPatchGenerator.RandomDarkening = EditorGUILayout.Slider("Random darkening", _grassPatchGenerator.RandomDarkening, 0f, 1f);
            _grassPatchGenerator.RootAmbient = EditorGUILayout.Slider("Root ambient", _grassPatchGenerator.RootAmbient, 0f, 1f);
            _grassPatchGenerator.TextureCutoff = EditorGUILayout.Slider("Alpha cutoff", _grassPatchGenerator.TextureCutoff, 0f, 1f);

            if (EditorGUI.EndChangeCheck())
            {
                _grassPatchGenerator.UpdateTexture();
                _grassPatchGenerator.GenerateGrassPatch();
                EditorUtility.SetDirty(_grassPatchGenerator);
            }

            EditorGUILayout.EndVertical();
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Ambient occlusion", LabelStyle);
            //EditorGUI.BeginDisabledGroup(true);
            _grassPatchGenerator.AmbientOcclusion = EditorGUILayout.CurveField(_grassPatchGenerator.AmbientOcclusion, Color.green, new Rect(0, 0, 1, 1), GUILayout.Height(75));
            //EditorGUI.EndDisabledGroup();
            EditorGUILayout.HelpBox("Horizontal: min -> max height. Vertical: bottom: no ambient -> top: max ambient", MessageType.Info);
            EditorGUILayout.EndVertical();
           

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Wind bending", LabelStyle);
            _grassPatchGenerator.WindBend = EditorGUILayout.CurveField(_grassPatchGenerator.WindBend, Color.green, new Rect(0, 0, 1, 1), GUILayout.Height(75));
            EditorGUILayout.HelpBox("Horizontal: min -> max height. Vertical: bottom: do not bend -> top: max bend", MessageType.Info);

            _grassPatchGenerator.BakePhase = EditorGUILayout.Toggle("Include phase", _grassPatchGenerator.BakePhase);
            _grassPatchGenerator.BakeBend = EditorGUILayout.Toggle("Include bending", _grassPatchGenerator.BakeBend);
            _grassPatchGenerator.BakeAo = EditorGUILayout.Toggle("Include Ambient occlusion", _grassPatchGenerator.BakeAo);
            EditorGUILayout.LabelField("Vertex colors", LabelStyle);
            _grassPatchGenerator.ShowVertexColors = EditorGUILayout.Toggle("Show vertex colors", _grassPatchGenerator.ShowVertexColors);

            EditorGUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Random", LabelStyle);
            _grassPatchGenerator.RandomSeed = EditorGUILayout.IntSlider("Seed", _grassPatchGenerator.RandomSeed, 1, 100);
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                _grassPatchGenerator.GenerateGrassPatch();
                EditorUtility.SetDirty(_grassPatchGenerator);
            }
       
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Mesh info", LabelStyle);
            EditorGUILayout.LabelField("Verts: " + _grassPatchGenerator.GetMeshVertexCount().ToString(), LabelStyle);
            EditorGUILayout.LabelField("Tris: " + _grassPatchGenerator.GetMeshTriangleCount().ToString(), LabelStyle);
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Save prefab and add to scene"))
            {
                _grassPatchGenerator.BuildPrefab();
            }

            if (GUILayout.Button("Save prefab with LOD and add to scene"))
            {
                _grassPatchGenerator.BuildPrefabLod();
            }

            EditorUtility.SetDirty(_grassPatchGenerator);
        }

        void OnSceneGUI()
        {
            if (Event.current.type == EventType.Repaint)
            {
                HandleUtility.Repaint();
            }
        }
    }
}
