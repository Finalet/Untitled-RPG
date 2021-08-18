using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    [DefaultExecutionOrder(-1000)]
    [AddComponentMenu("Malbers/Runtime Vars/Hook Transform")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/global-components/scriptables/transform-hook")]
    public class TransformHook : MonoBehaviour
    {
        [Tooltip("Transform that it will be saved on the Transform var asset")]
        public Transform Reference;
      
        [Tooltip("Transform Scritable var that will store at runtime a transform")]
        [CreateScriptableAsset] public TransformVar Hook;

        private void OnEnable()
        {
            UpdateHook();
        }

        private void OnDisable()
        {
            if (Hook.Value == Reference) DisableHook();  //Disable it only when is not this transform
        }

        private void OnValidate()
        {
            if (Reference == null) Reference = transform;
        }

        public virtual void UpdateHook() => Hook.Value = Reference;
        public virtual void DisableHook() => Hook.Value = null;
    }


#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(TransformHook)), UnityEditor.CanEditMultipleObjects]
    public class TransformHookEditor : UnityEditor.Editor
    {
        UnityEditor.SerializedProperty Hook, Reference;

        private void OnEnable()
        {
            Hook = serializedObject.FindProperty("Hook");
            Reference = serializedObject.FindProperty("Reference");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            UnityEditor.EditorGUILayout.Space();
            UnityEditor.EditorGUILayout.PropertyField(Hook, new GUIContent("Hook","Scriptable Asset to store the Reference Transform. Used to avoid scene dependencies"));
            UnityEditor.EditorGUILayout.PropertyField(Reference);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}