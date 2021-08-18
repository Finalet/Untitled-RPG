using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{
    public abstract class State : ScriptableObject
    {
        /// <summary>  Name that will be represented on the creation State List</summary>
        public abstract string StateName { get; }

        /// <summary>You can enable/disable temporarly  the State</summary>
        [HideInInspector] public bool Active = true;

        /// <summary>Reference for the Animal that Holds this State</summary>
        protected MAnimal animal; 

        [Tooltip("Input to Activate the State, leave empty for automatic states")]
        /// <summary>Input to Activate the State</summary>
        public string Input;

        [Tooltip("Priority of the State. Higher value -> more priority to be activated")]
        /// <summary>Priority of the State.  Higher value more priority</summary>
        public int Priority;

        [Tooltip("Main/Core Modifier. When the Animal enters the Core Tag, it will change the core parameters of the Animal")]
        public AnimalModifier General;


       // [Space]
        [Tooltip("If the Active State is one of one on the List, This State cannot be enable")]
        public List<StateID> SleepFromState = new List<StateID>();
        [Tooltip(" If A mode is Enabled and is one of one on the List ...This State cannot be enable")]
        public List<ModeID> SleepFromMode = new List<ModeID>();


        [Tooltip("If The state is trying to be active but the active State is on this list, the State will be queued until the Active State changes for a different one from this list")]
        public List<StateID> QueueFrom = new List<StateID>();
        public List<TagModifier> TagModifiers = new List<TagModifier>();

        //[Space]
        [Tooltip("Try States will try to activate every X frames")]
        public IntReference TryLoop = new IntReference(1);

        //[Space]
        [Tooltip("Tag to Identify Entering Animations on a State.\nE.g. (TakeOff) in Fly, EnterWater on Swim")]
        public StringReference EnterTag = new StringReference();
        [Tooltip("Tag to Identify Exiting Animations on a State.\nE.g. (Land) in Fall, or SwimClimb in Swim")]
        public StringReference ExitTag = new StringReference();
        [Tooltip("if True, the state will execute another frame of logic while entering the other state ")]
        public bool ExitFrame = true;
        [Tooltip("Try Exit State on Main State Animation. E.g. The Fall Animation can try to exit only when is on the Fall Animation")]
        public bool ExitOnMain = true;
        [Tooltip("Amount of time need to activate this state again after being exited")]
        public FloatReference ExitCooldown = new FloatReference(0);

        //[Space]
        [Tooltip("Can straffing be used with this State?")]
        public bool CanStrafe;
        [Tooltip("Strafe Multiplier when movement is detected. This will make the Character be aligned to the Strafe Direction Quickly")]
        [Range(0,1)]
        public float MovementStrafe = 1f;
        [Tooltip("Strafe Multiplier when there's no movement. This will make the Character be aligned to the Strafe Direction Quickly")]
        [Range(0,1)]
        public float IdleStrafe = 1f;

       // [Space]
        public bool debug = true;

        [HideInInspector] public int Editor_Tabs1;


        #region Properties
        protected QueryTriggerInteraction IgnoreTrigger => QueryTriggerInteraction.Ignore;

        /// <summary>Unique ID used on performance</summary>
        public int UniqueID { get; private set; }
        /// <summary>Reference for the Animal Transform</summary>
        protected Transform transform;

        /// <summary>Reference for the Animal Animator</summary>
        protected Animator Anim => animal.Anim;

        /// <summary> Store the OnEnterOnExit Event</summary>
        protected OnEnterExitState EnterExitEvent;

        /// <summary>Check all the Rules to see if the state can b e activated</summary>
        public bool CanBeActivated
        {
            get
            {
                // if (animal.m_IsAnimatorTransitioning) return false; //Cannot be activated while is on transition (NO, MODES ARE BEING PLAYED)
                if (CurrentActiveState == null) return false;                                                       //Means there's no active State 
                if (!Active || IsSleep) return false;                                                               //if the New state is disabled or is sleep or the Input is Locked: Ignore Activation
                if (CurrentActiveState.Priority > Priority && CurrentActiveState.IgnoreLowerStates) return false;   //if the Active state is set to ignore the lower States: Ignore the Activation
                if (CurrentActiveState.IsPersistent) return false;                                                  //if the Active state is persitent: Ignore the Activation
                if (IsActiveState) return false;                                                                    //We are already on this state: Ignore the Activation
                if (OnExitCoolDown) return false;           //Check Exit Cooldown

                return true;
            }
        }

        /// <summary>  Has completed the Exit Cooldown so it can be activated again  </summary>
        public bool OnExitCoolDown => ExitCooldown > 0 && !MTools.ElapsedTime(CurrentExitTime, ExitCooldown + 0.01f);

        /// <summary>Main Tag of the Animation State which it will rule the State the ID name Converted to Hash</summary>
        public int MainTagHash { get; private set; }


        /// <summary> Hash of the Exit Tag Animation</summary>
        protected int ExitTagHash { get; private set; }

        /// <summary> Hash of the Tag of an Enter Animation</summary>
        protected int EnterTagHash { get; private set; }

        /// <summary>The State is on an Exit Animation</summary>
        public bool InExitAnimation => ExitTagHash != 0 && ExitTagHash == CurrentAnimTag;

        /// <summary>The State is on an Enter Animation</summary>
        public bool InEnterAnimation => EnterTagHash != 0 && EnterTagHash == CurrentAnimTag;

        protected float CurrentExitTime;
        
        /// <summary>Returns the Active Animation State tag Hash on the Base Layer</summary>
        protected int CurrentAnimTag => animal.AnimStateTag;

        /// <summary>Animal Current Active State</summary>
        protected State CurrentActiveState => animal.ActiveState;

        /// <summary>Can the State use the TryExitMethod</summary>
        public bool CanExit { get; internal set; }

        /// <summary>True if this state is the Animal Active State</summary>
        public bool IsActiveState { get; internal set; }


        /// <summary>Input Value for a State (Some states can by activated by inputs</summary>
        public virtual bool InputValue { get; set; }

        /// <summary>Put a state to sleep it works with the Avoid States list</summary>
        public virtual bool IsSleepFromState { get; internal set; }

        /// <summary>Put a state to sleep When Certaing Mode is Enable</summary>
        public virtual bool IsSleepFromMode { get; internal set; }

        /// <summary>The State is Sleep</summary>
        public virtual bool IsSleep => IsSleepFromMode || IsSleepFromState;

        /// <summary>is this state on queue?</summary>
        public virtual bool OnQueue { get; internal set; }

        /// <summary>The State wants to be activated but is on QUEUE!</summary>
        public bool OnActiveQueue { get; internal set; }

        /// <summary>The State is on the Main State Animation</summary>
        public bool InCoreAnimation => CurrentAnimTag == MainTagHash;

        /// <summary>Quick Access to Animal.currentSpeedModifier.position</summary>
        public float CurrentSpeedPos
        {
            get => animal.CurrentSpeedModifier.position;
            set => animal.currentSpeedModifier.position = value;
        }

        public MSpeed CurrentSpeed =>  animal.CurrentSpeedModifier;
     

        /// <summary>If True this state cannot be interrupted by other States</summary>
        public bool IsPersistent { get; set; }
        /// <summary>If true the states below it will not try to Activate themselves</summary>
        public bool IgnoreLowerStates { get; set; }
        //{
        //    get => ignoreLowerStates;
        //    set
        //    {
        //        ignoreLowerStates = value;
        //        Debug.Log($"ignoreLowerStates: {ignoreLowerStates} ");
        //    }
        //}
        //bool ignoreLowerStates;


        /// <summary>Means that is already active but is Still exiting the Last State and it does not have entered any of the Active State Animations</summary>
        public bool IsPending { get; set; }
        public bool PendingExit { get; set; }


        /// <summary>Speed Set this State has... if Null the state will not change speeds</summary>
        public List<MSpeedSet> SpeedSet { get; internal set; }
        #endregion




        [Tooltip("ID to Identify the State. The name of the ID is the Core Tag used on the Animator")]
        /// <summary>ID Asset Reference</summary>
        public StateID ID;


        #region Methods
        /// <summary> Return if this state have a current Tag used on the animal</summary>
        protected bool StateAnimationTags(int MainTag)
        {
            if (MainTagHash == MainTag) return true;

            var Foundit = TagModifiers.Find(tag => tag.TagHash == MainTag);

            return Foundit != null;
        }

        /// <summary>Set all the values for all the States on Awake</summary>
        public void AwakeState(MAnimal mAnimal)
        {
            animal = mAnimal;
            transform = animal.transform;

            AwakeState();
        }

        /// <summary>Called on Awake</summary>
        public virtual void AwakeState()
        {
            MainTagHash = Animator.StringToHash(ID.name);                       //Store the Main Tag at Awake
            ExitTagHash = Animator.StringToHash(ExitTag.Value);                       //Store the Main Tag at Awake
            EnterTagHash = Animator.StringToHash(EnterTag.Value);                       //Store the Main Tag at Awake

            foreach (var mod in TagModifiers)
                mod.TagHash = Animator.StringToHash(mod.AnimationTag);          //Convert all the Tags to HashTags

            SpeedSet = new List<MSpeedSet>();

            foreach (var set in animal.speedSets) //Find if this state has a Speed Set
                if (set.states.Contains(ID)) SpeedSet.Add(set);

            SpeedSet.Sort(); //IMPORTANT!

            EnterExitEvent = animal.OnEnterExitStates.Find(st => st.ID == ID);

            InputValue = false;
            ResetState();
            ResetStateValues();

            CurrentExitTime = -ExitCooldown;

            //DirectionalVelocity = transform.forward; //As default the Directional is the Transform.forward

            if (TryLoop < 1) TryLoop = 1;

            UniqueID = UnityEngine.Random.Range(0, 99999);
        }

        /// <summary>Current Direction Speed Applied to the Additional Speed, by default is the Animal Forward Direction</summary>
        public virtual Vector3 Speed_Direction() => animal.Forward * Mathf.Abs(animal.VerticalSmooth);


        /// <summary>Check if the Active State is Queued</summary>
        public bool QUEUED()
        {
            if (OnQueue)
            {
                OnActiveQueue = true;
                Debugging("Queued");
                return true;
            }
            return false;
        }

        /// <summary>Activate the State. Code is Applied on base.Activate()</summary>
        public virtual void Activate()
        {
            if (QUEUED()) {    return; }
            if (animal.JustActivateState) { return; }

            animal.LastState = animal.ActiveState;                          //Set a new Last State
            LastStateExit();
            
            //Debug.Log("this = [" + this.name + "] CurRENT AC <<"+ animal.ActiveState.name);
            animal.ActiveState = this;                  //Update to the Current State
            Debugging("Activated");
            SetSpeed(); //Set the Speed on the New State

            IsActiveState = true;                       //Set this state as the Active State
            CanExit = false;



            if (animal.LastState != animal.ActiveState)
            {
                IsPending = true; //We need to set is as pending since we have not enter this states animations yet IMPORTANT IF we are not activating outselves
                PendingExit = true;
            }
            EnterExitEvent?.OnEnter.Invoke();
        }

        public virtual void ForceActivate()
        {
            Debugging("Force Activated");
           // animal.State_SetFloat(0);
            animal.LastState = animal.ActiveState;                          //Set a new Last State
            LastStateExit();
            animal.ActiveState = this;                  //Update to the Current State
            SetSpeed();                                 //Set the Speed on the New State

            IsActiveState = true;                       //Set this state as the Active State
            CanExit = false;

            if (animal.LastState != animal.ActiveState)
            {
                IsPending = true; //We need to set is as pending since we have not enter this states animations yet IMPORTANT IF we are not activating outselves
                PendingExit = true;
            }
            EnterExitEvent?.OnEnter.Invoke();
        }

        /// <summary>Used to set the speeds </summary>
        internal virtual void SetSpeed()
        {
            var SpeedSet = this.SpeedSet.Find(set => set.HasStance(animal.Stance));

            if (SpeedSet != null && SpeedSet != animal.CurrentSpeedSet)
            {
                animal.CustomSpeed = false;
                animal.CurrentSpeedSet = SpeedSet;    //Set a new Speed Set
                animal.CurrentSpeedIndex = SpeedSet.CurrentIndex;
            }
        }

        /// <summary> Reset a State values to its first Awakening </summary>
        public virtual void ResetState()
        {
            IgnoreLowerStates = false;
            IsPersistent = false;
            IsPending = false;
            OnQueue = false;
            IsActiveState = false;
            CanExit = false;
            IsSleepFromMode = false;
            IsSleepFromState = false;
            OnActiveQueue = false; 
        }

        /// <summary>Restore some of the Animal Parameters when the State exits</summary>
        public virtual void RestoreAnimalOnExit()  {} 

        public virtual void ExitState()
        {
            ResetStateValues();
            ResetState();
            RestoreAnimalOnExit();
            //  Debugging("Exit State");
        }

        /// <summary>Invoke the Exit State for the Laset State and Execute the Exit State method</summary>
        internal void LastStateExit()
        {
            animal.LastState.EnterExitEvent?.OnExit.Invoke();
            animal.LastState.CurrentExitTime = Time.time;
            animal.LastState.CanExit = false;
            animal.LastState.ExitState();
        }


        /// <summary>Status Value of the State</summary>
        public void SetStatus(int value) => animal.State_SetStatus(value);
        public void SetFloat(float value) => animal.State_SetFloat(value);
        public void SetFloatSmooth(float value, float time)
        {
            if (animal.State_Float != 0f)
                animal.State_SetFloat(Mathf.MoveTowards(animal.State_Float, 0, time));
        }


        /// <summary>Exit Status Value of the State</summary>
        public void SetExitStatus(int value) => animal.State_SetExitStatus(value);

        public virtual void ActivateQueued()
        {
            OnQueue = false;
            OnActiveQueue = false;
            Debugging("[No Longer on Queue]");
            Activate();
        }

        /// <summary>When a Tag Changes apply this modifications</summary>
        public void AnimationTagEnter(int animatorTagHash)
        {
            if (MainTagHash == animatorTagHash && IsActiveState) //Do this for the Active State ... This is also Called in the Last state.. so this need to be ignored
            {
                General.Modify(animal);
                IsPending = false;
                SetExitStatus(0); //Reset the exit status.
                CheckPendingExit();

                EnterCoreAnimation();
                animal.SprintUpdate();
            }
            else
            {
                TagModifier ActiveTag = TagModifiers.Find(tag => tag.TagHash == animatorTagHash);

                if (ActiveTag != null)
                {
                    IsPending = false;
                    ActiveTag.modifier.Modify(animal);
                    CheckPendingExit();
                    EnterTagAnimation();
                    animal.SprintUpdate();
                }
            }
        }


        private void CheckPendingExit()
        {
            if (!IsPending && PendingExit)
            {
                animal.LastState?.PendingAnimationState();
                PendingExit = false;
            }
        }

        public void SetInput(bool value) => InputValue = value;

        /// <summary>Receive messages from the Animator Controller</summary>
        public void ReceiveMessages(string message, object value) => this.Invoke(message, value);


        /// <summary>Enable the State using an Input. Example :Fly, Jump </summary>
        internal void ActivatebyInput(bool InputValue)
        { 
            this.InputValue = InputValue;
            if (animal.LockInput) return;           //All Inputs are locked so Ignore Activation by input
            if (CanBeActivated)  StatebyInput();
        }


        //internal void OnMoveState(float deltatime)
        //{
        //    OnStateMove(deltatime);
        //}

        internal void CheckCanExit()
        {
            if (!CanExit && !IsPending && !animal.m_IsAnimatorTransitioning)
            {
                if (ExitOnMain)
                {
                    if (InCoreAnimation) CanExit = true;
                }
                else
                {
                    CanExit = true;
                }
            }
        }



        /// <summary> Notifies all the  States that a new state has started</summary>
        public virtual void NewActiveState(StateID newState) { }


        /// <summary> Notifies all the  States the Speed Have Changed</summary>
        public virtual void SpeedModifierChanged(MSpeed speed, int SpeedIndex)  { }


        /// <summary>Allow the State to be Replaced by lower States</summary>
        public virtual void AllowExit()
        {
            if (CanExit)
            {
                IgnoreLowerStates = false;
                IsPersistent = false;
               // Debugging("[Allow Exit]");
            }
        }

        /// <summary>Allow the State to Exit. It forces the Next state to be activated</summary>
        public virtual void AllowExit(int value) => animal.State_Allow_Exit(value);

        /// <summary>Allow the State to Exit. It forces the Next state to be activated. Set a value for the Exit Status </summary>
        public virtual void AllowExit(int NextState, int StateExitStatus)
        { 
            SetExitStatus(StateExitStatus);
            animal.State_Allow_Exit(NextState);
        }
        public void Debugging(string value)
        {
#if UNITY_EDITOR
            if (debug && animal.debugStates)
                Debug.Log($"<B>[{animal.name}]</B> → <B>[{this.GetType().Name}]</B> → <color=white>{value}</color>");
#endif
        }
        #endregion 

        #region Empty Methods

        /// <summary> Reset a State values to its first Awakening </summary>
        public void Enable(bool value) => Active = value;

        /// <summary>This will be called on the Last State before the Active state enters Core animations</summary>
        public virtual void PendingAnimationState() { }

        /// <summary>Set all the values for all the States on Start of the Animal... NOT THE START(ACTIVATION OF THE STATE) OF THE STATE</summary>
        public virtual void InitializeState() { }


        /// <summary>When Entering Core Animation of the State (the one tagged like the State) </summary>
        public virtual void EnterCoreAnimation() => SetStatus(0);


        /// <summary>When Entering a new animation State do this</summary>
        public virtual void EnterTagAnimation() { }

        /// <summary>Logic to Try exiting to Lower Priority States</summary>
        public virtual void TryExitState(float DeltaTime) { }


        ///// <summary>Called when Sleep is false</summary>
        //public virtual void JustWakeUp() { }


        /// <summary>Logic Needed to Try to Activate the State, By Default is the Input Value for the State </summary>
        public virtual bool TryActivate() => InputValue;

        public virtual void StatebyInput() 
        {
            if (!IsActiveState != this && TryActivate())                       //Enable the State if is not already active
                Activate();
        }

        /// <summary> Restore Internal values on the State (DO NOT INLCUDE Animal Methods or Parameter calls here</summary>
        public virtual void ResetStateValues() { } 


        /// <summary> Restore Internal values on the State (DO NOT INLCUDE Animal Methods or Parameter calls here</summary>
        public virtual void OnStateMove(float deltatime) { }

        /// <summary>Called before Adding Additive Position and Rotation</summary>
        public virtual void OnStatePreMove(float deltatime) { }


        public virtual void StateGizmos(MAnimal animal) { }



        /// <summary> Use this method to draw a custom inspector on the States</summary>
        public virtual bool CustomStateInspector() => false;
        #endregion
    }


    /// <summary>When an new Animation State Enters and it have a tag = "AnimationTag"</summary>
    [System.Serializable]
    public class TagModifier
    { 
        /// <summary>Animation State with the Tag  to apply the modifiers</summary>
        public string AnimationTag;
        public AnimalModifier modifier;
        /// <summary>"Animation Tag" Converted to TagHash</summary>
        public int TagHash { get; set; }
    }

    /// <summary>Modifier for the Animals</summary>
    [System.Serializable]
    public struct AnimalModifier
    {
        ///// <summary>Animation State with the Tag  to apply the modifiers</summary>
        //public string AnimationTag;

        [Utilities.Flag]
        public modifier modify;

        /// <summary>Enable/Disable the Root Motion on the Animator</summary>
        public bool RootMotion;
        /// <summary>Enable/Disable the Sprint on the Animal </summary>
        public bool Sprint;
        /// <summary>Enable/Disable the Gravity on the Animal, only used when the animal is on the air, falling, jumping ..etc</summary>
        public bool Gravity;
        /// <summary>Enable/Disable the if the Animal is Grounded (Align|Snap to ground position) </summary>
        public bool Grounded;
        /// <summary>Enable/Disable the Rotation Alignment while grounded </summary>
        public bool OrientToGround;
        /// <summary>Enable/Disable the  Custom Rotations (Used in Fly, Climb, UnderWater Swimming, etc)</summary>
        public bool CustomRotation;
        /// <summary>Enable/Disable the Free Movement... This allow to Use the Pitch direction vector</summary>
        public bool FreeMovement;
        /// <summary>Enable/Disable Additive Position use on the Speed Modifiers</summary>
        public bool AdditivePosition;
        /// <summary>Enable/Disable Additive Rotation use on the Speed Modifiers</summary>
        public bool AdditiveRotation;
        /// <summary>Enable/Disable is Persistent on the Active State ... meaning it cannot activate any other states whatsoever</summary>
        public bool Persistent;

        /// <summary>Enable/Disable is Persistent on the Active State ... meaning it cannot activate any other states whatsoever</summary>
        public bool IgnoreLowerStates;

        /// <summary>Enable/Disable the movement on the Animal</summary>
        public bool LockMovement;

        /// <summary>Enable/Disable is AllInputs on the Animal</summary>
        public bool LockInput;

        ///// <summary>Enable/Disable is AllInputs on the Animal</summary>
        //public bool Colliders;

        public void Modify(MAnimal animal)
        {
            if ((int)modify == 0) return; //Means that the animal have no modification

            if (Modify(modifier.IgnoreLowerStates)) { animal.ActiveState.IgnoreLowerStates = IgnoreLowerStates; }
            if (Modify(modifier.AdditivePositionSpeed))animal.UseAdditivePos = AdditivePosition;
         
            if (Modify(modifier.AdditiveRotationSpeed)) animal.UseAdditiveRot = AdditiveRotation;
            if (Modify(modifier.RootMotion)) animal.RootMotion = RootMotion;
            if (Modify(modifier.Gravity)) animal.UseGravity = Gravity;
            if (Modify(modifier.Sprint)) animal.UseSprintState = Sprint;
           
            if (Modify(modifier.Grounded)) animal.Grounded = Grounded;
            if (Modify(modifier.OrientToGround)) animal.UseOrientToGround = OrientToGround;
            if (Modify(modifier.CustomRotation)) animal.UseCustomAlign = CustomRotation;
            if (Modify(modifier.Persistent)) animal.ActiveState.IsPersistent = Persistent;
            if (Modify(modifier.LockInput)) animal.LockInput = LockInput;
            if (Modify(modifier.LockMovement)) animal.LockMovement = LockMovement;
           // if (Modify(modifier.Colliders)) animal.EnableColliders(Colliders);
            if (Modify(modifier.FreeMovement)) animal.FreeMovement = FreeMovement;

          
        }

        private bool Modify(modifier modifier) => ((modify & modifier) == modifier);
    }
    public enum modifier
    {
        RootMotion = 1,
        Sprint = 2,
        Gravity = 4,
        Grounded = 8,
        OrientToGround = 16,
        CustomRotation = 32,
        IgnoreLowerStates = 64,
        Persistent = 128,
        LockMovement = 256,
        LockInput = 512,
        AdditiveRotationSpeed = 1024,
        AdditivePositionSpeed = 2048,
        FreeMovement = 4096,
    }



#if UNITY_EDITOR

    [CustomEditor(typeof(State), true)]
    public class StateEd : Editor
    {
        SerializedProperty
           ID, Input, Priority, General, TryLoop, EnterTag, ExitTag, ExitFrame, ExitOnMain, ExitCooldown, CanStrafe, MovementStrafe, IdleStrafe, debug,
           SleepFromState, SleepFromMode, TagModifiers, QueueFrom, Editor_Tabs1
           ;
        State M;

        string[] Tabs = new string[4] { "General", "Tags", "Limits", "" };

        private void OnEnable()
        {
            M = (State)target;
            Tabs[3] = M.GetType().Name;

            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");

            ID = serializedObject.FindProperty("ID");
            Input = serializedObject.FindProperty("Input");
            Priority = serializedObject.FindProperty("Priority");
            TryLoop = serializedObject.FindProperty("TryLoop");


            EnterTag = serializedObject.FindProperty("EnterTag");
            ExitTag = serializedObject.FindProperty("ExitTag");
            TagModifiers = serializedObject.FindProperty("TagModifiers");

            General = serializedObject.FindProperty("General");
            ExitFrame = serializedObject.FindProperty("ExitFrame");
            ExitOnMain = serializedObject.FindProperty("ExitOnMain");
            ExitCooldown = serializedObject.FindProperty("ExitCooldown");

            CanStrafe = serializedObject.FindProperty("CanStrafe");
            MovementStrafe = serializedObject.FindProperty("MovementStrafe");
            IdleStrafe = serializedObject.FindProperty("IdleStrafe");


            debug = serializedObject.FindProperty("debug");


            SleepFromState = serializedObject.FindProperty("SleepFromState");
            SleepFromMode = serializedObject.FindProperty("SleepFromMode");
            QueueFrom = serializedObject.FindProperty("QueueFrom");
        }

        public GUIContent Deb;      

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs);


            int Selection = Editor_Tabs1.intValue;
            if (Selection == 0) ShowGeneral();
            else if (Selection == 1) ShowTags();
            else if (Selection == 2) ShowLimits();
            else if (Selection == 3) ShowState();

            serializedObject.ApplyModifiedProperties();

            Deb = new GUIContent((Texture)(AssetDatabase.LoadAssetAtPath("Assets/Malbers Animations/Common/Scripts/Editor/Icons/Debug_Icon.png", typeof(Texture))), "Debug");

            // base.OnInspectorGUI();
        }

        private void ShowGeneral()
        {
            MalbersEditor.DrawDescription($"Common parameters of the State");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(ID);

               // MTools.DrawDebugIcon(debug);
                var currentGUIColor = GUI.color;
                GUI.color = debug.boolValue ? Color.red : currentGUIColor;

                if (Deb == null) 
                    Deb = new GUIContent((Texture)
                        (AssetDatabase.LoadAssetAtPath("Assets/Malbers Animations/Common/Scripts/Editor/Icons/Debug_Icon.png", typeof(Texture))), "Debug");

                debug.boolValue = GUILayout.Toggle(debug.boolValue, Deb, EditorStyles.miniButton, GUILayout.Width(25));
                GUI.color = currentGUIColor;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(Input);
                EditorGUILayout.PropertyField(Priority);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(ExitFrame);
                EditorGUILayout.PropertyField(ExitOnMain);
                EditorGUILayout.PropertyField(ExitCooldown);
                EditorGUILayout.PropertyField(TryLoop);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(CanStrafe);
                if (M.CanStrafe)
                {
                    EditorGUILayout.PropertyField(MovementStrafe);
                    EditorGUILayout.PropertyField(IdleStrafe);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void ShowTags()
        {
            MalbersEditor.DrawDescription($"Animator Tags will modify the core parameters on the Animal.\nThe core tag value is the name of the ID - [{Tabs[3]}]");



            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(EnterTag);
                EditorGUILayout.PropertyField(ExitTag);
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(General, new GUIContent("Tag [" + Tabs[3] + "]"), true);
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(TagModifiers, new GUIContent(TagModifiers.displayName + " [" + TagModifiers.arraySize + "]"), true);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }

        private void ShowLimits()
        {
            MalbersEditor.DrawDescription($"Limits will disable temporarly the Activation of the State");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(SleepFromState, new GUIContent(SleepFromState.displayName + " [" + SleepFromState.arraySize + "]"), true);
                EditorGUILayout.PropertyField(SleepFromMode, new GUIContent(SleepFromMode.displayName + " [" + SleepFromMode.arraySize + "]"), true);
                EditorGUILayout.PropertyField(QueueFrom, new GUIContent(QueueFrom.displayName + " [" + QueueFrom.arraySize + "]"), true);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }

        protected virtual void ShowState()
        {
            MalbersEditor.DrawDescription($"{Tabs[3]} Parameters");

            if (!M.CustomStateInspector())
            {
                var skip = 18;
                var property = serializedObject.GetIterator();
                property.NextVisible(true);

                for (int i = 0; i < skip; i++)
                    property.NextVisible(false);

                do
                {
                    EditorGUILayout.PropertyField(property, true);
                } while (property.NextVisible(false));
            }
        }
    }


    [UnityEditor.CustomPropertyDrawer(typeof(AnimalModifier))]
    public class AnimalModifierDrawer : UnityEditor.PropertyDrawer
    {

        private float Division;
        int activeProperties;

        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            UnityEditor.EditorGUI.BeginProperty(position, label, property);

            GUI.Box(position, GUIContent.none, UnityEditor.EditorStyles.helpBox);

            position.x += 2;
            position.width -= 2;

            position.y += 2;
            position.height -= 2;


            var indent = UnityEditor.EditorGUI.indentLevel;
            UnityEditor.EditorGUI.indentLevel = 0;

            var height = UnityEditor.EditorGUIUtility.singleLineHeight;

            #region Serialized Properties
            var modify = property.FindPropertyRelative("modify");
            var Colliders = property.FindPropertyRelative("Colliders");
            var RootMotion = property.FindPropertyRelative("RootMotion");
            var Sprint = property.FindPropertyRelative("Sprint");
            var Gravity = property.FindPropertyRelative("Gravity");
            var OrientToGround = property.FindPropertyRelative("OrientToGround");
            var CustomRotation = property.FindPropertyRelative("CustomRotation");
            var IgnoreLowerStates = property.FindPropertyRelative("IgnoreLowerStates");
            var AdditivePositionSpeed = property.FindPropertyRelative("AdditivePosition");
            var AdditiveRotation = property.FindPropertyRelative("AdditiveRotation");
            var Grounded = property.FindPropertyRelative("Grounded");
            var FreeMovement = property.FindPropertyRelative("FreeMovement");
            var Persistent = property.FindPropertyRelative("Persistent");
            var LockInput = property.FindPropertyRelative("LockInput");
            var LockMovement = property.FindPropertyRelative("LockMovement");
            #endregion

            var line = position;
            var lineLabel = line;
            line.height = height;

            var foldout = lineLabel;
            foldout.width = 10;
            foldout.x += 10;

            UnityEditor.EditorGUIUtility.labelWidth = 16;
            UnityEditor.EditorGUIUtility.labelWidth = 0;

            modify.intValue = (int)(modifier)UnityEditor.EditorGUI.EnumFlagsField(line, label, (modifier)(modify.intValue));

            line.y += height + 2;
            Division = line.width / 3;

            activeProperties = 0;
            int ModifyValue = modify.intValue;

            if (Modify(ModifyValue, modifier.RootMotion))
                DrawProperty(ref line, RootMotion, new GUIContent("RootMotion", "Root Motion:\nEnable/Disable the Root Motion on the Animator"));

            if (Modify(ModifyValue, modifier.Sprint))
                DrawProperty(ref line, Sprint, new GUIContent("Sprint", "Sprint:\nEnable/Disable Sprinting on the Animal"));

            if (Modify(ModifyValue, modifier.Gravity))
                DrawProperty(ref line, Gravity, new GUIContent("Gravity", "Gravity:\nEnable/Disable the Gravity on the Animal. Used when is falling or jumping"));

            if (Modify(ModifyValue, modifier.Grounded))
                DrawProperty(ref line, Grounded, new GUIContent("Grounded", "Grounded\nEnable/Disable if the Animal is Grounded (If True it will  calculate  the Alignment for Position with the ground ). If False:  Orient to Ground is also disabled."));

            if (Modify(ModifyValue, modifier.CustomRotation))
                DrawProperty(ref line, CustomRotation, new GUIContent("Custom Rot", "Custom Rotation: \nEnable/Disable the Custom Rotations (Used in Fly, Climb, UnderWater, Swim), This will disable Orient to Ground"));

            UnityEditor.EditorGUI.BeginDisabledGroup(CustomRotation.boolValue || !Grounded.boolValue);
            if (Modify(ModifyValue, modifier.OrientToGround))
                DrawProperty(ref line, OrientToGround, new GUIContent("Orient Ground", "Orient to Ground:\nEnable/Disable the Rotation Alignment while grounded. (If False the Animal will be aligned with the Up Vector)"));
            UnityEditor.EditorGUI.EndDisabledGroup();

            if (Modify(ModifyValue, modifier.IgnoreLowerStates))
                DrawProperty(ref line, IgnoreLowerStates, new GUIContent("Ignore Lower States", "States below will not be able to try to activate themselves"));

            if (Modify(ModifyValue, modifier.Persistent))
                DrawProperty(ref line, Persistent, new GUIContent("Persistent", "Persistent:\nEnable/Disable is Persistent on the Active State ... meaning the Animal will not Try to activate any States"));

            if (Modify(ModifyValue, modifier.LockMovement))
                DrawProperty(ref line, LockMovement, new GUIContent("Lock Move", "Lock Movement:\nLock the Movement on the Animal, does not include Action Inputs for Attack, Jump, Action, etc"));

            if (Modify(ModifyValue, modifier.LockInput))
                DrawProperty(ref line, LockInput, new GUIContent("Lock Input", "Lock Input:\nLock the Inputs, (Jump, Attack, etc) does not include Movement Input (WASD or Axis Inputs)"));

            if (Modify(ModifyValue, modifier.AdditiveRotationSpeed))
               DrawProperty(ref line, AdditiveRotation, new GUIContent("+ Rot Speed", "Additive Rotation Speed:\nEnable/Disable Additive Rotation used on the Speed Modifier"));

            if (Modify(ModifyValue, modifier.AdditivePositionSpeed))
                DrawProperty(ref line, AdditivePositionSpeed, new GUIContent("+ Pos Speed", "Additive Position Speed:\nEnable/Disable Additive Position used on the Speed Modifiers"));


            if (Modify(ModifyValue, modifier.FreeMovement))
                DrawProperty(ref line, FreeMovement, new GUIContent("Free Move", "Free Movement:\nEnable/Disable the Free Movement... This allow to Use the Pitch direction vector and the Rotator Transform"));

            UnityEditor.EditorGUI.indentLevel = indent;
            UnityEditor.EditorGUI.EndProperty();
        }

        private void DrawProperty(ref Rect rect, UnityEditor.SerializedProperty property, GUIContent content)
        {
            Rect splittedLine = rect;
            splittedLine.width = Division - 1;

            splittedLine.x += (Division * (activeProperties % 3)) + 1;

            // property.boolValue = GUI.Toggle(splittedLine, property.boolValue, content, EditorStyles.miniButton);
            property.boolValue = UnityEditor.EditorGUI.ToggleLeft(splittedLine, content, property.boolValue);

            activeProperties++;
            if (activeProperties % 3 == 0)
            {
                rect.y += UnityEditor.EditorGUIUtility.singleLineHeight + 2;
            }
        }


        private bool Modify(int modify, modifier modifier)
        {
            return ((modify & (int)modifier) == (int)modifier);
        }

        public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
        {
            int activeProperties = 0;

            var modify = property.FindPropertyRelative("modify");
            int ModifyValue = modify.intValue;

            if (Modify(ModifyValue, modifier.RootMotion)) activeProperties++;
            if (Modify(ModifyValue, modifier.Sprint)) activeProperties++;
            if (Modify(ModifyValue, modifier.Gravity)) activeProperties++;
            if (Modify(ModifyValue, modifier.Grounded)) activeProperties++;
            if (Modify(ModifyValue, modifier.CustomRotation)) activeProperties++;
            if (Modify(ModifyValue, modifier.OrientToGround)) activeProperties++;
            if (Modify(ModifyValue, modifier.IgnoreLowerStates)) activeProperties++;
            if (Modify(ModifyValue, modifier.AdditivePositionSpeed)) activeProperties++;
            if (Modify(ModifyValue, modifier.AdditiveRotationSpeed)) activeProperties++;
            if (Modify(ModifyValue, modifier.Persistent)) activeProperties++;
            if (Modify(ModifyValue, modifier.FreeMovement)) activeProperties++;
            if (Modify(ModifyValue, modifier.LockMovement)) activeProperties++;
            if (Modify(ModifyValue, modifier.LockInput)) activeProperties++;
            //  if (Modify(ModifyValue, modifier.Colliders)) activeProperties++;

            float lines = (int)((activeProperties + 2) / 3) + 1;

            return base.GetPropertyHeight(property, label) * lines + (2 * lines);
        }
    }
#endif
}
