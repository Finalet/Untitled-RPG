using UnityEngine;

namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Utilities/Tools/Comment")]
    /// <summary>Adding Coments on the Inspector</summary>.
    public class Comment : MonoBehaviour {[Multiline] public string text; }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(Comment))]
    public class CommentEditor : UnityEditor.Editor
    {
        private GUIStyle style;
        private UnityEditor.SerializedProperty text;

        private void OnEnable()
        {
            text = serializedObject.FindProperty("text");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (style == null)
            style = new GUIStyle(UnityEditor.EditorStyles.helpBox)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
            };

            UnityEditor.EditorGUILayout.BeginVertical(MTools.StyleBlue);
            text.stringValue = UnityEditor.EditorGUILayout.TextArea(text.stringValue, style);
            UnityEditor.EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}