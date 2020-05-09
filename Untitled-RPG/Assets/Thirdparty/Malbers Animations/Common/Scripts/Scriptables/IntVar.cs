using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary>
    /// Int Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple
    /// </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Scriptables/Int Var")]
    public class IntVar : ScriptableVar
    {
        /// <summary> The current value</summary>
        [SerializeField] private int value = 0;

        /// <summary>Invoked when the value changes </summary>
        public Events.IntEvent OnValueChanged = new Events.IntEvent();

        /// <summary>Value of the Int Scriptable variable</summary>
        public virtual int Value
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


        /// <summary>Set the Value using another IntVar</summary>
        public virtual void SetValue(IntVar var)
        {
            Value = var.Value;
        }

        /// <summary>Add or Remove the passed var value</summary>
        public virtual void Add(IntVar var)
        {
            Value += var.Value;
        }

        /// <summary>Add or Remove the passed var value</summary>
        public virtual void Add(int var)
        {
            Value += var;
        }

        public static implicit operator int(IntVar reference)
        {
            return reference.Value;
        }
    }
}