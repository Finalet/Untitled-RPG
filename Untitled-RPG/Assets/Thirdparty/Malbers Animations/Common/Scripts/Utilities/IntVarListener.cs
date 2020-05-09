using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;
using UnityEngine;

namespace MalbersAnimations
{
    public class IntVarListener : MonoBehaviour
    {
        public IntReference value;
        public IntEvent Raise = new IntEvent();

        public int Value { get => value; set => this.value.Value = value; }

        void OnEnable()
        {
            value.Variable?.OnValueChanged.AddListener(InvokeInt);
            Raise.Invoke(value ?? 0);
        }

        void OnDisable()
        {
            value.Variable?.OnValueChanged.RemoveListener(InvokeInt);
        }

        public virtual void InvokeInt(int value)  
        {  Raise.Invoke(value);}
    }
}