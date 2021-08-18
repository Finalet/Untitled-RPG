using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{
    /// <summary>Simple Script to make damage anything with a stat</summary>
    [AddComponentMenu("Malbers/Damage/Attack Trigger")]
    public class MAttackTrigger : MDamager
    {
        [RequiredField, Tooltip("Collider used for the Interaction")]
        public Collider Trigger;

        /// <summary>When the Attack Trigger is Enabled, Affect your stat</summary>
        [Tooltip("When the Attack Trigger is Enabled, Affect your stat")]
        public StatModifier SelfStatEnter;

        /// <summary>When the Attack Trigger is Disabled, Affect your stat</summary>
        [Tooltip("When the Attack Trigger is Disabled, Affect your stat")]
        public StatModifier SelfStatExit;

        /// <summary>When the Attack Trigger Exits an enemy, Affect his stats</summary>
        [Tooltip("When the Attack Trigger Exits an enemy, Affect his stats")]
        public StatModifier EnemyStatExit;

        public UnityEvent OnAttackBegin = new UnityEvent();
        public UnityEvent OnAttackEnd = new UnityEvent();

        public Color DebugColor = new Color(1, 0.25f, 0, 0.15f);

        /// <summary>Enemy that can be Damaged</summary>
        private IMDamage enemy;
        //private Stats enemyStats;

        protected List<Collider> AlreadyHitted = new List<Collider>(); 
          
        [HideInInspector] public int Editor_Tabs1;

        private void Awake()
        {
            if (Trigger) Trigger.enabled = enabled;
        }

        void OnEnable()
        {
            if (Owner == null) Owner = transform.root.gameObject;                         //Set which is the owner of this AttackTrigger
            if (Trigger)
            {
                Trigger.enabled = Trigger.isTrigger = true;
            }

            AlreadyHitted = new List<Collider>();
            OnAttackBegin.Invoke();
            enemy = null;
        }

        public override void DoDamage(bool value) => Active = value;


        void OnDisable()
        {
            if (Trigger) Trigger.enabled = false;

            TryDamage(enemy, EnemyStatExit); //Means the Colliders was disable before Exit Trigger
            enemy = null;

            AlreadyHitted = new List<Collider>();
            OnAttackEnd.Invoke();
        }

        void OnTriggerEnter(Collider other)
        {
            if (IsInvalid(other)) return;                                           //Check Layers and Don't hit yourself

            var Newenemy = other.GetComponentInParent<IMDamage>();                  //Get the Animal on the Other collider
            if (!AlreadyHitted.Contains(other)) AlreadyHitted.Add(other);           //if the entering collider is not already on the list add it

          //  Direction = (Owner.transform.position - other.bounds.center).normalized;              //Calculate the direction of the attack
            Direction = (other.bounds.center - Owner.transform.position).normalized;              //Calculate the direction of the attack

            TryInteract(other.gameObject);                                              //Get the interactable on the Other collider
            TryPhysics(other.attachedRigidbody, other, Direction, Force);               //If the other has a riggid body and it can be pushed

            if (enemy == Newenemy) return;                                              //if the animal is the same, do nothing we already in one of the Animal Colliders
            else                                                                        //Is a new Animal
            {
                if (enemy != null)
                    AlreadyHitted = new List<Collider>();                               //Clean the colliders if you had a previus animal

                enemy = Newenemy;                                                       //Get the Damager on the Other collider
               // enemyStats = other.GetComponentInParent<Stats>();


                OnHit.Invoke(other.transform);

                TryDamage(enemy, statModifier); //if the other does'nt have the Damagable Interface dont send the Damagable stuff
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (IsInvalid(other)) return;

            if (enemy != other.GetComponentInParent<IMDamage>()) return;                //If is another animal exiting the trigger SKIP


            if (AlreadyHitted.Find(col => col == other))                                //Remove the collider from the list that is exiting the zone.
                AlreadyHitted.Remove(other);


            if (AlreadyHitted.Count == 0)                                               //When all the collides are removed from the list..
            {
                TryDamage(enemy, EnemyStatExit); //if the other does'nt have the Damagable Interface dont send the Damagable stuff
                enemy = null;
            }
        }



#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            #region Find the Trigger
            Trigger = this.FindComponent<Collider>();
            if (!Trigger) Trigger = gameObject.AddComponent<BoxCollider>();
            Trigger.isTrigger = true;
            enabled = false;
            #endregion
        }


        void OnDrawGizmos()
        { if (Trigger != null)
           MTools.DrawTriggers(transform, Trigger, DebugColor, false);
        }

        void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            MTools.DrawTriggers(transform, Trigger, DebugColor, true);
        }

#endif
    }

#if UNITY_EDITOR


    [CustomEditor(typeof(MAttackTrigger)), CanEditMultipleObjects]
    public class MAttackTriggerEd : MDamagerEd
    {
        SerializedProperty Trigger, EnemyStatExit, DebugColor, OnAttackBegin, OnAttackEnd, Editor_Tabs1;
        protected string[] Tabs1 = new string[] { "General", "Damage", "Extras", "Events" };


        private void OnEnable()
        {
            FindBaseProperties();

            Trigger = serializedObject.FindProperty("Trigger");

            EnemyStatExit = serializedObject.FindProperty("EnemyStatExit");
           
            DebugColor = serializedObject.FindProperty("DebugColor");

            OnAttackBegin = serializedObject.FindProperty("OnAttackBegin");
            OnAttackEnd = serializedObject.FindProperty("OnAttackEnd");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");

        }


        protected override void DrawCustomEvents()
        {
            EditorGUILayout.PropertyField(OnAttackBegin);
            EditorGUILayout.PropertyField(OnAttackEnd);
        }

        protected override void DrawStatModifier(bool drawbox =true)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Enemy Stat", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(statModifier, new GUIContent("Enemy Stat Enter"), true);
            EditorGUILayout.PropertyField(EnemyStatExit, true);
            EditorGUILayout.PropertyField(pureDamage);
            EditorGUILayout.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDescription("Attack Trigger Logic. By default should be Disabled.");


            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs1);

            int Selection = Editor_Tabs1.intValue;

            if (Selection == 0) DrawGeneral();
            else if (Selection == 1) DrawDamage();
            else if (Selection == 2) DrawExtras();
            else if (Selection == 3) DrawEvents();

            serializedObject.ApplyModifiedProperties();
        }

        protected override void DrawGeneral(bool drawbox = true)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                EditorGUILayout.PropertyField(Trigger);

                    if (debug.boolValue)
                        EditorGUILayout.PropertyField(DebugColor, GUIContent.none, GUILayout.Width(55));

                 //   MTools.DrawDebugIcon(debug);

                    //var currentGUIColor = GUI.color;
                    //GUI.color = debug.boolValue ? Color.red : currentGUIColor;
                    //debug.boolValue = GUILayout.Toggle(debug.boolValue, new GUIContent ("Debug"), EditorStyles.miniButton, GUILayout.Width(50));
                    //GUI.color = currentGUIColor;

                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            base.DrawGeneral(true);
        }

        private void DrawDamage()
        {
            DrawStatModifier();
            DrawCriticalDamage();
        }

        private void DrawExtras()
        {
            DrawPhysics();
            DrawMisc();
        }
    }
#endif
}

