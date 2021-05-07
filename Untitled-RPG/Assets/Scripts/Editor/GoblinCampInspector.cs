using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GoblinCamp))]
public class GoblinCampInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GoblinCamp camp = (GoblinCamp)target;
        GUILayout.Space(10);
        if(GUILayout.Button("Generate Camp")) {
            camp.GenerateCamp();
        }
    }
}
