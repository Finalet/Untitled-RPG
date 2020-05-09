using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.Events
{
    ///<summary>
    /// Listener to use with the GameEvents
    /// Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple
    /// </summary>
    public class MEventListener : MonoBehaviour
    {
        public List<MEventItemListener> Events = new List<MEventItemListener>();

        private void OnEnable()
        {
            foreach (var item in Events)
            {
                if (item.Event) item.Event.RegisterListener(item);
            }
        }

        private void OnDisable()
        {
            foreach (var item in Events)
            {
                if (item.Event) item.Event.UnregisterListener(item);
            }
        }
    }
     
    [System.Serializable]
    public class AdvancedIntegerEvent
    {
        public string name;
        public ComparerInt comparer = ComparerInt.Equal;
        public IntReference Value =  new IntReference();
        public IntEvent Response = new IntEvent();

        /// <summary>Use the comparer to execute a response using the Int Event and the Value</summary>
        /// <param name="IntValue">Value that comes from the IntEvent</param>
            public void ExecuteAdvanceIntegerEvent(int IntValue)
            {
                switch (comparer)
                {
                    case ComparerInt.Equal:
                        if (IntValue == Value) Response.Invoke(IntValue);
                        break;
                    case ComparerInt.Greater:
                        if (IntValue > Value) Response.Invoke(IntValue);
                        break;
                    case ComparerInt.Less:
                        if (IntValue < Value) Response.Invoke(IntValue);
                        break;
                    case ComparerInt.NotEqual:
                        if (IntValue != Value) Response.Invoke(IntValue);
                        break;
                    default:
                        break;
                }
            }
    }



    [System.Serializable]
    public class AdvancedBoolEvent
    {
        public string name;
        public ComparerBool comparer = ComparerBool.Equal;
        public BoolReference Value = new BoolReference();
        public UnityEvent Response = new UnityEvent();

        /// <summary>Use the comparer to execute a response using the Int Event and the Value</summary>
        /// <param name="boolValue">Value that comes from the IntEvent</param>
        public void ExecuteAdvanceBoolEvent(bool boolValue)
        {
            switch (comparer)
            {
                case ComparerBool.Equal:
                    if (boolValue == Value) Response.Invoke();
                    break;
                case ComparerBool.NotEqual:
                    if (boolValue != Value) Response.Invoke();
                    break;
                default:
                    break;
            }
        }
    }

    [System.Serializable]
    public class MEventItemListener
    {
        public MEvent Event;

        [HideInInspector]
        public bool useInt = false, useFloat = false, useVoid = true, useString = false, useBool = false, useGO = false, useTransform = false, useVector3, useVector2 = false;

        public UnityEvent Response = new UnityEvent();
        public FloatEvent ResponseFloat = new FloatEvent();
        public IntEvent ResponseInt = new IntEvent();

        public BoolEvent ResponseBool = new BoolEvent();
        public UnityEvent ResponseBoolFalse = new UnityEvent();
        public UnityEvent ResponseBoolTrue = new UnityEvent();

        public StringEvent ResponseString = new StringEvent();
        public GameObjectEvent ResponseGO = new GameObjectEvent();
        public TransformEvent ResponseTransform = new TransformEvent();
        public Vector3Event ResponseVector3 = new Vector3Event();
        public Vector2Event ResponseVector2 = new Vector2Event();

        public List<AdvancedIntegerEvent> IntEventList = new List<AdvancedIntegerEvent>();
        public bool AdvancedInteger = false;
        public bool AdvancedBool = false;
        [Tooltip("Inverts the value of the Bool Event")]
        public bool InvertBool = false;

        public virtual void OnEventInvoked() { Response.Invoke(); }
        public virtual void OnEventInvoked(string value) { ResponseString.Invoke(value); }
        public virtual void OnEventInvoked(float value) { ResponseFloat.Invoke(value); }

        public virtual void OnEventInvoked(int value)
        {
            ResponseInt.Invoke(value);

            if (AdvancedInteger)
            {
                foreach (var item in IntEventList)
                    item.ExecuteAdvanceIntegerEvent(value);
            }
        }

        public virtual void OnEventInvoked(bool value)
        {
            ResponseBool.Invoke(InvertBool ? !value : value);

            if (AdvancedBool)
            {
                if (value)
                    ResponseBoolTrue.Invoke();
                else
                    ResponseBoolFalse.Invoke();
            }
        }
        public virtual void OnEventInvoked(GameObject value) { ResponseGO.Invoke(value); }
        public virtual void OnEventInvoked(Transform value) { ResponseTransform.Invoke(value); }
        public virtual void OnEventInvoked(Vector3 value) { ResponseVector3.Invoke(value); }
        public virtual void OnEventInvoked(Vector2 value) { ResponseVector2.Invoke(value); }

        public MEventItemListener()
        {
            useVoid = true;
            useInt = useFloat = useString = useBool = useGO = useTransform = useVector3 = useVector2 = false;
        }
    }

}