using System;
using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary>String Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple</summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Variables/String", order = 1000)]
    public class StringVar : ScriptableVar
    {
        [SerializeField]
        /// <summary> The current value</summary>
        [TextArea(0, 20)] 
        private string value = "";

        /// <summary>Invoked when the value changes </summary>
        public Action<string> OnValueChanged = delegate { };

        /// <summary>Value of the String Scriptable variable</summary>
        public virtual string Value
        {
            get => value;
            set
            { 
                this.value = value;
                OnValueChanged(value);         //If we are using OnChange event Invoked

#if UNITY_EDITOR
                if (debug) Debug.Log($"<B>{name} -> [<color=green> {value} </color>] </B>", this);
#endif 
            }
        }

        public virtual void SetValue(StringVar var) => Value = var.Value;

        public static implicit operator string(StringVar reference) => reference.Value;
    }

    [System.Serializable]
    public class StringReference
    {
        public bool UseConstant = true;

        public string ConstantValue;
        [RequiredField] public StringVar Variable;

        public StringReference()
        {
            UseConstant = true;
            ConstantValue = string.Empty;
        }

        public StringReference(bool variable = false)
        {
            UseConstant = !variable;

            if (!variable)
            {
                ConstantValue = string.Empty;
            }
            else
            {
                Variable = ScriptableObject.CreateInstance<StringVar>();
                Variable.Value = string.Empty;
            }
        }

        public StringReference(string value) => Value = value;

        public string Value
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
        public static implicit operator string(StringReference reference) => reference.Value;
        #endregion
    }
}
