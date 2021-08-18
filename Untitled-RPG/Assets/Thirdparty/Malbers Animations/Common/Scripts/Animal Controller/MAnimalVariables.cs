using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System;
using System.Collections;
using MalbersAnimations.Utilities;

namespace MalbersAnimations.Controller
{
    /// Variables
    public partial class MAnimal
    {
        /// <summary>Sets a Bool Parameter on the Animator using the parameter Hash</summary>
        public Action<int, bool> SetBoolParameter { get; set; }
        /// <summary>Sets a float Parameter on the Animator using the parameter Hash</summary>
        public Action<int, float> SetFloatParameter { get; set; }
        /// <summary>Sets a Integer Parameter on the Animator using the parameter Hash</summary> 
        public Action<int, int> SetIntParameter { get; set; }

        /// <summary>Check when a Animation State is Starting</summary>
        public System.Action<int> StateCycle { get; set; }

        /// <summary>Invoked after the ActiveState execute its code</summary>
        public System.Action PreStateMovement = delegate { };

        /// <summary>Invoked after the ActiveState execute its code</summary>
        public System.Action PostStateMovement = delegate { };

        /// <summary>Get all the Animator Parameters the Animal Controller has</summary>
        private Hashtable animatorParams;

        #region Static Properties
        /// <summary>List of all the animals on the scene</summary>
        public static List<MAnimal> Animals;
        /// <summary>Main Animal used as the player controlled by Input</summary>
        public static MAnimal MainAnimal;
        #endregion
         
        #region States


        /// <summary>NECESARY WHEN YOU ARE USING MULTIPLE ANIMALS</summary>
        public bool CloneStates = true;

        ///<summary> List of States for this animal  </summary>
        public List<State> states = new List<State>();

        ///<summary>List of Events to Use on the States</summary>
        public List<OnEnterExitState> OnEnterExitStates;
        ///<summary>List of Events to Use on the Stances</summary>
        public List<OnEnterExitStance> OnEnterExitStances;

        ///<summary>On Which State the animal should Start on Enable</summary>
        public StateID OverrideStartState;

        /// <summary> Current State. Changing this variable wil not exectute the Active State logic</summary> 
        internal State activeState;
        /// <summary> Store the Last State, This will not change the Last State parameter on the animator</summary> 
        internal State lastState;
        /// <summary> Store the Last State </summary> 
        public State LastState
        {
            get => lastState;
            internal set
            {
                lastState = value;
                SetIntParameter(hash_LastState, lastState.ID);   //Sent to the Animator the previews Active State 
            }
        }

        ///<summary> Store a State (PreState) that can be used later</summary>
        protected State Pin_State;

       

        /// <summary>Used to call the Last State one more time before it changes to the new state </summary>
        public bool JustActivateState { get; internal set; }

        public StateID ActiveStateID { get; private set; }


        /// <summary>State Float Value</summary>
        public float State_Float { get; private set; }

        /// <summary>Set/Get the Active State</summary>
        public State ActiveState
        {
            get => activeState;
            internal set
            {
                activeState = value;

                if (value == null) return;

                JustActivateState = true;

                ActiveStateID = activeState.ID;
                SetIntParameter(hash_State, activeState.ID.ID);           //Sent to the Animator the value to Apply  
                OnStateChange.Invoke(ActiveStateID);

                foreach (var st in states)  st.NewActiveState(activeState.ID); //Notify all states that a new state has been activated

                Set_Sleep_FromStates(activeState);
                Set_Queue_States(activeState);

                if (IsPlayingMode && ActiveMode.StateCanInterrupt(ActiveStateID))//If a mode is playing check a State Change
                {
                    Mode_Interrupt();
                } 
            }
        }

        /// <summary>When a new State is Activated, Make sure the other States are sent to sleep</summary>
        internal void Set_Sleep_FromStates(State state)
        {
            foreach (var st in states)
                st.IsSleepFromState = st.SleepFromState.Contains(state.ID);        //Sent to sleep states that has some Avoid State
        }

        /// <summary>Check if there's a State that cannot be enabled when playing a mode </summary>
        internal virtual void Set_State_Sleep_FromMode(bool playingMode)
        {
            foreach (var state in states)
                state.IsSleepFromMode = playingMode && state.SleepFromMode.Contains(ActiveMode.ID);
        }

        /// <summary>When a new State is Activated, Make sure the other States are sent to sleep</summary>
        internal void Set_Queue_States(State state)
        {
            foreach (var st in states)
                st.OnQueue = st.QueueFrom.Contains(state.ID);        //Sent to sleep states that has some Avoid State
        }

        #endregion

        #region General
        [ContextMenuItem("Debug AdditivePos", nameof(DebLogAdditivePos))]
        [ContextMenuItem("Debug AdditiveRot", nameof(DebLogAdditiveRot))]
        /// <summary>Is this animal is the main Player?</summary>
        public BoolReference isPlayer = new BoolReference(true);

        /// <summary> Layers the Animal considers ground</summary>
        [SerializeField] private LayerReference groundLayer = new LayerReference(1);

        /// <summary> Layers the Animal considers ground</summary>
        public LayerMask GroundLayer => groundLayer.Value;

        /// <summary>Distance from the Pivots to the ground </summary>
        public float height = 1f;
        /// <summary>Height from the ground to the hip multiplied for the Scale Factor</summary>
        public float Height => (height) * ScaleFactor;         

        /// <summary>The Scale Factor of the Animal.. if the animal has being scaled this is the multiplier for the raycasting things </summary>
        public float ScaleFactor => transform.localScale.y;

        /// <summary>Does this Animal have an InputSource </summary>
        public IInputSource InputSource;

        [SerializeField] private Vector3 center;
        /// <summary>Center of the Animal to be used for AI and Targeting  </summary>
        public Vector3 Center
        {
            private set => center = value;
            get => transform.TransformPoint(center);
        }
        #endregion

        #region Stance

        [SerializeField] private StanceID currentStance;
        [SerializeField] private StanceID defaultStance;


        /// <summary>Last Stance the Animal was</summary>
        public int LastStance { get; private set; }

