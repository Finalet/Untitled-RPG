using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary>
    /// String Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple
    /// </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Scriptables/String Var")]
    public class StringVar : ScriptableVar
    {
        [SerializeField]
        /// <summary> The current value</summary>
        private string value = "";

        /// <summary>Value of the String Scriptable variable</summary>
        public virtual string Value
        {
            get { return value; }
            set
            {
                this.value = value;
            }
        }

        public virtual void SetValue(StringVar var)
        {
            Value = var.Value;
        }

        public static implicit operator string(StringVar reference)
        {
            return reference.Value;
        }
    }
}
