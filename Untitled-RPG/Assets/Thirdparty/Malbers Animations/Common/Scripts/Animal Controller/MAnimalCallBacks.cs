using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MalbersAnimations.Scriptables;
using UnityEngine.Events;

namespace MalbersAnimations.Controller
{
    /// All Callbacks/Public Methods are Here
    public partial class MAnimal
    {
        #region INPUTS

        /// <summary>Disconnect and Reconnect the States and Modes to the Input Source</summary>
        public virtual void ResetInputSource()
        {
            UpdateInputSource(false); //Disconnect
            UpdateInputSource(true); //Reconnect
        }

        /// <summary>Updates all the Input from Malbers Input in case needed (Rewired conextion(</summary>
        public virtual void UpdateInputSource(bool connect)
        {
            if (InputSource == null)
                InputSource = gameObject.FindInterface<IInputSource>(); //Find if we have a InputSource

            if (InputSource != null)
            {
                foreach (var state in states)
                {
                    SetStatesInput(state, false);
                    if (connect) SetStatesInput(state, true);
                }

                foreach (var mode in modes)
                {
                    SetModesInput(mode, false);
                    if (connect) SetModesInput(mode, true);
                }
            }
        }

        /// <summary> Get the Inputs for the Source to add it to the States </summary>
        internal virtual void SetStatesInput(State state, bool connect)
        {
            if (!string.IsNullOrEmpty(state.Input)) //If a State has an Input 
            {
                var input = InputSource.GetInput(state.Input);

                if (input != null)
                {
                    if (connect)
                        input.InputChanged.AddListener(state.ActivatebyInput);
                    else
                        input.InputChanged.RemoveListener(state.ActivatebyInput);
                }
            }
        }

        /// <summary> Get the Inputs for the Source to add it to the States </summary>
        internal virtual void SetModesInput(Mode mode, bool connect)
        {
            if (!string.IsNullOrEmpty(mode.Input)) //If a mode has an Input 
            {
                var input = InputSource.GetInput(mode.Input);

                if (input != null)
                {
                    if (connect)
                        input.InputChanged.AddListener(mode.ActivatebyInput);
                    else
                        input.InputChanged.RemoveListener(mode.ActivatebyInput);
                }
            }
        }
        #endregion

        #region Player
        /// <summary>Set an Animal as the Main Player and remove the otherOne</summary>
        public virtual void SetMainPlayer()
        {
            if (MainAnimal) //if there's a main animal already seted
            {
                MainAnimal.isPlayer.Value = false;
            }

            this.isPlayer.Value = true;
            MainAnimal = this;
        }

        public void DisableMainPlayer()
        {
            if (MainAnimal == this) MainAnimal = null;
        }
        #endregion

        #region Teleport    
        public virtual void Teleport(Transform newPos)
        {
            if (newPos)
            {
                Teleport(newPos.position);

            }
            else
            {
                Debug.LogWarning("You are using Teleport but the Transform you are entering on the parameters is null");
            }
        }

        public virtual void TeleportRot(Transform newPos)
        {
            if (newPos)
            {
                Teleport(newPos.position);
                transform.rotation = newPos.rotation;
            }
            //else
            //{
            //    Debug.LogWarning("You are using TeleportRot but the Transform you are entering on the parameters is null");
            //}
        }


        public virtual void Teleport(Vector3 newPos)
        {
            Teleport_Internal(newPos);
            OnTeleport.Invoke(newPos);
        }

        /// <summary>  Used by the States to Teleport withouth sending the Event </summary>
        internal void Teleport_Internal(Vector3 newPos)
        {
            base.transform.position = newPos;
            LastPos = base.transform.position;
            platform = null;
        }

        #endregion

        #region Gravity
        /// <summary>Resets the gravity to the default Vector.Down value</summary>
        public void ResetGravityDirection() => Gravity = Vector3.down;

        /// <summary>Clears the Gravity Logic</summary>
        internal void ResetGravityValues()
        {
            GravityTime = m_gravityTime;
            GravityStoredVelocity = Vector3.zero;
            //  GravitySpeed = 0;
        }

     

        internal void ResetUPVector()
        {
            RB.velocity = Vector3.ProjectOnPlane(RB.velocity, UpVector); //Cleann the Gravity IMPORTAAANT!!!!!
            //Extra OPTIONAL
            AdditivePosition = Vector3.ProjectOnPlane(AdditivePosition, UpVector);
            DeltaPos = Vector3.ProjectOnPlane(DeltaPos, UpVector);
        }

