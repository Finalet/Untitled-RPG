using MalbersAnimations.Scriptables;
using UnityEngine;


namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Check Scriptable Variable")]
    public class CheckScriptableVar : MAIDecision
    {
        [Space, Tooltip("Check on the Target or Self if it has a Listener Variable Component <Int><Bool><Float> and compares it with the local variable)")]
        public VarType varType = VarType.Bool;


        [Space, ConditionalHide("showBoolValue", true)]
        public BoolVar Bool;
        [Space, ConditionalHide("showIntValue", true)]
        public IntVar Int;
        [Space, ConditionalHide("showFloatValue", true)]
        public FloatVar Float;

        [ConditionalHide("showBoolValue", true,true)]
        public ComparerInt compare;

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

        public override bool Decide(MAnimalBrain brain, int Index)
        {
            switch (varType)
            {
                case VarType.Bool:
                    return Bool != null ? Bool.Value == boolValue : false;
                case VarType.Int:
                    return Int != null ? CompareInteger(Int.Value) : false;
                case VarType.Float:
                    return Float != null ? CompareFloat(Float.Value) : false;
                default:
                    return false;
            }
        }

        public override void FinishDecision(MAnimalBrain brain, int Index)
        {
            //Reset all variables
            Bool = null;
            Int = null;
            Float = null;
        }

        public enum VarType { Bool, Int, Float }
        public enum BoolType { True, False }


        public bool CompareInteger(int IntValue)
        {
            switch (compare)
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
            switch (compare)
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
