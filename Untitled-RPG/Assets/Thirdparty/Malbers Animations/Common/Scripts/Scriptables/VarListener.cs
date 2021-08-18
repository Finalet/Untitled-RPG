using UnityEngine;

namespace MalbersAnimations
{
    using Scriptables;
    public abstract class VarListener : MonoBehaviour
    {
        [HideInInspector] public bool ShowEvents = false;
        
        [Tooltip("ID value is used on the AI Brain to know which Var Listener is picked, in case there more than one on one Game Object")]
        public IntReference ID;

        public bool Enable => gameObject.activeInHierarchy && enabled;

        [Tooltip("The Events will be invoked when the Listener Value changes.\n" +
            "If is set to false, call Invoke() to invoke the events manually")]
        public bool Auto = true;

        public string Description = "";
        [HideInInspector] public bool ShowDescription = false;
        [ContextMenu("Show Description")]
        internal void EditDescription() => ShowDescription ^= true;

        public bool debug = false;
    }


    //INSPECTOR
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(VarListener))]
    public class VarListenerEditor : UnityEditor.Editor
    {
        protected UnityEditor.SerializedProperty value, Description, Index, ShowEvents, ShowDescription, Debug, Auto;
        protected GUIStyle style;

        void OnEnable()    { SetEnable(); }

        protected void SetEnable()
        {
            value = serializedObject.FindProperty("value");
            Description = serializedObject.FindProperty("Description");
            ShowDescription = serializedObject.FindProperty("ShowDescription");
            Index = serializedObject.FindProperty("ID");
            ShowEvents = serializedObject.FindProperty("ShowEvents");
            Debug = serializedObject.FindProperty("debug");
            Auto = serializedObject.FindProperty("Auto");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (ShowDescription.boolValue)
            {
                if (style == null)
                    style = new GUIStyle(UnityEditor.EditorStyles.helpBox)
                    {
                        fontSize = 12,
                        fontStyle = FontStyle.Bold,
                    };

                UnityEditor.EditorGUILayout.BeginVertical(MalbersEditor.StyleBlue);
                Description.stringValue = UnityEditor.EditorGUILayout.TextArea(Description.stringValue, style);
                UnityEditor.EditorGUILayout.EndVertical();
            }


            UnityEditor.EditorGUILayout.BeginHorizontal(UnityEditor.EditorStyles.helpBox);
            UnityEditor.EditorGUIUtility.labelWidth = 55;
            UnityEditor.EditorGUILayout.PropertyField(value, GUILayout.MinWidth(25));
            UnityEditor.EditorGUIUtility.labelWidth = 35;
            UnityEditor.EditorGUILayout.PropertyField(Index, GUILayout.MinWidth(25), GUILayout.MaxWidth(100));
            UnityEditor.EditorGUIUtility.labelWidth = 0;
            ShowEvents.boolValue = GUILayout.Toggle(ShowEvents.boolValue, new GUIContent("E", "Show Events"), UnityEditor.EditorStyles.miniButton, GUILayout.Width(22));
            UnityEditor.EditorGUILayout.EndHorizontal();

            if (ShowEvents.boolValue)
            {
                UnityEditor.EditorGUILayout.BeginHorizontal(UnityEditor.EditorStyles.helpBox);
                UnityEditor.EditorGUIUtility.labelWidth = 55;
                UnityEditor.EditorGUILayout.PropertyField(Auto);
                UnityEditor.EditorGUIUtility.labelWidth = 0;
                MalbersEditor.DrawDebugIcon(Debug);
                //Debug.boolValue = GUILayout.Toggle(Debug.boolValue, new GUIContent("D"), UnityEditor.EditorStyles.miniButton, GUILayout.Width(22));
                UnityEditor.EditorGUILayout.EndHorizontal();

                DrawEvents();
            }
            serializedObject.ApplyModifiedProperties();
        }


        protected virtual void DrawEvents()  {
          
        }
    }
#endif
}