using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MalbersAnimations
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/malbers-input")]
    public class MInput : MonoBehaviour, IInputSource
    {
        #region Variables
        public IInputSystem Input_System;

        public List<InputRow> inputs = new List<InputRow>();                                        //Used to convert them to dictionary
        public Dictionary<string, InputRow> DInputs = new Dictionary<string, InputRow>();        //Shame it cannot be Serialided :(

        public bool showInputEvents = false;
        public UnityEvent OnInputEnabled = new UnityEvent();
        public UnityEvent OnInputDisabled = new UnityEvent();

        public string PlayerID = "Player0"; //This is use for Rewired Asset
        #endregion

        void Awake()
        {
            Input_System = DefaultInput.GetInputSystem(PlayerID);                   //Get Which Input System is being used

            foreach (var i in inputs)
                i.InputSystem = Input_System;                 //Update to all the Inputs the Input System


            DInputs = new Dictionary<string, InputRow>();

            foreach (var item in inputs)
                DInputs.Add(item.name, item); 
        }

        /// <summary>Enable Disable the Input Script</summary>
        public virtual void Enable(bool val)
        { enabled = val; }

        private void OnEnable() { OnInputEnabled.Invoke(); }


        protected virtual void OnDisable()
        {
            if (Application.isPlaying)
            {
                OnInputDisabled.Invoke();

                foreach (var input in inputs)
                    input.OnInputChanged.Invoke(false);  //Sent false to all Input listeners 
            }
        }


        void Update() { SetInput(); }

        /// <summary>Send all the Inputs to the Animal</summary>
        protected virtual void SetInput()
        {
            foreach (var item in inputs)
            { var InputValue = item.GetInput;}             //This will set the Current Input value to the inputs and Invoke the Values
        }


        /// <summary>Enable/Disable an Input Row</summary>
        public virtual void EnableInput(string name, bool value)
        {
            if (DInputs.TryGetValue(name, out InputRow input))
                input.active.Value = value;
        }


        /// <summary>Enable an Input Row</summary>
        public virtual void EnableInput(string name)
        {

            if (DInputs.TryGetValue(name, out InputRow input))
                input.active.Value = true;
        }

        /// <summary> Disable an Input Row </summary>
        public virtual void DisableInput(string name)
        {
            if (DInputs.TryGetValue(name, out InputRow input))
                input.active.Value = false;
        }

        /// <summary>Check if an Input Row  is active</summary>
        public virtual bool IsActive(string name)
        {
            if (DInputs.TryGetValue(name, out InputRow input))
                return input.active;

            return false;
        }

        /// <summary>Check if an Input Row  exist  and returns it</summary>
        public virtual InputRow FindInput(string name)
        {
            return inputs.Find(item => item.name == name);
        }

        public bool GetInputValue(string name)
        {
            if (DInputs.TryGetValue(name, out InputRow getInput))
                return getInput.InputValue;
          
            return false;
        }

        public InputRow GetInput(string name)
        {
            if (DInputs.TryGetValue(name, out InputRow getInput))
                return getInput;

            return null;
        }
    }
}