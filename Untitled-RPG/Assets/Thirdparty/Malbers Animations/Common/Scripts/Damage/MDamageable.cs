using MalbersAnimations.Controller;
using MalbersAnimations.Controller.Reactions;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    [DisallowMultipleComponent]
    /// <summary> Damager Receiver</summary>
    [AddComponentMenu("Malbers/Damage/MDamageable")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/secondary-components/mdamageable")]
    public class MDamageable : MonoBehaviour, IMDamage
    {
        [Tooltip("Animal Reaction to apply when the damage is done")]
        public MReaction reaction;

        [Tooltip("Stats component to apply the Damage")]
        public Stats stats;

        [Tooltip("Multiplier for the Stat modifier Value")]
        public FloatReference multiplier = new FloatReference(1);
       
        public MDamageable Root; 
        public damagerEvents events;

        public Vector3 HitDirection { get; set; }
        public GameObject Damager { get; set; }
        public GameObject Damagee => gameObject;

        public DamageData LastDamage;

        /// <summary>Animal Component set as Monobehavior to avoid Dependencies </summary>
        private MonoBehaviour character;

        private void Start()
        {
            character = reaction.FindReactionComponent(transform.root.GetComponents<MonoBehaviour>()); //Find the Animal Component
        }

        public virtual void ReceiveDamage(Vector3 Direction, GameObject Damager, StatModifier modifier, bool isCritical, bool react, bool damageeMult)
        {
            Stat st = stats.Stat_Get(modifier.ID);

            if (st != null && !st.IsInmune)
            {
                SetDamageable(Direction, Damager);
                Root?.SetDamageable(Direction, Damager);                                 //Send the Direction and Damager to the Root 

                if (isCritical)
                {
                    events.OnCriticalDamage.Invoke();
                    Root?.events.OnCriticalDamage.Invoke();
                }

                if (!damageeMult) modifier.Value *= multiplier;                                           //Apply to the Stat modifier a new Modification

                events.OnReceivingDamage.Invoke(modifier.Value);
                Root?.events.OnReceivingDamage.Invoke(modifier.Value);

                LastDamage = new DamageData(Damager, modifier);
                if (Root) Root.LastDamage = LastDamage;

                modifier.ModifyStat(st);

                if (react) reaction?.React(character);     //Let the Damagee to apply a reaction
            }
        }

        internal void SetDamageable(Vector3 Direction, GameObject Damager)
        {
            HitDirection = Direction;
            this.Damager = Damager;
        }

        [System.Serializable]
        public class damagerEvents
        {
            // public UnityEvent BeforeReceivingDamage = new UnityEvent();
            public FloatEvent OnReceivingDamage = new FloatEvent();
            // public UnityEvent AfterReceivingDamage = new UnityEvent();
            public UnityEvent OnCriticalDamage = new UnityEvent();
        }

        public struct DamageData
        {
            /// <summary>  Who made the Damage ? </summary>
            public GameObject Damager;
            /// <summary>  Final Stat Modifier ? </summary>
            public StatModifier stat;
            /// <summary> Final value who modified the Stat</summary>
            public float Damage => stat.modify != StatOption.None ? stat.Value.Value : 0f;

            public DamageData(GameObject damager, StatModifier stat)
            {
                Damager = damager;
                this.stat = new StatModifier(stat);
            }
        }


#if UNITY_EDITOR
        private void Reset()
        {
            reaction = MTools.GetInstance<ModeReaction>("Damaged");
            stats = this.FindComponent<Stats>();
            Root = transform.root.GetComponent<MDamageable>();     //Check if there's a Damageable on the Root
            if (Root == this) Root = null;

            if (stats == null)
            {
                stats = gameObject.AddComponent<Stats>();

                var health = MTools.GetInstance<StatID>("Health");

                if (health != null)
                {
                    stats.stats = new System.Collections.Generic.List<Stat>();
                    var HealthStat = new Stat()
                    {
                        ID = health,
                        MaxValue = 100,
                        MinValue = 0,
                        value = new FloatReference(100)
                    };
                    stats.stats.Add(HealthStat);
                }
            }
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MDamageable))]
    public class MDamageableEditor : Editor
    {
        SerializedProperty reaction, stats, multiplier, events, Root;
        MDamageable M;
     

        private void OnEnable()
        {
            M = (MDamageable)target;

            reaction = serializedObject.FindProperty("reaction");
            stats = serializedObject.FindProperty("stats");
            multiplier = serializedObject.FindProperty("multiplier");
            events = serializedObject.FindProperty("events");
            Root = serializedObject.FindProperty("Root");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Allows the Animal React and Receive damage from external sources");
            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (M.transform.parent != null)
            EditorGUILayout.PropertyField(Root);
            EditorGUILayout.PropertyField(reaction);
            EditorGUILayout.PropertyField(stats);
            EditorGUILayout.PropertyField(multiplier);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(events,true);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}