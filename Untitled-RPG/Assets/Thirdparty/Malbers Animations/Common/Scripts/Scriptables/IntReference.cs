using UnityEngine;
namespace MalbersAnimations.Scriptables
{
    [System.Serializable]
    public class IntReference
    {
        public bool UseConstant = true;

        public int ConstantValue;
        [RequiredField] public IntVar Variable;

        public IntReference()
        {
            Value = 0;
        } 

        public IntReference(int value)
        {
            Value = value;
        }

        public IntReference(IntVar value)
        {
            Value = value.Value;
        }

        public int Value
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

        #region Operators
        public static implicit operator int(IntReference reference)
        {
            return reference.Value;
        }

        public static implicit operator IntReference(int reference)
        {
            return new IntReference(reference);
        }

        public static implicit operator IntReference(IntVar reference)
        {
            return new IntReference(reference);
        }


        #endregion
    }
}