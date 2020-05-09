using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary>
    /// Bool Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple
    /// </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Scriptables/Bool Var")]
    public class BoolVar : ScriptableVar
    {
        [SerializeField] private bool value;


        /// <summary>Invoked when the value changes </summary>
        public Events.BoolEvent OnValueChanged = new Events.BoolEvent();

        /// <summary> Value of the Float Scriptable variable</summary>
        public virtual bool Value
        {
            get { return value; }
            set
            {
                if (this.value != value)                                //If the value is diferent change it
                {
                    this.value = value;
                    OnValueChanged.Invoke(value);         //If we are using OnChange event Invoked
                }
            }
        }

        public virtual void SetValue(BoolVar var)
        {
            Value = var.Value;
        }
        public virtual void SetValue(bool var)
        {
            Value = var;
        }



        public static implicit operator bool(BoolVar reference)
        {
            return reference.Value;
        }

        ///// <summary>When active OnValue changed will ve used every time the value changes (you can subscribe only at runtime .?)</summary>
        //public bool UseEvent = true;
        ///// <summary>Invoked when the value changes</summary>
        //public Events.BoolEvent OnValueChanged = new Events.BoolEvent();
    }
}
