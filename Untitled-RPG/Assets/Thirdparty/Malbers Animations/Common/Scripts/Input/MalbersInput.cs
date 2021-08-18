using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/malbers-input")]
    [AddComponentMenu("Malbers/Input/Malbers Input")]
    public class MalbersInput : MInput, IInputSource
    {
        #region Variables
        private ICharacterMove mCharacterMove;
        public IInputSystem InputSystem;

        public InputAxis Horizontal = new InputAxis("Horizontal", true, true);
        public InputAxis Vertical = new InputAxis("Vertical", true, true);
        public InputAxis UpDown = new InputAxis("UpDown", false, true);


        /// <summary>Send to the Character to Move using the interface ICharacterMove</summary>
        public bool MoveCharacter { set; get; }

        private float horizontal;        //Horizontal Right & Left   Axis X
        private float vertical;          //Vertical   Forward & Back Axis Z
        private float upDown;
        #endregion

        protected Vector3 RawInputAxis;

        public virtual void SetMoveCharacter(bool val) => MoveCharacter = val;

        protected override void OnDisable()
        {
            base.OnDisable();
             mCharacterMove?.Move(Vector3.zero);       //When the Input is Disable make sure the character/animal is not moving.
        }


        void Awake()
        {
            InputSystem = DefaultInput.GetInputSystem(PlayerID);                   //Get Which Input System is being used

            //Update to all the Inputs to the active Input System
            Horizontal.InputSystem = Vertical.InputSystem = UpDown.InputSystem = InputSystem;
            foreach (var i in inputs)
                i.InputSystem = InputSystem;              

            List_to_Dictionary();       //Convert the Inputs to Dic... easier to find
            InitializeCharacter();
            MoveCharacter = true;       //Set that the Character can be moved
        }

        protected void InitializeCharacter() => mCharacterMove = GetComponent<ICharacterMove>();


        public virtual void UpAxis(bool input)
        {
            if (upDown == -1) return;        //This means that the Down Button was pressed so ignore the Up button
            upDown = input ? 1 : 0;
        }

        public virtual void DownAxis(bool input) => upDown = input ? -1 : 0;

        void Update() => SetInput();


        /// <summary>Send all the Inputs and Axis to the Animal</summary>
        protected override void SetInput()
        {
            horizontal = Horizontal.GetAxis;
            vertical = Vertical.GetAxis;
            upDown = UpDown.GetAxis;

            RawInputAxis = new Vector3(horizontal, upDown, vertical);
            if (MoveCharacter) mCharacterMove?.SetInputAxis(RawInputAxis);

            base.SetInput();
        }

        public void ResetInputAxis() => RawInputAxis = Vector3.zero;

        /// <summary>Convert the List of Inputs into a Dictionary</summary>
        void List_to_Dictionary()
        {
            DInputs = new Dictionary<string, InputRow>();
            foreach (var item in inputs)
                DInputs.Add(item.name, item);
        }
         
    }
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    #region InputRow and Input Axis

    
    
    /// <summary>Input Class to change directly between Keys and Unity Inputs </summary>
    [System.Serializable]
    public class InputRow : IInputAction
    {
        public string name = "InputName";
        public BoolReference active = new BoolReference(true);
        public InputType type = InputType.Input;
        public string input = "Value";
        public KeyCode key = KeyCode.A;

        /// <summary>Type of Button of the Row System</summary>
        public InputButton GetPressed = InputButton.Press;
        /// <summary>Current Input Value</summary>
        public bool InputValue = false;
        public bool ToggleValue = false;
        [Tooltip("When the Input is Disabled the Button will a false value to all their connections")]
        public bool ResetOnDisable = true;


        public UnityEvent OnInputDown = new UnityEvent();
        public UnityEvent OnInputUp = new UnityEvent();
        public UnityEvent OnLongPress = new UnityEvent();
        public UnityEvent OnDoubleTap = new UnityEvent();
        public BoolEvent OnInputChanged = new BoolEvent();
        public UnityEvent OnInputEnable = new UnityEvent();
        public UnityEvent OnInputDisable = new UnityEvent();

        protected IInputSystem inputSystem = new DefaultInput();

       // public bool ShowEvents = false;

        #region LONG PRESS and Double Tap
        public float DoubleTapTime = 0.3f;                          //Double Tap Time
        public float LongPressTime = 0.5f;
        //public FloatReference LongPressTime = new FloatReference(0.5f);
        private bool FirstInputPress= false;
        private bool InputCompleted = false;
        private float InputStartTime;
        public UnityEvent OnInputPressed = new UnityEvent();
        public FloatEvent OnPressedNormalized = new FloatEvent();

        #endregion

        /// <summary>Return True or False to the Selected type of Input of choice</summary>
        public virtual bool GetValue
        {
            get
            {
                if (!active) return false;
                if (inputSystem == null) return false;

                var oldValue = InputValue;

                switch (GetPressed)
                {
                    case InputButton.Press:

                        InputValue = (type == InputType.Input) ? InputSystem.GetButton(input) : Input.GetKey(key);

                        if (oldValue != InputValue)
                        {
                            if (InputValue)
                                OnInputDown.Invoke();
                            else
                                OnInputUp.Invoke();

                            OnInputChanged.Invoke(InputValue);
                        }

                        if (InputValue) OnInputPressed.Invoke();

                        break;
                    //-------------------------------------------------------------------------------------------------------
                    case InputButton.Down:

                        InputValue = (type == InputType.Input) ? InputSystem.GetButtonDown(input) : Input.GetKeyDown(key);

                        if (oldValue != InputValue)
                        {
                            if (InputValue) OnInputDown.Invoke();

                            OnInputChanged.Invoke(InputValue);
                        }
                        break;
                    //-------------------------------------------------------------------------------------------------------
                    case InputButton.Up:

                        InputValue = (type == InputType.Input) ? InputSystem.GetButtonUp(input) : Input.GetKeyUp(key);

                        if (oldValue != InputValue)
                        {
                            if (!InputValue) OnInputUp.Invoke();

                            OnInputChanged.Invoke(InputValue);
                        }
                        break;
                    //-------------------------------------------------------------------------------------------------------
                    case InputButton.LongPress:

                        InputValue = (type == InputType.Input) ? InputSystem.GetButton(input) : Input.GetKey(key);

                        if (oldValue != InputValue) OnInputChanged.Invoke(InputValue); //Just to make sure the Input is Pressed

                        if (InputValue)
                        {
                            if (!InputCompleted)
                            {
                                if (!FirstInputPress)
                                {
                                    InputStartTime = Time.time;
                                    FirstInputPress = true;
                                    OnInputDown.Invoke();
                                }
                                else
                                {
                                    if (MTools.ElapsedTime(InputStartTime, LongPressTime))
                                    {
                                        OnPressedNormalized.Invoke(1);
                                        OnLongPress.Invoke();
                                        InputCompleted = true;                     //This will avoid the longpressed being pressed just one time
                                        return (InputValue = true);
                                    }
                                    else
                                        OnPressedNormalized.Invoke((Time.time - InputStartTime) / LongPressTime);
                                }
                            }
                        }
                        else
                        {
                            //If the Input was released before the LongPress was completed ... take it as Interrupted
                            if (!InputCompleted && FirstInputPress) OnInputUp.Invoke();

                            FirstInputPress = InputCompleted = false;  //This will reset the Long Press
                        }



                        break;
                    //-------------------------------------------------------------------------------------------------------
                    case InputButton.DoubleTap:
                        InputValue = (type == InputType.Input) ? InputSystem.GetButton(input) : Input.GetKey(key);


                        if (oldValue != InputValue)
                        {
                            OnInputChanged.Invoke(InputValue); //Just to make sure the Input is Pressed

                            if (InputValue)
                            {
                                if (InputStartTime != 0 && MTools.ElapsedTime(InputStartTime, DoubleTapTime))
                                {
                                    FirstInputPress = false;    //This is in case it was just one Click/Tap this will reset it
                                }

                                if (!FirstInputPress)
                                {
                                    OnInputDown.Invoke();
                                    InputStartTime = Time.time;
                                    FirstInputPress = true;
                                }
                                else
                                {
                                    if ((Time.time - InputStartTime) <= DoubleTapTime)
                                    {
                                        FirstInputPress = false;
                                        InputStartTime = 0;
                                        OnDoubleTap.Invoke();       //Sucesfull Double tap
                                    }
                                    else
                                    {
                                        FirstInputPress = false;
                                    }
                                }
                            }
                        }
                        break;
                    case InputButton.Toggle:

                        InputValue = (type == InputType.Input) ? InputSystem.GetButtonDown(input) : Input.GetKeyDown(key);

                        if (oldValue != InputValue)
                        {
                            if (InputValue)
                            {
                                ToggleValue ^= true;
                                OnInputChanged.Invoke(ToggleValue);
                            }
                        }

                        break;
                    default: break;
                }
                return InputValue;
            }
        }

        public IInputSystem InputSystem { get => inputSystem; set => inputSystem = value; }
        public string Name { get => name; set => name = value; }
        public bool Active 
        { get => active.Value;
            set
            { active.Value = value;
                if (value)
                    OnInputEnable.Invoke();
                else 
                    OnInputEnable.Invoke();
            }
        }
        public InputButton Button => GetPressed;

        public UnityEvent InputDown => this.OnInputDown;

        public UnityEvent InputUp => this.OnInputUp;

        public BoolEvent InputChanged => this.OnInputChanged;


        #region Constructors

        public InputRow(KeyCode k)
        {
            active.Value = true;
            type = InputType.Key;
            key = k;
            GetPressed = InputButton.Down;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        public InputRow(string input, KeyCode key)
        {
            active.Value = true;
            type = InputType.Key;
            this.key = key;
            this.input = input;
            GetPressed = InputButton.Down;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        public InputRow(string unityInput, KeyCode k, InputButton pressed)
        {
            active.Value = true;
            type = InputType.Key;
            key = k;
            input = unityInput;
            GetPressed = InputButton.Down;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        public InputRow(string name, string unityInput, KeyCode k, InputButton pressed, InputType itype)
        {
            this.name = name;
            active.Value = true;
            type = itype;
            key = k;
            input = unityInput;
            GetPressed = pressed;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        public InputRow(bool active , string name, string unityInput, KeyCode k, InputButton pressed, InputType itype)
        {
            this.name = name;
            this.active.Value = active;
            type = itype;
            key = k;
            input = unityInput;
            GetPressed = pressed;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        public InputRow()
        {
            active.Value = true;
            name = "InputName";
            type = InputType.Input;
            input = "Value";
            key = KeyCode.A;
            GetPressed = InputButton.Press;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        #endregion
    }
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    [System.Serializable]
    public class InputAxis
    {
        public bool active = true;
        public string name = "NewAxis";
        public bool raw = true;
        public string input = "Value";
        IInputSystem inputSystem = new DefaultInput();
        public FloatEvent OnAxisValueChanged = new FloatEvent();
        float currentAxisValue = 0;



        /// <summary>Returns the Axis Value</summary>
        public float GetAxis
        {
            get
            {
                if (inputSystem == null || !active) return 0f;
                currentAxisValue = raw ? inputSystem.GetAxisRaw(input) : inputSystem.GetAxis(input);
                return currentAxisValue;
            }
        }

        /// <summary> Set/Get which Input System this Axis is using by Default is set to use the Unity Input System </summary>
        public IInputSystem InputSystem { get => inputSystem; set => inputSystem = value; }

        public InputAxis()
        {
            active = true;
            raw = true;
            input = "Value";
            name = "NewAxis";
            inputSystem = new DefaultInput();
        }

        public InputAxis(string value)
        {
            active = true;
            raw = false;
            input = value;
            name = "NewAxis";
            inputSystem = new DefaultInput();
        }

        public InputAxis(string InputValue, bool active, bool isRaw)
        {
            this.active = active;
            this.raw = isRaw;
            input = InputValue;
            name = "NewAxis";
            inputSystem = new DefaultInput();
        }

        public InputAxis(string name, string InputValue, bool active, bool raw)
        {
            this.active = active;
            this.raw = raw;
            input = InputValue;
            this.name = name;
            inputSystem = new DefaultInput();
        }

    }
    #endregion
}