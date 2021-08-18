using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>Animation Tags for Malbers</summary>
    //[CreateAssetMenu(menuName = "Malbers Animations/Scriptables/AnimationTag", fileName = "New AnimationTag", order = 1000)]
    public class AnimationTag : IDs
    {
        /// <summary> Re Calculate the ID on enable</summary>
        private void OnEnable() => ID = Animator.StringToHash(name);
    }
#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(AnimationTag))]
    public class AnimTagEd : UnityEditor.Editor
    {
        private void OnEnable()
        {
           var ID = serializedObject.FindProperty("ID");
            ID.intValue = target.name.GetHashCode();
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUILayout.HelpBox("Animation Tags ID are calculated using Animator.StringtoHash()", UnityEditor.MessageType.None);
            UnityEditor.EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            UnityEditor.EditorGUI.EndDisabledGroup();
        }
    }
#endif
}