        public StanceID DefaultStance  { get => defaultStance ; set=> defaultStance = value; }

       

        /// <summary>Stance Integer Value sent to the animator</summary>
        public StanceID Stance
        {
            get => currentStance;
            set
            {
                if (Sleep || !enabled) return;  //Do nothing if is not active

                LastStance = currentStance;
                currentStance = value;

                var exit = OnEnterExitStances.Find(st => st.ID.ID == LastStance);
                exit?.OnExit.Invoke();
                OnStanceChange.Invoke(value);
                var enter = OnEnterExitStances.Find(st => st.ID.ID == value);
                enter?.OnEnter.Invoke();

                SetOptionalAnimParameter(hash_Stance, currentStance.ID); //Set on the Animator the Current Stance
                SetOptionalAnimParameter(hash_LastStance, LastStance);//Set on the Animator the Last Stance
                ActiveState.SetSpeed(); //Check if the speed modifier has changed

                if (IsPlayingMode && ActiveMode.StanceCanInterrupt(currentStance))//If a mode is playing check a State Change
                {
                    Mode_Interrupt();
                } 
            }
        }
        #endregion

        #region Movement

        /// <summary> Global Animator Speed for the Animal </summary>
        public FloatReference AnimatorSpeed = new FloatReference(1);

        //public FloatReference MovementDeathValue = new FloatReference(0.05f);
        [SerializeField] private BoolReference alwaysForward = new BoolReference(false);

        /// <summary>Sets to Zero the Z on the Movement Axis when this is set to true</summary>
        [Tooltip("Sets to Zero the Z on the Movement Axis when this is set to true")]
        [SerializeField] private BoolReference lockForwardMovement = new BoolReference(false);
        /// <summary>Sets to Zero the X on the Movement Axis when this is set to true</summary>
        [Tooltip("Sets to Zero the X on the Movement Axis when this is set to true")]
        [SerializeField] private BoolReference lockHorizontalMovement = new BoolReference(false);
        /// <summary>Sets to Zero the Y on the Movement Axis when this is set to true</summary>
        [Tooltip("Sets to Zero the Y on the Movement Axis when this is set to true")]
        [SerializeField] private BoolReference lockUpDownMovement = new BoolReference(false);

        ////public bool AdditiveX;
        ////public bool AdditiveY;
        //[Tooltip("The Up Down Input is interpreted as Additive (Spyro Underwater Movement Style)")]
        //public bool AdditiveUp;

        /// <summary>(Z), horizontal (X) and Vertical (Y) Movement Input</summary>
        public Vector3 MovementAxis;

        /// <summary>(Z), horizontal (X) and Vertical (Y) Raw Movement Input</summary>
        public Vector3 MovementAxisRaw;

        /// <summary>Current Raw Input Axis gotted from an Input Entry </summary>
        public Vector3 RawInputAxis;// { get; set; }
        //{
        //    get => rawInputAxis;
        //    set
        //    {
        //        rawInputAxis = value;
        //       Debug.Log($"RawInputAxis: {rawInputAxis} ");
        //    }
        //}
        //Vector3 rawInputAxis;


        /// <summary>The Animal is using Input instead of a Direction to move</summary>
        public bool UseRawInput { get; internal set; }

        /// <summary>Forward (Z), horizontal (X) and Vertical (Y) Smoothed Movement Input AFTER aplied Speeds Multipliers (THIS GOES TO THE ANIMATOR)</summary>
        public Vector3 MovementAxisSmoothed;

        ///// <summary>Forward (Z), horizontal (X) and Vertical (Y) Smoothed Movement Input BEFORE aplied Speeds Multipliers</summary>
        //public Vector3 MovementAxisRawSmoothed;


        /// <summary>The animal will always move forward</summary>
        public bool AlwaysForward
        {
            get => alwaysForward.Value;
            set
            {
                alwaysForward.Value = value;
                MovementAxis.z = alwaysForward.Value ? 1 : 0;
                MovementDetected = AlwaysForward;
            }
        }

        /// <summary>[Raw Direction the Character will go Using Inputs or a Move Direction</summary>
        public Vector3 Move_Direction { get; internal set; }



        ///// <summary>if False then the Directional Speed wont be Updated, used to Rotate the Animal but still moving on the Last Direction </summary>
        //public bool UpdateDirectionSpeed { get; set; }
        ///// <summary>Current Direction Speed Applied to the Additional Speed </summary>
        //public Vector3 Speed_Direction { get; internal set; }


        private bool movementDetected;

        /// <summary>Checking if the movement input was activated</summary>
        public bool MovementDetected
        {
            get => movementDetected;
            internal set
            {
                if (movementDetected != value)
                {
                    movementDetected = value;
                    OnMovementDetected.Invoke(value);
                    SetBoolParameter?.Invoke(hash_Movement, MovementDetected);
                }
            }
        }

    /// <summary>The Animal uses the Camera Forward Diretion to Move</summary>
    public BoolReference useCameraInput = new BoolReference();

        /// <summary>Use the Camera Up Vector to Move while flying or Swiming UnderWater</summary>
        public BoolReference useCameraUp = new BoolReference();

        /// <summary>The Animal uses the Camera Forward Direction to Move</summary>
        public bool UseCameraInput { get => useCameraInput.Value; set => useCameraInput.Value = value; }

        /// <summary>Store the Starting Use camera Input Value</summary>
        public bool DefaultCameraInput { get; set; }

        /// <summary>Use the Camera Up Vector to Move while flying or Swiming UnderWater</summary>
        public bool UseCameraUp { get => useCameraUp.Value; set => useCameraUp.Value = value; }

        /// <summary> Is the animal using a Direction Vector for moving? or a  World Direction for moving</summary>
        public bool UsingMoveDirection { private set; get; }
        public bool Rotate_at_Direction { private set; get; }

        /// <summary>Main Camera on the Game</summary>
        public TransformReference m_MainCamera = new TransformReference();

        public Transform MainCamera => m_MainCamera.Value;