        /// <summary>The Ground will change the Gravity Direction</summary>
        public void GroundChangesGravity(bool value) => ground_Changes_Gravity.Value = value;


        /// <summary>Aling with no lerp to the Gravity Direction</summary>
        public void AlignGravity()
        {
            Quaternion AlignRot = Quaternion.FromToRotation(transform.up, UpVector) * transform.rotation;  //Calculate the orientation to Terrain 
            base.transform.rotation = AlignRot;
        }
        #endregion

        #region Stances

        ///// <summary>Toggle the New Stance with the Default Stance▼▲ </summary>
        //public void Stance_Toggle(int NewStance) => Stance = (Stance == NewStance) ? DefaultStance : NewStance;

        /// <summary>Toggle the New Stance with the Default Stance▼▲ </summary>
        public void Stance_Toggle(StanceID NewStance) => Stance = (Stance.ID == NewStance.ID) ? DefaultStance : NewStance;

        public void Stance_Set(StanceID id) => Stance = id;
        public void Stance_Set(int id)
        {
            var NewStance = ScriptableObject.CreateInstance<StanceID>();
            NewStance.name = "Stance(" + id+")";
            NewStance.ID = id;

            Stance = NewStance;
        }
        public void Stance_Reset() => Stance = defaultStance;

        #endregion

        #region Animator Methods

        /// <summary>  Method required for the Interface IAnimator Listener to send messages From the Animator to any class who uses this Interface</summary>
        public virtual bool OnAnimatorBehaviourMessage(string message, object value)
        {
            foreach (var state in states) state.ReceiveMessages(message, value);

            return this.InvokeWithParams(message, value);
        }

        /// <summary>Set a Int on the Animator</summary>
        public void SetAnimParameter(int hash, int value) => Anim.SetInteger(hash, value);

        /// <summary>Set a float on the Animator</summary>
        public void SetAnimParameter(int hash, float value) => Anim.SetFloat(hash, value);

        /// <summary>Set a Bool on the Animator</summary>
        public void SetAnimParameter(int hash, bool value) => Anim.SetBool(hash, value);

        /// <summary>Set a Trigger to the Animator</summary>
        public void SetAnimParameter(int hash) => Anim.SetTrigger(hash);

        public void SetOptionalAnimParameter(int Hash, float value)
        {
            if (animatorParams.ContainsKey(Hash)) SetFloatParameter(Hash, value);
        }

        private void SetOptionalAnimParameter(int Hash, int value)
        {
            if (animatorParams.ContainsKey(Hash)) SetIntParameter?.Invoke(Hash, value);
        }

        private void SetOptionalAnimParameter(int Hash, bool value)
        {
            if (animatorParams.ContainsKey(Hash)) SetBoolParameter(Hash, value);
        }

        /// <summary> Set the Parameter Random to a value and pass it also to the Animator </summary>
        public void SetRandom(int value)
        {
            if (!enabled || Sleep) return;

            RandomID = Randomizer/* && !IsPlayingMode */? value : 0;
            SetOptionalAnimParameter(hash_Random, RandomID);
        }



        /// <summary>Used by Animator Events </summary>
        public virtual void EnterTag(string tag) => AnimStateTag = Animator.StringToHash(tag);
        #endregion

        #region States
        /// <summary> Set the StateFloat value and pass it  to the StateFloat parameter on the  Animator </summary>
        public void State_SetFloat(float value)
        { 
           // Debug.Log($"State_Float: {value:F3}");
            SetFloatParameter(hash_StateFloat, State_Float = value);
        }

        /// <summary>Find an old State and replace it for  a new one at Runtime </summary>
        public void State_Replace(State NewState)
        {
            if (CloneStates)
            {
                State instance = (State)ScriptableObject.CreateInstance(NewState.GetType());
                instance = ScriptableObject.Instantiate(NewState);                                 //Create a clone from the Original Scriptable Objects! IMPORTANT
                instance.name = instance.name.Replace("(Clone)", "(C)");
                NewState = instance;
            }

            var oldState = states.Find(s => s.ID == NewState.ID);

            if (oldState)
            {
                var index = states.IndexOf(oldState);
                var oldStatePriority = oldState.Priority;

                if (CloneStates) Destroy(oldState); //Destroy the Clone

                oldState = NewState;
                oldState.AwakeState(this);
                oldState.Priority = oldStatePriority;
                oldState.InitializeState();
                oldState.ExitState();


                states[index] = oldState; //Replace the list Item

                UpdateInputSource(true); //Need to Update the Sources
            }
        }

