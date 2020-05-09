using UnityEngine;
namespace MalbersAnimations.Scriptables
{
    [System.Serializable]
    public class ColorReference
    {
        public bool UseConstant = true;

        public Color ConstantValue = Color.white;
        public ColorVar Variable;

        public ColorReference()
        {
            UseConstant = true;
            ConstantValue = Color.white;
        }

        public ColorReference(bool variable = false)
        {
            UseConstant = !variable;

            if (!variable)
            {
                ConstantValue = Color.white;
            }
            else
            {
                Variable = ScriptableObject.CreateInstance<ColorVar>();
                Variable.Value = Color.white;
            }
        }

        public ColorReference(Color value)
        {
            Value = value;
        }

        public Color Value
        {
            get { return UseConstant ? ConstantValue : Variable.Value; }
            set
            {
                if (UseConstant)
                    ConstantValue = value;
                else
                    Variable.Value = value;
            }
        }

        #region Operators
        public static implicit operator Color(ColorReference reference)
        {
            return reference.Value;
        }
       
       
        #endregion
    }
}
