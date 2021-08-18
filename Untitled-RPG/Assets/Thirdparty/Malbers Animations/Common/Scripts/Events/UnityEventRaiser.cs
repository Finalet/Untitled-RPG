using MalbersAnimations.Scriptables;
using UnityEngine;
using UnityEngine.Serialization;

namespace MalbersAnimations.Events
{
    /// <summary>Simple Event Raiser On Enable</summary>
    [AddComponentMenu("Malbers/Events/Unity Event Raiser")]
    public class UnityEventRaiser : UnityUtils
    {
        [Tooltip("Delayed time for invoking the Events, or the Repeated time  when Repeat is enable")]
        public FloatReference Delayed = new FloatReference();
        public FloatReference RepeatTime = new FloatReference();
        public bool Repeat;


        [FormerlySerializedAs("OnEnableEvent")]
        public UnityEngine.Events.UnityEvent onEnable;


        public string Description = "";
        [HideInInspector] public bool ShowDescription = false;
        [ContextMenu("Show Description")]
        internal void EditDescription() => ShowDescription ^= true;

        public void OnEnable()
        {
            if (Repeat && RepeatTime > 0f)
            {
                InvokeRepeating(nameof(StartEvent), Delayed, RepeatTime);
            }
            else if (Delayed > 0)
            {
                Invoke(nameof(StartEvent), Delayed);
            }
            else
            {
                onEnable.Invoke();
            }
        }

        private void StartEvent() => onEnable.Invoke();

        private void OnDisable()
        {
            CancelInvoke();
            StopAllCoroutines();
        }


        public virtual void Restart()
        {
            CancelInvoke();
            OnEnable();
        }
    } 


#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(UnityEventRaiser)),UnityEditor.CanEditMultipleObjects] 
    public class UnityEventRaiserInspector : UnityEditor.Editor
    {
        UnityEditor.SerializedProperty Delayed, Repeat, RepeatTime, OnEnableEvent, ShowDescription, Description;
        public static GUIStyle StyleBlue => Style(new Color(0, 0.5f, 1f, 0.3f));
        private GUIStyle style;


        private void OnEnable()
        {
            Delayed = serializedObject.FindProperty("Delayed");
            ShowDescription = serializedObject.FindProperty("ShowDescription");
            Description = serializedObject.FindProperty("Description");
            Repeat = serializedObject.FindProperty("Repeat");
            RepeatTime = serializedObject.FindProperty("RepeatTime");
            OnEnableEvent = serializedObject.FindProperty("onEnable");
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

                UnityEditor.EditorGUILayout.BeginVertical(StyleBlue);
                Description.stringValue = UnityEditor.EditorGUILayout.TextArea(Description.stringValue, style);
                UnityEditor.EditorGUILayout.EndVertical();
            }


            UnityEditor.EditorGUILayout.BeginHorizontal();

            UnityEditor.EditorGUILayout.PropertyField(Delayed, GUILayout.MinWidth(100));
            if (Repeat.boolValue)
            {

                UnityEditor.EditorGUIUtility.labelWidth = 35;
                UnityEditor.EditorGUILayout.PropertyField(RepeatTime, new GUIContent(" RT", "Repeat Time"), GUILayout.MinWidth(40));
                UnityEditor.EditorGUIUtility.labelWidth = 0;
            }

            Repeat.boolValue = GUILayout.Toggle(Repeat.boolValue, new GUIContent("R","Repeat"), UnityEditor.EditorStyles.miniButton, GUILayout.Width(25));
            UnityEditor.EditorGUILayout.EndHorizontal();
            UnityEditor.EditorGUILayout.PropertyField(OnEnableEvent);
            serializedObject.ApplyModifiedProperties();
        }

        public static GUIStyle Style(Color color)
        {
            GUIStyle currentStyle = new GUIStyle(GUI.skin.box) { border = new RectOffset(-1, -1, -1, -1) };
            Color[] pix = new Color[1];
            pix[0] = color;
            Texture2D bg = new Texture2D(1, 1);
            bg.SetPixels(pix);
            bg.Apply();

            currentStyle.normal.background = bg;
            // MW 04-Jul-2020: Check if system supports newer graphics formats used by Unity GUI
            Texture2D bgActual = currentStyle.normal.scaledBackgrounds[0];

            if (SystemInfo.IsFormatSupported(bgActual.graphicsFormat, UnityEngine.Experimental.Rendering.FormatUsage.Sample) == false)
            {
                currentStyle.normal.scaledBackgrounds = new Texture2D[] { }; // This can't be null
            }
            return currentStyle;
        }
    }
#endif
}