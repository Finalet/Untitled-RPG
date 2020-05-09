using MalbersAnimations.Scriptables;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations
{
    public class BoolVarListener : MonoBehaviour 
    {
        public BoolReference value = new BoolReference();
        public UnityEvent OnTrue = new UnityEvent();
        public UnityEvent OnFalse = new UnityEvent();

        public bool Value { get => value; set => this.value.Value = value; }

        void OnEnable()
        {
            value.Variable?.OnValueChanged.AddListener(InvokeBool);
        }

        void OnDisable()
        {
            value.Variable?.OnValueChanged.RemoveListener(InvokeBool);
        }

        public virtual void InvokeBool(bool value)
        {
            if (value)
                OnTrue.Invoke();
            else
                OnFalse.Invoke();
        }
    }
}