        /// <summary>Force the Activation of an state regarding if is enable or not</summary>
        public virtual void State_Force(StateID ID) => State_Force(ID.ID);

        /// <summary>Returns if the Animal has a state by its ID</summary>
        public bool HasState(StateID ID) => HasState(ID.ID);


        /// <summary>Returns if the Animal has a state by its int ID value</summary>
        public bool HasState(int ID) => State_Get(ID) != null;

        /// <summary>Returns if the Animal has a state by its name</summary>
        public bool HasState(string statename) => states.Exists(s => s.name == statename);

        /// <summary>Set the State Status on the Animator</summary>
        public virtual void State_SetStatus(int status)
        {
            SetOptionalAnimParameter(hash_StateEnterStatus, status);
        }

        /// <summary>Set the State Exit Status on the Animator</summary>
        public virtual void State_SetExitStatus(int ExitStatus)
        {
            SetOptionalAnimParameter(hash_StateExitStatus, ExitStatus);
        }

        public virtual void State_Enable(StateID ID) => State_Enable(ID.ID);
        public virtual void State_Disable(StateID ID) => State_Disable(ID.ID);

        public virtual void State_Enable(int ID) => State_Get(ID)?.Enable(true);

        public virtual void State_Disable(int ID) => State_Get(ID)?.Enable(false);

        /// <summary>Force the Activation of an state regarding if is enable or not</summary>
        public virtual void State_Force(int ID)
        {
            State state = State_Get(ID);
            
            if (state == ActiveState)
            {
                state.ForceActivate();

                StartCoroutine(C_EnterCoreAnim(state));  //Little Hack! 
            }
            else
                state.ForceActivate(); 
        }

        IEnumerator C_EnterCoreAnim(State state)
        {
            state.IsPending = true;
            yield return null;
           // yield return null;
            state.AnimationTagEnter(AnimStateTag);
        }

        /// <summary>  Allow Lower States to be activated  </summary>
        public virtual void State_AllowExit(StateID ID) => State_AllowExit(ID.ID);

        /// <summary>  Allow Lower States to be activated  </summary>
        public virtual void State_AllowExit(int ID)
        {
            State state = State_Get(ID);
            if (state && state != ActiveState) return; //Do not Exit if we are not the Active State
            state?.AllowExit();
        }

        public virtual void State_Allow_Exit(int nextState)
        {
            ActiveState.AllowExit();

            if (nextState != -1) State_Activate(nextState);
        }

        public virtual void State_InputTrue(StateID ID) => State_Get(ID)?.SetInput(true);
        public virtual void State_InputFalse(StateID ID) => State_Get(ID)?.SetInput(false);
        public virtual void ActiveStateAllowExit() => ActiveState.AllowExit();


        /// <summary>Try to Activate a State direclty from the Animal Script </summary>
        public virtual void State_Activate(StateID ID) => State_Activate(ID.ID);


        /// <summary>Try to Activate a State by its ID, Checking the necessary conditions for activation</summary>
        public virtual bool State_TryActivate(int ID)
        {
            State NewState = State_Get(ID);
            if (NewState && NewState.CanBeActivated)
            {
                if (!NewState.QUEUED())
                    return NewState.TryActivate();
            }
            return false;
        }

        /// <summary>Try to Activate a State direclty from the Animal Script </summary>
        public virtual void State_Activate(int ID)
        {
            State NewState = State_Get(ID);

            if (NewState && NewState.CanBeActivated)
            {
                if (!NewState.QUEUED())
                    NewState.Activate();
            }
        }

        /// <summary> Return a State by its  ID value </summary>
        public virtual State State_Get(int ID) => states.Find(s => s.ID == ID);

        /// <summary> Return a State by its ID</summary>
        public virtual State State_Get(StateID ID)
        {
            if (ID == null) return null;
            return State_Get(ID.ID);
        }

        /// <summary> Call the Reset State on the given State </summary>
        public virtual void State_Reset(int ID) => State_Get(ID)?.ResetState();

