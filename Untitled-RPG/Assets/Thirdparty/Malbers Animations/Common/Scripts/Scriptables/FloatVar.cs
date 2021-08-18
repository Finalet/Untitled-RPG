using System;
using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary>  Float Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple  </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Variables/Float",order = 1000)]
    public class FloatVar : ScriptableVar
    {
        /// <summary>The current value</summary>
        [SerializeField] private float value = 0;

        /// <summary> Invoked when the value changes</summary>
        public Action<float> OnValueChanged = delegate { };

        /// <summary>Value of the Float Scriptable variable </summary>
        public virtual float Value
        {
            get => value;
            set
            {
                if (this.value != value)                                //If the value is diferent change it
                {
                    this.value = value;
                    OnValueChanged(value);         //If we are using OnChange event Invoked
#if UNITY_EDITOR
                    if (debug) Debug.Log($"<B>{name} -> [<color=red> {value:F3} </color>] </B>", this);
#endif
                }
            }
        }

        /// <summary>Set the Value using another FoatVar</summary>
        public virtual void SetValue(FloatVar var) => Value = var.Value;

        /// <summary>Add or Remove the passed var value</summary>
        public virtual void Add(FloatVar var) => Value += var.Value;

        /// <summary>Add or Remove the passed var value</summary>
        public virtual void Add(float var) => Value += var;

        public static implicit operator float(FloatVar reference) => reference.Value;
    }

    [System.Serializable]
    public class FloatReference
    {
        public bool UseConstant = true;

        public float ConstantValue;
        [RequiredField] public FloatVar Variable;

        public FloatReference() => Value = 0;

        public FloatReference(float value) => Value = value;

        public FloatReference(FloatVar value) => Value = value.Value;

        public float Value
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

        public static implicit operator float(FloatReference reference) =>  reference.Value;

        public static implicit operator FloatReference(float reference) => new FloatReference(reference);

        public static implicit operator FloatReference(FloatVar reference) => new FloatReference(reference);
    }
}