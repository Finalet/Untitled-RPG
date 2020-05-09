using MalbersAnimations.Scriptables;
using UnityEngine;


namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Check Var Listener", order = 4)]
    public class CheckVarListener : MAIDecision
    {
        [Space]
        /// <summary>Range for Looking forward and Finding something</summary>
        [Space, Tooltip("Check the Decision on the Animal(Self) or the Target(Target)")]
        public Affected checkOn = Affected.Self;
        [Space, Tooltip("Check on the Target or Self if it has a Listener Variable Component <Int><Bool><Float> and compares it with the local variable)")]
        public VarType varType = VarType.Bool;


        private BoolVarListener boolListener;
        private IntVarListener intListener;
        private FloatVarListener floatListener;

        [ConditionalHide("showBoolValue", true,true)]
        public ComparerInt comparer;

        [ConditionalHide("showBoolValue", true)]
        public bool boolValue = true;
        [ConditionalHide("showIntValue", true)]
        public int intValue = 0;
        [ConditionalHide("showFloatValue", true)]
        public float floatValue = 0f;

        [HideInInspector] public bool showFloatValue;
        [HideInInspector] public bool showBoolValue = true;
        [HideInInspector] public bool showIntValue;


        private void OnValidate()
        {
            switch (varType)
            {
                case VarType.Bool:
                    showFloatValue = false;
                    showBoolValue = true;
                    showIntValue = false;
                    break;
                case VarType.Int:
                    showFloatValue = false;
                    showBoolValue = false;
                    showIntValue = true;
                    break;
                case VarType.Float:
                    showFloatValue = true;
                    showBoolValue = false;
                    showIntValue = false;
                    break;
                default:
                    break;
            }
        }

        public override void PrepareDecision(MAnimalBrain brain, int Index)
        {
            //Reset all variables
            boolListener = null;
            intListener = null;
            floatListener = null;

            if (checkOn == Affected.Target)
            {
                var objective = brain.Target;

                switch (varType)
                {
                    case VarType.Bool:
                        boolListener = objective.GetComponent<BoolVarListener>();
                        break;
                    case VarType.Int:
                        intListener = objective.GetComponent<IntVarListener>();
                        break;
                    case VarType.Float:
                        floatListener = objective.GetComponent<FloatVarListener>();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                var objective = brain.transform;

                switch (varType)
                {
                    case VarType.Bool:
                        boolListener = objective.GetComponent<BoolVarListener>();
                        break;
                    case VarType.Int:
                        intListener = objective.GetComponent<IntVarListener>();
                        break;
                    case VarType.Float:
                        floatListener = objective.GetComponent<FloatVarListener>();
                        break;
                    default:
                        break;
                }
            }
        }

        public override bool Decide(MAnimalBrain brain, int Index)
        {
            switch (varType)
            {
                case VarType.Bool:
                    return boolListener != null ? boolListener.Value == boolValue : false;
                case VarType.Int:
                    return intListener != null ? CompareInteger(intListener.Value) : false;
                case VarType.Float:
                    return floatListener != null ? CompareFloat(floatListener.Value) : false;
                default:
                    return false;
            }
        }

        public override void FinishDecision(MAnimalBrain brain, int Index)
        {
            //Reset all variables
            boolListener = null;
            intListener = null;
            floatListener = null;
        }

        public enum VarType { Bool, Int, Float }
        public enum BoolType { True, False }


        public bool CompareInteger(int IntValue)
        {
            switch (comparer)
            {
                case ComparerInt.Equal:
                    return (IntValue == intValue);
                case ComparerInt.Greater:
                    return (IntValue > intValue);
                case ComparerInt.Less:
                    return (IntValue < intValue);
                case ComparerInt.NotEqual:
                    return (IntValue != intValue);
                default:
                    return false;
            }
        }

        public bool CompareFloat(float IntValue)
        {
            switch (comparer)
            {
                case ComparerInt.Equal:
                    return (IntValue == floatValue);
                case ComparerInt.Greater:
                    return (IntValue > floatValue);
                case ComparerInt.Less:
                    return (IntValue < floatValue);
                case ComparerInt.NotEqual:
                    return (IntValue != floatValue);
                default:
                    return false;
            }
        }
    }
}
