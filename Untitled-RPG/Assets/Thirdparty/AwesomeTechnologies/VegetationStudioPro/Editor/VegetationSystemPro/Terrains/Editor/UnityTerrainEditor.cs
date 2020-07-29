using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    [CustomEditor(typeof(UnityTerrain))]
    [CanEditMultipleObjects]
    public class UnityTerrainEditor : VegetationStudioProBaseEditor
    {
        SerializedProperty _autoAddToVegegetationSystemProp;
        SerializedProperty _terrainPositionProp;
        SerializedProperty _terrainSourceIDProp;

        //SerializedProperty _disableUnityTreesIDProp;
        void OnEnable()
        {
            // Setup the SerializedProperties.
            _autoAddToVegegetationSystemProp = serializedObject.FindProperty("AutoAddToVegegetationSystem");
            _terrainPositionProp = serializedObject.FindProperty("TerrainPosition");
            _terrainSourceIDProp = serializedObject.FindProperty("TerrainSourceID");
            //_disableUnityTreesIDProp = serializedObject.FindProperty("DisableTerrainTreesAndDetails");
        }

        private UnityTerrain _unityTerrain;
        public override void OnInspectorGUI()
        {
            _unityTerrain = (UnityTerrain)target;

            OverrideLogoTextureName = "Banner_UnityTerrain";
            base.OnInspectorGUI();

            EditorGUILayout.HelpBox("The Unity Terrain component implements the interface needed and communication with the Vegetation System Pro component. ", MessageType.Info);
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            //_unityTerrain.AutoAddToVegegetationSystem = EditorGUILayout.Toggle("Add/Remove at Enable/Disable", _unityTerrain.AutoAddToVegegetationSystem);          
            EditorGUILayout.PropertyField(_autoAddToVegegetationSystemProp, new GUIContent("Add/Remove at Enable/Disable"));
            EditorGUILayout.HelpBox("When set the terrain will add and remove itself when enabled/disabled. This will only work if automatic area calculation is disabled on the VegetationSystem. This is only done in playmode and builds", MessageType.Info);
            //_unityTerrain.ApplyFloatingOriginOffset =  EditorGUILayout.Toggle("Apply floating origin at enable.", _unityTerrain.ApplyFloatingOriginOffset);
            //EditorGUILayout.HelpBox("Disable this if the loaded terrain is instantiated at original location and then moved to match floating origin.", MessageType.Info);
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {               
                SetSceneDirty();
            }

            GUILayout.BeginVertical("box");
            EditorGUI.BeginDisabledGroup(true);
            //EditorGUILayout.Vector3Field("Terrain position", _unityTerrain.TerrainPosition);
            EditorGUILayout.PropertyField(_terrainPositionProp, new GUIContent("Terrain position"));
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(_autoAddToVegegetationSystemProp, new GUIContent("Disable Unity trees/details"));
            EditorGUILayout.HelpBox("When enabled this will disable Unity trees and details at startup", MessageType.Info);
            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            //_unityTerrain.TerrainSourceID = (TerrainSourceID)EditorGUILayout.EnumPopup("Terrain Source ID", _unityTerrain.TerrainSourceID);
            EditorGUILayout.PropertyField(_terrainSourceIDProp, new GUIContent("Terrain Source ID"));
            EditorGUILayout.HelpBox("The Terrain Source ID can be set different on each terrain and used for spawning rules.", MessageType.Info);

            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                _unityTerrain.RefreshTerrainArea();
                SetSceneDirty();
            }

            serializedObject.ApplyModifiedProperties();
        }

        void SetSceneDirty()
        {
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(_unityTerrain.gameObject.scene);
                EditorUtility.SetDirty(_unityTerrain);
            }
        }
    }
}