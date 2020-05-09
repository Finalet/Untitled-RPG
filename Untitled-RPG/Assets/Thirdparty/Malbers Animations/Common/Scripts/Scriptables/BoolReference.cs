
namespace MalbersAnimations.Scriptables
{
    [System.Serializable]
    public class BoolReference
    {
        public bool UseConstant = true;

        public bool ConstantValue;
        [RequiredField] public BoolVar Variable;

        public BoolReference()
        {
             Value = false;
        }

        public BoolReference(bool value)
        {
            Value = value;
        }

        public BoolReference(BoolVar value)
        {
            Value = value.Value;
        }

        public bool Value
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
        public static implicit operator bool(BoolReference reference)
        {
            return reference.Value;
        }
        #endregion
    }
}