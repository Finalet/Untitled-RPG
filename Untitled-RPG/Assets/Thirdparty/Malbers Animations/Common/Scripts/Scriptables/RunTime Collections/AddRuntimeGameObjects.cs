using UnityEngine;



namespace MalbersAnimations.Scriptables
{
    [AddComponentMenu("Malbers/Runtime Vars/Add Runtime GameObjects")]
    public class AddRuntimeGameObjects : MonoBehaviour
    {
        [CreateScriptableAsset] public RuntimeGameObjects Collection;

        private void OnEnable() => Collection?.Item_Add(gameObject);

        private void OnDisable() => Collection?.Item_Remove(gameObject);
    }


#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(AddRuntimeGameObjects)), UnityEditor.CanEditMultipleObjects]
    public class AddRuntimeGameObjectsEditor : UnityEditor.Editor
    {
        public static GUIStyle StyleBlue => MTools.Style(new Color(0, 0.5f, 1f, 0.3f));
        AddRuntimeGameObjects M;

        private void OnEnable()
        {
            M = (AddRuntimeGameObjects)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var Collection = serializedObject.FindProperty("Collection");
            UnityEditor.EditorGUILayout.PropertyField(Collection);

            if (M.Collection && !string.IsNullOrEmpty(M.Collection.Description))
            {
                UnityEditor.EditorGUILayout.BeginVertical(StyleBlue); ;
                UnityEditor.EditorGUILayout.HelpBox(M.Collection.Description, UnityEditor.MessageType.None);
                UnityEditor.EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}