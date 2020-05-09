using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary>
    /// Float Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple
    /// </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Scriptables/Float Var")]
    public class FloatVar : ScriptableVar
    {
        /// <summary>The current value</summary>
        [SerializeField] private float value = 0;

        /// <summary> Invoked when the value changes</summary>
        public Events.FloatEvent OnValueChanged = new Events.FloatEvent();

        /// <summary>Value of the Float Scriptable variable </summary>
        public virtual float Value
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

        /// <summary>Set the Value using another FoatVar</summary>
        public virtual void SetValue(FloatVar var)
        {
            Value = var.Value;
        }

        /// <summary>Add or Remove the passed var value</summary>
        public virtual void Add(FloatVar var)
        {
            Value += var.Value;
        }

        /// <summary>Add or Remove the passed var value</summary>
        public virtual void Add(float var)
        {
            Value += var;
        }


        public static implicit operator float(FloatVar reference)
        {
            return reference.Value;
        }
    }
}