        /// <summary> Call the Reset State on the given State </summary>
        public virtual void State_Reset(StateID ID) => State_Reset(ID.ID);

        ///<summary> Find to the Possible State and store it to the (PreState) using an StateID value</summary>
        public virtual void State_Pin(StateID stateID) => State_Pin(stateID.ID);

        ///<summary> Find to the Possible State and store it to the (PreState) using an int value</summary>
        public virtual void State_Pin(int stateID) => Pin_State = State_Get(stateID);

        ///<summary>Use the (PreState) the and Try to activate it using an Input</summary>
        public virtual void State_Pin_ByInput(bool input) => Pin_State?.ActivatebyInput(input);
        public virtual void State_Pin_ByInputToggle() => Pin_State?.ActivatebyInput(!Pin_State.InputValue);

        ///<summary> Send to the Possible State (PreState) the value of the Input</summary>
        public virtual void State_Activate_by_Input(StateID stateID, bool input) => State_Activate_by_Input(stateID.ID, input);

        ///<summary> Send to the Possible State (PreState) the value of the Input</summary>
        public virtual void State_Activate_by_Input(int stateID, bool input)
        {
            State_Pin(stateID);
            State_Pin_ByInput(input);
        }

        ///<summary>If the Pin State is the Active State then it will set the StateExit Status</summary>
        public virtual void State_Pin_ExitStatus(int stateExitStatus)
        {
            if (Pin_State != null && Pin_State.IsActiveState)
                State_SetExitStatus(stateExitStatus);
        }

        #endregion

        #region Modes
        public bool HasMode(ModeID ID) => HasMode(ID.ID);

        /// <summary> Returns if the Animal has a mode By its ID</summary>
        public bool HasMode(int ID) => Mode_Get(ID) != null;

        /// <summary> Returns a Mode by its ID</summary>
        public virtual Mode Mode_Get(ModeID ModeID) => Mode_Get(ModeID.ID);

        /// <summary> Returns a Mode by its ID</summary>
        public virtual Mode Mode_Get(int ModeID) => modes.Find(m => m.ID == ModeID);

        /// <summary> Set the Parameter Int ID to a value and pass it also to the Animator </summary>
        public void SetModeStatus(int value) => SetIntParameter?.Invoke(hash_ModeStatus, ModeStatus = value);
        public void Mode_SetPower(float value) => SetOptionalAnimParameter(hash_ModePower, ModePower = value);

        /// <summary>Activate a Random Ability on the Animal using a Mode ID</summary>
        public virtual void Mode_Activate(ModeID ModeID) => Mode_Activate(ModeID.ID, -99);

        /// <summary>Enable a mode on the Animal</summary>
        /// <param name="ModeID">ID of the Mode</param>
        /// <param name="AbilityIndex">Ability Index. If this value is -1 then the Mode will activate a random Ability</param>
        public virtual void Mode_Activate(ModeID ModeID, int AbilityIndex) => Mode_Activate(ModeID.ID, AbilityIndex);

        #region INTERFACE ICHARACTER ACTION
        public bool PlayAction(int Set, int Index) => Mode_TryActivate(Set, Index);

        public bool ForceAction(int Set, int Index) => Mode_ForceActivate(Set, Index);

        public bool IsPlayingAction => IsPlayingMode;
        #endregion

        /// <summary>Activate a mode on the Animal combining the Mode and Ability e.g 4002</summary>
        public virtual void Mode_Activate(int ModeID)
        {
            if (ModeID == 0) return;

            var id = Mathf.Abs(ModeID / 1000);

            if (id == 0)
            {
                Mode_Activate(ModeID, -99);
            }
            else
            {
                Mode_Activate(id, ModeID % 100);
            }
        }

        /// <summary>Activate a mode on the Animal</summary>
        /// <param name="ModeID">ID of the Mode</param>
        /// <param name="AbilityIndex">Ability Index. If this value is -99 then the Mode will activate a random Ability</param>
        public virtual void Mode_Activate(int ModeID, int AbilityIndex)
        {
            var mode = Mode_Get(ModeID);

            if (mode != null)
            {
                Pin_Mode = mode;
                Pin_Mode.TryActivate(AbilityIndex);
            }
            else
            {
                Debug.LogWarning("You are trying to Activate a Mode but here's no Mode with the ID or is Disabled: " + ModeID);
            }
        }


