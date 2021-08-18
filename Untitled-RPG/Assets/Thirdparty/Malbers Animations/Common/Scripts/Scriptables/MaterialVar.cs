using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    [CreateAssetMenu(menuName = "Malbers Animations/Variables/Material", order = 2000)]
    public class MaterialVar : ScriptableVar
    {
        /// <summary> The current value</summary>
        [SerializeField] private Material value;


        /// <summary>Value of the String Scriptable variable</summary>
        public Material Value
        {
            get => value;
            set
            {
                this.value = value;
#if UNITY_EDITOR
                if (debug) Debug.Log($"<B>{name} -> [<color=red> {value} </color>] </B>", this);
#endif
            }
        }

        public virtual void SetValue(MaterialVar var) => Value = var.Value;

        public virtual void SetValue(Material var) => Value = var;
    }
}