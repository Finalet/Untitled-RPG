using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GoblinCampGenerator))]
public class GoblinCampGeneratorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GoblinCampGenerator camp = (GoblinCampGenerator)target;
        GUILayout.Space(10);
        if(GUILayout.Button("Generate Camp")) {
            camp.GenerateCamp();
        }
        if(GUILayout.Button("Generate Enemies")) {
            camp.PlaceEnemies();
        }
        GUILayout.Space(10);
        if(GUILayout.Button("Regenerate Details")) {
            camp.PlaceDetails();
        }
        GUILayout.Space(10);
        if(GUILayout.Button("Bake Camp")) {
            camp.BakeCamp();
        }
    }
}
