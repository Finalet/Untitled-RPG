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
                _transform.rotation = newPos.rotation;
            }
            else
            {
                Debug.LogWarning("You are using TeleportRot but the Transform you are entering on the parameters is null");
            }
        }


        public virtual void Teleport(Vector3 newPos)
        {
            transform.position = newPos;
            LastPos = transform.position;
            platform = null;
        }
        #endregion

        #region Gravity
        /// <summary>Resets the gravity to the default Vector.Down value</summary>
        public void ResetGravityDirection() { GravityDirection = Vector3.down; }
        /// <summary>Resets the gravity to the default Vector.Down value</summary>
        public void ResetGravity() { ResetGravityDirection(); }

        /// <summary>The Ground</summary>
        public void GroundChangesGravity(bool value) 
        {
            ground_Changes_Gravity = value; 
        }

        /// <summary>Aling with no lerp to the Gravity Direction</summary>
        public void AlignGravity()
        {
            Quaternion AlignRot = Quaternion.FromToRotation(_transform.up, UpVector) * _transform.rotation;  //Calculate the orientation to Terrain 
            transform.rotation = AlignRot;
        }
        #endregion

        #region Stances
        /// <summary>Toogle the New Stance with the Default Stance▼▲ </summary>
        public void Stance_Toggle(int NewStance)
        {
            Stance = Stance == NewStance ? defaultStance : NewStance;
        }

        /// <summary>Toogle the New Stance with the Default Stance▼▲ </summary>
        public void StanceToggle(StanceID NewStance) { Stance_Toggle(NewStance.ID); }

        public void Stance_Set(StanceID id) { Stance = id ?? 0; }

        public void Stance_Reset() { Stance = defaultStance; }

        #endregion

        #region Animator Methods
        /// <summary>  Method required for the Interface IAnimator Listener to send messages From the Animator to any class who uses this Interface</summary>
        public virtual void OnAnimatorBehaviourMessage(string message, object value)
        {
            this.InvokeWithParams(message, value);

            foreach (var state in states)
                state.ReceiveMessages(message, value);
        }

        /// <summary>Set a Int on the Animator</summary>
        public void SetAnimParameter(int hash, int value) { Anim.SetInteger(hash, value); }

        /// <summary>Set a float on the Animator</summary>
        public void SetAnimParameter(int hash, float value) { Anim.SetFloat(hash, value); }

        /// <summary>Set a Bool on the Animator</summary>
        public void SetAnimParameter(int hash, bool value) { Anim.SetBool(hash, value); }

        /// <summary>Set a Trigger to the Animator</summary>
        public void SetAnimParameter(int hash) { Anim.SetTrigger(hash); }

        /// <summary> Set the Parameter Int ID to a value and pass it also to the Animator </summary>
        public void SetIntID(int value)
        {
            //Debug.Log(value);
            SetAnimParameter(hash_IDInt, IntID = value);
        }

        /// <summary> Set the Parameter Random to a value and pass it also to the Animator </summary>
        public void SetRandomID(int value)
        {
            if (hasRandom)
            {
                RandomID = Randomizer && !IsPlayingMode ? value : 0;
                SetAnimParameter(hash_Random, RandomID);
            }
        }

        /// <summary> Set the Parameter Float ID to a value and pass it also to the Animator </summary>
        public void SetFloatID(float value) { SetAnimParameter(hash_IDFloat, IDFloat = value); }

        ///// <summary>Set a Random number to ID Int , that work great for randomly Play More animations</summary>
        //protected void SetIntIDRandom(int range) { SetIntID(IntID = Random.Range(1, range + 1)); }

        /// <summary>Used by Animator Events </summary>
        public virtual void EnterTag(string tag) { AnimStateTag = Animator.StringToHash(tag); }
        #endregion

        #region States

        /// <summary>Force the Activation of an state regarding if is enable or not</summary>
        public virtual void State_Force(StateID ID) { State_Force(ID.ID); }


        public bool HasState(StateID ID) { return HasState(ID.ID); }
        public bool HasState(int ID)
        {
            return statesD.ContainsKey(ID);
        }

        public bool HasMode(ModeID ID) { return HasMode(ID.ID); }
        public bool HasMode(int ID)
        {
            return modes.Find(x=> x.ID == ID) != null;
        }

        /// <summary>Force the Activation of an state regarding if is enable or not</summary>
        public virtual void State_SetStatus(int status)
        {
            if (hasStateStatus) SetAnimParameter(hash_StateStatus, StateStatus = status);
        }

        public virtual void State_Enable(StateID ID) { State_Enable(ID.ID); }
        public virtual void State_Disable(StateID ID) { State_Disable(ID.ID); }

        public virtual void State_Enable(int ID)
        {
            if (statesD.TryGetValue(ID, out State s))
            {
                s.Active = true;
            }
            else
            {
                Debug.LogWarning("There's no State with the ID: <B>"+ID+"</B> to Enable");
            }
        }

        public virtual void State_Disable(int ID)
        {
            if (statesD.TryGetValue(ID, out State s))
            {
                s.Active = false;
            }
            else
            {
                Debug.LogWarning("There's no State with the ID: <B>" + ID + "</B> to Disable");
            }
        }

        /// <summary>Force the Activation of an state regarding if is enable or not</summary>
        public virtual void State_Force(int ID)
        {
            if (statesD.TryGetValue(ID, out State s))
            {
                if (s == ActiveState) return; //Do not Activate the Already Active State
                s.Activate();
            }
            else
            {
                Debug.LogWarning("There's no State with the ID: <B>" + ID + "</B> to Force Activate");
            }
        }

        //public virtual void State_Exit(StateID ID) { State_Exit(ID.ID); }
        //public virtual void State_Exit(int ID)
        //{
        //    if (statesD.TryGetValue(ID, out State s))
        //    {
        //        if (s != ActiveState) return; //Do not Exit if we are not the Active State
        //        s.ExitState();
        //    }
        //    else
        //    {
        //        Debug.LogWarning("There's no State with the ID: <B>" + ID + "</B> to Exit");
        //    }
        //}

        public virtual void State_AllowExit(StateID ID) { State_AllowExit(ID.ID); }

        public virtual void State_AllowExit(int ID)
        {
            if (statesD.TryGetValue(ID, out State s))
            {
                if (s != ActiveState) return; //Do not Exit if we are not the Active State
                s.AllowExit();
            }
            else
            {
                Debug.LogWarning("There's no State with the ID: <B>" + ID + "</B> to Allow Exit");
            }
        }

        /// <summary>Try to Activate a State direclty from the Animal Script </summary>
        public virtual void State_Activate(StateID id) 
        { State_TryActivate(id.ID); }

        public virtual bool State_TryActivate(int ID)
        {
            if (statesD.TryGetValue(ID, out State NewState))
            {
                if (NewState.CanBeActivated)
                {
                    NewState.Activate();
                    return true;
                }
            }
            return false;
        }

        /// <summary>Try to Activate a State direclty from the Animal Script </summary>
        public virtual void State_Activate(int ID)
        {
            if (statesD.TryGetValue(ID, out State NewState))
            {
                if (NewState.CanBeActivated) 
                    NewState.Activate();
            }
        }



        /// <summary> Return a State by its ID </summary>
        public virtual State State_Get(int ID)
        {
            if (statesD.TryGetValue(ID, out State state))
                return state;

            return null;
        }

        public virtual State State_Get(StateID ID)
        { return State_Get(ID.ID); }

        /// <summary> Call the Reset State on the given State </summary>
        public virtual void State_Reset(int ID)
        {
            if (statesD.TryGetValue(ID, out State toReset))
                toReset.ResetState();
        }

        /// <summary> Call the Reset State on the given State </summary>
        public virtual void State_Reset(StateID ID) 
        { State_Reset(ID.ID); }


        ///<summary> Find to the Possible State and store it to the (PreState) using an StateID value</summary>
        public virtual void State_Pin(StateID stateID)
        {
            State_Pin(stateID.ID);
        }

        ///<summary> Find to the Possible State and store it to the (PreState) using an int value</summary>
        public virtual void State_Pin(int stateID)
        {
            statesD.TryGetValue(stateID, out Pin_State);
        }

        ///<summary>Use the (PreState) the and Try to activate it using an Input</summary>
        public virtual void State_Pin_ByInput(bool input)
        {
            Pin_State?.ActivatebyInput(input);
        }

        ///<summary> Send to the Possible State (PreState) the value of the Input</summary>
        public virtual void State_Activate_by_Input(StateID stateID, bool input)
        {
            State_Activate_by_Input(stateID.ID, input);
        }

        ///<summary> Send to the Possible State (PreState) the value of the Input</summary>
        public virtual void State_Activate_by_Input(int stateID, bool input)
        {
            State_Pin(stateID);
            State_Pin_ByInput(input);
        }
        #endregion

        /// <summary>InertiaPositionSpeed = TargetSpeed</summary>
        public void KeepInertiaSpeedModifier() { InertiaPositionSpeed = TargetSpeed; }

        public void UseCameraBasedInput() {UseCameraInput = true;}
        

        #region Modes
        /// <summary> Returns a Mode by its ID</summary>
        public virtual Mode Mode_Get(ModeID ModeID)   { return Mode_Get(ModeID.ID); }


        /// <summary> Returns a Mode by its ID</summary>
        public virtual Mode Mode_Get(int ModeID)  { return modes.Find(x => x.ID.ID == ModeID); }

        /// <summary>Activate a Random Ability on the Animal using a Mode ID</summary>
        public virtual void Mode_Activate(ModeID ModeID) { Mode_Activate(ModeID.ID, -1); }
       
        /// <summary>Adds a listener to a Mode Ability On Enter </summary>
        public virtual void Mode_Ability_AddListenerEnter(string AbilityName, UnityAction action)
        {
            Ability abi = null;

            foreach (var mode in modes)
            {
                abi = mode.Abilities.Find(x => x.Name == AbilityName);

                if (abi != null)
                {
                    abi.OverrideProp = true;
                    break;
                }
            }

            if (abi != null)
                abi.OverrideProperties.OnEnter.AddListener(action);

        }
        /// <summary>Adds a listener to a Mode Ability On Enter </summary>
        public virtual void Mode_Ability_AddListenerExit(string AbilityName, UnityAction action)
        {
            Ability abi = null;

            foreach (var mode in modes)
            {
                abi = mode.Abilities.Find(x => x.Name == AbilityName);

                if (abi != null)
                {
                    abi.OverrideProp = true;
                    break;
                }
            }

            if (abi != null)
                abi.OverrideProperties.OnExit.AddListener(action);

        }
        /// <summary>Removes a listener to a Mode Ability On Enter </summary>
        public virtual void Mode_Ability_RemoveListenerEnter(string AbilityName, UnityAction action)
        {
            Ability abi = null;

            foreach (var mode in modes)
            {
                abi = mode.Abilities.Find(x => x.Name == AbilityName);

                if (abi != null)
                {
                    abi.OverrideProp = true;
                    break;
                }
            }

            if (abi != null)
                abi.OverrideProperties.OnEnter.RemoveListener(action);

        }
        /// <summary>Removes a listener to a Mode Ability On Exit </summary>
        public virtual void Mode_Ability_RemoveListenerExit(string AbilityName, UnityAction action)
        {
            Ability abi = null;

            foreach (var mode in modes)
            {
                abi = mode.Abilities.Find(x => x.Name == AbilityName);

                if (abi != null)
                {
                    abi.OverrideProp = true;
                    break;
                }
            }

            if (abi != null)
                abi.OverrideProperties.OnExit.RemoveListener(action);

        }


        /// <summary>Enable a mode on the Animal</summary>
        /// <param name="ModeID">ID of the Mode</param>
        /// <param name="AbilityIndex">Ability Index. If this value is -1 then the Mode will activate a random Ability</param>
        public virtual void Mode_Activate(ModeID ModeID, int AbilityIndex)
        {
            Mode_Activate(ModeID.ID, AbilityIndex);
        }


        /// <summary>Activate a mode on the Animal combingin the Mode and Ability e.g 4002</summary>
        public virtual void Mode_Activate(int ModeID)
        { Mode_Activate(ModeID, -1); }


        /// <summary>Activate a mode on the Animal</summary>
        /// <param name="ModeID">ID of the Mode</param>
        /// <param name="AbilityIndex">Ability Index. If this value is -1 then the Mode will activate a random Ability</param>
        public virtual void Mode_Activate(int ModeID, int AbilityIndex)
        {
            var mode = Mode_Get(ModeID);

            if (mode != null && mode.active && AbilityIndex !=0)
            {
                Pin_Mode = mode;
                Pin_Mode.AbilityIndex = AbilityIndex;
                Pin_Mode.TryActivate();
            }
            else
            {
                Debug.LogWarning("You are trying to Activate a Mode but here's no Mode with the ID or is Disabled: " + ModeID);
            }
        }

        /// <summary>
        /// Returns True and Activate  the mode in case ir can be Activated, if not it will return false</summary>
        public bool Mode_TryActivate(int ModeID, int AbilityIndex = -1)
        {
            var mode = Mode_Get(ModeID);

            if (mode != null && mode.active)
            {
                Pin_Mode = mode;
                Pin_Mode.AbilityIndex = AbilityIndex;
                return Pin_Mode.TryActivate();
            }
            return false;
        }

        /// <summary>Stop all modes </summary>
        public virtual void Mode_Stop() 
        {
            ActiveMode = null;
            SetModeParameters(null);
            InputMode = null;
        }

        /// <summary>Set IntID to -2 to exit the Mode Animation</summary>
        public virtual void Mode_Interrupt() 
        {
            SetIntID(Int_ID.Interrupted);//Means the Mode is interrupted
            if (IsPlayingMode) LastModeStatus = MStatus.Interrupted;
        }         
        

        /// <summary>Disable a Mode by his ID</summary>
        public virtual void Mode_Disable(ModeID id) { Mode_Disable((int)id); }

        /// <summary>Disable a Mode by his ID</summary>
        public virtual void Mode_Disable(int id)
        {
            var mod = Mode_Get(id);
            if (mod != null)
            {
                mod.Disable();
            }
        }


        /// <summary>Enable a Mode by his ID</summary>
        public virtual void Mode_Enable(ModeID id)
        { Mode_Enable( id.ID); }

        /// <summary>Enable a Mode by his ID</summary>
        public virtual void Mode_Enable(int id)
        {
            var newMode = Mode_Get(id);
            if (newMode != null)
                newMode.active = true;
        }


        /// <summary>Pin a mode to Activate later</summary>
        public virtual void Mode_Pin(ModeID ID)
        {
            if (Pin_Mode != null && Pin_Mode.ID == ID) return;  //the mode is already pinned

            var pin = Mode_Get(ID);

            Pin_Mode = null; //Important! Clean the Pin Mode 

            if (pin == null)
                Debug.LogWarning("There's no " + ID.name + "Mode");
            else if (pin.active)
                Pin_Mode = pin;

        }


        /// <summary>Pin an Ability on the Pin Mode to Activate later</summary>
        public virtual void Mode_Pin_Ability(int AbilityIndex)
        {
            if (AbilityIndex == 0) return;

            Pin_Mode?.SetAbilityIndex(AbilityIndex);
        }


        /// <summary>Changes the Pinned Mode Status</summary>
        public virtual void Mode_Pin_Status(int aMode)
        {
            if (Pin_Mode != null)
                Pin_Mode.GlobalProperties.Status = (AbilityStatus)aMode;
        }

        /// <summary>Changes the Pinned Mode time when using Hold by time Status</summary>
        public virtual void Mode_Pin_Time(float time)
        {
            if (Pin_Mode != null)
                Pin_Mode.GlobalProperties.HoldByTime = time;
        }

        public virtual void Mode_Pin_Input(bool value)
        {
            Pin_Mode?.ActivatebyInput(value);
        }

        /// <summary>Tries to Activate the Pin Mode</summary>
        public virtual void Mode_Pin_Activate()
        {
            Pin_Mode?.TryActivate();
        }

        /// <summary>Tries to Activate the Pin Mode with an Ability</summary>
        public virtual void Mode_Pin_AbilityActivate(int AbilityIndex)
        {
            if (AbilityIndex == 0) return;

            if (Pin_Mode != null)
            {
                Pin_Mode.AbilityIndex = AbilityIndex;
                Pin_Mode.TryActivate();
            }
        }
        #endregion

        #region Old Animal Methods

        //  public virtual void SetAction(Action actionID)
        //  {
        //      Mode_Activate(ModeEnum.Action, actionID);          //4 is the ID for the Actions
        //  }

        //  public virtual void SetAction(int actionID)
        //  {
        //      Mode_Activate(ModeEnum.Action, actionID);          //4 is the ID for the Actions
        //  }

        //  #region Attacks Commands
        //  /// <summary> Tries to Activate the Primary Attack Mode with an Attack ID Animation</summary>
        //  public virtual void SetAttack(int attackID) { Mode_Activate(ModeEnum.Attack1, attackID); }

        //  /// <summary> Tries to Activate the Primary Attack Mode with an Random Attack Animation</summary>
        //  public virtual void SetAttack() { Mode_Activate(ModeEnum.Attack1, -1); }

        //  ///// <summary> Tries to Activate the Primary Attack Forever... until StopAttack Is Called... usefull for AI</summary>
        //  //public virtual void SetAttack_Endless(int attackID) { Mode_Activate_Endless(ModeEnum.Attack1); }

        //  /// <summary> Stop Primary Attack Animations... usefull for AI</summary>
        //  public virtual void StopAttack() { Mode_Stop(); }

        //  /// <summary> Tries to Activate the Secondary Attack Mode with an Attack ID Animation</summary>
        //  public virtual void SetAttack2(int attackID) { Mode_Activate(ModeEnum.Attack2, attackID); }

        //  /// <summary> Try to Activate the Secondary Attack Mode with an Random Attack Animation</summary>
        //  public virtual void SetAttack2() { Mode_Activate(ModeEnum.Attack2, -1); }

        ////  /// <summary> Try to Activate the Secondary Attack Forever... until StopAttack Is Called... usefull for AI</summary>
        ////  public virtual void SetAttack2_Endless(int attackID) { Mode_Activate_Endless(ModeEnum.Attack2); }

        //  /// <summary> Stop Secondary Attack Animations... usefull for AI</summary>
        //  public virtual void StopAttack2() { Mode_Stop(); }
        #endregion


        #region Movement

        /// <summary> Get the Inputs for the Source to add it to the States </summary>
        internal virtual void GetInputs(bool add)
        {
            InputSource = GetComponentInParent<IInputSource>();

            if (InputSource != null)
            {
                //Enable Disable the Inputs for the States
                foreach (var state in states)
                {
                    if (state.Input != string.Empty)
                    {
                        var input = InputSource.GetInput(state.Input);

                        if (input != null)
                        {
                            if (add)
                            {
                                if (input.GetPressed == InputButton.Down || input.GetPressed == InputButton.Up)
                                {
                                    input.OnInputDown.AddListener(() => { state.ActivatebyInput(true); });
                                    input.OnInputUp.AddListener(() => { state.ActivatebyInput(false); });
                                }
                                else
                                {
                                    input.OnInputChanged.AddListener(state.ActivatebyInput);
                                }
                            }
                            else
                            {
                                if (input.GetPressed == InputButton.Down || input.GetPressed == InputButton.Up)
                                {
                                    input.OnInputDown.RemoveAllListeners();
                                    input.OnInputUp.RemoveAllListeners();
                                }
                                else
                                {
                                    input.OnInputChanged.RemoveListener(state.ActivatebyInput);
                                }
                            }
                        }
                    }
                }


                //Enable Disable the Inputs for the States
                foreach (var mode in modes)
                {
                    if (mode.Input != string.Empty)
                    {
                        var input = InputSource.GetInput(mode.Input);

                        if (input != null)
                        {
                            if (add)
                                input.OnInputChanged.AddListener(mode.ActivatebyInput);
                            else
                                input.OnInputChanged.RemoveListener(mode.ActivatebyInput);
                        }
                    }
                }
            }
        }
        /// <summary>Gets the movement from the Input Script or AI</summary>
        public virtual void Move(Vector3 move)
        {
            MoveDirection(move);
        }

        /// <summary>Gets the movement from the Input using a 2 Vector  (ex UI Axis Joystick)</summary>
        public virtual void Move(Vector2 move)
        {
            Vector3 move3 = new Vector3(move.x, 0, move.y);
            MoveDirection(move3);
        }

        /// <summary>Gets the movement from the Input ignoring the Direction Vector, using a 2 Vector  (ex UI Axis Joystick)</summary>
        public virtual void MoveWorld(Vector2 move)
        {
            Vector3 move3 = new Vector3(move.x, 0, move.y);
            MoveWorld(move3);
        }

        /// <summary>Stop the animal from moving, cleaning the Movement Axis</summary>
        public virtual void StopMoving() { Move(Vector3.zero); }

        /// <summary>Add Inertia to the Movement</summary>
        public virtual void AddInertia(ref Vector3 Inertia, float deltatime)
        {
            AdditivePosition += Inertia;
            Inertia = Vector3.Lerp(Inertia, Vector3.zero, deltatime);
        }
        #endregion

        #region Speeds

    

        /// <summary>Change the Speed Up</summary>
        public virtual void SpeedUp() { Speed_Add(+1); }

        /// <summary> Changes the Speed Down </summary>
        public virtual void SpeedDown() { Speed_Add(-1); }

        public virtual void SetCustomSpeed(MSpeed customSpeed, bool keepInertiaSpeed = false)
        {
            CustomSpeed = true;
            CurrentSpeedModifier = customSpeed;

            currentSpeedSet = null; //IMPORTANT SET THE CURRENT SPEED SET TO NULL

            if (keepInertiaSpeed)
                InertiaPositionSpeed = TargetSpeed; //Set the Target speed to the Fall Speed so there's no Lerping when the speed changes
        }

        private void Speed_Add(int change)
        {
            if (CustomSpeed) return;
            if (SpeedChangeLocked) return;
            if (CurrentSpeedSet == null) return;

            var SP = CurrentSpeedSet.Speeds;

            var value = speedIndex + change;

            value = Mathf.Clamp(value, 1, SP.Count);        //Clamp the Speed Index
            var sprintSpeed = Mathf.Clamp(value + 1, 1, SP.Count);

            if (value > CurrentSpeedSet.TopIndex)
            {
               // Debug.Log("REACH TOP");
                return;
            }

            speedIndex = value;
            CurrentSpeedModifier = SP[speedIndex - 1];

            SprintSpeed = SP[sprintSpeed - 1];
            if (CurrentSpeedSet != null) CurrentSpeedSet.CurrentIndex = speedIndex; //Keep the Speed saved on the state too in case the active speed was changed
        }

        /// <summary> Set an specific Speed for a State </summary>
        public virtual void Speed_Set(int speedIndex) { CurrentSpeedIndex = speedIndex; }

        /// <summary>Lock Speed Changes on the Animal</summary>
        public virtual void Speed_Change_Lock(bool lockSpeed)
        {
            SpeedChangeLocked = lockSpeed;
        }

        public virtual void SpeedSet_Set_Active(string SpeedSetName, int activeIndex)
        {
            var speed = speedSets.Find(x => x.name == SpeedSetName);

            if (speed != null)
            {
                speed.CurrentIndex = activeIndex;

                if (CurrentSpeedSet == speed)
                    CurrentSpeedIndex = activeIndex;
            }
        }

        public virtual void Speed_Update_Current()
        {
            CurrentSpeedIndex = CurrentSpeedIndex;
        }

        public virtual void SpeedSet_Set_Active(string SpeedSetName, string activeSpeed)
        {
            var speed = speedSets.Find(x => x.name == SpeedSetName);

            if (speed != null)
            {
               var mspeedIndex = speed.Speeds.FindIndex(x => x.name == activeSpeed);

                if (mspeedIndex != -1)
                {
                    speed.CurrentIndex = mspeedIndex + 1;

                    if (CurrentSpeedSet == speed) 
                        CurrentSpeedIndex = mspeedIndex + 1;
                }

            }
            else
            {
                Debug.LogWarning("There's no Speed Set called : " + SpeedSetName);
            }
        }


        /// <summary> if True Disable changing the speeds  </summary>
        public bool SpeedChangeLocked { get; private set; }
        /// <summary> Set an specific Speed for a State using IntVars </summary>
        public virtual void Speed_Set(IntVar speedIndex) { CurrentSpeedIndex = speedIndex; }
        #endregion

        #region Extras

        /// <summary>Method for the IDamagable Interface</summary>
        public virtual void Damage(int IDDamage, int Index = -1)
        {
            Mode_Activate(IDDamage, Index);
        }

        /// <summary>Activate Attack triggers  </summary>
        internal virtual void AttackTrigger(int triggerIndex)
        {
            if (triggerIndex == -1)                         //Enable all Attack Triggers
            {
                foreach (var trigger in Attack_Triggers)
                {
                    trigger.enabled = true;
                }
                return;
            }

            if (triggerIndex == 0)                          //Disable all Attack Triggers
            {
                foreach (var trigger in Attack_Triggers)
                {
                   if (trigger) trigger.enabled = false;
                }

                return;
            }


            List<MAttackTrigger> Att_T =
                Attack_Triggers.FindAll(item => item.index == triggerIndex);        //Enable just a trigger with an index

            if (Att_T != null)
            {
                foreach (var trigger in Att_T)
                {
                   if (trigger) trigger.enabled = true;
                }
            }
        }


        internal void GetAnimalColliders()
        {
           var colls = GetComponentsInChildren<Collider>(true).ToList();      //Get all the Active colliders

           colliders = new List<Collider>();

            foreach (var item in colls)
            {
                if (!item.isTrigger && item.gameObject.layer == gameObject.layer) colliders.Add(item);        //Add the Animal Colliders Only
            }
        }

        /// <summary>Enable/Disable All Colliders on the animal. Avoid the Triggers </summary>
        public virtual void EnableColliders(bool active)
        {
            foreach (var item in colliders) item.enabled = active;
        }

        /// <summary>Check if there's a State that cannot be enabled when playing a mode </summary>
        internal virtual void CheckStateToModeSleep(bool playingMode)
        {
            foreach (var state in states)
            {
                state.IsSleepFromMode = playingMode && state.SleepFromMode.Contains(ActiveMode.ID);
            }
        }

        /// <summary>Disable this Script and MalbersInput Script if it has it.</summary>
        public virtual void DisableAnimal()
        {
            enabled = false;
            MalbersInput MI = GetComponent<MalbersInput>();
            if (MI) MI.enabled = false;
        }
        #endregion

    }
}