using MalbersAnimations.Scriptables;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Check Stat", order = 4)]
    public class CheckStatDecision : MAIDecision
    {
        public override string DisplayName => "General/Check Stat";


        public enum checkStatOption { Compare, CompareNormalized, isInmune, Regenerating, Degenerating, Empty, Full, Active, ValueChanged, ValueReduced, ValueIncreased }

        [Tooltip("Check the Decision on the Animal(Self) or the Target(Target)")]
        public Affected checkOn = Affected.Self;
        /// <summary>Range for Looking forward and Finding something</summary>

        [Tooltip("Stat you want to find")]
        public StatID Stat;
        [Tooltip("What do you want to do with the Stat?")]
        public checkStatOption Option = checkStatOption.Compare;
        [Tooltip("(Option Compare Only) Type of the comparation")]
        public ComparerInt StatIs = ComparerInt.Less;
        public float Value;
        [Tooltip("(Option Compare Only) Value to Compare the Stat")]

        [ContextMenuItem("Recover Value", "RecoverValue")]
        public FloatReference m_Value = new FloatReference();
        [Space, Tooltip("Uses TryGet Value in case you don't know if your target or your animal has the Stat you are looking for. Disabling this Improves performance")]
        public bool TryGetValue = true;

        public override void PrepareDecision(MAnimalBrain brain, int Index)
        {
            //Store the Value the Stat has starting this Decision

            switch (checkOn)
            {
                case Affected.Self:
                    if (TryGetValue)
                    {
                        if (brain.AnimalStats.TryGetValue(Stat.ID, out Stat statS))
                            brain.DecisionsVars[Index].floatValue = statS.Value;
                    }
                    else
                    {
                        brain.DecisionsVars[Index].floatValue = brain.AnimalStats[Stat.ID].Value;
                    }
                    break;

                case Affected.Target:

                    if (brain.TargetHasStats)
                    {
                        if (TryGetValue)
                        {
                            if (brain.TargetStats.TryGetValue(Stat.ID, out Stat statS))
                                brain.DecisionsVars[Index].floatValue = statS.Value;
                        }
                        else
                        {
                            brain.DecisionsVars[Index].floatValue = brain.TargetStats[Stat.ID].Value;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public override bool Decide(MAnimalBrain brain, int index)
        {
            bool result = false;

            switch (checkOn)
            {
                case Affected.Self:
                    if (TryGetValue)
                    {
                        if (brain.AnimalStats.TryGetValue(Stat.ID, out Stat statS))
                            result = CheckStat(statS, brain, index);
                    }
                    else
                    {
                        var SelfStatValue = brain.AnimalStats[Stat.ID];
                        result = CheckStat(SelfStatValue, brain, index);
                    }
                    break;
                case Affected.Target:

                    if (brain.TargetHasStats)
                    {
                        if (TryGetValue)
                        {
                            if (brain.TargetStats.TryGetValue(Stat.ID, out Stat statT))
                                result = CheckStat(statT, brain, index);
                        }
                        else
                        {
                            var TargetStatValue = brain.TargetStats[Stat.ID];
                            result = CheckStat(TargetStatValue, brain, index);
                        }
                    }
                    break;
            }
            return result;
        }


        private void RecoverValue()
        {
            m_Value.Value = Value;
        }

        private bool CheckStat(Stat stat, MAnimalBrain brain, int Index)
        {
            switch (Option)
            {
                case checkStatOption.Compare:
                    return CompareWithValue(stat.Value);
                case checkStatOption.CompareNormalized:
                    return CompareWithValue(stat.NormalizedValue);
                case checkStatOption.isInmune:
                    return stat.IsInmune;
                case checkStatOption.Regenerating:
                    return stat.IsRegenerating;
                case checkStatOption.Degenerating:
                    return stat.IsDegenerating;
                case checkStatOption.Empty:
                    return stat.Value == stat.MinValue;
                case checkStatOption.Full:
                    return stat.Value == stat.MaxValue;
                case checkStatOption.Active:
                    return stat.Active;
                case checkStatOption.ValueChanged:
                    return stat.value != brain.DecisionsVars[Index].floatValue; ;
                case checkStatOption.ValueReduced:
                    return stat.value < brain.DecisionsVars[Index].floatValue; ;
                case checkStatOption.ValueIncreased:
                    return stat.value > brain.DecisionsVars[Index].floatValue; ;
                default:
                    return false;
            }
        }
        private bool CompareWithValue(float stat)
        {
            switch (StatIs)
            {
                case ComparerInt.Equal:
                    return stat == m_Value;
                case ComparerInt.Greater:
                    return stat > m_Value;
                case ComparerInt.Less:
                    return stat < m_Value;
                case ComparerInt.NotEqual:
                    return stat != m_Value;
                default:
                    return false;
            }
        }


        [HideInInspector] public bool hideVars = false;
        private void OnValidate()
        {
            hideVars = (Option != checkStatOption.Compare && Option != checkStatOption.CompareNormalized);
        }


        private void Reset() { Description = "Checks for a Stat value, Compares or search for a Stat Property and returns the succeded value"; }
    }


    /// <summary>  Inspector!!!  </summary>

#if UNITY_EDITOR

    [CustomEditor(typeof(CheckStatDecision))]
    [CanEditMultipleObjects]
    public class CheckStatDecisionEditor : Editor
    {
        public static GUIStyle StyleBlue => MTools.Style(new Color(0, 0.5f, 1f, 0.3f));

        SerializedProperty
            Description, checkOn, MessageID, send, interval, Stat, Option, StatIs, Value, TryGetValue;

        MonoScript script;
        private void OnEnable()
        {
            script = MonoScript.FromScriptableObject((ScriptableObject)target);

            Description = serializedObject.FindProperty("Description");
            checkOn = serializedObject.FindProperty("checkOn");
            MessageID = serializedObject.FindProperty("DecisionID");
            send = serializedObject.FindProperty("send");
            interval = serializedObject.FindProperty("interval");

            Stat = serializedObject.FindProperty("Stat");
            Option = serializedObject.FindProperty("Option");
            StatIs = serializedObject.FindProperty("StatIs");
            Value = serializedObject.FindProperty("m_Value");
            TryGetValue = serializedObject.FindProperty("TryGetValue");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(Description);
            EditorGUILayout.PropertyField(MessageID);
            EditorGUILayout.PropertyField(send);
            EditorGUILayout.PropertyField(interval);

            EditorGUILayout.PropertyField(checkOn);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(Stat);

            var m_stlye = new GUIStyle(EditorStyles.miniButton);
            m_stlye.fontStyle = TryGetValue.boolValue ? FontStyle.Bold : FontStyle.Normal;

            TryGetValue.boolValue =  GUILayout.Toggle(TryGetValue.boolValue,"Try*", m_stlye, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(Option); 
            var o = (CheckStatDecision.checkStatOption)Option.intValue;

            var compare = o == CheckStatDecision.checkStatOption.Compare || o == CheckStatDecision.checkStatOption.CompareNormalized;

            if (compare) 
                EditorGUILayout.PropertyField(StatIs,GUIContent.none, GUILayout.Width(90));
            
            EditorGUILayout.EndHorizontal();

            if (compare)
                EditorGUILayout.PropertyField(Value);
            serializedObject.ApplyModifiedProperties();
        }
    } 
#endif
}