        /// <summary> Additive Position Modifications for the  animal (Terrian Snapping, Speed Modifiers Positions, etc)</summary>
        public Vector3 AdditivePosition//;
        {
            get => additivePosition;
            set
            {
                additivePosition = value;
                if (additivePosLog)  Debug.Log($"Additive Pos:  {(additivePosition / DeltaTime):F3} ");
            }
        }
        Vector3 additivePosition;

        /// <summary> Additive Rotation Modifications for the  animal (Terrian Aligment, Speed Modifiers Rotations, etc)</summary>
        public Quaternion AdditiveRotation//;
        {
            get => additiveRotation;
            set
            {
                additiveRotation = value;
                if (additiveRotLog) Debug.Log($"Additive ROT:  {(additiveRotation):F3} ");
            }
        }
        Quaternion additiveRotation;


        [SerializeField] private bool additivePosLog;
        [SerializeField] private bool additiveRotLog;


        private void DebLogAdditivePos()
        {
            additivePosLog ^= true;
            MTools.SetDirty(this); 
        }

        private void DebLogAdditiveRot()
        {
            additiveRotLog ^= true;
            MTools.SetDirty(this);
        }

        /// <summary>Inertia Speed to smoothly change the Speed Modifiers </summary>
        public Vector3 InertiaPositionSpeed   { get; internal set; }
        //{
        //    get => InertiaPPS;
        //    set
        //    {
        //        InertiaPPS = value;
        //        Debug.Log($"InertiaPositionSpeed:  {(InertiaPPS):F3} ");
        //    }
        //}
        //Vector3 InertiaPPS;




        /// <summary> If true it will keep the Conrtoller smooth push of the movement stick</summary>
        [SerializeField] private BoolReference SmoothVertical = new BoolReference(true);
        /// <summary>Global turn multiplier</summary>
        public FloatReference TurnMultiplier = new FloatReference(0f);
        /// <summary>Up Down Axis Smooth Factor</summary>
        public FloatReference UpDownLerp = new FloatReference(10f);


        /// <summary>Difference from the Last Frame and the Current Frame</summary>
        public Vector3 DeltaPos  { get; internal set; }
        //{
        //    set
        //    {
        //        m_DeltaPos = value;
        //        Debug.Log($"DeltaPos POS:  {(m_DeltaPos / DeltaTime):F3} ");
        //    }
        //    get => m_DeltaPos;
        //}
        //Vector3 m_DeltaPos;
         

        /// <summary>World Position on the last Frame</summary>
        public Vector3 LastPos { get; internal set; }

        /// <summary>Velocity acumulated from the last Frame</summary>
        public Vector3 Inertia => DeltaPos / DeltaTime;

        /// <summary>Difference between the Current Rotation and the desire Input Rotation </summary>
        public float DeltaAngle { get; internal set; }

        /// <summary>Pitch direction used when Free Movement is Enable (Direction of the Move Input) </summary>
        public Vector3 PitchDirection { get; internal set; }
        /// <summary>Pitch Angle </summary>
        public float PitchAngle { get; internal set; }
        /// <summary>Bank</summary>
        public float Bank { get; internal set; }

        /// <summary>Speed from the Vertical input multiplied by the speeds inputs(Walk Trot Run) this is the value thats goes to the Animator, is not the actual Speed of the animals</summary>
        public float VerticalSmooth { get => MovementAxisSmoothed.z; internal set => MovementAxisSmoothed.z = value; }

        /// <summary>Direction from the Horizontal input multiplied by the speeds inputs this is the value thats goes to the Animator, is not the actual Speed of the animals</summary>
        public float HorizontalSmooth { get => MovementAxisSmoothed.x; internal set => MovementAxisSmoothed.x = value; }

        /// <summary>Direction from the Up Down input multiplied by the speeds inputs this is the value thats goes to the Animator, is not the actual Speed of the animals</summary>
        public float UpDownSmooth
        {
            get => MovementAxisSmoothed.y;
            internal set
            {
                MovementAxisSmoothed.y = value;
               // Debug.Log("UD" + value);
            }
        }

        /// <summary> If true it will keep the Controller smooth push of the movement stick</summary>
        public bool UseSmoothVertical { get => SmoothVertical.Value; set => SmoothVertical.Value = value; }

       
        internal bool sprint;
        internal bool realSprint;

        /// <summary>Sprint Input</summary>
        public bool Sprint
        {
            get => UseSprintState && sprint && UseSprint && MovementDetected;
            set
            {
                var newRealSprint = UseSprintState && value && UseSprint && MovementDetected; //Check if the animal has movement

                sprint = value; //Store only the Input value for the sprint I think it works

                if (realSprint != newRealSprint)
                {
                    realSprint = newRealSprint;

                    OnSprintEnabled.Invoke(realSprint);

                    int currentPI = CurrentSpeedIndex;
                    var speed = CurrentSpeedModifier;

                    if (realSprint)
                    {
                        speed = SprintSpeed;
                        currentPI++;
                    }

                    OnSpeedChange.Invoke(speed);       //Invoke the Speed again
                    ActiveState?.SpeedModifierChanged(speed, currentPI);
                }
            }
        }

        public void SetSprint(bool value) => Sprint = value;

        /// <summary> The current value of the Delta time the animal is using (Fixed or not)</summary>
        public float DeltaTime { get; private set; }

        #endregion

        #region Alignment Ground
        /// <summary>Smoothness value to Snap to ground </summary>
        public FloatReference AlignPosLerp = new FloatReference(15f);
        /// <summary>Smoothness Position value when Entering from Non Grounded States</summary>
        public FloatReference AlignPosDelta = new FloatReference(2.5f);
        /// <summary>Smoothness Rotation value when Entering from Non Grounded States</summary>
        public FloatReference AlignRotDelta = new FloatReference(2.5f);

        /// <summary>Smoothness Position value when Entering from Non Grounded States </summary>
        public float AlignPosLerpDelta { get; private set; }

        /// <summary>Smoothness Rotation value when Entering from non Grounded States </summary>
        public float AlignRotLerpDelta { get; private set; }


        /// <summary>Smoothness value to Snap to ground  </summary>
        public FloatReference AlignRotLerp = new FloatReference(15f);


