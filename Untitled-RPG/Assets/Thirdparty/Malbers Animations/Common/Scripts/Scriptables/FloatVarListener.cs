using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;
using UnityEngine;

namespace MalbersAnimations
{
    public class FloatVarListener : MonoBehaviour
    {
        public FloatReference value;
        public FloatEvent Raise = new FloatEvent();

        public float Value { get => value; set => this.value.Value = value; }

        void OnEnable()
        {
            value.Variable?.OnValueChanged.AddListener(InvokeFloat);
            Raise.Invoke(value ?? 0f);
        }

        void OnDisable()
        {
            value.Variable?.OnValueChanged.RemoveListener(InvokeFloat);
        }

        public virtual void InvokeFloat(float value)   
        {  Raise.Invoke(value);}
    }
}