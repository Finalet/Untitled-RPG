using UnityEngine;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{
    [AddComponentMenu("Malbers/Interaction/Pickable")]

    public class Pickable : MonoBehaviour , ICollectable
    {
      //  public enum CollectType { Collectable, Hold, OneUse } //For different types of collectable items? FOR ANOTHER UPDATE

        public bool Align = true;
        public bool AlignPos = true;
        public float AlignTime = 0.15f;
        public float AlignDistance = 1f;

        public FloatReference PickDelay = new FloatReference(0);
        public FloatReference DropDelay = new FloatReference(0);
        public FloatReference coolDown = new FloatReference(0f);
        public BoolReference m_Collectable = new BoolReference(false);
        public BoolReference m_ByAnimation = new BoolReference(false);
        public BoolReference m_DestroyOnPick = new BoolReference(false);

        public FloatReference m_Value = new FloatReference(1f); //Not done yet
        public BoolReference m_AutoPick = new BoolReference(false); //Not done yet
        public IntReference m_ID = new IntReference();         //Not done yet

        /// <summary>Who Did the Picking </summary>
        public GameObject Picker { get; set; }


        public BoolEvent OnFocused = new BoolEvent();
        public GameObjectEvent OnPicked = new GameObjectEvent();
        public GameObjectEvent OnPrePicked = new GameObjectEvent();
        public GameObjectEvent OnDropped = new GameObjectEvent();
        public GameObjectEvent OnPreDropped = new GameObjectEvent();

        [SerializeField] private Rigidbody rb;
        [RequiredField] public Collider m_collider;

        float currentPickTime;

        /// <summary>Is this Object being picked </summary>
        public bool IsPicked { get; set; }

        /// <summary>Current value of the Item</summary>
        public float Value { get => m_Value.Value; set => m_Value.Value = value; }

        /// <summary>The Item will be autopicked if the Picker is focusing it</summary>
        public bool AutoPick { get => m_AutoPick.Value; set => m_AutoPick.Value = value; }
        public bool Collectable { get => m_Collectable.Value; set => m_Collectable.Value = value; }

        /// <summary>The Pick Up Drop Logic will be called via animator events/messages</summary>
        public bool ByAnimation { get => m_ByAnimation.Value; set => m_ByAnimation.Value = value; }
        public bool DestroyOnPick { get => m_DestroyOnPick.Value; set => m_DestroyOnPick.Value = value; }
        public bool InCoolDown => !MTools.ElapsedTime(currentPickTime, coolDown);
        public int ID { get => m_ID.Value; set => m_ID.Value = value; }


        private bool focused;
        public bool Focused
        {
            get => focused;
            set => OnFocused.Invoke(focused = value);
        }

        private void OnDisable()
        {
            Focused = false;
        }

        private void Awake() 
        { 
            rb = GetComponent<Rigidbody>();
            currentPickTime = -coolDown;
        }

        public virtual void Pick()
        {
            if (rb)
            {
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.isKinematic = true;
            }

            m_collider.enabled = false;
            IsPicked = true;
            OnPicked.Invoke(Picker);
            currentPickTime = Time.time;
        }

        public virtual void Drop()
        {
            IsPicked = false;

            if (rb)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
            m_collider.enabled = true;

            var localScale = transform.localScale;
            transform.parent = null;
            transform.localScale = localScale;

            OnDropped.Invoke(Picker);

            Picker = null; //Reset who did the picking

            currentPickTime = Time.time;
        }

        [HideInInspector] public bool ShowEvents = true;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, AlignDistance);
        }

        private void Reset()
        {
            m_collider = GetComponent<Collider>();
            rb = GetComponent<Rigidbody>();
#if UNITY_EDITOR
            var EInteract = MTools.GetInstance<MEvent>("Interact UI");  

            if (EInteract)
            {
                UnityEditor.Events.UnityEventTools.AddObjectPersistentListener<Transform>(OnFocused, EInteract.Invoke, transform);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(OnFocused, EInteract.Invoke);
                UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(OnPicked, EInteract.Invoke,false);
            }
#endif
        }
#endif
    }


    //INSPECTOR
