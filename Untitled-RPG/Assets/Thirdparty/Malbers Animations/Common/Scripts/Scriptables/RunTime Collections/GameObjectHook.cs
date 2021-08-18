using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    [DefaultExecutionOrder(-1001)]
    [AddComponentMenu("Malbers/Runtime Vars/Hook GameObject")]

    public class GameObjectHook : MonoBehaviour
    {

        [RequiredField,Tooltip("Scriptable Asset to Store this GameObject as a reference to avoid Scene Dependencies")]
        public GameObjectVar Hook;

        private void OnEnable() => UpdateHook();

        private void OnDisable()
        {
            if (Hook.Value == gameObject) DisableHook(); //Disable it only when is not this gameobject
        }

        public virtual void UpdateHook() => Hook.Value = gameObject;
        public virtual void DisableHook() => Hook.Value = null;
    }


#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(GameObjectHook)), UnityEditor.CanEditMultipleObjects]
    public class GameObjectHookEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            UnityEditor.EditorGUILayout.Space();
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("Hook"));
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}