using System;
using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary> V3 Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple  </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Variables/Vector3", order = 1000)]
    public class Vector3Var : ScriptableVar
    {
        /// <summary>The current value</summary>
        [SerializeField] private Vector3 value = Vector3.zero;

        /// <summary>Invoked when the value changes </summary>
        public Action<Vector3> OnValueChanged = delegate { };


        /// <summary> Value of the Float Scriptable variable</summary>
        public virtual Vector3 Value
        {
            get => value;
            set
            {
                this.value = value;
                OnValueChanged(value);
#if UNITY_EDITOR
                if (debug) Debug.Log($"<B>{name} -> [<color=gray> {value} </color>] </B>", this);
#endif
            }
        }
        public float x { get => value.x; set => this.value.x = value; }
        public float y { get => value.y; set => this.value.y = value; }
        public float z { get => value.z; set => this.value.z = value; }

        public void SetValue(Vector3Var var) => Value = var.Value;
        public void SetValue(Vector3 var) => Value = var;
        public void SetX(float var) => value.x = var;
        public void SetY(float var) => value.y = var;
        public void SetZ(float var) => value.z = var;

        public static implicit operator Vector3(Vector3Var reference) => reference.Value;

        public static implicit operator Vector2(Vector3Var reference) => reference.Value;

    }

    [System.Serializable]
    public class Vector3Reference
    {
        public bool UseConstant = true;

        public Vector3 ConstantValue = Vector3.zero;
        [RequiredField] public Vector3Var Variable;

        public Vector3Reference()
        {
            UseConstant = true;
            ConstantValue = Vector3.zero;
        }

        public Vector3Reference(bool variable)
        {
            UseConstant = !variable;

            if (!variable)
            {
                ConstantValue = Vector3.zero;
            }
            else
            {
                Variable = ScriptableObject.CreateInstance<Vector3Var>();
                Variable.Value = Vector3.zero;
            }
        }

        public Vector3Reference(Vector3 value) => Value = value;

        public Vector3 Value
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

        public float x
        {
            get => UseConstant ? ConstantValue.x : Variable.x;
            set
            {
                if (UseConstant)
                    ConstantValue.x = value;
                else
                    Variable.x = value;
            }
        }

        public float y
        {
            get => UseConstant ? ConstantValue.y : Variable.y;
            set
            {
                if (UseConstant)
                    ConstantValue.y = value;
                else
                    Variable.y = value;
            }
        }

        public float z
        {
            get => UseConstant ? ConstantValue.z : Variable.z;
            set
            {
                if (UseConstant)
                    ConstantValue.z = value;
                else
                    Variable.z = value;
            }
        }

        #region Operators
        public static implicit operator Vector3(Vector3Reference reference) => reference.Value;

        public static implicit operator Vector2(Vector3Reference reference) => reference.Value;
        #endregion
    }
}