#if UNITY_EDITOR
    [CustomEditor(typeof(Pickable)), CanEditMultipleObjects]
    public class PickableEditor : Editor
    {
        private SerializedProperty //   PickAnimations, PickUpMode, PickUpAbility, DropMode, DropAbility,DropAnimations, 
            Align, AlignTime, AlignDistance, AlignPos, PickReaction, DropReacion, m_AutoPick, DropDelay, PickDelay,rb, CoolDown,
            OnFocused, OnPrePicked, OnPicked, OnDropped,OnPreDropped, ShowEvents, FloatID, IntID, m_collider, m_Collectable, m_ByAnimation, m_DestroyOnPick;

        private Pickable m;

        private void OnEnable()
        {
            m = (Pickable)target;

            PickReaction = serializedObject.FindProperty("PickReaction");
            rb = serializedObject.FindProperty("rb");
            DropReacion = serializedObject.FindProperty("DropReaction");
            PickDelay = serializedObject.FindProperty("PickDelay");
            DropDelay = serializedObject.FindProperty("DropDelay");
            m_Collectable = serializedObject.FindProperty("m_Collectable");
            m_ByAnimation = serializedObject.FindProperty("m_ByAnimation");
            m_DestroyOnPick = serializedObject.FindProperty("m_DestroyOnPick");


            Align = serializedObject.FindProperty("Align");
            AlignTime = serializedObject.FindProperty("AlignTime");
            AlignDistance = serializedObject.FindProperty("AlignDistance");
            OnFocused = serializedObject.FindProperty("OnFocused");
            OnPicked = serializedObject.FindProperty("OnPicked");
            OnPrePicked = serializedObject.FindProperty("OnPrePicked");
            OnDropped = serializedObject.FindProperty("OnDropped");
            OnPreDropped = serializedObject.FindProperty("OnPreDropped");
            ShowEvents = serializedObject.FindProperty("ShowEvents");
            FloatID = serializedObject.FindProperty("m_Value");
            IntID = serializedObject.FindProperty("m_ID");
            m_collider = serializedObject.FindProperty("m_collider");
            AlignPos = serializedObject.FindProperty("AlignPos");
            //Collectable = serializedObject.FindProperty("Collectable");
            m_AutoPick = serializedObject.FindProperty("m_AutoPick");
            CoolDown = serializedObject.FindProperty("coolDown");
        }

        public override void OnInspectorGUI()
        {
            MalbersEditor.DrawDescription("Pickable - Collectable Object");
            EditorGUILayout.BeginVertical(MTools.StyleGray);
            {
                serializedObject.Update();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(m_collider);
                EditorGUILayout.PropertyField(rb,new GUIContent("Rigid Body"));
                EditorGUILayout.PropertyField(m_AutoPick, new GUIContent("Auto Pick", "The Item will be Picked Automatically"));
                EditorGUILayout.PropertyField(m_Collectable, new GUIContent("Collectable", "The Item will Picked by the Pickable and it will be stored"));
                EditorGUILayout.PropertyField(m_ByAnimation, new GUIContent("By Animation", "The Item will Pre-Picked/Droped. Pick-Drop Logic is called by Animation Event|Message"));

                if (m.Collectable)
                EditorGUILayout.PropertyField(m_DestroyOnPick, new GUIContent("Destroy", "The Item will be destroyed after is picked"));
                //EditorGUILayout.PropertyField(Collectable, new GUIContent("Item Type", "The Type of logic this item will apply when is picked"));
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ToggleLeft("Is Picked", m.IsPicked);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUIUtility.labelWidth = 60;
                EditorGUILayout.PropertyField(IntID, new GUIContent("ID", "Int value the Pickable Item can store.. that it can be use for anything"));
                EditorGUILayout.PropertyField(FloatID, new GUIContent("Value", "Float value the Pickable Item can store.. that it can be use for anything"));
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                //  EditorGUILayout.PropertyField(PickReaction, new GUIContent("Pick Reaction", "The Reaction will be applied to the Character when it can pick"));
                EditorGUILayout.PropertyField(CoolDown);
                EditorGUILayout.PropertyField(PickDelay, new GUIContent("Pick Delay", "Delay time after Calling the Pick Action"));
               // EditorGUILayout.PropertyField(DropReacion, new GUIContent("Drop Reaction ", "The Reaction will be applied to the Character when it can drop an Item"));
                EditorGUILayout.PropertyField(DropDelay, new GUIContent("Drop Delay", "Delay time after Calling the Drop Action"));
                EditorGUILayout.EndVertical();



              //  if (PickReaction.objectReferenceValue != null)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(Align, new GUIContent("Align On Pick", "Align the Animal to the Item"));

                    if (Align.boolValue)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(AlignPos, new GUIContent("Align Pos", "Also Align the Position"));
                        EditorGUIUtility.labelWidth = 60;
                        EditorGUILayout.PropertyField(AlignDistance, new GUIContent("Distance", "Distance to move the Animal towards the Item"), GUILayout.MinWidth(50));
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.PropertyField(AlignTime, new GUIContent("Time", "Time needed to do the Alignment"));
                    }
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    if (MalbersEditor.Foldout( ShowEvents,"Events"))
                    {
                        EditorGUILayout.PropertyField(OnFocused);
                        if (m.PickDelay > 0 || m.m_ByAnimation.Value)
                            EditorGUILayout.PropertyField(OnPrePicked, new GUIContent("On Pre-Picked By"));
                        EditorGUILayout.PropertyField(OnPicked, new GUIContent("On Picked By"));
                        if (m.DropDelay > 0 || m.m_ByAnimation.Value)
                            EditorGUILayout.PropertyField(OnPreDropped, new GUIContent("On Pre-Dropped By"));
                        EditorGUILayout.PropertyField(OnDropped, new GUIContent("On Dropped By"));
                    }
                }
                EditorGUILayout.EndVertical();

                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndVertical();
            }
        }
    }
#endif
}