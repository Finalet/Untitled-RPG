using System;
using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary>  Int Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple  </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Variables/Integer", order = 1000)]
    public class IntVar : ScriptableVar
    {
        /// <summary> The current value</summary>
        [SerializeField] private int value = 0;

        /// <summary>Invoked when the value changes </summary>
        public Action<int> OnValueChanged  = delegate { };

        /// <summary>Value of the Int Scriptable variable</summary>
        public virtual int Value
        {
            get => value;
            set
            {
                if (this.value != value)                   //If the value is different change it
                {
                    this.value = value;
                    OnValueChanged(value);         //If we are using OnChange event Invoked
#if UNITY_EDITOR
                    if (debug) Debug.Log($"<B>{name} -> [<color=yellow> {value} </color>] </B>",this);
#endif
                }
            }
        }


        /// <summary>Set the Value using another IntVar</summary>
        public virtual void SetValue(IntVar var) => Value = var.Value;

        /// <summary>Add or Remove the passed var value</summary>
        public virtual void Add(IntVar var) => Value += var.Value;

        /// <summary>Add or Remove the passed var value</summary>
        public virtual void Add(int var) => Value += var;
        public virtual void Multiply(int var) => Value *= var;
        public virtual void Multiply(IntVar var) => Value *= var;
        public virtual void Divide(IntVar var) => Value /= var;

        public static implicit operator int(IntVar reference) => reference.Value;
    }


    [System.Serializable]
    public class IntReference
    {
        public bool UseConstant = true;

        public int ConstantValue;
#pragma warning disable CA2235 // Mark all non-serializable fields
        [RequiredField] public IntVar Variable;
#pragma warning restore CA2235 // Mark all non-serializable fields

        public IntReference() => Value = 0;

        public IntReference(int value) => Value = value;

        public IntReference(IntVar value) => Value = value.Value;

        public int Value
        {
            get => UseConstant || Variable == null ? ConstantValue : Variable.Value;
            set
            {
                if (UseConstant || Variable == null)
                    ConstantValue = value;
                else
                    Variable.Value = value;
            }
        }
        #region Operators
        public static implicit operator int(IntReference reference) => reference.Value;

        public static implicit operator IntReference(int reference) => new IntReference(reference);

        public static implicit operator IntReference(IntVar reference) => new IntReference(reference);
        #endregion
    }
}