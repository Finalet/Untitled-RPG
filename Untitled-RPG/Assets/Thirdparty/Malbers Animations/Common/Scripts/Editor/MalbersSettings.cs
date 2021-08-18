using UnityEngine;
using UnityEditor;

/// <summary> This Class is use for creating Layers and Tags </summary>
namespace MalbersAnimations
{
    [InitializeOnLoad]
    public class MalbersSettings : Editor
    {
#if UNITY_EDITOR

        static MalbersSettings()
        {
            CreateLayer("Animal", 20);
            CreateLayer("Enemy", 23);
            CreateLayer("Item", 30);
            CreateTag("Fly");
            CreateTag("Climb");
            CreateTag("Stair");
        }

        static void CreateLayer(string LayerName, int index)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            if (layers == null || !layers.isArray)
            {
                Debug.LogWarning("Can't set up the layers.  It's possible the format of the layers and tags data has changed in this version of Unity.");
                Debug.LogWarning("Layers is null: " + (layers == null));
                return;
            }

            if (LayerMask.GetMask(LayerName) == 0)
            {
                var layerEnemy = layers.GetArrayElementAtIndex(index);

                if (layerEnemy.stringValue == string.Empty)
                {
                    Debug.Log("Setting up layers.  Layer " + "["+index+"]" + " is now called " + "["+ LayerName + "]");
                    layerEnemy.stringValue = LayerName;
                    tagManager.ApplyModifiedProperties();
                }
            }
        }

        static void CreateTag(string s)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            // First check if it is not already present
            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(s)) { found = true; break; }
            }

            // if not found, add it
            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(0);
                SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
                n.stringValue = s;
                Debug.Log("Tag: <B>"+s+"</B> Added");
                tagManager.ApplyModifiedProperties();
            }
        }
#endif
    }
}