using UnityEngine;
namespace MalbersAnimations.Scriptables
{
    [System.Serializable]
    public class LayerReference
    {
        public bool UseConstant = true;

        public LayerMask ConstantValue = ~0;
        [RequiredField] public LayerVar Variable;

        public LayerReference()
        {
            Value = ~0;
        } 

        public LayerReference(LayerMask value)
        {
            UseConstant = true;
            Value = value;
        }

        public LayerReference(LayerVar value)
        {
            UseConstant = false;
            Value = value.Value;
        }

        public LayerMask Value
        {
            get { return UseConstant || Variable == null ? ConstantValue : Variable.Value; }
            set
            {
                if (UseConstant || Variable == null)
                    ConstantValue = value;
                else
                    Variable.Value = value;
            }
        }
    }
}