        public IntReference AlignLoop = new IntReference(1);

        [Tooltip("Tag your small rocks, debree,steps and stair objects  with this Tag. It will help the animal to recognize better the Terrain")]
        public StringReference DebreeTag = new StringReference("Stair");

        ///// <summary>Removes Horizontal movement Used on the Locomotion Movement to block falling ??</summary>
        //public bool Remove_HMovement { get; internal set; }

        /// <summary>Maximun angle on the terrain the animal can walk </summary>
        [Range(1f, 90f), Tooltip("Maximun angle on the terrain the animal can walk")]
        public float maxAngleSlope = 45f;

        /// <summary>Main Pivot Slope Angle</summary>
        public float MainPivotSlope { get; private set; }

        /// <summary>Used to add extra Rotations to the Animal</summary>
        public Transform Rotator;
        public Transform RootBone;

        /// <summary>Check if can Fall on slope while on the ground "Decline Slope"</summary>
        public bool DeepSlope => TerrainSlope < -maxAngleSlope;

        /// <summary>Check if the Animal is on any Slope</summary>
        public bool isinSlope => Mathf.Abs(TerrainSlope) > maxAngleSlope;

        /// <summary>Speed of the Animal used on the RIgid Body (Useful for Speed Modifiers)</summary>
        public float HorizontalSpeed { get; private set; }

        /// <summary>Velocity of the Animal used on the RIgid Body (Useful for Speed Modifiers)</summary>
        public Vector3 HorizontalVelocity { get; private set; }
        

        /// <summary>Calculation of the Average Surface Normal</summary>
        public Vector3 SurfaceNormal { get; private set; } 

        /// <summary>Calculate slope Angle and normalize it with the Max Angle Slope</summary>
        public float SlopeNormalized => TerrainSlope / maxAngleSlope; //Normalize the AngleSlop by the MAX Angle Slope and make it positive(HighHill) or negative(DownHill)

        /// <summary>Slope Calculate from the Surface Normal. Positive = Higher Slope, Negative = Lower Slope </summary>
        public float TerrainSlope { get; private set; }


       [SerializeField] private BoolReference grounded = new BoolReference(false);
        /// <summary> Is the Animal on a surface, when True the Raycasting for the Ground is Applied</summary>
        public bool Grounded
        {
            get => grounded.Value;
            internal set
            {
                if (grounded.Value != value)
                {
                    grounded.Value = value;

                    if (!value)
                    {
                        platform = null; //If groundes is false remove the stored Platform 
                    }
                    else
                    {
                        ResetGravityValues();
                        Force_Reset();

                        UpDownAdditive = 0; //Reset UpDown Additive 
                        UsingUpDownExternal = false; //Reset UpDown Additive 
                        GravityMultiplier = 1;
                        ExternalForceAirControl = true; //Reset the External Force Air Control
                    }

                    SetBoolParameter(hash_Grounded, grounded.Value);
                }
                OnGrounded.Invoke(value);
              // Debug.Log("Grounded: " + value);
            }
        }
        #endregion

        #region External Force

        /// <summary>Add an External Force to the Animal</summary>
        public Vector3 ExternalForce { get; set; }

        /// <summary>Current External Force the animal current has</summary>
        public Vector3 CurrentExternalForce { get; set; }
        //{
        //    set
        //    {
        //        m_CurrentExternalForce = value;
        //        Debug.Log($"CurrentExternalForce:  {m_CurrentExternalForce} ");
        //    }
        //    get => m_CurrentExternalForce;
        //}
        //Vector3 m_CurrentExternalForce;

        /// <summary>External Force Aceleration /summary>
        public float ExternalForceAcel { get; set; }

        /// <summary>External Force Air Control, Can it be controlled while on the air?? /summary>
        public bool ExternalForceAirControl { get; set; }

        public bool HasExternalForce => ExternalForce != Vector3.zero;
        #endregion

        #region References
        /// <summary>Returns the Animator Component of the Animal </summary>

        [RequiredField] public Animator Anim;
        [RequiredField] public Rigidbody RB;                   //Reference for the RigidBody

        /// <summary>Transform.UP (Stored)</summary>
        public Vector3 Up => transform.up;
        /// <summary>Transform.Right (Stored)</summary>
        public Vector3 Right => transform.right;
        /// <summary>Transform.Forward (Stored) </summary>
        public Vector3 Forward => transform.forward;


        #endregion

        #region Modes
        /// <summary>Allows the Animal Start Playing a Mode</summary>
        public IntReference StartWithMode = new IntReference(0);

        /// <summary>Status value for the Mode (-1: Loop, -2: Interrupted, 0: Available</summary>
        public int ModeStatus { get; private set; }

        /// <summary>Float value for the Mode</summary>
        public float ModePower { get; set; }

        // private int modeStatus;
        internal Mode activeMode;

        ///<summary> List of States for this animal  </summary>
        public List<Mode> modes = new List<Mode>();

        /// <summary>Is Playing a mode on the Animator</summary>
        public bool IsPlayingMode => activeMode != null;

        /// <summary>A mode is Set, but the Animation has not started yet</summary>
        public bool IsPreparingMode { get; set; }
        //{
        //    get => m_IsPreparingMode;
        //    internal set
        //    {
        //        m_IsPreparingMode = value;
        //        Debug.Log("m_IsPreparingMode: " + m_IsPreparingMode);
        //    }
        //}
        //bool m_IsPreparingMode;

        /// <summary>Is the Animal on any Zone</summary>
        //public bool IsOnZone { get; internal set; }
        public Zone Zone { get; internal set; }

        ///// <summary>Checks if there's any Mode with an Input Active </summary>
        //public Mode InputMode { get; internal set; }

        /// <summary>ID Value for the Last Mode Played </summary>
        public int LastModeID { get; internal set; }

        /// <summary>ID Value for the Last Ablity Played </summary>
        public int LastAbilityIndex { get; internal set; }

        /// <summary>Last Mode Played Status (None, Playing, Completed, Interrupted)</summary>
       // public MStatus ModeInternalStatus { get; internal set; }
        //{
        //    get => mStatus;
        //    internal set 
        //    {
        //        mStatus = value;
        //        Debug.Log(mStatus);
        //    }
        //}
        //MStatus mStatus;

