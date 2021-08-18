using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Controller;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.HAP
{
    [AddComponentMenu("Malbers/Riding/Mount")]
    public class Mount : MonoBehaviour, IAnimatorListener
    {
        #region References
        /// <summary>Reference of the Animal</summary>
        [RequiredField] public MAnimal Animal;
      

        #region Mount Point References
        public Transform MountPoint;     // Reference for the RidersLink Bone  
        public Transform MountBase;     // Reference for the RidersLink Bone  
        public Transform FootLeftIK;     // Reference for the LeftFoot correct position on the mount
        public Transform FootRightIK;    // Reference for the RightFoot correct position on the mount
        public Transform KneeLeftIK;     // Reference for the LeftKnee correct position on the mount
        public Transform KneeRightIK;    // Reference for the RightKnee correct position on the mount


        public Transform LeftRein;     // Reference for the LeftRein 
        public Transform RightRein;    // Reference for the RightRein  
        #endregion

        #endregion

        #region General
        /// <summary>Enable Disable the Mount Logic</summary>
        public BoolReference active = new BoolReference(true);
        
        /// <summary></summary>
        [Tooltip("Set the AI when the animal is Mounted")]
        public BoolReference Set_AIMount = new BoolReference(false);

        /// <summary></summary>
        [Tooltip("Set the AI when the animal is Dismounted")]
        public BoolReference Set_AIDismount = new BoolReference(true);


        [Tooltip("Set the Input when the animal is Mounted")]
        public BoolReference Set_InputMount = new BoolReference(true);

        [Tooltip("Set the Input when the animal is Dismounted")]
        public BoolReference set_InputDismount= new BoolReference(false);



        [Tooltip("Set the Mount Triggers the Animal is Mounted")]
        public BoolReference Set_MTriggersMount = new BoolReference(false);

        [Tooltip("Set the Mount Triggers the Animal is Dismounted")]
        public BoolReference Set_MTriggersDismount = new BoolReference(true);

        /// <summary>Works for the ID of the Mount (EX Wagon</summary>
        public IntReference ID;


        /// <summary>if true then it will ignore the Mounting Animations</summary>
        public BoolReference instantMount = new BoolReference(false);
        public string mountIdle = "Idle";

        /// <summary>The Rider can only Mount when the Animal is on any of these states on the list</summary>
        public bool MountOnly;
        /// <summary>The Rider can only Dismount when the Animal is on any of these states on the list</summary>
        public bool DismountOnly;
        /// <summary>The Rider is Forced to dismount if the animal is on any of these states</summary>
        public bool ForceDismount;
        public List<StateID> MountOnlyStates = new List<StateID>();
        public List<StateID> DismountOnlyStates = new List<StateID>();
        public List<StateID> ForceDismountStates = new List<StateID>();


        /// <summary>Reference for the Animator Update Mode</summary>
        public AnimatorUpdateMode DefaultAnimUpdateMode { get; set; }
        #endregion

        /// <summary>Velocity changes for diferent Animation Speeds... used on other animals</summary>
        public List<SpeedTimeMultiplier> SpeedMultipliers;

        #region Straight Mount
        public BoolReference straightSpine;                              //Activate this only for other animals but the horse 
        public BoolReference UseSpeedModifiers;
        public Vector3 pointOffset = new Vector3(0, 0, 3);
        public Vector3 MonturaSpineOffset => StraightSpineOffsetTransform.TransformPoint(pointOffset);

        //public float LowLimit = 45;
        //public float HighLimit = 135;

        public float smoothSM = 0.5f;
        #endregion

        #region Events
        public UnityEvent OnMounted = new UnityEvent();
        public UnityEvent OnDismounted = new UnityEvent();
        public BoolEvent OnCanBeMounted = new BoolEvent();
        #endregion

        #region Properties
        /// <summary>Straighen the Spine bone while mounted depends on the Mount</summary>
        public bool StraightSpine { get => straightSpine; set => straightSpine.Value = value; }


        /// <summary>Straighen the Spine bone while mounted depends on the Mount</summary>
        public Transform StraightSpineOffsetTransform;
        private bool defaultStraightSpine;

        /// <summary>Reference for the Animal Animator</summary>
        public Animator Anim => Animal.Anim;
        /// <summary>Reference for the Animal Input Source</summary>
        public IInputSource MountInput => Animal.InputSource;
        /// <summary>Reference for the AI Animal Control</summary>
        public IAIControl AI { get; internal set; }  

        /// <summary>Input for the Mount</summary>
        public List<MountTriggers> MountTriggers { get; private set; }


        protected bool mounted;
        /// <summary> Is the animal Mounted</summary>
        public bool Mounted
        {
            get => mounted;
            set
            {
                if (value != mounted)
                {
                    mounted = value;
                    
                    if (mounted)
                        OnMounted.Invoke();    //Invoke the Event 
                    else
                        OnDismounted.Invoke();
                }
            }
        }


        /// <summary> Dismount only when the Animal is Still on place </summary>
        public virtual bool CanDismount => Mounted;

        public virtual string MountIdle { get => mountIdle; set => mountIdle = value; }

        /// <summary>Animal Mountable Script 'Enabled/Disabled'</summary>
        public virtual bool CanBeMounted { get => active; set => active.Value = value; }

        /// <summary> The Mount has all the IK Links</summary>
        public bool HasIKFeet =>  FootLeftIK != null &&  FootRightIK != null && KneeLeftIK != null && KneeRightIK != null;  


        /// <summary>If "Mount Only" is enabled, this will capture the State the animal is at, in order to Mount</summary>
        public bool CanBeMountedByState { get; set; }
        /// <summary>If "Mount Only" is enabled, this will capture the State the animal is at, in order to Mount</summary>
        public bool CanBeDismountedByState { get; set; }

        /// <summary>Active Ride the Montura. is setted by the Rider Script </summary>
        public MRider Rider { get; set; }
        
        /// <summary>Rider that is near the Mount</summary>
        public MRider NearbyRider { get; set; }
      
        /// <summary> Ignore Mounting Animations </summary>
        public bool InstantMount { get => instantMount; set => instantMount.Value = value; }
        #endregion

        #region IK Reins
        /// <summary>Left Rein Handle Default Local Position  </summary>
        public Vector3 DefaultLeftReinPos { get; internal set; }
       
        /// <summary>Right Rein Handle Default Local Position  </summary>
        public Vector3 DefaultRightReinPos { get; internal set; }
        #endregion




        public bool debug;

        public void Awake()
        {
            if (Animal == null)
                Animal = this.FindComponent<MAnimal>();

            AI = Animal.FindInterface<IAIControl>();

            MountTriggers = GetComponentsInChildren<MountTriggers>(true).ToList(); //Catche all the MountTriggers of the Mount

            CanBeDismountedByState = CanBeMountedByState = true; //Set as true can be mounted and canbe dismounted by state
            defaultStraightSpine = StraightSpine;
            if (Anim) DefaultAnimUpdateMode = Anim.updateMode;

            if (!StraightSpineOffsetTransform)
            {
                StraightSpineOffsetTransform = transform;
            }


            if (LeftRein && RightRein)
            {
                DefaultLeftReinPos = LeftRein.localPosition;             //Set the Reins Local Values Values
                DefaultRightReinPos = RightRein.localPosition;            //Set the Reins Local Values Values
            }
        }



        void OnEnable()
        {
            Animal?.OnSpeedChange.AddListener(SetAnimatorSpeed);
            Animal?.OnStateChange.AddListener(AnimalStateChange);
        }

        void OnDisable()
        {
            Animal?.OnSpeedChange.RemoveListener(SetAnimatorSpeed);
            Animal?.OnStateChange.RemoveListener(AnimalStateChange);

            if (NearbyRider)  NearbyRider.MountTriggerExit();
        }

        /// <summary>Enable the Input for the Mount</summary>
        public virtual void EnableInput(bool value)
        {
            MountInput?.Enable(value);
            Animal.StopMoving();
        }


        public void ResetRightRein()
        {
            if (RightRein) RightRein.localPosition = DefaultRightReinPos;
        }

        public void ResetLeftRein()
        {
            if (LeftRein) LeftRein.localPosition = DefaultLeftReinPos;
        }

        public virtual void StartMounting(MRider rider)
        {
            Mounted = true;         //Set Mounting to true
            Rider = rider;          //Send to the Montura that it has a rider

            Set_MountTriggers(Set_MTriggersMount.Value);   //Disable all Mount Trigger to avoid ON Enter ON Exit Trigger Events**
        }


        public void End_Mounting()
        {
            EnableInput(Set_InputMount.Value);                //Enable Animal Controls
            AI?.SetActive(Set_AIMount.Value);                 //Set the AI Value

            SetAnimatorSpeed(Animal.currentSpeedModifier); //Update the Speed Modifier /Rider and Animal
        }

        public virtual void Start_Dismounting()
        {
            Mounted = false;
            EnableInput(set_InputDismount.Value); //Enable/Disable Animal Controls

            Animal.Mode_Interrupt();

            ResetLeftRein();
            ResetRightRein();
        }

        public virtual void EndDismounting()
        {
            Set_MountTriggers(Set_MTriggersDismount.Value);                            //Enable all Mount Triggers
            AI?.SetActive(Set_AIDismount.Value); //Reactivate the AI.
            Rider = null;
        }
       

        /// <summary>Used for Aiming while on the horse.... Straight Spine needs to be pause </summary>
        public void PauseStraightSpine(bool value) => StraightSpine = !value && defaultStraightSpine;

        /// <summary>Enable/Disable Mount Triggers</summary>
        public void Set_MountTriggers(bool value)
        {
            foreach (var mt in MountTriggers)
            {
                mt.gameObject.SetActive(value);
                mt.WasAutomounted = true;
            }

            if ( !value) ExitMountTrigger();
        }

        /// <summary>Reset All Values when Exiting Mount Triggers</summary>
        public void ExitMountTrigger()
        {
            OnCanBeMounted.Invoke(false);
            NearbyRider = null;

            foreach (var mt in MountTriggers)
                mt.WasAutomounted = false; //Reset Automounted on the Mount Triggers
        }

        private void AnimalStateChange(int StateID)
        {
            var ActiveState = Animal.ActiveStateID;

            if (MountOnly)
            {
                CanBeMountedByState = MountOnlyStates.Contains(ActiveState);   //Set MountOnly by State
            }

            if (DismountOnly)
            {
                CanBeDismountedByState = DismountOnlyStates.Contains(ActiveState);   //Set DimountOnly by State
            }

            if (Rider)
            {
                Rider.UpdateCanMountDismount();

                if (ForceDismount) //Means the Rider is forced to dismount
                {
                    if (ForceDismountStates.Contains(ActiveState))
                        Rider.ForceDismount();
                }
            }
        }
         
        /// <summary>Align the Animator Speed of the Mount  with the Rider Speed</summary>
        private void SetAnimatorSpeed(MSpeed SpeedModifier)
        {
            if (!Rider || !Rider.IsRiding) return;                            //if there's No Rider Skip

            if (UseSpeedModifiers)
            {
                var speed = SpeedMultipliers.Find(s => s.name == SpeedModifier.name); //Find the Curren Animal Speed

                float TargetAnimSpeed = speed != null ? speed.AnimSpeed * SpeedModifier.animator * Animal.AnimatorSpeed : 1f;

                Rider.TargetSpeedMultiplier = TargetAnimSpeed;
            }
        }

        /// <summary>Enable/Disable the StraightMount Feature </summary>
        public virtual void StraightMount(bool value) => StraightSpine = value;

        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);

        [HideInInspector] public int Editor_Tabs1;
        [HideInInspector] public int Editor_Tabs2;
      


#if UNITY_EDITOR
        private void Reset()
        {
            Animal = GetComponent<MAnimal>();
            StraightSpineOffsetTransform = transform;

            MEvent RiderMountUIE = MTools.GetInstance<MEvent>("Rider Mount UI");

            if (RiderMountUIE != null)
            {
                UnityEditor.Events.UnityEventTools.AddObjectPersistentListener<Transform>(OnCanBeMounted, RiderMountUIE.Invoke, transform);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(OnCanBeMounted, RiderMountUIE.Invoke);
            }
        }

        /// <summary> Debug Options </summary>
        void OnDrawGizmos()
        {
            if (!debug) return;

            Gizmos.color = Color.red;
            if (StraightSpineOffsetTransform)
            {
                Gizmos.DrawSphere(MonturaSpineOffset, 0.125f);
            }
            else
            {
                StraightSpineOffsetTransform = transform;
            }
        }
#endif
    }

    [System.Serializable]
    public class SpeedTimeMultiplier
    {
        /// <summary>Name of the Speed the on the animal to apply the AnimSpeed</summary>
        public string name = "SpeedName";

        /// <summary>Speed Modifier multiplier for the Rider</summary>
        public float AnimSpeed = 1f;
    }

    #region INSPECTOR