        /// <summary>Activate a mode on the Animal</summary>
        /// <param name="ModeID">ID of the Mode</param>
        /// <param name="AbilityIndex">Ability Index. If this value is -99 then the Mode will activate a random Ability</param>
        public virtual void Mode_Activate(int ModeID, int AbilityIndex, AbilityStatus status)
        {
            var mode = Mode_Get(ModeID);

            if (mode != null)
            {
                Pin_Mode = mode;

                var ability = Pin_Mode.GetAbility(AbilityIndex);

                if (ability != null)
                {
                    ability.Properties.Status = status; //Change the Status of the ability
                }

                Pin_Mode.TryActivate(AbilityIndex);
            }
            else
            {
                Debug.LogWarning("You are trying to Activate a Mode but here's no Mode with the ID or is Disabled: " + ModeID);
            }
        }


        public virtual bool Mode_ForceActivate(ModeID ModeID, int AbilityIndex) => Mode_ForceActivate(ModeID.ID, AbilityIndex);

        public virtual void Mode_ForceActivate(ModeID ModeID) => Mode_ForceActivate(ModeID.ID, 0);

        public virtual bool Mode_ForceActivate(int ModeID, int AbilityIndex)
        {
            var mode = Mode_Get(ModeID);

            if (mode != null)
            {
                Pin_Mode = mode;
                return Pin_Mode.ForceActivate(AbilityIndex);
            }
            return false;
        }


        /// <summary>  Returns True and Activate  the mode in case ir can be Activated, if not it will return false</summary>
        public bool Mode_TryActivate(int ModeID, int AbilityIndex = -99)
        {
            var mode = Mode_Get(ModeID);

            if (mode != null)
            {
                Pin_Mode = mode;
                return Pin_Mode.TryActivate(AbilityIndex);
            }
            return false;
        }

        public bool Mode_TryActivate(int ModeID, int AbilityIndex, AbilityStatus status, float time = 0)
        {
            var mode = Mode_Get(ModeID);

            if (mode != null)
            {
                Pin_Mode = mode;
                return Pin_Mode.TryActivate(AbilityIndex, status, time);
            }
            return false;
        }


        /// <summary>Stop all modes </summary>
        public virtual void Mode_Stop()
        {
            if (IsPlayingMode)
            {
                activeMode.InputValue = false;
                Mode_Interrupt();
            }
            else
            {
                SetModeStatus(ModeAbility = Int_ID.Available);
                //ModeInternalStatus = MStatus.None; //IMPORTANT!
                return;
            }

            ActiveMode = null;
            ModeTime = 0;                            //Reset Mode Time 
        }

        /// <summary> Re-Check all the Sprint conditions and Invoke the Sprint Event </summary>
        public virtual void SprintUpdate() => Sprint = sprint; //Check Again the sprint everytime a new state is active IMPORTANT



        /// <summary>Set IntID to -2 to exit the Mode Animation</summary>
        public virtual void Mode_Interrupt() => SetModeStatus(Int_ID.Interrupted);

        /// <summary>Deactivate all modes</summary>
        public virtual void Mode_Disable_All()
        {
            foreach (var mod in modes)  mod.Disable();
        }

        /// <summary>Reactivate all modes</summary>
        public virtual void Mode_Enable_All()
        {
            foreach (var mod in modes) mod.Enable();
        }

        /// <summary>Disable a Mode by his ID</summary>
        public virtual void Mode_Disable(ModeID id) => Mode_Disable((int)id);

        /// <summary>Disable a Mode by his ID</summary>
        public virtual void Mode_Disable(int id) => Mode_Get(id)?.Disable();

        public virtual void Mode_Disable(string mod)
        {
            /// <summary>Enable/Disable an Input Row</summary>
            string[] inputs_Split = mod.Split(',');

            foreach (var inp in inputs_Split)
            {
                var M = this.modes.Find(x => x.Name.Contains(inp));
                M?.Disable();
            }
        }

        public virtual void Mode_Enable(string mod)
        {
            /// <summary>Enable/Disable an Input Row</summary>
            string[] inputs_Split = mod.Split(',');

            foreach (var inp in inputs_Split)
            {
                var M = this.modes.Find(x => x.Name.Contains(inp));
                M?.Enable();
            }
        }