        /// <summary>Set/Get the Active Mode, Prepare the values for the Animator... Does not mean the Mode is Playing</summary>
        public Mode ActiveMode
        {
            get => activeMode;
            internal set
            {
                var lastMode = activeMode;
                activeMode = value;
                ModeTime = 0;

                if (value != null)
                {
                    ActiveModeID = value.ID;
                    OnModeStart.Invoke(ActiveModeID, value.AbilityIndex);
                }
                else
                {
                    ActiveModeID = 0;
                }
                
                if (lastMode != null)
                {
                    LastModeID = lastMode.ID;
                    LastAbilityIndex = lastMode.AbilityIndex;
                    OnModeEnd.Invoke(lastMode.ID, LastAbilityIndex);
                    Stance = Stance; //Updates the Stance Code
                }

             //   Debug.Log("IsPlayingMode = " + IsPlayingMode);
            }
        }

        /// <summary>Set the Values to the Animator to Enable a mode... Does not mean that the mode is enabled</summary>
        internal virtual void SetModeParameters(Mode value, int status)
        {
            if (value != null)
            {
                var ability = (value.ActiveAbility != null ? (int)value.ActiveAbility.Index : 0);

                int mode = Mathf.Abs(value.ID * 1000) + Mathf.Abs(ability);      //Convert it into a 4 int value Ex: Attack 1001

                //If the Mode is negative or the Ability is negative then Set the Animator Parameter negative too
                ModeAbility = (value.ID < 0 || ability < 0) ? -mode : mode;      

                SetModeStatus(status);                                           //IMPORTANT WHEN IS MAKING SOME RANDOM STUFF

                IsPreparingMode = true;
                ModeTime = 0;
            }
            else
            {
                SetModeStatus(ModeAbility = Int_ID.Available);
            }
        }

        /// <summary>Current Mode ID and Ability Append Together</summary>
        public int ModeAbility
        {
            get => m_ModeIDAbility;
            internal set
            {
                m_ModeIDAbility = value;
                SetIntParameter(hash_Mode, m_ModeIDAbility);
            }
        }
        private int m_ModeIDAbility;

        /// <summary>Current Animation Time of the Mode,used in combos</summary>
        public float ModeTime { get; internal set; }

        /// <summary>Active Mode ID</summary>
        public int ActiveModeID { get; private set; }

        public Mode Pin_Mode { get; private set; }

        #endregion


        [SerializeField] private BoolReference sleep = new BoolReference(false);

        /// <summary>Put the Controller to sleep, is like disalbling the script but internally</summary>
        public bool Sleep
        {
            get => sleep.Value;
            set
            {
                if (!value && Sleep) //Means is out of sleep
                {
                    MTools.ResetFloatParameters(Anim);                         //Set All Float values to their defaut (For all the Float Values on the Controller  while is not riding)
                    ResetController();
                }
                sleep.Value = value;

                //Debug.Log("Sleep" + Sleep);

                LockInput = LockMovement = value; //Also Set to sleep the Movement and Input
                if (Sleep) SetOptionalAnimParameter(hash_Random, 0); //Set Random to 0
            }
        }

        #region Strafe
        public BoolEvent OnStrafe = new BoolEvent();

        [SerializeField] private BoolReference m_strafe = new BoolReference(false);
        [SerializeField] private BoolReference m_CanStrafe = new BoolReference(false);
        [SerializeField] private BoolReference m_StrafeNormalize = new BoolReference(false);
        [SerializeField] private FloatReference m_StrafeLerp = new FloatReference(5f);


        public bool StrafeNormalize => m_StrafeNormalize.Value;



        /// <summary> Is the Animal on the Strafe Mode</summary>
        public bool Strafe
        {
            get => m_CanStrafe.Value && m_strafe.Value && ActiveState.CanStrafe;
            set
            {
                if (sleep) return;

                m_strafe.Value = value && m_CanStrafe.Value && ActiveState.CanStrafe;
              
                OnStrafe.Invoke(m_strafe.Value);
               
                SetOptionalAnimParameter(hash_Strafe, m_strafe.Value);
            }
        }

        public bool CanStrafe { get => m_CanStrafe.Value; set => m_CanStrafe.Value = value; }

        private float StrafeDeltaValue;
        private float HorizontalAimAngle_Raw;
        //private float HorizontalAimAngle_Smooth;
        public Aim Aimer;


        #endregion

        #region Pivots

        internal RaycastHit hit_Hip;            //Hip and Chest Ray Cast Information
        internal RaycastHit hit_Chest;            //Hip and Chest Ray Cast Information

        public List<MPivots> pivots = new List<MPivots>
            { new MPivots("Hip", new Vector3(0,0.7f,-0.7f), 1), new MPivots("Chest", new Vector3(0,0.7f,0.7f), 1), new MPivots("Water", new Vector3(0,1,0), 0.05f) };

        public MPivots Pivot_Hip;
        public MPivots Pivot_Chest;

        public int AlignUniqueID { get; private set; }

        /// <summary>Does it have a Hip Pivot?</summary>
        public bool Has_Pivot_Hip;

        /// <summary>Does it have a Hip Pivot?</summary>
        public bool Has_Pivot_Chest;

        /// <summary> Do the Main (Hip Ray) found ground </summary>
        public bool MainRay { get; private set; }
        /// <summary> Do the Fron (Chest Ray) found ground </summary>
        public bool FrontRay { get; private set; }

        /// <summary>Main pivot Point is the Pivot Chest Position, if not the Pivot Hip Position one</summary>
        public Vector3 Main_Pivot_Point
        {
            get
            {
                Vector3 pivotPoint;
                if (Has_Pivot_Chest)
                {
                    pivotPoint = Pivot_Chest.World(transform);
                }
                else if (Has_Pivot_Hip)
                {
                    pivotPoint = Pivot_Hip.World(transform);
                }
                else
                {
                    pivotPoint = transform.TransformPoint(new Vector3(0, Height, 0));
                }

                return pivotPoint + DeltaVelocity; 
            }
        }

