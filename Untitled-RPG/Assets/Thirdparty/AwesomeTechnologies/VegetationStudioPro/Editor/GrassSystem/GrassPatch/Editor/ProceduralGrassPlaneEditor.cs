using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Grass
{
    [CustomEditor(typeof(AwesomeTechnologies.Grass.ProceduralGrassPlane))]
    public class ProceduralGrassPlaneEditor : Editor
    {
        private ProceduralGrassPlane _proceduralGrassPlane;

        public override void OnInspectorGUI()
        {
            _proceduralGrassPlane = (AwesomeTechnologies.Grass.ProceduralGrassPlane)target;
             
            if (GUILayout.Button("GeneratePlane"))
            {
                _proceduralGrassPlane.CreateGrassPlane();
            }

            DrawDefaultInspector();
        }
    }

}