#if UNITY_EDITOR

    [CanEditMultipleObjects, CustomEditor(typeof(Mount))]
    public class MountEd : Editor
    {
        bool helpUseSpeeds;
        bool helpEvents;
        Mount M;

        SerializedProperty
            UseSpeedModifiers, MountOnly, DismountOnly, active, mountIdle, instantMount, straightSpine, ID, StraightSpineOffsetTransform,
           pointOffset, Animal, smoothSM, mountPoint, rightIK, rightKnee, leftIK, leftKnee, SpeedMultipliers,
            OnMounted, Editor_Tabs1, Editor_Tabs2, OnDismounted, OnCanBeMounted, MountOnlyStates, DismountOnlyStates, MountBase,

            ForceDismountStates, ForceDismount, debug,

            LeftRein, RightRein,

            Set_AIMount, Set_InputMount, Set_MTriggersMount,
            Set_AIDismount, Set_InputDismount, Set_MTriggersDismount
            ;


        private void OnEnable()
        {
            M = (Mount)target;

            UseSpeedModifiers = serializedObject.FindProperty("UseSpeedModifiers");
            //syncAnimators = serializedObject.FindProperty("syncAnimators");
            Animal = serializedObject.FindProperty("Animal");
            // ShowLinks = serializedObject.FindProperty("ShowLinks");
            debug = serializedObject.FindProperty("debug");
            ID = serializedObject.FindProperty("ID");


            LeftRein = serializedObject.FindProperty("LeftRein");
            RightRein = serializedObject.FindProperty("RightRein");
        

            MountOnly = serializedObject.FindProperty("MountOnly");
            DismountOnly = serializedObject.FindProperty("DismountOnly");
            active = serializedObject.FindProperty("active");
            mountIdle = serializedObject.FindProperty("mountIdle");
            instantMount = serializedObject.FindProperty("instantMount");
            straightSpine = serializedObject.FindProperty("straightSpine");
          

            smoothSM = serializedObject.FindProperty("smoothSM");

            mountPoint = serializedObject.FindProperty("MountPoint");
            MountBase = serializedObject.FindProperty("MountBase");
            rightIK = serializedObject.FindProperty("FootRightIK");
            rightKnee = serializedObject.FindProperty("KneeRightIK");
            leftIK = serializedObject.FindProperty("FootLeftIK");
            leftKnee = serializedObject.FindProperty("KneeLeftIK");

            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            Editor_Tabs2 = serializedObject.FindProperty("Editor_Tabs2");

            SpeedMultipliers = serializedObject.FindProperty("SpeedMultipliers");
            //  DebugSync = serializedObject.FindProperty("DebugSync");
            OnMounted = serializedObject.FindProperty("OnMounted");
            pointOffset = serializedObject.FindProperty("pointOffset");
            StraightSpineOffsetTransform = serializedObject.FindProperty("StraightSpineOffsetTransform");

            OnDismounted = serializedObject.FindProperty("OnDismounted");
            OnCanBeMounted = serializedObject.FindProperty("OnCanBeMounted");
            MountOnlyStates = serializedObject.FindProperty("MountOnlyStates");
            DismountOnlyStates = serializedObject.FindProperty("DismountOnlyStates");

            ForceDismountStates = serializedObject.FindProperty("ForceDismountStates");
            ForceDismount = serializedObject.FindProperty("ForceDismount");

            Set_MTriggersMount = serializedObject.FindProperty("Set_MTriggersMount");
            Set_AIMount = serializedObject.FindProperty("Set_AIMount");
            Set_InputMount = serializedObject.FindProperty("Set_InputMount");

            Set_MTriggersDismount = serializedObject.FindProperty("Set_MTriggersDismount");
            Set_AIDismount = serializedObject.FindProperty("Set_AIDismount");
            Set_InputDismount = serializedObject.FindProperty("set_InputDismount");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Makes this animal mountable. Requires Mount Triggers and Moint Points");

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                EditorGUI.BeginChangeCheck();
                {
                    Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, new string[] { "General", "Links", "Custom Mount" });
                    if (Editor_Tabs1.intValue != 3) Editor_Tabs2.intValue = 3;

                    Editor_Tabs2.intValue = GUILayout.Toolbar(Editor_Tabs2.intValue, new string[] { "M/D States", "Events", "Debug" });
                    if (Editor_Tabs2.intValue != 3) Editor_Tabs1.intValue = 3;


                    //First Tabs
                    int Selection = Editor_Tabs1.intValue;

                    if (Selection == 0) ShowGeneral();
                    else if (Selection == 1) ShowLinks();
                    else if (Selection == 2) ShowCustom();

                    //2nd Tabs
                    Selection = Editor_Tabs2.intValue;

                    if (Selection == 0) ShowStates();
                    else if (Selection == 1) ShowEvents();
                    else if (Selection == 2) ShowDebug();
                }

                EditorGUILayout.EndVertical();


                if (M.MountPoint == null)
                {
                    EditorGUILayout.HelpBox("'Mount Point'  is empty, please set a reference", MessageType.Warning);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Mount Inspector");
                //EditorUtility.SetDirty(target);
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void ShowDebug()
        {
            //EditorGUILayout.PropertyField(debug);

            if (Application.isPlaying)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField("Current Rider", M.Rider, typeof(MRider), false);
                EditorGUILayout.ObjectField("Nearby Rider", M.NearbyRider, typeof(MRider), false);
                EditorGUILayout.Space();
                EditorGUILayout.ToggleLeft("Mounted/Can Dismount", M.Mounted);
                EditorGUILayout.ToggleLeft("Can Be Mounted by State", M.CanBeDismountedByState);
                EditorGUILayout.ToggleLeft("Can Be Mounted", M.CanBeMounted);
                EditorGUILayout.ToggleLeft("Straight Spine", M.StraightSpine);
                Repaint();
                EditorGUI.EndDisabledGroup();
            }
        }


        private void ShowEvents()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
                    helpEvents = GUILayout.Toggle(helpEvents, "?", EditorStyles.miniButton, GUILayout.Width(18));
                }
                EditorGUILayout.EndHorizontal();
                if (helpEvents) EditorGUILayout.HelpBox("On Mounted: Invoked when the rider start to mount the animal\nOn Dismounted: Invoked when the rider start to dismount the animal\nInvoked when the Mountable has an available Rider Nearby", MessageType.None);

                EditorGUILayout.PropertyField(OnMounted);
                EditorGUILayout.PropertyField(OnDismounted);
                EditorGUILayout.PropertyField(OnCanBeMounted);
            }
            EditorGUILayout.EndVertical();
        }

        private void ShowStates()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Mount/Dismount States", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(MountOnly, new GUIContent("Mount Only", "The Rider can only Mount when the Animal is on any of these states"));

                if (MountOnly.boolValue) MalbersEditor.Arrays(MountOnlyStates);

                EditorGUILayout.PropertyField(DismountOnly, new GUIContent("Dismount Only", "The Rider can only Dismount when the Animal is on any of these states"));

                if (DismountOnly.boolValue) MalbersEditor.Arrays(DismountOnlyStates);


                EditorGUILayout.PropertyField(ForceDismount, new GUIContent("Force Dismount", "The Rider is forced to dismount when the Animal is on any of these states"));

                if (ForceDismount.boolValue) MalbersEditor.Arrays(ForceDismountStates);
            }
            EditorGUILayout.EndVertical();
        }

        private void ShowCustom()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(straightSpine, new GUIContent("Straight Spine", "Straighten the Mount Point to fix the Rider Animation"));

                if (M.StraightSpine)
                {
                    EditorGUILayout.PropertyField(StraightSpineOffsetTransform, new GUIContent("Transf Ref", "Transform to use for the Point Offset Calculation"));
                    EditorGUILayout.PropertyField(pointOffset, new GUIContent("Point Offset", "Point in front of the Mount to Straight the Spine of the Rider"));
                    EditorGUILayout.PropertyField(smoothSM, new GUIContent("Smoothness", "Smooth changes between the rotation and the straight Mount"));
                }
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PropertyField(UseSpeedModifiers, new GUIContent("Animator Speeds", "Use this for other animals but the horse"));
                    helpUseSpeeds = GUILayout.Toggle(helpUseSpeeds, "?", EditorStyles.miniButton, GUILayout.Width(18));
                }
                EditorGUILayout.EndHorizontal();

                if (M.UseSpeedModifiers)
                {
                    if (helpUseSpeeds) EditorGUILayout.HelpBox("Changes the Speed on the Rider's Animator to Sync with the Animal Animator.\nThe Original Riding Animations are meant for the Horse. Only change the Speeds for other creatures", MessageType.None);
                    MalbersEditor.Arrays(SpeedMultipliers, new GUIContent("Animator Speed Multipliers", "Velocity changes for diferent Animation Speeds... used on other animals"));
                }
            }
            EditorGUILayout.EndVertical();

        }

        private void ShowLinks()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.HelpBox("'Mount Point' is obligatory, the rest are optional", MessageType.None);

                EditorGUILayout.PropertyField(MountBase, new GUIContent("Mount Base", "Reference for the Mount Base, Parent of the Mount Point, used for Straight movement for the mount"));
                EditorGUILayout.PropertyField(mountPoint, new GUIContent("Mount Point", "Reference for the Mount Point"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(rightIK, new GUIContent("Right Foot", "Reference for the Right Foot correct position on the mount"));
                EditorGUILayout.PropertyField(rightKnee, new GUIContent("Right Knee", "Reference for the Right Knee correct position on the mount"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(leftIK, new GUIContent("Left Foot", "Reference for the Left Foot correct position on the mount"));
                EditorGUILayout.PropertyField(leftKnee, new GUIContent("Left Knee", "Reference for the Left Knee correct position on the mount"));
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Reins [Optional]", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(LeftRein, new GUIContent("Left Rein Point", "Reference for the Left Rein, to parent it to the Rider Left Hand while mounting"));
                EditorGUILayout.PropertyField(RightRein, new GUIContent("Right Rein Point","Reference for the Right Rein, to parent it to the Rider Right Hand while mounting"));
            }
            EditorGUILayout.EndVertical();
        }

        private void ShowGeneral()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(active, new GUIContent("Active", "If the animal can be mounted. Deactivate if the mount is death or destroyed or is not ready to be mountable"));
                MalbersEditor.DrawDebugIcon(debug);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(Animal, new GUIContent("Animal", "Animal Reference for the Mounting System"));
                EditorGUILayout.PropertyField(ID, new GUIContent("ID", "Default should be 0.... change this and the Stance parameter on the Rider will change to that value... allowing other types of mounts like Wagon"));
                EditorGUILayout.PropertyField(instantMount, new GUIContent("Instant Mount", "Ignores the Mounting Animations"));
                EditorGUILayout.PropertyField(mountIdle, new GUIContent("Mount Idle", "Animation to Play directly when instant mount is enabled"));
            }
            EditorGUILayout.EndVertical(); 


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Set values on Mounted",EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(Set_InputMount, new GUIContent("Mount Input"));
                EditorGUILayout.PropertyField(Set_AIMount, new GUIContent("Mount AI"));
                EditorGUILayout.PropertyField(Set_MTriggersMount, new GUIContent("Mount Triggers"));
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Set values on Dismounted", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(Set_InputDismount, new GUIContent("Mount Input"));
                EditorGUILayout.PropertyField(Set_AIDismount, new GUIContent("Mount AI"));
                EditorGUILayout.PropertyField(Set_MTriggersDismount, new GUIContent("Mount Triggers"));
            }
            EditorGUILayout.EndVertical();
        }
    }



    [CustomPropertyDrawer(typeof(SpeedTimeMultiplier))]
    public class SpeedTimeMultiplierDrawer : PropertyDrawer
    {
        // Use this for initialization
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            label = EditorGUI.BeginProperty(position, label, property);
            // position = EditorGUI.PrefixLabel(position, label);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginChangeCheck();

            var name = property.FindPropertyRelative("name");
            var AnimSpeed = property.FindPropertyRelative("AnimSpeed");
            var height = EditorGUIUtility.singleLineHeight;
            var line = position;
            line.height = height;

            //line.x += 4;
            //line.width -= 8;


            var MainRect = new Rect(line.x, line.y, line.width / 2, height);
            var lerpRect = new Rect(line.x + line.width / 2, line.y, line.width / 2, height);

            EditorGUIUtility.labelWidth = 45f;
            EditorGUI.PropertyField(MainRect, name, new GUIContent("Name", "Name of the Speed to modify for the Rider"));
            EditorGUIUtility.labelWidth = 75f;
            EditorGUI.PropertyField(lerpRect, AnimSpeed, new GUIContent(" Speed Mult", "Anim Speed Multiplier"));
            if (name.stringValue == string.Empty) name.stringValue = "SpeedName";
            EditorGUIUtility.labelWidth = 0;

            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
#endif

    #endregion
}