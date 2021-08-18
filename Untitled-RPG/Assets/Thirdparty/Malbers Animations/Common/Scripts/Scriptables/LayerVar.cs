using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary> Layer Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple</summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Variables/Layer Mask", order = 2000)]
    public class LayerVar : ScriptableVar
    {
        /// <summary>The current value</summary>
        [SerializeField] private LayerMask value = 0;

        /// <summary>Value of the Layer Scriptable variable </summary>
        public virtual LayerMask Value
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

        public static implicit operator int(LayerVar reference) => reference.Value;
    }



    [System.Serializable]
    public class LayerReference
    {
        public bool UseConstant = true;

        public LayerMask ConstantValue = ~0;
        [RequiredField] public LayerVar Variable;

        public LayerReference() => Value = ~0;

        public LayerReference(LayerMask value)
        {
            UseConstant = true;
            Value = value;
        }

        public LayerReference(LayerVar value)
        {
            UseConstant = false;
            Value = value.Value;
        }

        public LayerMask Value
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

        public static implicit operator int(LayerReference reference) => reference.Value;
        public static implicit operator LayerMask(LayerReference reference) => reference.Value;
    }
}