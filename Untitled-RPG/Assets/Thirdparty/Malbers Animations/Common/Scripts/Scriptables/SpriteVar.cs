using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary>  Prefab Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Variables/Sprite Var", order = 2000)]
    public class SpriteVar : ScriptableVar
    {
        [SerializeField] private Sprite value;

        /// <summary>Invoked when the value changes </summary>
        public Events.SpriteEvent OnValueChanged = new Events.SpriteEvent();

        /// <summary> Value of the Bool variable</summary>
        public virtual Sprite Value
        {
            get { return value; }
            set
            {
                if (this.value != value)                  //If the value is diferent change it
                {
                    this.value = value;
                    OnValueChanged.Invoke(value);         //If we are using OnChange event Invoked

#if UNITY_EDITOR
                    if (debug) Debug.Log($"<B>{name} -> [<color=red> {value} </color>] </B>", this);
#endif
                }
            }
        }

        public virtual void SetValue(SpriteVar var) { Value = var.Value; }
        public virtual void SetValue(Sprite var) { Value = var; }
        
    }

    [System.Serializable]
    public class SpriteReference
    {
        public bool UseConstant = true;

        public Sprite ConstantValue;
        [RequiredField] public SpriteVar Variable;

        public SpriteReference() => UseConstant = true;
        public SpriteReference(Sprite value) => Value = value;

        public SpriteReference(SpriteVar value) => Value = value.Value;

        public Sprite Value
        {
            get => UseConstant ? ConstantValue : (Variable != null ? Variable.Value : null);
            set
            {
                if (UseConstant)
                    ConstantValue = value;
                else
                    Variable.Value = value;
            }
        }
    }
}
