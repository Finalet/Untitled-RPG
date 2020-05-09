using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary>
    /// Float Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple
    /// </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Scriptables/Color Var")]
    public class ColorVar : ScriptableVar
    {
        /// <summary>The current value </summary>
        [SerializeField] private Color value = Color.white;
   
        /// <summary>Value of the Float Scriptable variable</summary>
        public virtual Color Value
        {
            get { return value; }
            set
            {
                this.value = value; 
            }
        }

        public virtual void SetValue(ColorVar var)
        {
            Value = var.Value;
        }

        public static implicit operator Color(ColorVar reference)
        {
            return reference.Value;
        }
    }
}