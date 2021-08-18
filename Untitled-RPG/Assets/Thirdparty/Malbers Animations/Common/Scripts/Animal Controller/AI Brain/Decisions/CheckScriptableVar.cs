using MalbersAnimations.Scriptables;
using UnityEngine;


namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Check Scriptable Variable", order = 6)]
    public class CheckScriptableVar : MAIDecision
    {
        public override string DisplayName => "Variables/Check Scriptable Variable";


        [Space, Tooltip("Check on the Target or Self if it has a Listener Variable Component <Int><Bool><Float> and compares it with the local variable)")]
        public VarType varType = VarType.Bool;


        [ContextMenuItem("Create Bool", "CreateBoolVar"), Hide("showBoolValue", true),]
        public BoolVar Bool;
        [ContextMenuItem("Create Int", "CreateIntVar"), Hide("showIntValue", true)]
        public IntVar Int;
        [ContextMenuItem("Create Float", "CreateFloatVar"), Hide("showFloatValue", true)]
        public FloatVar Float;

        [Space(15), Hide("showBoolValue", true, true)]
        public ComparerInt compare;

        [Hide("showBoolValue", true)]
        public bool boolValue = true;
        [Hide("showIntValue", true)]
        public int intValue = 0;
        [Hide("showFloatValue", true)]
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
                    return Bool != null && Bool.Value == boolValue;
                case VarType.Int:
                    return Int != null && CompareInteger(Int.Value);
                case VarType.Float:
                    return Float != null && CompareFloat(Float.Value);
                default:
                    return false;
            }
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

#if UNITY_EDITOR
        private void CreateIntVar() => Int = (IntVar)MTools.CreateScriptableAsset(typeof(IntVar));

        private void CreateFloat() => Float = (FloatVar)MTools.CreateScriptableAsset(typeof(FloatVar));

        private void CreateBoolVar() => Bool = (BoolVar)MTools.CreateScriptableAsset(typeof(BoolVar));
#endif
    }
}
