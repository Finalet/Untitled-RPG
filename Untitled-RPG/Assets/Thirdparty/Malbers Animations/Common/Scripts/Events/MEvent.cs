using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Events
{
    ///<summary>
    /// The list of listeners that this event will notify if it is Invoked. 
    /// Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple
    /// </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Event", fileName = "New Event Asset", order = 3000)]
    public class MEvent : ScriptableObject
    {
        /// <summary>The list of listeners that this event will notify if it is raised.</summary>
        private readonly List<MEventItemListener> eventListeners = new List<MEventItemListener>();


#if UNITY_EDITOR
        [TextArea(3, 10)]
        public string Description;
#endif
        public bool debug;

        public virtual void Invoke()
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked();

            if (debug) Debug.Log($"{name} Invoke()");
        }

        public virtual void Invoke(float value)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);

            DebugEvent(value);
        }
        public virtual void Invoke(FloatVar value)
        {
            float val = value;

            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(val);

            DebugEvent(value);

        }
        public virtual void Invoke(bool value)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);

            DebugEvent(value);
        }
        public virtual void Invoke(BoolVar value)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value.Value);

            DebugEvent(value.Value);
        }

        public virtual void Invoke(string value)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);

            DebugEvent(value);
        }

        public virtual void Invoke(StringVar value)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value.Value);

            DebugEvent(value.Value);
        }

        public virtual void Invoke(int value)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);

            DebugEvent(value);

        }

        public virtual void Invoke(IntVar value)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value.Value);

            DebugEvent(value.Value);

        }

        public virtual void Invoke(IDs value)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value.ID);

#if UNITY_EDITOR
            if (debug) Debug.Log($"{name} Invoke({value.name} - {value.ID})");
#endif
        }

        public virtual void Invoke(GameObject value)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);

            DebugEvent(value);
        }

        public virtual void Invoke(Transform value)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);

            DebugEvent(value);
        }

        public virtual void Invoke(Vector3 value)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);

            DebugEvent(value);
        }

        public virtual void Invoke(Vector2 value)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);

            DebugEvent(value);
        }

        public virtual void Invoke(Component value)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);

            DebugEvent(value);
        }


        public virtual void Invoke(Sprite value)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);

            DebugEvent(value);
        }

        public virtual void RegisterListener(MEventItemListener listener)
        {
            if (!eventListeners.Contains(listener)) eventListeners.Add(listener);
        }

        public virtual void UnregisterListener(MEventItemListener listener)
        {
            if (eventListeners.Contains(listener)) eventListeners.Remove(listener);
        }

        private void DebugEvent(object value)
        {
#if UNITY_EDITOR
            if (debug) Debug.Log($"<B>{name}</B> - Invoke({value})");
#endif
        }


        ////This is for Debugin porpuses
        #region Debuging Methods
        public virtual void LogDeb(string text) => Debug.Log(text);
        public virtual void Pause() => Debug.Break();
        public virtual void LogDeb(bool value) => Debug.Log(name + ": " + value);
        public virtual void LogDeb(Vector3 value) => Debug.Log(name + ": " + value);
        public virtual void LogDeb(int value) => Debug.Log(name + ": " + value);
        public virtual void LogDeb(float value) => Debug.Log(name + ": " + value);
        public virtual void LogDeb(object value) => Debug.Log(name + ": " + value);
        public virtual void LogDeb(Component value) => Debug.Log(name + ": " + value);
        #endregion

#if UNITY_EDITOR
        [HideInInspector] public IntReference m_int;
        [HideInInspector] public FloatReference m_float;
        [HideInInspector] public StringReference m_string;
        [HideInInspector] public BoolReference m_bool;
        [HideInInspector] public Vector2Reference m_V2;
        [HideInInspector] public Vector3Reference m_V3;
        [HideInInspector] public GameObjectReference m_go;

        [HideInInspector] public TransformReference m_transform;
        [HideInInspector] public Sprite m_Sprite;
#endif
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(MEvent))]
    public class MEventEditor : Editor
    {
        public static GUIStyle StyleBlue => MTools.Style(new Color(0, 0.5f, 1f, 0.3f));

        SerializedProperty
            Description, debug, m_int, m_float, m_string, m_bool, m_V2, m_V3, m_go, m_transform, m_Sprite;



        MEvent ev;
        private void OnEnable()
        {
            ev = (MEvent)target;
            Description = serializedObject.FindProperty("Description");
            debug = serializedObject.FindProperty("debug");
            m_int = serializedObject.FindProperty("m_int");
            m_float = serializedObject.FindProperty("m_float");
            m_string = serializedObject.FindProperty("m_string");
            m_bool = serializedObject.FindProperty("m_bool");
            m_V2 = serializedObject.FindProperty("m_V2");
            m_V3 = serializedObject.FindProperty("m_V3");
            m_go = serializedObject.FindProperty("m_go");
            m_transform = serializedObject.FindProperty("m_transform");
            m_Sprite = serializedObject.FindProperty("m_Sprite");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Description");
            MalbersEditor.DrawDebugIcon(debug);
            //debug.boolValue = GUILayout.Toggle(debug.boolValue, "Debug", EditorStyles.miniButton);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(Description, GUIContent.none);


            if (debug.boolValue && Application.isPlaying)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                var w = 60;

                if (GUILayout.Button("Invoke Void")) { ev.Invoke(); }


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(m_bool);
                if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_bool); }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(m_int);
                if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_int); }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(m_float);
                if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_float); }
                EditorGUILayout.EndHorizontal();



                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(m_string);
                if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_string); }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(m_V2);
                if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_V2.Value); }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(m_V3);
                if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_V3.Value); }
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(m_go);
                if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_go.Value); }
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(m_transform);
                if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_transform.Value); }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(m_Sprite);
                if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_Sprite); }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
