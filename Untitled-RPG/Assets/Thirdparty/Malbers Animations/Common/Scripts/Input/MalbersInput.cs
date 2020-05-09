using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations
{
   // [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/malbers-input")]
    public class MalbersInput : MInput, IInputSource
    {
        #region Variables
        private ICharacterMove mCharacterMove;
        public IInputSystem InputSystem;

        public InputAxis Horizontal = new InputAxis("Horizontal", true, true);
        public InputAxis Vertical = new InputAxis("Vertical", true, true);
        public InputAxis UpDown = new InputAxis("UpDown", true, true);


        /// <summary>Send to the Character to Move using the interface ICharacterMove</summary>
        public bool MoveCharacter { set; get; }

        private float horizontal;        //Horizontal Right & Left   Axis X
        private float vertical;          //Vertical   Forward & Back Axis Z
        private float upDown;
        #endregion


        public virtual void SetMoveCharacter(bool val) { MoveCharacter = val; }

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

        void InitializeCharacter()
        { mCharacterMove = GetComponent<ICharacterMove>(); }


        public virtual void UpAxis(bool input)
        {
            //Debug.Log("UpAxis" + input);

            if (upDown == -1) return;        //This means that the Down Button was pressed so ignore the Up button
            upDown = input ? 1 : 0;
        }

        public virtual void DownAxis(bool input)
        {
            upDown = input ? -1 : 0;
        }

        void Update()  { SetInput(); }


        /// <summary>Send all the Inputs and Axis to the Animal</summary>
        protected override void SetInput()
        {
            horizontal = Horizontal.GetAxis;
            vertical = Vertical.GetAxis;
            if (UpDown.active) upDown = UpDown.GetAxis;
            CharacterAxisMove();
            base.SetInput();
        }

        protected void CharacterAxisMove()
        {
            if (MoveCharacter && mCharacterMove != null)
                mCharacterMove.SetInputAxis(new Vector3(horizontal, upDown, vertical));
        } 

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
    public class InputRow
    {
        public BoolReference active = new BoolReference(true);
        public string name = "InputName";
        public InputType type = InputType.Input;
        public string input = "Value";
        public KeyCode key = KeyCode.A;

        /// <summary>Type of Button of the Row System</summary>
        public InputButton GetPressed = InputButton.Press;
        /// <summary>Current Input Value</summary>
        public bool InputValue = false;


        public UnityEvent OnInputDown = new UnityEvent();
        public UnityEvent OnInputUp = new UnityEvent();
        public UnityEvent OnLongPress = new UnityEvent();
        public UnityEvent OnDoubleTap = new UnityEvent();
        public BoolEvent OnInputChanged = new BoolEvent();

        protected IInputSystem inputSystem = new DefaultInput();

       // public bool ShowEvents = false;

        #region LONG PRESS and Double Tap
        public float DoubleTapTime = 0.3f;                          //Double Tap Time
        public float LongPressTime = 0.5f;
        //public FloatReference LongPressTime = new FloatReference(0.5f);
        private bool FirstInputPress= false;
        private bool InputCompleted = false;
        private float InputCurrentTime;
        public UnityEvent OnInputPressed = new UnityEvent();
        public FloatEvent OnPressedNormalized = new FloatEvent();

        #endregion

        /// <summary>Return True or False to the Selected type of Input of choice</summary>
        public virtual bool GetInput
        {
            get
            {
                if (!active) return false;
                if (inputSystem == null) return false;

                var oldValue = InputValue;

                switch (GetPressed)
                {

                    case InputButton.Press:

                        InputValue = type == InputType.Input ? InputSystem.GetButton(input) : Input.GetKey(key);


                        if (oldValue != InputValue)
                        {
                            if (InputValue)
                                OnInputDown.Invoke();
                            else
                                OnInputUp.Invoke();
                        }
                        
                        OnInputChanged.Invoke(InputValue);

                        if (InputValue) OnInputPressed.Invoke();

                        return InputValue;


                    //-------------------------------------------------------------------------------------------------------
                    case InputButton.Down:

                        InputValue = type == InputType.Input ? InputSystem.GetButtonDown(input) : Input.GetKeyDown(key);

                        if (oldValue != InputValue)
                        {
                            if (InputValue) OnInputDown.Invoke();
                            OnInputChanged.Invoke(InputValue);
                        }
                        return InputValue;


                    //-------------------------------------------------------------------------------------------------------
                    case InputButton.Up:

                        InputValue = type == InputType.Input ? InputSystem.GetButtonUp(input) : Input.GetKeyUp(key);

                        if (oldValue != InputValue)
                        {
                            if (InputValue) OnInputUp.Invoke();
                            OnInputChanged.Invoke(InputValue);
                        }
                        return InputValue;

                    //-------------------------------------------------------------------------------------------------------
                    case InputButton.LongPress:

                        InputValue = type == InputType.Input ? InputSystem.GetButton(input) : Input.GetKey(key);

                        if (InputValue)
                        {
                            if (!InputCompleted)
                            {
                                if (!FirstInputPress)
                                {
                                    InputCurrentTime = Time.time;
                                    FirstInputPress = true;
                                    OnInputDown.Invoke();
                                }
                                else
                                {
                                    if (Time.time - InputCurrentTime >= LongPressTime)
                                    {
                                        OnLongPress.Invoke();
                                        OnPressedNormalized.Invoke(1);
                                        InputCompleted = true;                     //This will avoid the longpressed being pressed just one time
                                        return (InputValue = true);
                                    }
                                    else
                                    {
                                        OnPressedNormalized.Invoke((Time.time - InputCurrentTime) / LongPressTime);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!InputCompleted && FirstInputPress)
                            {
                                OnInputUp.Invoke();      //If the Input was released before the LongPress was completed ... take it as Interrupted
                            }
                            FirstInputPress = InputCompleted = false;  //This will reset the Long Press
                        }
                        return (InputValue = false);

                    //-------------------------------------------------------------------------------------------------------
                    case InputButton.DoubleTap:

                        InputValue = type == InputType.Input ? InputSystem.GetButtonDown(input) : Input.GetKeyDown(key);

                        if (InputValue)
                        {
                            if (InputCurrentTime != 0 && (Time.time - InputCurrentTime) > DoubleTapTime)
                            {
                                FirstInputPress = false;    //This is in case it was just one Click/Tap this will reset it
                            }

                            if (!FirstInputPress)
                            {
                                OnInputDown.Invoke();
                                InputCurrentTime = Time.time;
                                FirstInputPress = true;
                            }
                            else
                            {
                                if ((Time.time - InputCurrentTime) <= DoubleTapTime)
                                {
                                    FirstInputPress = false;
                                    InputCurrentTime = 0;
                                    OnDoubleTap.Invoke();       //Sucesfull Double tap

                                    return (InputValue = true);
                                }
                                else
                                {
                                    FirstInputPress = false;
                                }
                            }
                        }
                      
                        return (InputValue = false);
                }
                return false;
            }
        }

        public IInputSystem InputSystem
        {
            get { return inputSystem; }
            set { inputSystem = value; }
        }

        #region Constructors

        public InputRow(KeyCode k)
        {
            active.Value = true;
            type = InputType.Key;
            key = k;
            GetPressed = InputButton.Down;
            inputSystem = new DefaultInput();
        }

        public InputRow(string input, KeyCode key)
        {
            active.Value = true;
            type = InputType.Key;
            this.key = key;
            this.input = input;
            GetPressed = InputButton.Down;
            inputSystem = new DefaultInput();
        }

        public InputRow(string unityInput, KeyCode k, InputButton pressed)
        {
            active.Value = true;
            type = InputType.Key;
            key = k;
            input = unityInput;
            GetPressed = InputButton.Down;
            inputSystem = new DefaultInput();
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

             //   OnAxisValueChanged.Invoke(currentAxisValue);
                return currentAxisValue;
            }
        }

        /// <summary>
        /// Set/Get which Input System this Axis is using by Default is set to use the Unity Input System
        /// </summary>
        public IInputSystem InputSystem
        {
            get { return inputSystem; }
            set { inputSystem = value; }
        }

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