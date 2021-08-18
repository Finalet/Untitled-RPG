using System;
using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary>  Prefab Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Variables/Game Object", order = 3000)]
    public class GameObjectVar : ScriptableVar
    {
        [SerializeField] private GameObject value;

        /// <summary>Invoked when the value changes </summary>
        public Action<GameObject> OnValueChanged;

        /// <summary> Value of the Bool variable</summary>
        public virtual GameObject Value
        {
            get => value;
            set
            {
                this.value = value;
                OnValueChanged?.Invoke(value);         //If we are using OnChange event Invoked
#if UNITY_EDITOR
                if (debug) Debug.Log($"<B>{name} -> [<color=cyan> {value} </color>] </B>");
#endif
            }
        }

        public virtual void SetValue(GameObjectVar var) => Value = var.Value;
        public virtual void SetNull(GameObjectVar var) => Value = null;
        public virtual void SetValue(GameObject var) => Value = var;
        public virtual void SetValue(Component var) => Value = var.gameObject;

    }

    [System.Serializable]
    public class GameObjectReference
    {
        public bool UseConstant = true;

#pragma warning disable CA2235 // Mark all non-serializable fields
        public GameObject ConstantValue;
        [RequiredField] public GameObjectVar Variable;
#pragma warning restore CA2235 // Mark all non-serializable fields

        public GameObjectReference() => UseConstant = true;
        public GameObjectReference(GameObject value) => Value = value;

        public GameObjectReference(GameObjectVar value)
        {
            Variable = value;
            UseConstant = false;
        }

        public GameObject Value
        {
            get => UseConstant ? ConstantValue : (Variable != null ? Variable.Value : null);
            set
            {
                if (UseConstant || Variable == null)
                { 
                    ConstantValue = value;
                    UseConstant = true;
                }
                else
                {
                    Variable.Value = value;
                }
            }
        }
    }
}
