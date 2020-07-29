using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AwesomeTechnologies.Grass
{
    [CustomEditor(typeof(WindController))]
    public class WindControllerEditor : VegetationStudioProBaseEditor
    {
        private WindController _windController;
        public override void OnInspectorGUI()
        {
            _windController = (WindController) target;
            base.OnInspectorGUI();

            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("The wind controller component is used to control the wind for Vegetation Studio grass patches. You need this for scenes that does not have a VegetationSystem component.", MessageType.Info);
            GUILayout.EndVertical();


            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings", LabelStyle);

            _windController.WindZone  = EditorGUILayout.ObjectField("WindZone", _windController.WindZone, typeof(WindZone), true) as WindZone;

            if (!_windController.WindZone)
            {
                EditorGUILayout.HelpBox("No directional wind zone found. Select one.", MessageType.Error);
            }
            _windController.WindWavesTexture = (Texture2D)EditorGUILayout.ObjectField("Wind wave noise texture", _windController.WindWavesTexture, typeof(Texture2D), false);
            _windController.WindSpeedFactor = EditorGUILayout.Slider("Wind speed factor", _windController.WindSpeedFactor, 0, 5);
            _windController.WindWavesSize = EditorGUILayout.Slider("Wind wave size", _windController.WindWavesSize, 0, 30);
            GUILayout.EndVertical();
        }
    }
}

