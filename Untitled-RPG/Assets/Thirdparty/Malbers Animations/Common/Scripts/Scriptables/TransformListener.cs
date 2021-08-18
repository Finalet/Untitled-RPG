using MalbersAnimations.Events;
using System;
using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    public class TransformListener : MonoBehaviour
    {
        [RequiredField] public TransformVar value;

        public TransformEvent OnValueChanged = new TransformEvent();


        void OnEnable()
        {
            value.OnValueChanged += Invoke;
            Invoke(value.Value);
        }

        void OnDisable()
        {
            value.OnValueChanged -= Invoke;
            Invoke(value.Value);
        }


        /// <summary> Used to use turn Objects to True or false </summary>
        public virtual void Invoke(Transform value)
        {
            OnValueChanged.Invoke(value);
        }
    }
}
