using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.External.MapMagicInterface
{
    [CustomEditor(typeof(MapMagicInfiniteTerrain))]
    public class MapMagicInfiniteTerrainEditor : VegetationStudioProBaseEditor
    {
        //private MapMagicInfiniteTerrain _mapMagicInfiniteTerrain;
        public override void OnInspectorGUI()
        {
            HelpTopic = "map-magic-infinite-terrain";
            //_mapMagicInfiniteTerrain = (MapMagicInfiniteTerrain)target;
            base.OnInspectorGUI();
#if MAPMAGIC
            EditorGUILayout.HelpBox("Map Magic installed", MessageType.Info);
#else
            EditorGUILayout.HelpBox("Map Magic not detected", MessageType.Error);
#endif
            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("This component will add the UnityTerrain component to all run-time created terrains from MapMagic. Add it to any GameObject in the same scene as Vegetation Studio Pro", MessageType.Info);            
            GUILayout.EndVertical();
        }
    }
}