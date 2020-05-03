using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

[CustomEditor(typeof(ES3ReferenceMgr))]
[System.Serializable]
public class ES3ReferenceMgrEditor : Editor
{
    public bool isDraggingOver = false;

    public override void OnInspectorGUI() 
    {
        EditorGUILayout.HelpBox("This allows Easy Save to maintain references to objects in your scene.\n\nIt is automatically updated when you enter Playmode or build your project.", MessageType.Info);

        var mgr = (ES3ReferenceMgr)serializedObject.targetObject;


        if (Application.productName == "ES3 Development")
        {
            EditorGUILayout.LabelField("References foldout is only shown");
            EditorGUILayout.LabelField("in the ES3 Development project");
            mgr.openReferences = EditorGUILayout.Foldout(mgr.openReferences, "References");
            // Make foldout drag-and-drop enabled for objects.
            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                Event evt = Event.current;

                switch (evt.type)
                {
                    case EventType.DragUpdated:
                    case EventType.DragPerform:
                        isDraggingOver = true;
                        break;
                    case EventType.DragExited:
                        isDraggingOver = false;
                        break;
                }

                if (isDraggingOver)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        Undo.RecordObject(mgr, "Add References to Easy Save 3 Reference List");
                        foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
                            mgr.Add(obj);
                        // Return now because otherwise we'll change the GUI during an event which doesn't allow it.
                        return;
                    }
                }
            }

            if (mgr.openReferences)
            {
                EditorGUI.indentLevel++;

                var keys = mgr.idRef.Keys;
                var values = mgr.idRef.Values;

                foreach (var kvp in mgr.idRef)
                {
                    EditorGUILayout.BeginHorizontal();

                    var value = EditorGUILayout.ObjectField(kvp.Value, typeof(UnityEngine.Object), true);
                    var key = EditorGUILayout.LongField(kvp.Key);

                    EditorGUILayout.EndHorizontal();

                    if (value != kvp.Value || key != kvp.Key)
                    {
                        Undo.RecordObject(mgr, "Change Easy Save 3 References");
                        // If we're deleting a value, delete it.
                        if (value == null)
                            mgr.Remove(key);
                        // Else, update the ID.
                        else
                            mgr.ChangeId(kvp.Key, key);
                        // Break, as removing or changing Dictionary items will make the foreach out of sync.
                        break;
                    }
                }

                EditorGUI.indentLevel--;
            }

            mgr.openPrefabs = EditorGUILayout.Foldout(mgr.openPrefabs, "ES3Prefabs");
            if (mgr.openPrefabs)
            {
                EditorGUI.indentLevel++;

                foreach (var prefab in mgr.prefabs)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.ObjectField(prefab, typeof(UnityEngine.Object), true);

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
            }
        }

        if(GUILayout.Button("Refresh References"))
        {
            mgr.RefreshDependencies();
            mgr.GeneratePrefabReferences();
        }

        if (GUILayout.Button("Optimize References"))
        {
            mgr.Optimize();
        }
    }

    [MenuItem("GameObject/Easy Save 3/Add Manager to Scene", false, 1002)]
    [MenuItem("Assets/Easy Save 3/Add Manager to Scene", false, 1002)]
    public static void EnableForScene()
    {
        var scene = SceneManager.GetActiveScene();
        if(!scene.isLoaded)
            EditorUtility.DisplayDialog("Could not add manager to scene", "Could not add Easy Save 3 Manager to scene because there is not currently a scene open.", "Ok");
        Selection.activeObject = ES3Postprocessor.AddManagerToScene();
    }

    [MenuItem("GameObject/Easy Save 3/Add Manager to Scene", true, 1002)]
    [MenuItem("Assets/Easy Save 3/Add Manager to Scene", true, 1002)]
    private static bool CanEnableForScene()
    {
        var scene = SceneManager.GetActiveScene();
        if(!scene.isLoaded)
            return false;
        if(UnityEngine.Object.FindObjectOfType<ES3ReferenceMgr>() != null)
            return false;
        return true;
    }
}
