using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations
{
    public class IntComparer : MonoBehaviour
    {
        public IntReference value = new IntReference(0);
        public List<AdvancedIntegerEvent> IntEventList = new List<AdvancedIntegerEvent>();

        void OnEnable()
        {
            if (!value.UseConstant) 
                value.Variable.OnValueChanged.AddListener(Compare);
        }

        void OnDisable()
        {
            if (!value.UseConstant) 
                value.Variable.OnValueChanged.RemoveListener(Compare);
        }

        public virtual void SetValue(int newval) { value = newval; }
        
        public virtual void AddValue(int newval) { value.Value += newval; }


        /// <summary>Compares the Int parameter on this Component and if the condition is made then the event will be invoked</summary>
        public virtual void Compare()
        {
            foreach (var item in IntEventList)
                item.ExecuteAdvanceIntegerEvent(value);
        }


        /// <summary>Compares an given int Value and if the condition is made then the event will be invoked</summary>
        public virtual void Compare(int value)
        {
            foreach (var item in IntEventList)
                item.ExecuteAdvanceIntegerEvent(value);
        }

        /// <summary>Compares an given intVar Value and if the condition is made then the event will be invoked</summary>
        public virtual void Compare(IntVar value)
        {
            foreach (var item in IntEventList)
                item.ExecuteAdvanceIntegerEvent(value.Value);
        }
    }
}