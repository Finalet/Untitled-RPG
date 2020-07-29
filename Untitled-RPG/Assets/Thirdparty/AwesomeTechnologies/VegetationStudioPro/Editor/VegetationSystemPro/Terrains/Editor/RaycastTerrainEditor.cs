using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    [CustomEditor(typeof(RaycastTerrain))]
    public class RaycastTerrainEditor : VegetationStudioProBaseEditor
    {
        private RaycastTerrain _raycastTerrain;
        static List<int> layerNumbers = new List<int>();
        public override void OnInspectorGUI()
        {
            _raycastTerrain = (RaycastTerrain) target;
            OverrideLogoTextureName = "Banner_RaycastTerrain";
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Runtime terrain loading/Floating origin", LabelStyle);
            _raycastTerrain.AutoAddToVegegetationSystem =
                EditorGUILayout.Toggle("Add/Remove at Enable/Disable", _raycastTerrain.AutoAddToVegegetationSystem);
            EditorGUILayout.HelpBox("When set the terrain will add and remove itself when enabled/disabled. This will only work if automatic area calculation is disabled on the VegetationSystem. This is only done in playmode and builds", MessageType.Info);
            //_raycastTerrain.ApplyFloatingOriginOffset = EditorGUILayout.Toggle("Apply floating origin at enable.", _raycastTerrain.ApplyFloatingOriginOffset);
            //EditorGUILayout.HelpBox("Disable this if the loaded terrain is instantiated at original location and then moved to match floating origin.", MessageType.Info);
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                SetSceneDirty();
            }

            GUILayout.BeginVertical("box");
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Vector3Field("Terrain position", _raycastTerrain.TerrainPosition);
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings", LabelStyle);
            EditorGUI.BeginChangeCheck();
            Bounds oldBounds = _raycastTerrain.RaycastTerrainBounds;
            _raycastTerrain.RaycastTerrainBounds =EditorGUILayout.BoundsField("Area", _raycastTerrain.RaycastTerrainBounds);
            _raycastTerrain.RaycastLayerMask = LayerMaskField("Ground layers", _raycastTerrain.RaycastLayerMask);
            if (EditorGUI.EndChangeCheck())
            {
                oldBounds.Encapsulate(_raycastTerrain.RaycastTerrainBounds);
                _raycastTerrain.RefreshTerrain(oldBounds);

                SetSceneDirty();
            }

            GUILayout.EndVertical();
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            _raycastTerrain.TerrainSourceID = (TerrainSourceID)EditorGUILayout.EnumPopup("Terrain Source ID", _raycastTerrain.TerrainSourceID);
            EditorGUILayout.HelpBox("The Terrain Source ID can be set different on each terrain and used for spawning rules.", MessageType.Info);
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                _raycastTerrain.RefreshTerrain();
                SetSceneDirty();
            }

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Info", LabelStyle);
            EditorGUILayout.HelpBox("The raycast terrain is designed to be in a fixed location. Except for floating origin movement it should not be moved run-time or follow cameras or characters. ", MessageType.Info);
            GUILayout.EndVertical();
        }

        void SetSceneDirty()
        {
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(_raycastTerrain.gameObject.scene);
                EditorUtility.SetDirty(_raycastTerrain);
            }
        }


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