using UnityEngine;
namespace MalbersAnimations.Scriptables
{
    [System.Serializable]
    public class Vector2Reference
    {
        public bool UseConstant = true;

        public Vector2 ConstantValue = Vector2.zero;
        [RequiredField] public Vector2Var Variable;

        public Vector2Reference()
        {
            UseConstant = true;
            ConstantValue = Vector2.zero;
        }

        public Vector2Reference(bool variable = false)
        {
            UseConstant = !variable;

            if (!variable)
            {
                ConstantValue = Vector2.zero;
            }
            else
            {
                Variable = ScriptableObject.CreateInstance<Vector2Var>();
                Variable.Value =  Vector2.zero;
            }
        }

        public Vector2Reference(Vector2 value)
        {
            UseConstant = true;
            Value = value;
        }

        public Vector2Reference(float x, float y)
        {
            UseConstant = true;
            Value = new Vector2(x, y);
        }

        public Vector2 Value
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
        public static implicit operator Vector2(Vector2Reference reference)
        {
            return reference.Value;
        } 
        #endregion
    }
}
