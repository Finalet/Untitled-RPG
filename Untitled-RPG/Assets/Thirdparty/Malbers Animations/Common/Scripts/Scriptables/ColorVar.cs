using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary>  Float Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Variables/Color", order = 2000)]
    public class ColorVar : ScriptableVar
    {
        /// <summary>The current value </summary>
        [SerializeField] private Color value = Color.white;

        /// <summary>Value of the Float Scriptable variable</summary>
        public virtual Color Value
        {
            get => value;
            set
            {
                this.value = value;
#if UNITY_EDITOR
                if (debug) Debug.Log($"<B>{name} -> [<color=black> {value} </color>] </B>", this);
#endif
            }
        }

        public virtual void SetValue(ColorVar var) => Value = var.Value;

        public static implicit operator Color(ColorVar reference) => reference.Value;
    }

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

        public ColorReference(Color value) => Value = value;

        public Color Value
        {
            get => UseConstant ? ConstantValue : Variable.Value;
            set
            {
                if (UseConstant)
                    ConstantValue = value;
                else
                    Variable.Value = value;
            }
        }

        #region Operators
        public static implicit operator Color(ColorReference reference) => reference.Value;
        #endregion
    }
}