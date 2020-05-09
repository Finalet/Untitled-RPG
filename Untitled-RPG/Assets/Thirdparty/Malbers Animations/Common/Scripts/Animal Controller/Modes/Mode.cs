using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.Controller
{
    [System.Serializable] //DO NOT REMOVE!!!!!!!!!
    public class Mode
    {
        #region Public Variables
        /// <summary>Is this Mode Active?</summary>
        public bool active = true;

        [SerializeField] private bool ignoreLowerModes = false;

        /// <summary>Animation Tag Hash of the Mode</summary>
        public string AnimationTag;
        /// <summary>Animation Tag Hash of the Mode</summary>
        protected int ModeTagHash;
        /// <summary>Which Input Enables the Ability </summary>
        public string Input;
        /// <summary>ID of the Mode </summary>
        [SerializeField] public ModeID ID;
        /// <summary>Modifier that can be used when the Mode is Enabled/Disabled or Interrupted</summary>
        public ModeModifier modifier;

        /// <summary>Elapsed time to be interrupted by another Mode If 0 then the Mode cannot be interrupted by any other mode </summary>
        public FloatReference CoolDown = new FloatReference(0);

        ///// <summary>Last Time the Mode was activated</summary>
        //protected float ModeActivatedTime;

        /// <summary>Global Properties for the Mode </summary>
        public ModeProperties GlobalProperties;
        /// <summary>List of Abilities </summary>
        public List<Ability> Abilities;
        /// <summary>Active Ability index</summary>
        [SerializeField] private IntReference abilityIndex = new IntReference(-1);
        public IntReference DefaultIndex = new IntReference(-1);
        public IntEvent OnAbilityIndex = new IntEvent();
        public bool ResetToDefault = false;

        [SerializeField] private bool allowRotation = false;
        [SerializeField] private bool allowMovement = false;
        #endregion

        #region Properties

        /// <summary>Is this Mode Playing?</summary>
        public bool PlayingMode { get; set; }
        /// <summary>Priority of the Mode.  Higher value more priority</summary>
        public int Priority { get; internal set; }

        /// <summary>Allows Additive rotation while the mode is playing </summary>
        public bool AllowRotation { get => allowRotation; set => allowRotation = value; }

        /// <summary>Allows Additive Speeds while the mode is playing </summary>
        public bool AllowMovement { get => allowMovement; set => allowMovement = value; }
       
        public string Name => ID != null ? ID.name : string.Empty;

        /// <summary>Does this Mode uses Cool Down? False if Cooldown = 0</summary>
        public bool HasCoolDown => CoolDown != 0;

        /// <summary>Is this mode in CoolDown?</summary>
        public bool InCoolDown { get; internal set; }
     
        /// <summary>If enabled, it will play this Mode even if a Lower Mode is Playing </summary>
        public bool IgnoreLowerModes { get => ignoreLowerModes; set => ignoreLowerModes = value; }

        protected bool AffectStates_Empty
        {
            get { return (GlobalProperties.affectStates == null || GlobalProperties.affectStates.Count == 0); }
        }

        /// <summary> Active Ability Index of the mode</summary>
        public int AbilityIndex
        {
            get { return abilityIndex; }
            set
            {
                abilityIndex.Value = value;
                OnAbilityIndex.Invoke(value);
            }
        }

        public void SetAbilityIndex(int index)
        { AbilityIndex = index; }

        /// <summary>Check if the Active Ability has Override Properties Enabled</summary>
        public bool AbilityOverride => (ActiveAbility != null && ActiveAbility.OverrideProp);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "<Pending>")]
        public MAnimal Animal { get; private set; }

        /// <summary> Current Selected Ability to Play on the Mode</summary>
        public Ability ActiveAbility { get; private set; }

        /// <summary>Current Value of the Input if this mode was called  by an Input</summary>
        public bool InputValue { get; internal set; }


        #endregion

        #region EditorVars
        public bool showGeneral = true;
        public bool showIndex = true;
        public bool showProperties = true;

        #endregion

        /// <summary>Set everyting up when the Animal Script Start</summary>
        public virtual void AwakeMode(MAnimal animal)
        {
            Animal = animal;                                   //Cache the Animal
          //  ModeTagHash = Animator.StringToHash(AnimationTag);      //Convert the Tag on a TagHash
            OnAbilityIndex.Invoke(AbilityIndex);
          //  ModeActivatedTime = -CoolDown;                          //First Time IMPORTANT
            InCoolDown = false;
           // Debug.Log(Priority);
        }

        /// <summary>Exit the current mode an ability</summary> 
        public virtual void ExitMode()
        {
            if (Animal.ActiveMode == this) //if is the same Mode then set the AnimalPlaying mode to false
            {
                Animal.CheckStateToModeSleep(false);
              
                if (Animal.LastModeStatus != MStatus.Interrupted)
                    Animal.LastModeStatus =  MStatus.Completed;
            }

            PlayingMode = false;

            if (modifier != null) modifier.OnModeExit(this);

            OnExitInvoke();

            if (ResetToDefault) ResetAbilityIndex();        //Reset to the default

            ActiveAbility = null;                            //Reset to the default

            InputValue = false;                             //IMPORTANT meaning it can be enable back again

            // if (Animal.debugModes) Debug.Log("Exit Mode: <B>" + ID.name + "</B>");
        }

        /// <summary>Resets the Ability Index on the  animal to the default value</summary>
        public virtual void ResetAbilityIndex()
        {
            if (!Animal.IsOnZone) AbilityIndex = DefaultIndex; //Dont reset it if you are on a zone... the Zone will do it automatically if you exit it
        }

        public void ActivatebyInput(bool Input_Value)
        {
            if (!active) return;
            if (!Animal.enabled) return;
            if (Animal.LockInput) return;               //Do no Activate if is sleep or disable or lock input is Enable;

            if (InputValue != Input_Value)              //Only Change if the Inputs are Different
            {
                InputValue = Input_Value;

                if (Animal.InputMode == null && InputValue)
                {
                    Animal.InputMode = this;
                }
                else if (Animal.InputMode == this && !InputValue)
                {
                    Animal.InputMode = null;
                }


                if (Animal.debugModes) Debug.Log(" Mode: " + ID.name + " Input: " + Input_Value);


                if (InputValue)
                {
                    if (Animal.debugModes) Debug.Log("Try Activate Mode: " + ID.name + " by INPUT");

                    if (CheckStatus(AbilityStatus.Toggle)) { Animal.InputMode = null; }

                    TryActivate();
                }
                else
                {
                    if (PlayingMode && CheckStatus(AbilityStatus.HoldByInput) && !InputValue) //if this mode is playing && is set to Hold by Input & the Input was true
                    {
                       // Debug.Log("PlayingMode: "+ PlayingMode+ ", HoldByInput: "+ CheckStatus(AbilityStatus.HoldByInput) + "; InputValue:" + InputValue);

                        Animal.Mode_Interrupt();
                        if (Animal.debugModes) Debug.Log("Exit Mode: " + ID.name + " by INPUT Up");
                    }
                }
            }
        }


        /// <summary>Activates an Ability from this mode using the AbilityIndex</summary>
        public virtual bool TryActivate()
        {
            if (!active) return false;                //If the mode is Disabled Ingnore
            if (AbilityIndex == 0) return false;      //Means that no Ability is Active

            if (Abilities == null || Abilities.Count == 0)
            {
                if (Animal.debugModes) Debug.LogWarning("There's no Abilities on <b>" + Name + " Mode</b>, Please set a list of Abilities");
                return false;
            }

            //FINALLY IF EVERYTHING IS WORKING THEN ACTIVATE THE MODE
            if (modifier != null) modifier.OnModeEnter(this); //Check first if there's a modifier on Enter

            int NewIndex = (AbilityIndex == -1) ? Abilities[Random.Range(0, Abilities.Count)].Index.Value : AbilityIndex; //Set the Index of the Ability for the Mode

            var newAbility = Abilities.Find(item => item.Index == NewIndex);

            if (newAbility == null)
            {
                if (Animal.debugModes) Debug.Log("There's no Ability with the Index: <B>" + AbilityIndex + "</B> on the Mode <B> " + Name + "</B>");
                return false;
            }

            if (StateBlockMode(newAbility)) return false; //Check if the States can block the mode
          

            if (PlayingMode && CheckStatus(AbilityStatus.Toggle)) //if is set to Toggle then if is already playing this mode then stop it
            {
                InputValue = false;                     //Reset the Input Value to false of this mode

                if (Animal.InputMode == this)
                    Animal.InputMode = null;

                if (Animal.debugModes) Debug.Log("Mode <b>" + ID.name + "</b> Toggle Off");
                Animal.Mode_Interrupt();
                return false;
            }

            var ActiveMode = Animal.ActiveMode;


            if (Animal.IsPlayingMode)              // Means the there's a Mode Playing
            {
                if (this.Priority > ActiveMode.Priority && IgnoreLowerModes && !InCoolDown)
                {
                    ActiveMode.ExitMode();
                    Animal.Mode_Stop(); //This allows to Play a mode again
                }
                else
                {
                    if (!ActiveMode.HasCoolDown) return false; //Means that the Animations needs to finish first if the Active Mode Has no Cool Down so skip the code


                    if (!ActiveMode.InCoolDown)   //Means that the Active mode can be Interrupted since is no longer on 
                    {
                        ActiveMode.ExitMode();
                        Animal.Mode_Stop(); //This allows to Play a mode again INT ID  = 0 to it can ve available again
                    }
                    return false;
                }
            }

            if (InCoolDown) return false;                     //Exit the Mode After the cooldown has Passed no matter if is NOT PLAYING A MODE *GOOOD CODE

            Activate(newAbility);

            return true;
        }


        /// <summary>Randomly Activates an Ability from this mode</summary>
        private void Activate(Ability newAbility)
        {
            ActiveAbility = newAbility;
             
            if (Animal.debugModes) Debug.Log("Mode: <B>" + Name + "</B> with Ability: <B>" + ActiveAbility.Name + "</B> Activated");
           
            Animal.SetModeParameters(this);
        }

        /// <summary>  The Current State can interrupt or not allow the Ability or Mode to play</summary>
        private bool StateBlockMode(Ability checkAbility)
        {
            var affect = GlobalProperties.affect;
            var ID = Animal.ActiveState.ID;

            if (checkAbility.OverrideProp)
            {
                var ModOverride = checkAbility.OverrideProperties;

                affect = ModOverride.affect;

                if (affect != AffectStates.None && !AffectStates_Empty && !checkAbility.AffectStates_Empty)
                {
                    if (affect == AffectStates.Exclude && ContainStateOverride(ModOverride, ID)      //If the new state is on the Exclude State
                || (affect == AffectStates.Inlcude && !ContainStateOverride(ModOverride, ID)))    //OR If the new state is not on the Include State
                    {
                        if (Animal.debugModes) Debug.Log("Override Properties States: Current State is Blocking <B>" + checkAbility.Name + "</B>");
                        return true;
                    }
                }
            }
            else
            {
                if (affect != AffectStates.None && !AffectStates_Empty)
                {
                    if (affect == AffectStates.Exclude && ContainState(ID)   //If the new state is on the Exclude State
                || (affect == AffectStates.Inlcude && !ContainState(ID)))    //OR If the new state is not on the Include State
                    {
                        if (Animal.debugModes) Debug.Log("Current State is Blocking <B>" + checkAbility.Name + "</B>");
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Called by the Mode Behaviour on Entering the Animation State 
        /// Done this way to check for Modes that are on other Layers besides the Base Layer </summary>
        public void AnimationTagEnter(int animatorTagHash)
        {
           // if (animatorTagHash != ModeTagHash) return; // if we are not on this Tag then Skip the code

            if (ActiveAbility != null)
            {
                Animal.ActiveMode = this;
                PlayingMode = true;
                
                Animal.CheckStateToModeSleep(true);                      //Put to sleep the states needed
                Animal.LastModeStatus = MStatus.Playing;
                OnEnterInvoke();                                        //Invoke the ON ENTER Event

                AbilityStatus AMode = ActiveAbility.OverrideProp ? ActiveAbility.OverrideProperties.Status : GlobalProperties.Status; //Check if the Current Ability overrides the global properties

                int IntID = Int_ID.Loop;    //That means the Ability is Loopable

                if (AMode == AbilityStatus.PlayOneTime)
                {
                    IntID = Int_ID.OneTime;                //That means the Ability is OneTime 
                    if (Animal.debugModes) Debug.Log("Animation <b>Enter</b>. Mode: <b>" + Name + "</b>. Ability: <b>" + ActiveAbility.Name + "</b> PlayOneTime");
                }
                else if (AMode == AbilityStatus.HoldByTime)
                {
                    float HoldByTime = ActiveAbility.OverrideProp ? ActiveAbility.OverrideProperties.HoldByTime : GlobalProperties.HoldByTime;
                    Animal.StartCoroutine(Ability_By_Time(HoldByTime));
                    if (Animal.debugModes) Debug.Log("Animation <b>Enter</b>. Mode: <b>" + Name + "</b>. Ability: <b>" + ActiveAbility.Name + "</b> HoldByTime");
                }

                if (HasCoolDown)
                    Animal.StartCoroutine(C_SetCoolDown(CoolDown));

                Animal.SetIntID(IntID);
            }
        }

        /// <summary>Called by the Mode Behaviour on Exiting the  Animation State 
        /// Done this way to check for Modes that are on other Layers besides the base one </summary>
        public void AnimationTagExit()
        {
            if (Animal.ActiveMode == this)               //Means that we just exit This Mode mode 
            {
                if (Animal.debugModes) Debug.Log("Animation <b>Exit</b>. Mode: <b>" + Name + "</b>. Ability: <b>" + ActiveAbility.Name + "</b>");

                ExitMode();
                Animal.Mode_Stop();
            }
        }

        public virtual void OnModeStateMove(AnimatorStateInfo stateInfo, Animator anim, int Layer)
        {
            if (modifier != null) modifier.OnModeMove(this, stateInfo, anim, Layer);
        }

        /// <summary> Check for Exiting the Mode, If the animal changed to a new state and the Affect list has some State</summary>
        public virtual void StateChanged(StateID ID)
        {
            var affect = AbilityOverride ? ActiveAbility.OverrideProperties.affect : GlobalProperties.affect;
            if (affect == AffectStates.None) return;


            if (!AffectStates_Empty)
            {
                if (affect == AffectStates.Exclude && ContainState(ID)      //If the new state is on the Exclude State
                || (affect == AffectStates.Inlcude && !ContainState(ID)))   //OR If the new state is not on the Include State
                {
                    Animal.Mode_Interrupt();
                }
            }
        }


        /// <summary>Find if a State ID is on the Avoid/Inlcude global list</summary>
        protected bool ContainState(StateID ID)
        {
            return GlobalProperties.affectStates.Contains(ID);
        }

        /// <summary>Find if a State ID is on the Avoid/Include Override list</summary>
        protected bool ContainStateOverride(ModeProperties OverrideProperties, StateID ID)
        {
            return OverrideProperties.affectStates.Contains(ID);
        }


        public IEnumerator C_SetCoolDown(float time)
        {
            InCoolDown = true;
            yield return new WaitForSeconds(time);
            InCoolDown = false;
        }




        protected IEnumerator Ability_By_Time(float time)
        {
            if (Animal.debugModes) Debug.Log("ActiveByTime: " + time);
            yield return new WaitForSeconds(time);
            Animal.Mode_Interrupt();
        }


        //public bool CoolDownFinished(Mode mode)
        //{
        //    return   (Time.time - mode.ModeActivatedTime) > mode.CoolDown;
        //}

        //public bool CoolDownFinished()
        //{
        //    return CoolDownFinished(this);
        //}

        private void OnExitInvoke()
        {
            if (AbilityOverride) ActiveAbility.OverrideProperties.OnExit.Invoke();
            GlobalProperties.OnExit.Invoke();
        }

        private void OnEnterInvoke()
        {
            if (AbilityOverride) ActiveAbility.OverrideProperties.OnEnter.Invoke();

            GlobalProperties.OnEnter.Invoke();
        }


        private bool CheckStatus(AbilityStatus status)
        {
            if (AbilityOverride)
            {
                return ActiveAbility.OverrideProperties.Status == status;
            }
            return GlobalProperties.Status == status;
        }

        /// <summary>Disable the Mode. If the mode is playing it check the status and it disable it properly </summary>
        public virtual void Disable()
        {
            active = false;
            InputValue = false;

            if (PlayingMode)
            {
                Animal.InputMode = null;
                if (!CheckStatus(AbilityStatus.PlayOneTime))
                { Animal.Mode_Interrupt(); }
                else
                {
                    //Do nothing ... let the mode finish since is on AbilityStatus.PlayOneTime
                }
            }
        }
    }
    /// <summary> Ability for the Modes</summary>
    [System.Serializable]
    public class Ability
    {
        ///// <summary>Is the Ability Active</summary>
        //public bool Active = true;
        /// <summary>Name of the Ability (Visual Only)</summary>
        public string Name;
        /// <summary>index of the Ability </summary>
        public IntReference Index;
        /// <summary>if true Overrides the Global properties on the Mode </summary>
        public bool OverrideProp;
        /// <summary>Overrides Properties on the mode</summary>
        public ModeProperties OverrideProperties;

        public bool AffectStates_Empty => OverrideProperties.affectStates != null || OverrideProperties.affectStates.Count > 0;

        ///// <summary>Enable Disable the Ability</summary>
        //public BoolReference active = new BoolReference(true);
    }

    public enum AbilityStatus
    {
        /// <summary> The Ability is Enabled One time and Exit when the Animation is finished </summary>
        PlayOneTime = 0,
        /// <summary> The Ability is On while the Input is pressed</summary>
        HoldByInput = 1,
        /// <summary> The Ability is On for an x ammount of time</summary>
        HoldByTime = 2,
        /// <summary> The Ability is ON and OFF everytime the Activate method is called</summary>
        Toggle = 3,
    }
    public enum AffectStates
    {
        Inlcude,
        Exclude,
        None,
    }

    [System.Serializable]
    public class ModeProperties
    {
        /// <summary>The Ability can Stay Active until it finish the Animation, by Holding the Input Down, by x time </summary>
        [Tooltip("The Ability can Stay Active until it finish the Animation, by Holding the Input Down, by x time ")]
        public AbilityStatus Status = AbilityStatus.PlayOneTime;
        
        /// <summary>The Ability can Stay Active by x seconds </summary>
        [Tooltip("The Ability can Stay Active by x seconds")]
        public FloatReference HoldByTime;
   
        ///// <summary>If Exlude then the Mode will not be Enabled when is on a State on the List, If Include, then the mode will only be active when the Animal is on a state on the List </summary>
        [Tooltip("If Exlude then the Mode will not be Enabled when is on a State on the List, If Include, then the mode will only be active when the Animal is on a state on the List")]
        public AffectStates affect = AffectStates.None;
        /// <summary>Include/Exclude the  States on this list depending the Affect variable</summary>
        [Tooltip("Include/Exclude the  States on this list depending the Affect variable")]
        public List<StateID> affectStates;

        /// <summary> The Abilty can be interrupted by another Ability after x time... if InterruptTime = 0 then it cannot be interrupted</summary>
        [Tooltip(" The Abilty can be interrupted by another Ability after x time... if InterruptTime = 0 then it cannot be interrupted")]
        public float InterruptTime = 0f;

        [SerializeField] private bool ShowEvents;
        public UnityEvent OnEnter;
        public UnityEvent OnExit;
    }
}