        public Vector3 DeltaVelocity { get; private set; }

        /// <summary> Does the Animal Had a Pivot Chest at the beggining?</summary>
        private bool Starting_PivotChest;

        /// <summary> Disable Temporally the Pivot Chest in case the animal is on 2 legs </summary>
        public void DisablePivotChest() => Has_Pivot_Chest = false;

        /// <summary> Used for when the Animal is on 2 feet instead of 4</summary>
        public void EnablePivotChest() => Has_Pivot_Chest = Starting_PivotChest;


        /// <summary>Check if there's no Pivot Active </summary>
        public bool NoPivot => !Has_Pivot_Chest && !Has_Pivot_Hip;

        /// <summary> Gets the the Main Pivot Multiplier * Scale factor (Main Pivot is the Chest, if not then theHip Pivot) </summary>
        public float Pivot_Multiplier
        {
            get
            {
                float multiplier = Has_Pivot_Chest ? Pivot_Chest.multiplier : (Has_Pivot_Hip ? Pivot_Hip.multiplier : 1f);
                return multiplier * ScaleFactor * (NoPivot ? 1.5f : 1f);
            }
        }
        #endregion

        #region Speed Modifiers

        /// <summary> if True Disable changing the speeds  </summary>
        public bool SpeedChangeLocked { get; private set; }

        /// <summary>Speed Set for Stances</summary>
        public List<MSpeedSet> speedSets;
        /// <summary>Active Speed Set</summary>
        private MSpeedSet currentSpeedSet = new MSpeedSet();
        /// <summary>True if the State is modifing the current Speed Modifier</summary>
        public bool CustomSpeed;

        public MSpeed currentSpeedModifier = MSpeed.Default;
        internal MSpeed SprintSpeed = MSpeed.Default;
        //public List<MSpeed> speedModifiers = new List<MSpeed>();

        protected int speedIndex;


        /// <summary>What is the Speed modifier the Animal is current using (Walk? trot? Run?)</summary>
        public MSpeed CurrentSpeedModifier
        {
            get
            {
                var speedMod = currentSpeedModifier;
                if (Sprint && !CustomSpeed) speedMod = SprintSpeed;

                return speedMod;
            }
            private set
            {
                currentSpeedModifier = value;
                OnSpeedChange.Invoke(CurrentSpeedModifier);
                ActiveState?.SpeedModifierChanged(CurrentSpeedModifier, CurrentSpeedIndex);
            }
        }


        /// <summary>Current Speed Index used of the Current Speed Set E.G. (1 for Walk, 2 for trot)</summary>
        public int CurrentSpeedIndex
        {
            get => speedIndex;
            internal set
            {
                if (CustomSpeed || SpeedChangeLocked || CurrentSpeedSet == null) return;

                speedIndex = value;

               // speedIndex %= CurrentSpeedSet.TopIndex + 1;

                if (speedIndex > CurrentSpeedSet.TopIndex)   
                    speedIndex = CurrentSpeedSet.TopIndex;

                var SP = CurrentSpeedSet.Speeds; 
               
                speedIndex = Mathf.Clamp(speedIndex, 1, SP.Count); //Clamp the Speed Index

                var sprintSpeed = Mathf.Clamp(speedIndex + 1, 1, SP.Count);

                CurrentSpeedModifier = SP[speedIndex - 1];

               // Debug.Log("CurrentSpeedIndex: " + speedIndex);


                SprintSpeed = SP[sprintSpeed - 1];

                if (CurrentSpeedSet != null) CurrentSpeedSet.CurrentIndex = speedIndex; //Keep the Speed saved on the state too in case the active speed was changed
            }
        }

        /// <summary>Current Speed Set used on the Animal</summary>
        public MSpeedSet CurrentSpeedSet
        {
            get => currentSpeedSet;
            internal set
            {
                currentSpeedSet = value;
                CurrentSpeedIndex = currentSpeedSet.CurrentIndex;

                //Debug.Log("CurrentSpeedSet: " + value.name);
            }
        }

        /// <summary> Value for the Speed  Global Multiplier Parameter on the Animator</summary>
        internal float SpeedMultiplier { get; set; }

        internal int CurrentFrame { get; private set; }

        #endregion 

        #region Gravity
        [SerializeField] private Vector3Reference m_gravityDir = new Vector3Reference(Vector3.down);

        [SerializeField] private FloatReference m_gravityPower = new FloatReference(9.8f);

        [SerializeField] private IntReference m_gravityTime = new IntReference(10);
        [SerializeField] private IntReference m_gravityTimeLimit = new IntReference(0);


        public int StartGravityTime { get => m_gravityTime.Value ; internal set => m_gravityTime.Value = value; }
        public int LimitGravityTime { get => m_gravityTimeLimit.Value ; internal set => m_gravityTimeLimit.Value = value; }

        /// <summary>Multiplier Added to the  Gravity Direction</summary>
        public float GravityMultiplier { get; internal set; }


        public int GravityTime { get; internal set; }
        //{
        //    get => m_GravityTime;
        //    set
        //    {
        //        m_GravityTime = value;
        //        Debug.Log("m_GravityTime    " + m_GravityTime);
        //    }
        //}
        //int m_GravityTime;


        public float GravityPower { get => m_gravityPower.Value * GravityMultiplier; set => m_gravityPower.Value = value; }


        /// <summary>Stored Gravity Velocity when the animal is using Gravity</summary>
        public Vector3 GravityStoredVelocity { get; internal set; }

        /// <summary> Direction of the Gravity </summary>
        public Vector3 Gravity { get => m_gravityDir.Value; set => m_gravityDir.Value = value; }

        /// <summary> Up Vector is the Opposite direction of the Gravity dir</summary>
        public Vector3 UpVector => -m_gravityDir.Value;

        /// <summary>if True the gravity will be the Negative Ground Normal Value</summary>
        public BoolReference ground_Changes_Gravity =  new BoolReference(false);

        #endregion

        #region Advanced Parameters
        public BoolReference rootMotion = new BoolReference(true);

