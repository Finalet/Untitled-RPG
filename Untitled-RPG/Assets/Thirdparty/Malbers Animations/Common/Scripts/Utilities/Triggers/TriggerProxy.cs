using UnityEngine;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Utilities
{
    /// <summary>
    /// This is used when the collider is in a different gameObject and you need to check the Collider Events
    /// Create this component at runtime and subscribe to the UnityEvents
    /// </summary>
    [AddComponentMenu("Malbers/Utilities/Colliders/Trigger Proxy")]
    public class TriggerProxy : MonoBehaviour, IMLayer
    {
        //[Tooltip("Proxy ID, can be used to Identify which is the Proxy Trigger used")]
        //[SerializeField] private IntReference m_ID = new IntReference(0);
        [Tooltip("Hit Layer for the Trigger Proxy")]
        [SerializeField] private LayerReference hitLayer = new LayerReference(-1);
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
        [Tooltip("Search only Tags")]
        public Tag[] Tags;

        public ColliderEvent OnTrigger_Enter = new ColliderEvent();
        public ColliderEvent OnTrigger_Exit = new ColliderEvent();

        public GameObjectEvent OnGameObjectEnter = new GameObjectEvent();
        public GameObjectEvent OnGameObjectExit = new GameObjectEvent();

        [SerializeField] private bool m_debug = false;

        internal List<Collider> m_colliders = new List<Collider>();

        /// <summary>All the Gameobjects using the Proxy</summary>
        internal List<GameObject> EnteringGameObjects = new List<GameObject>();

        public bool Active { get => enabled; set => enabled = value; }

        //public int ID { get => m_ID.Value; set => m_ID.Value = value; }
        public LayerMask Layer { get => hitLayer.Value; set => hitLayer.Value = value; }
        public QueryTriggerInteraction TriggerInteraction { get => triggerInteraction; set => triggerInteraction = value; }

        /// <summary> Collider Component used for the Trigger Proxy </summary>
        public Collider OwnCollider { get; private set; }

        [HideInInspector] public bool ShowEvents = true;

        public bool TrueConditions(Collider other)
        {
            if (!Active) return false;

            if (Tags != null && Tags.Length > 0)
            {
                if (!other.gameObject.HasMalbersTag(Tags)) return false;
            }

            if (OwnCollider == null) return false; // you are 
            if (triggerInteraction == QueryTriggerInteraction.Ignore && other.isTrigger) return false;
            if (!MTools.Layer_in_LayerMask(other.gameObject.layer, Layer)) return false;
            if (transform.IsChildOf(other.transform)) return false; // you are 

            return true;
        } 
 

        void OnTriggerEnter(Collider other)
        {
            if (TrueConditions(other))
            {
                var newGo = other.transform.root.gameObject;                              //Get the animal on the entering collider

                OnTrigger_Enter.Invoke(other); //Invoke when a Collider enters the Trigger
                if (m_debug) Debug.Log($"<b>{name}</b> [Entering Collider] -> [{other.name}]");


                //Check Recently destroyed Colliders (Strange bug)
                CheckMissingColliders();

                if (m_colliders.Find(coll => coll == other) == null)                               //if the entering collider is not already on the list add it
                    m_colliders.Add(other);

                if (EnteringGameObjects.Contains(newGo))
                {
                    return;
                }
                else
                {
                    EnteringGameObjects.Add(newGo);
                    OnGameObjectEnter.Invoke(newGo);
                    if (m_debug) Debug.Log($"<b>{name}</b> [Entering GameObject] -> [{newGo.name}]");
                }
            }
        }


        /// <summary>
        ///  Check Recently destroyed Colliders (Strange bug)
        /// </summary>
        private void CheckMissingColliders()
        {
            for (var i = m_colliders.Count - 1; i > -1; i--)
            {
                if (m_colliders[i] == null) m_colliders.RemoveAt(i);
            }

            if (m_colliders.Count == 0) EnteringGameObjects = new List<GameObject>();
        }

        void OnTriggerExit(Collider other)
        {
            if (TrueConditions(other))
            {
                OnTrigger_Exit.Invoke(other);
                m_colliders.Remove(other);

                if (m_debug) Debug.Log($"<b>{name}</b> [Exit Collider] -> [{other.name}]");

                var newGo = other.transform.root.gameObject;         //Get the gameObject on the entering collider

                if (EnteringGameObjects.Contains(newGo))             //Means that the Entering GameObject still exist
                {
                    if (!m_colliders.Exists(x => x.transform.root.gameObject == newGo)) //Means that all that root colliders are out
                    {
                        EnteringGameObjects.Remove(newGo);
                        OnGameObjectExit.Invoke(newGo);
                        if (m_debug) Debug.Log($"<b>{name}</b> - [Leaving Gameobject] -> [{newGo.name}]");
                    }
                }

              CheckMissingColliders();
            }
        }

        public void ResetTrigger()
        {
            m_colliders = new List<Collider>();
            EnteringGameObjects = new List<GameObject>();
        }

        private void OnDisable() => ResetTrigger();
        private void OnEnable() => ResetTrigger();

        private void Start()
        {
            OwnCollider = GetComponent<Collider>();

            if (OwnCollider) OwnCollider.isTrigger = true;
            else
                Debug.LogWarning("This Script requires a Collider, please add any type of collider");

            ResetTrigger();
        }

        public void SetLayer(LayerMask mask, QueryTriggerInteraction triggerInteraction)
        {
            TriggerInteraction = triggerInteraction;
            Layer = mask;
        }
    }

    #region Inspector


#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(TriggerProxy))]
    public class TriggerProxyEditor : Editor
    {
        SerializedProperty debug, OnTrigger_Enter, OnTrigger_Exit, ShowEvents, triggerInteraction, hitLayer, OnGameObjectEnter, OnGameObjectExit, Tags;

        TriggerProxy m;

        private void OnEnable()
        {
            m = (TriggerProxy)target;

         //   m_ID = serializedObject.FindProperty("m_ID");
            triggerInteraction = serializedObject.FindProperty("triggerInteraction");
            hitLayer = serializedObject.FindProperty("hitLayer");
            debug = serializedObject.FindProperty("m_debug");
            OnTrigger_Enter = serializedObject.FindProperty("OnTrigger_Enter");
            OnTrigger_Exit = serializedObject.FindProperty("OnTrigger_Exit");
            OnGameObjectEnter = serializedObject.FindProperty("OnGameObjectEnter");
            OnGameObjectExit = serializedObject.FindProperty("OnGameObjectExit");
            ShowEvents = serializedObject.FindProperty("ShowEvents");
            Tags = serializedObject.FindProperty("Tags");
        }


        public override void OnInspectorGUI()
        {
            MalbersEditor.DrawDescription("Use this component to do quick OnTrigger Enter/Exit logics");

            serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(hitLayer, new GUIContent("Layer"));
            MalbersEditor.DrawDebugIcon(debug);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(triggerInteraction);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(Tags,true);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (MalbersEditor.Foldout(ShowEvents, "Events"))
            {
                EditorGUILayout.PropertyField(OnTrigger_Enter, new GUIContent("On Trigger Enter"));
                EditorGUILayout.PropertyField(OnTrigger_Exit, new GUIContent("On Trigger Exit"));
                EditorGUILayout.PropertyField(OnGameObjectEnter, new GUIContent("On GameObject Enter "));
                EditorGUILayout.PropertyField(OnGameObjectExit, new GUIContent("On GameObject Exit"));
            }

            EditorGUILayout.EndVertical();

            if (Application.isPlaying && debug.boolValue)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);

                EditorGUILayout.ObjectField("Own Collider", m.OwnCollider, typeof(Collider), false);

                EditorGUILayout.LabelField("Entering GameObjects (" + m.EnteringGameObjects.Count + ")", EditorStyles.boldLabel);
                foreach (var item in m.EnteringGameObjects)
                {
                    EditorGUILayout.ObjectField(item.name, item, typeof(GameObject), false);
                }

                EditorGUILayout.LabelField("Entering Colliders (" + m.m_colliders.Count + ")", EditorStyles.boldLabel);
               
                foreach (var item in m.m_colliders)
                {
                    if (item != null)
                    EditorGUILayout.ObjectField(item.name, item, typeof(Collider), false);
                }

                EditorGUILayout.EndVertical();
                Repaint();
                EditorGUI.EndDisabledGroup();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
    #endregion
}