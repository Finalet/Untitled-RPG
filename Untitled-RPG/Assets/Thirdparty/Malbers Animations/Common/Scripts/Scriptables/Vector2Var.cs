using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary>  Vector2 Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Variables/Vector2",order = 1000)]
    public class Vector2Var : ScriptableVar
    {
        /// <summary>The current value</summary>
        [SerializeField] private Vector2 value = Vector2.zero;

        /// <summary> Value of the Float Scriptable variable</summary>
        public virtual Vector2 Value
        {
            get => value;
            set
            {
                this.value = value;
#if UNITY_EDITOR
                if (debug) Debug.Log($"<B>{name} -> [<color=gray> {value} </color>] </B>", this);
#endif
            }
        }
        public float x { get => value.x; set => this.value.x = value; }
        public float y { get => value.y; set => this.value.y = value; }

        public void SetValue(Vector2Var var) => Value = var.Value;
        public void SetX(float var) => value.x = var;
        public void SetY(float var) => value.y = var;

        public static implicit operator Vector2(Vector2Var reference) => reference.Value;
    }

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

        public Vector2Reference(bool variable)
        {
            UseConstant = !variable;

            if (!variable)
            {
                ConstantValue = Vector2.zero;
            }
            else
            {
                Variable = ScriptableObject.CreateInstance<Vector2Var>();
                Variable.Value = Vector2.zero;
            }
        }

        public Vector2Reference(Vector2 value) => Value = value;

        public Vector2Reference(float x, float y)
        {
            UseConstant = true;
            Value = new Vector2(x, y);
        }

        public Vector2 Value
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
            get => UseConstant ? ConstantValue.x : Variable.Value.x;
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
            get => UseConstant ? ConstantValue.y : Variable.Value.y;
            set
            {
                if (UseConstant)
                    ConstantValue.y = value;
                else
                    Variable.y = value;
            }
        }

        #region Operators
        public static implicit operator Vector2(Vector2Reference reference) => reference.Value;
        public static implicit operator Vector2Reference(Vector2 reference) => new Vector2Reference(reference);

        #endregion
    }
}