        /// <summary> Raudius for the Sphere Cast</summary>
        public FloatReference rayCastRadius = new FloatReference(0.05f);

        /// <summary>RayCast Radius for the Alignment Raycasting</summary>
        public float RayCastRadius => rayCastRadius.Value + 0.001f;
        /// <summary>This parameter exist to Add Additive pose to correct the animal</summary>
        public IntReference animalType = new IntReference(0);
        #endregion

        #region Use Stuff Properties

      

     

        /// <summary>Does the Active State uses Additive Position Speed?</summary>
        public bool UseAdditivePos// { get; internal set; } 
        {
            get => useAdditivePos; 
            set
            {
                useAdditivePos = value;
               if (!useAdditivePos)  ResetInertiaSpeed(); 
            }
        }
        private bool useAdditivePos;

        /// <summary>Does the Active State uses Additive Position Speed?</summary>
        public bool UseAdditiveRot { get; internal set; }

        /// <summary>Does the Active State uses Sprint?</summary>
        public bool UseSprintState { get; internal set; }
        public bool UseCustomAlign { get; set; }
        /// <summary>The Animal is on Free Movement... which means is flying or swiming underwater</summary>
        public bool FreeMovement { get; set; }
        /// <summary>Enable Disable the Global Sprint</summary>
        public bool UseSprint
        {
            get => useSprintGlobal; 
            set
            { 
                useSprintGlobal.Value = value;
                Sprint = sprint; //Update the Sprint value  IMPORTANT
            }
        }
        /// <summary>Enable Disable the Global Sprint (SAME AS USE SPRINT)</summary>
        public bool CanSprint { get => UseSprint; set => UseSprint = value; }

        /// <summary>Locks Input on the Animal, Ingore inputs like Jumps, Attacks , Actions etc</summary>
        public bool LockInput
        {
            get => lockInput.Value;
            set
            {
                lockInput.Value = value;
                OnInputLocked.Invoke(lockInput);
            }
        }

        /// <summary>Enable/Disable RootMotion on the Animator</summary>
        public bool RootMotion
        {
            get => rootMotion;
            set => Anim.applyRootMotion = rootMotion.Value = value;
        }

        private bool useGravity;
        /// <summary>Does it use Gravity or not? </summary>
        public bool UseGravity
        {
            get => useGravity;
            set
            {
                useGravity = value;

                if (!useGravity) ResetGravityValues();//Reset Gravity Logic when Use gravity is false
              //  Debug.Log("useGravity = " + useGravity);
            }
        }

        /// <summary>Locks the Movement on the Animal</summary>
        public bool LockMovement
        {
            get => lockMovement;
            set
            {
                lockMovement.Value = value;
                OnMovementLocked.Invoke(lockMovement);
            }
        }


        /// <summary>Sets to Zero the Z on the Movement Axis when this is set to true</summary>
        public bool LockForwardMovement { get => lockForwardMovement; set => lockForwardMovement.Value = value; }

        /// <summary>Sets to Zero the X on the Movement Axis when this is set to true</summary>
        public bool LockHorizontalMovement { get => lockHorizontalMovement; set => lockHorizontalMovement.Value = value; }


        /// <summary>Sets to Zero the Y on the Movement Axis when this is set to true</summary>
        public bool LockUpDownMovement { get => lockUpDownMovement; set => lockUpDownMovement.Value = value; }


        /// <summary>if True It will Aling it to the ground rotation depending the Front and Back Pivots</summary>
        public bool UseOrientToGround { get; set; }

        [SerializeField] private BoolReference lockInput = new BoolReference(false);
        [SerializeField] private BoolReference lockMovement = new BoolReference(false);
        [SerializeField] private BoolReference useSprintGlobal = new BoolReference(true);
        #endregion

        #region Animator States Info
        protected AnimatorStateInfo m_CurrentState;             // Information about the base layer of the animator cached.
        protected AnimatorStateInfo m_NextState;
        protected AnimatorStateInfo m_PreviousCurrentState;    // Information about the base layer of the animator from last frame.
        protected AnimatorStateInfo m_PreviousNextState;

        /// <summary> If we are  in any  animator transition </summary>
        internal bool m_IsAnimatorTransitioning;
        protected bool m_PreviousIsAnimatorTransitioning;


        /// <summary>Returns the Current Animation State Tag of animal, if is in transition it will return the NextState Tag</summary>
        public AnimatorStateInfo AnimState { get; set; }

        public int currentAnimTag;
        /// <summary>Current Active Animation Hash Tag </summary>
        public int AnimStateTag
        {
            get => currentAnimTag;
            private set
            {
                if (value != currentAnimTag)
                {
                    currentAnimTag = value;
                    activeState.AnimationTagEnter(value);

                    if (ActiveState.IsPending)                      //If the new Animation Tag is not on the New Active State try to activate it on the last State
                        LastState.AnimationTagEnter(value);
                }
            }
        }
        #endregion

        #region Platform
        public Transform platform;
        protected Vector3 platform_Pos;
        protected Quaternion platform_Rot;
        #endregion

        #region Interacter
        //public bool CanInteract { get => !enabled; set => enabled = !value; }
        //[Tooltip("Interacter ID pass to a Interactable to know who is making the interaction")]
        //[SerializeField] private IntReference m_InteracterID = new IntReference(0);
        ///// <summary>Interaction Trigger needed to interact with external gameObjects</summary>
        //[Tooltip("Interaction Trigger needed to interact with external gameObjects")]
        //public Collider m_InteractionArea;
        ///// <summary>Interaction Trigger Proxy to Subsribe to OnEnter OnExit Trigger</summary>
        //public TriggerProxy TriggerArea;
        //public GameObject Owner => gameObject;
        //public int ID => m_InteracterID.Value;
        ///// <summary>Collider that interacted with the Interaction area</summary>
        //public Collider InteractableCollider { get; internal set; }
        ///// <summary>Interaction Area</summary>
        //public Collider InteractionArea => m_InteractionArea;
        #endregion

        #region Extras
        /// <summary>Used for Disabling Additive Position and Additive Rotation on the ANimal (The Pulling Wagons on the Horse Car  take care of it)</summary>?????
        internal bool DisablePositionRotation = false;

