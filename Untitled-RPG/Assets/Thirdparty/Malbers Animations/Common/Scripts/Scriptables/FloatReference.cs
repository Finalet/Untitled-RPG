using UnityEngine;
namespace MalbersAnimations.Scriptables
{
    [System.Serializable]
    public class FloatReference
    {
        public bool UseConstant = true;

        public float ConstantValue;
        [RequiredField] public FloatVar Variable;

        public FloatReference()
        {
            Value = 0;
        } 

        public FloatReference(float value)
        {
            Value = value;
        }

        public FloatReference(FloatVar value)
        {
            Value = value.Value;
        }

        public float Value
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

        public static implicit operator float(FloatReference reference)
        {
            return reference.Value;
        }

        public static implicit operator FloatReference(float reference)
        {
            return new FloatReference(reference);
        }

        public static implicit operator FloatReference(FloatVar reference)
        {
            return new FloatReference(reference);
        }
    }
}