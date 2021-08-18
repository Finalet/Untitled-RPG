using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/malbers-input")]
    [AddComponentMenu("Malbers/Input/MInput")]
    public class MInput : MonoBehaviour, IInputSource
    {
        #region Variables
        public IInputSystem Input_System;

        public List<InputRow> inputs = new List<InputRow>();                                        //Used to convert them to dictionary
        public Dictionary<string, InputRow> DInputs = new Dictionary<string, InputRow>();        //Shame it cannot be Serialided :(

        public bool showInputEvents = false;
        public UnityEvent OnInputEnabled = new UnityEvent();
        public UnityEvent OnInputDisabled = new UnityEvent();

        [Tooltip("Inputs won't work on Time.Scale = 0")]
        public BoolReference IgnoreOnPause = new BoolReference(true);

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
        public virtual void Enable(bool val) { enabled = val; }

        private void OnEnable() { OnInputEnabled.Invoke(); }

        protected virtual void OnDisable()
        {
            if (Application.isPlaying && gameObject.activeInHierarchy)
            {
                OnInputDisabled.Invoke();

                foreach (var input in inputs)
                {
                    if (input.ResetOnDisable) input.OnInputChanged.Invoke(input.InputValue = false);  //Sent false to all Input listeners 
                }
            }
        }


        void Update() { SetInput(); }

        /// <summary>Send all the Inputs to the Animal</summary>
        protected virtual void SetInput()
        {
            if (IgnoreOnPause.Value && Time.timeScale == 0) return;

            foreach (var item in inputs)
                _ = item.GetValue;  //This will set the Current Input value to the inputs and Invoke the Values
        }


        /// <summary>Enable/Disable an Input Row</summary>
        public virtual void EnableInput(string name, bool value)
        {
            string[] inputs = name.Split(',');

            foreach (var inp in inputs)
            {
                if (DInputs.TryGetValue(inp, out InputRow input)) input.Active = value;
            }
        }

        public virtual void SetInput(string name, bool value)
        {
            if (DInputs.TryGetValue(name, out InputRow input))
            {
                input.InputValue = value;
                input.ToggleValue = value;
            }
        }


        /// <summary>Enable an Input Row</summary>
        public virtual void EnableInput(string name) => EnableInput(name, true);

        /// <summary> Disable an Input Row </summary>
        public virtual void DisableInput(string name) => EnableInput(name, false);

        /// <summary>Check if an Input Row  is active</summary>
        public virtual bool IsActive(string name)
        {
            if (DInputs.TryGetValue(name, out InputRow input))
                return input.Active;

            return false;
        }

        /// <summary>Check if an Input Row  exist  and returns it</summary>
        public virtual InputRow FindInput(string name) => inputs.Find(item => item.name == name);

        public bool GetInputValue(string name)
        {
            if (DInputs.TryGetValue(name, out InputRow getInput))
                return getInput.InputValue;

            return false;
        }

        public IInputAction GetInput(string name)
        {
            if (DInputs.TryGetValue(name, out InputRow getInput))
                return getInput;

            return null;
        }

        [ContextMenu("All Types = [Input]")]
        private void ChangeToInputs()
        {
            foreach (var inp in inputs)
                inp.type = InputType.Input;

            MTools.SetDirty(this);
        }

        [ContextMenu("All Types = [Keys]")]
        private void ChangeToKeys()
        {
            foreach (var inp in inputs)
                inp.type = InputType.Key;

            MTools.SetDirty(this);
        }


        [ContextMenu("Create/Jump")]
        private void CreateJumpInput()
        {
            inputs.Add(new InputRow(true, "Jump", "Jump", KeyCode.Space, InputButton.Press, InputType.Key));
            MTools.SetDirty(this);
        }

        [ContextMenu("Create/Sprint")]
        private void CreateSprintInput()
        {
            inputs.Add(new InputRow(true, "Sprint", "Sprint", KeyCode.LeftShift, InputButton.Press, InputType.Key));
            MTools.SetDirty(this);
        }

        [ContextMenu("Create/Main Attack")]
        private void CreateMainAttackInput()
        {
            inputs.Add(new InputRow(true, "Attack1", "Fire1", KeyCode.Mouse0, InputButton.Press, InputType.Key));
            MTools.SetDirty(this);
        }

        [ContextMenu("Create/Secondary Attack")]
        private void Create2ndAttackInput()
        {
            inputs.Add(new InputRow(true, "Attack2", "Fire2", KeyCode.Mouse1, InputButton.Press, InputType.Key));
            MTools.SetDirty(this);
        }

        [ContextMenu("Create/Action")]
        private void CreateInteraction()
        {
            inputs.Add(new InputRow(true, "Action", "Action", KeyCode.E, InputButton.Press, InputType.Key));
            MTools.SetDirty(this);
        }

        [ContextMenu("Create/Dodge")]
        private void CreateDodge()
        {
            inputs.Add(new InputRow(true, "Dodge", "Dodge", KeyCode.Q, InputButton.Press, InputType.Key));
            MTools.SetDirty(this);
        }
    }


[System.Serializable]
    public class InputProfile
    {
        public string name = "Default";
        public List<InputRow> inputs = new List<InputRow>();
    }
}