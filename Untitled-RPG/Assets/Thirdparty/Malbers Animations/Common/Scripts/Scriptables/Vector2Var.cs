using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary>
    /// Float Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple
    /// </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Scriptables/Vector2 Var")]
    public class Vector2Var : ScriptableVar
    {
        /// <summary>The current value</summary>
        [SerializeField] private Vector2 value = Vector2.zero;
 
 
        ///// <summary>When active OnValue changed will ve used every time the value changes (you can subscribe only at runtime .?)</summary>
        //public bool UseEvent = true;

        ///// <summary>Invoked when the value changes</summary>
        //public Events.Vector3Event OnValueChanged = new Events.Vector3Event();

        /// <summary> Value of the Float Scriptable variable</summary>
        public virtual Vector2 Value
        {
            get { return value; }
            set
            {
                this.value = value;
            }
        }

        public virtual void SetValue(Vector2Var var)
        {
            Value = var.Value;
        }

        public static implicit operator Vector2(Vector2Var reference)
        {
            return reference.Value;
        } 
    }
}