        /// <summary>Enable a Mode by his ID</summary>
        public virtual void Mode_Enable(ModeID id) => Mode_Enable(id.ID);

        /// <summary>Enable a Mode by his ID</summary>
        public virtual void Mode_Enable(int id) => Mode_Get(id)?.Enable();

        /// <summary>Pin a mode to Activate later</summary>
        public virtual void Mode_Pin(ModeID ID)
        {
            if (Pin_Mode != null && Pin_Mode.ID == ID) return;  //the mode is already pinned

            var pin = Mode_Get(ID);

            Pin_Mode = null; //Important! Clean the Pin Mode 

            if (pin != null && pin.Active) Pin_Mode = pin;
        }

        /// <summary>Pin an Ability on the Pin Mode to Activate later</summary>
        public virtual void Mode_Pin_Ability(int AbilityIndex)
        {
            if (AbilityIndex == 0) return;
            if (Pin_Mode != null) Pin_Mode.SetAbilityIndex(AbilityIndex);
        }


        /// <summary>Enable/Disables an Ability on a mode, Returns true if the Method succeeded</summary>
        public virtual bool Mode_Ability_Enable(int ModeID, int AbilityID, bool enable)
        {
            var mode = Mode_Get(ModeID);
            if (mode != null)
            {
                var ability = mode.GetAbility(AbilityID);
                if (ability != null)
                {
                    ability.Active = enable;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Pin an Ability on the Pin Mode to Activate later</summary>
        public virtual void Mode_Pin_Ability_Enable(int AbilityIndex)
        {
            if (AbilityIndex == 0) return;
            var ability = Pin_Mode?.GetAbility(AbilityIndex);
            if (ability != null) ability.Active = true;
        }

        /// <summary>Pin an Ability on the Pin Mode to Activate later</summary>
        public virtual void Mode_Pin_Disable_Ability(int AbilityIndex)
        {
            if (AbilityIndex == 0) return;
            var ability = Pin_Mode?.GetAbility(AbilityIndex);
            if (ability != null) ability.Active = false;
        }


        /// <summary>Changes  Pinned Mode Status in all the Abilities</summary>
        public virtual void Mode_Pin_Status(int aMode)
        {
            if (Pin_Mode != null)
            {
                foreach (var ability in Pin_Mode.Abilities)
                {
                    ability.Status = (AbilityStatus)aMode;
                }
            }
        }

        /// <summary>Changes the Pinned Mode time when using Hold by time Status</summary>
        public virtual void Mode_Pin_Time(float time)
        {
            if (Pin_Mode != null)
                foreach (var ab in Pin_Mode.Abilities)
                    ab.Properties.HoldByTime = time;
        }

        public virtual void Mode_Pin_Enable(bool value) => Pin_Mode?.SetActive(value);
        public virtual void Mode_Pin_EnableInvert(bool value) => Pin_Mode?.SetActive(!value);

        public virtual void Mode_Pin_Input(bool value) => Pin_Mode?.ActivatebyInput(value);

        /// <summary>Tries to Activate the Pin Mode</summary>
        public virtual void Mode_Pin_Activate() => Pin_Mode?.TryActivate();

        /// <summary>Tries to Activate the Pin Mode with an Ability</summary>
        public virtual void Mode_Pin_AbilityActivate(int AbilityIndex) => Pin_Mode?.TryActivate(AbilityIndex);
        
        #endregion

        #region Movement
        public virtual void Strafe_Toggle() => Strafe ^= true;

        /// <summary>Gets the movement from the Input Script or AI</summary>
        public virtual void Move(Vector3 move)
        {
            UseRawInput = false;
            RawInputAxis = move;//.normalized; //Store the Last Raw Direction sent (Important to Normalize?? why??)
            Rotate_at_Direction = false;
            DeltaAngle = 0;
        }

        /// <summary>Gets the movement from the Input using a 2 Vector  (ex UI Axis Joystick)</summary>
        public virtual void Move(Vector2 move) => Move(new Vector3(move.x, 0, move.y));

        /// <summary>Gets the movement from the Input ignoring the Direction Vector, using a 2 Vector  (ex UI Axis Joystick)</summary>
        public virtual void MoveWorld(Vector2 move) => MoveWorld(new Vector3(move.x, 0, move.y));

        /// <summary>Stop the animal from moving, cleaning the Movement Axis</summary>
        public virtual void StopMoving() => Move(Vector3.zero);

        /// <summary>Add Inertia to the Movement</summary>d
        public virtual void AddInertia(ref Vector3 Inertia, float speed = 1f)
        {
            AdditivePosition += Inertia;
            Inertia = Vector3.Lerp(Inertia, Vector3.zero, DeltaTime * speed);
        } 
        #endregion

        #region Speeds
        /// <summary>Change the Speed Up</summary>
        public virtual void SpeedUp() => Speed_Add(+1);

        /// <summary> Changes the Speed Down </summary>
        public virtual void SpeedDown() => Speed_Add(-1);

        /// <summary> Get a SpeedSet by its name</summary>
        public virtual MSpeedSet SpeedSet_Get(string name) => speedSets.Find(x => x.name == name);

        public virtual MSpeed Speed_GetModifier(string name, int index)
        {
            var set = SpeedSet_Get(name);

            if (set != null && index < set.Speeds.Count)
                return set[index - 1];

            return MSpeed.Default;
        }

        /// <summary>Set a custom speed created via script and it uses it as the Current Speed Modifier (used on the Fall and Jump State)</summary>
        public virtual void SetCustomSpeed(MSpeed customSpeed, bool keepInertiaSpeed = false)
        {
            CustomSpeed = true;
            CurrentSpeedModifier = customSpeed;

            currentSpeedSet = null; //IMPORTANT SET THE CURRENT SPEED SET TO NULL

            if (keepInertiaSpeed)
            {
                CalculateTargetSpeed(); //Important needs to calculate the Target Speed again
                InertiaPositionSpeed = TargetSpeed; //Set the Target speed to the Fall Speed so there's no Lerping when the speed changes
            }
        }

        private void Speed_Add(int change) => CurrentSpeedIndex += change;

        /// <summary> Set an specific Speed for the active State </summary>
        public virtual void Speed_CurrentIndex_Set(int speedIndex) => CurrentSpeedIndex = speedIndex;

        /// <summary> Set an specific Speed for the active State using IntVars </summary>
        public virtual void Speed_CurrentIndex_Set(IntVar speedIndex) => CurrentSpeedIndex = speedIndex;

        /// <summary>Lock Speed Changes on the Animal</summary>
        public virtual void Speed_Change_Lock(bool lockSpeed) => SpeedChangeLocked = lockSpeed;

        /// <summary>Set a Speed Modifier and its indexx</summary>
        public virtual void SpeedSet_Set_Active(string SpeedSetName, int activeIndex)
        {
            var speedSet = SpeedSet_Get(SpeedSetName);

            if (speedSet != null)
            {
                speedSet.CurrentIndex = activeIndex;

                if (CurrentSpeedSet == speedSet)
                {
                    CurrentSpeedIndex = activeIndex;
                    speedSet.StartVerticalIndex = activeIndex; //Set the Start Vertical Index as the new Speed 
                }
            }
        }

        public virtual void Speed_Update_Current() => CurrentSpeedIndex = CurrentSpeedIndex;
        public virtual void Speed_SetTopIndex(int topIndex) 
        {
            CurrentSpeedSet.TopIndex = topIndex;
            Speed_Update_Current();
        }

        public virtual void Speed_SetTopIndex(string SpeedSetName, int topIndex)
        {
            var speedSet = SpeedSet_Get(SpeedSetName);
            if (speedSet != null)
            {
                speedSet.TopIndex = topIndex;
                Speed_Update_Current();
            }
        }


        /// <summary> Change the Speed of a Speed Set</summary>
        public virtual void SpeedSet_Set_Active(string SpeedSetName, string activeSpeed)
        {
            var speedSet = speedSets.Find(x => x.name.ToLower() == SpeedSetName.ToLower());

            if (speedSet != null)
            {
               var mspeedIndex = speedSet.Speeds.FindIndex(x => x.name.ToLower() == activeSpeed.ToLower());

                if (mspeedIndex != -1)
                {
                    speedSet.CurrentIndex = mspeedIndex + 1;

                    if (CurrentSpeedSet == speedSet)
                    {
                        CurrentSpeedIndex = mspeedIndex + 1;
                        speedSet.StartVerticalIndex = CurrentSpeedIndex; //Set the Start Vertical Index as the new Speed 
                    }
                }
            }
            else
            {
                Debug.LogWarning("There's no Speed Set called : " + SpeedSetName);
            }
        }

        #endregion

        #region Extrass



        public virtual void Force_Add(
            Vector3 Direction, float Force, float Aceleration,
            bool ResetGravity, bool ForceAirControl = true,  float LimitForce = 0)
        {
            var CurrentForce = CurrentExternalForce + GravityStoredVelocity; //Calculate the Starting force

            if (LimitForce > 0 && CurrentForce.magnitude > LimitForce)
                CurrentForce = CurrentForce.normalized * LimitForce; //Add the Bounce

            CurrentExternalForce = CurrentForce;
            ExternalForce = Direction.normalized * Force;
            ExternalForceAcel = Aceleration;

            if (ActiveState.ID == StateEnum.Fall) //If we enter to a zone from the Fall state.. Reset the Fall Current Distance
            {
                var fall = ActiveState as Fall;
                fall.FallCurrentDistance = 0;
            }

            if (ResetGravity) GravityTime = 0;

            ExternalForceAirControl = ForceAirControl;

        }

        public virtual void Force_Remove(float Aceleration = 0)
        {
            ExternalForceAcel = Aceleration;
            ExternalForce = Vector3.zero;
        }

        internal void Force_Reset()
        {
            CurrentExternalForce = Vector3.zero;
            ExternalForce = Vector3.zero;
            ExternalForceAcel = 0;
        }

        public virtual void DisableAnimalComponent(float time)
        {
            StartCoroutine(disableself(time));
        }
        IEnumerator disableself(float time)
        {
            if (time >0) yield return new WaitForSeconds(time);
            enabled = false;
            yield return null;
        }

        public void SetPlatform(Transform newPlatform)
        {
            platform = newPlatform;
            platform_Pos = platform.position;
            platform_Rot = platform.rotation;
        }

        /// <summary> If the Animal has touched the ground then Grounded will be set to true  </summary>
        public bool CheckIfGrounded()
        {
            AlignRayCasting();

            if (MainRay && FrontRay && !DeepSlope)
            {
                return Grounded = true;   //Activate the Grounded Parameter so the Idle and the Locomotion State can be activated
            }
            
            return false;
        }

       public void Always_Forward(bool value) => AlwaysForward = value;

        /// <summary>Activate Attack triggers </summary>
        public virtual void ActivateDamager(int ID)
        {
            if (Sleep) return;

            if (ID == -1)                         //Enable all Attack Triggers
            {
                foreach (var dam in Attack_Triggers) dam.DoDamage(true);
            }
            else if (ID == 0)                     //Disable all Attack Triggers
            {
                foreach (var dam in Attack_Triggers) dam.DoDamage(false);
            }
            else
            {
                var Att_T = Attack_Triggers.FindAll(x => x.Index == ID);        //Enable just a trigger with an index

                if (Att_T != null)
                    foreach (var dam in Att_T) dam.DoDamage(true);
            }
        }

        /// <summary>Store all the Animal Colliders </summary>
        internal void GetAnimalColliders()
        {
           var colls = GetComponentsInChildren<Collider>(true).ToList();      //Get all the Active colliders

           colliders = new List<Collider>();

            foreach (var item in colls)
            {
                if (!item.isTrigger/* && item.gameObject.layer == gameObject.layer*/) colliders.Add(item);        //Add the Animal Colliders Only
            }
        }

        /// <summary>Enable/Disable All Colliders on the animal. Avoid the Triggers </summary>
        public virtual void EnableColliders(bool active)
        {
            foreach (var item in colliders) item.enabled = active;
        }

      

        /// <summary>Disable this Script and MalbersInput Script if it has it.</summary>
        public virtual void DisableAnimal()
        {
            enabled = false;
            MalbersInput MI = GetComponent<MalbersInput>();
            if (MI) MI.enabled = false;
        }

        public void SetTimeline(bool isonTimeline)
        {
            Sleep = isonTimeline;
            
            //Unparent the Rotator since breaks the Cinemachine Logic
            if (Rotator != null)   RootBone.parent = isonTimeline ? null : Rotator;
        }


        /// <summary>InertiaPositionSpeed = TargetSpeed</summary>
        public void ResetInertiaSpeed() => InertiaPositionSpeed = TargetSpeed;

        public void UseCameraBasedInput() => UseCameraInput = true;
        #endregion
    }
}