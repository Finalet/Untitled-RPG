using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Set VarListener")]
    public class SetVarListener : MTask
    {

        public override string DisplayName => "Variables/Set Var Listener";


        public enum VarType
        {
            Bool,
            Int,
            Float
        }

        public enum BoolType
        {
            True,
            False
        }

        [Space]
        [Tooltip("Check the Variable Listener ID Value, when this value is Zero, the ID is ignored")]
        public IntReference ListenerID = 0;

        /// <summary>Range for Looking forward and Finding something</summary>
        [Space, Tooltip("Check the Decision on the Animal(Self) or the Target(Target), or on an object with a tag")]
        public Affected checkOn = Affected.Self;

        [Space,
            Tooltip("Check on the Target or Self if it has a Listener Variable Component <Int><Bool><Float> and compares it with the local variable)")]
        public VarType varType = VarType.Bool;


        [Hide("showBoolValue", true)] public bool boolValue = true;
        [Hide("showIntValue", true)] public int intValue = 0;
        [Hide("showFloatValue", true)] public float floatValue = 0f;

      

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


        public override void StartTask(MAnimalBrain brain, int index)
        {
            switch (checkOn)
            {
                case Affected.Self:
                    Set_VarListener(brain.Animal);
                    break;
                case Affected.Target:
                    Set_VarListener(brain.Target);
                    break;
                default:
                    break;
            }

            brain.TaskDone(index);
        }

        public void Set_VarListener(Component comp)
        {
            var AllListeners = comp.GetComponentsInChildren<VarListener>();

            foreach (var listener in AllListeners)
            {
                if (ListenerID == 0 || listener.ID.Value == ListenerID.Value)
                {
                    switch (varType)
                    {
                        case VarType.Bool:
                            if (listener is BoolVarListener) (listener as BoolVarListener).value.Value = boolValue;
                            break;
                        case VarType.Int:
                            if (listener is IntVarListener) (listener as IntVarListener).value.Value = intValue;
                            break;
                        case VarType.Float:
                            if (listener is FloatVarListener) (listener as FloatVarListener).value.Value = floatValue;
                            break;
                        default:
                            break;
                    }
                }
            }



          
        }


        void Reset() { Description = "Search for any Var listener in the Animal or the Target and sets a value"; }
    }
}