        //[Tooltip("When Falling and the animal get stuck falling, the animal will be force to move forward.")]
        //public FloatReference FallForward = new FloatReference(2);

        protected List<IMDamager> Attack_Triggers;      //List of all the Damage Triggers on this Animal.


        /// <summary>Colliders to disable with animator </summary>
        List<Collider> colliders = new List<Collider>();

        /// <summary>Animator Normalized State Time for the Base Layer  </summary>
        public float StateTime { get; private set; }

        ///// <summary>Store from where the damage came from</summary>
        //public Vector3 HitDirection { set; get; }

        #endregion

        #region Events
        public IntEvent OnAnimationChange;
        public BoolEvent OnInputLocked = new BoolEvent();          //Used for Sync Animators
        public BoolEvent OnMovementLocked = new BoolEvent();        //Used for Sync Animators
        public BoolEvent OnSprintEnabled = new BoolEvent();       //Used for Sync Animators
        public BoolEvent OnGrounded = new BoolEvent();       //Used for Sync Animators
        public BoolEvent OnMovementDetected = new BoolEvent();       //Used for Sync Animators

        public IntEvent OnStateChange = new IntEvent();         //Invoked when is Changed to a new State
        public Int2Event OnModeStart = new Int2Event();          //Invoked when is Changed to a new Mode
        public Int2Event OnModeEnd = new Int2Event();          //Invoked when is Changed to a new Mode
        public IntEvent OnStanceChange = new IntEvent();        //Invoked when is Changed to a new Stance
        public SpeedModifierEvent OnSpeedChange = new SpeedModifierEvent();        //Invoked when a new Speed is changed
        public Vector3Event OnTeleport = new  Vector3Event();        //Invoked when a new Speed is changed
        #endregion

        #region Random
        public int RandomID { get; private set; }

        /// <summary>Let States have Random Animations</summary>
        public bool Randomizer { get; set; }
        #endregion

       

        #region Animator Parameters

        [SerializeField, Tooltip("Forward (Z) Movement for the Animator")] private string m_Vertical = "Vertical";
        [SerializeField, Tooltip("Horizontal (X) Movement for the Animator")] private string m_Horizontal = "Horizontal";
        [SerializeField, Tooltip("Vertical (Y) Movement for the Animator")] private string m_UpDown = "UpDown";

        [SerializeField, Tooltip("Is the animal on the Ground? ")] private string m_Grounded = "Grounded";
        [SerializeField, Tooltip("Is the animal moving?")] private string m_Movement = "Movement";

        [SerializeField, Tooltip("Active/Current State the animal is")]
        private string m_State = "State";
        [SerializeField, Tooltip("The Active State can have multiple status to change inside the State itself")]
        private string m_StateStatus = "StateEnterStatus";
        [SerializeField, Tooltip("The Active State can use this parameter to activate exiting animations")]
        private string m_StateExitStatus = "StateExitStatus";
        [SerializeField, Tooltip("Float value for the States to be used when needed")]
        private string m_StateFloat = "StateFloat";
        [SerializeField, Tooltip("Last State the animal was")]
        private string m_LastState = "LastState";

        [SerializeField, Tooltip("Active State Time for the States Animations")]
        private string m_StateTime = "StateTime";

        [SerializeField, Tooltip("Speed Multiplier for the Animations")]
        private string m_SpeedMultiplier = "SpeedMultiplier";


        [SerializeField, Tooltip("Active Mode the animal is... The Value is the Mode ID plus the Ability Index. Example Action Eat = 4002")]
        private string m_Mode = "Mode";
        [SerializeField, Tooltip("Store the Modes Status (Available=0  Started=1  Looping=-1 Interrupted=-2)")]
        private string m_ModeStatus = "ModeStatus";
        [SerializeField, Tooltip("Mode Float Value, Used to have a float Value for the modes to be used when needed")]
        private string m_ModePower = "ModePower";

        [SerializeField, Tooltip("Active/Current stance of the animal")] private string m_Stance = "Stance";
        [SerializeField, Tooltip("Previus/Last stance of the animal")] private string m_LastStance = "LastStance";
        [SerializeField, Tooltip("Normalized value of the Slope of the Terrain")] private string m_Slope = "Slope";
        [SerializeField, Tooltip("Type of animal for the Additive corrective pose")] private string m_Type = "Type";

        [SerializeField, Tooltip("Random Value for Animations States with multiple animations")] private string m_Random = "Random";
        [SerializeField, Tooltip("Delta Angle Value for Rotating the animal when is using Camera Input")] private string m_DeltaAngle = "DeltaAngle";
        [SerializeField, Tooltip("Does the Animal Uses Strafe")] private string m_Strafe = "Strafe";
        [SerializeField, Tooltip("Horizontal Angle for Strafing.")] private string m_strafeAngle = "StrafeAngle";

        internal int hash_Vertical;
        internal int hash_Horizontal;
        internal int hash_UpDown;

        internal int hash_Movement;
        internal int hash_Grounded;
        internal int hash_SpeedMultiplier;

        internal int hash_DeltaAngle;

        internal int hash_State;
        internal int hash_StateEnterStatus;
        internal int hash_StateExitStatus;
        internal int hash_StateFloat;
        internal int hash_StateTime;
        internal int hash_LastState;

        internal int hash_Mode;
        internal int hash_ModeStatus;
        internal int hash_ModePower;

        internal int hash_Stance;
        internal int hash_LastStance;

        internal int hash_Slope;
        internal int hash_Random;
        internal int hash_Strafe;
        internal int hash_StrafeAngle;
        #endregion
    }

    [System.Serializable]
    public class OnEnterExitState
    {
        public StateID ID;
        public UnityEvent OnEnter;
        public UnityEvent OnExit;
    }

    [System.Serializable]
    public class OnEnterExitStance
    {
        public StanceID ID;
        public UnityEvent OnEnter;
        public UnityEvent OnExit;
    }

    [System.Serializable]
    public class SpeedModifierEvent : UnityEvent<MSpeed> { }
}
