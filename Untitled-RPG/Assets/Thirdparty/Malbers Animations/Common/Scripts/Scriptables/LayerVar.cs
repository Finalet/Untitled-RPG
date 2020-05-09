using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary> Layer Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple</summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Scriptables/Layer Var")]
    public class LayerVar : ScriptableVar
    {
        /// <summary>The current value</summary>
        [SerializeField] private LayerMask value = 0;

        /// <summary>Value of the Layer Scriptable variable </summary>
        public virtual LayerMask Value { get => value; set => this.value = value; }

        public static implicit operator int(LayerVar reference)   { return reference.Value; }
     
    }
}