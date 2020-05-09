using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using MalbersAnimations.Events;
using MalbersAnimations.Utilities;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Controller;
using System.Collections;
using System.Linq;

namespace MalbersAnimations.HAP
{
    public enum DismountType { Random, Input, Last }
    [AddComponentMenu("Malbers/Riding/Rider")]
    public class MRider : MonoBehaviour, IAnimatorListener
    {
        #region#region Public Variables
        /// <summary>Parent to mount Point </summary>
        public BoolReference Parent = new BoolReference(true);

        /// <summary>Changes the Dismount animation on the Rider</summary>
        public DismountType DismountType = DismountType.Random;

        /// <summary>Mounted Layer Path</summary>
        public string LayerPath = "Layers/Mount v2";
        /// <summary>Mounted Layer Name</summary>
        public string MountLayer = "Mounted";


        /// <summary>Type to Update to set Everyframe the position and rotation of the rider to the Animal Mount Point</summary>
        [Utilities.Flag("Update Type")]
        public UpdateMode LinkUpdate = UpdateMode.Update | UpdateMode.FixedUpdate;

        /// <summary>This animal is the one that you can call or StartMount </summary>
        public Mount MountStored;

        /// <summary>True iff we want to start mounted an animal </summary>
        public BoolReference StartMounted;

        /// <summary>Time to Align to the Mount Trigger Position while is playing the Mount Animation</summary>
        public FloatReference AlingMountTrigger = new FloatReference(0.2f);

        public bool debug;

        #region Call Animal
        public AudioClip CallAnimalA;
        public AudioClip StopAnimalA;
        public AudioSource RiderAudio;
        /// <summary>Call for  the animal, True: Calls The Animal, False: if the animal was calling then stop its movement</summary>
        public bool ToggleCall { get; set; }
        #endregion

        #region ExtraCollider
        public bool CreateColliderMounted;
        public float Col_radius = 0.225f;
        public bool Col_Trigger = true;
        public float Col_height = 0.8f;
        public float Col_Center = 1.2f;
        #endregion

        #region UnityEvents

        public GameObjectEvent OnFindMount = new GameObjectEvent();
        public BoolEvent OnCanMount = new BoolEvent();
        public BoolEvent OnCanDismount = new BoolEvent();
        public BoolEvent CanCallMount = new BoolEvent();

        public UnityEvent OnStartMounting = new UnityEvent();
        public UnityEvent OnEndMounting = new UnityEvent();
        public UnityEvent OnStartDismounting = new UnityEvent();
        public UnityEvent OnEndDismounting = new UnityEvent();
        public UnityEvent OnAlreadyMounted = new UnityEvent();

        #endregion

        public BoolReference DisableComponents;
        public Behaviour[] DisableList;
        #endregion



        #region Auto Properties

        /// <summary>Montura stored when the Riders enters a MountTrigger</summary>
        public Mount Montura { get; private set; }

        /// <summary>Does the Mount have AI</summary>
        public IAIControl MonturaAI { get; private set; }

        /// <summary>Animal Input Script</summary>
        public virtual IInputSource MountInput { get; private set; }

        /// <summary> If Null means that we are NOT Near to an Animal</summary>
        public MountTriggers MountTrigger { get; private set; }

        /// <summary> Check if can mount an Animal </summary>
        public bool CanMount { get; private set; }
        /// <summary>Check if we can dismount the Animal</summary>
        public bool CanDismount { get; private set; }

        private bool mounted;
        /// <summary>True: Rider starts Mounting. False: Rider starts Dismounting This value goes to the Animator </summary>
        public bool Mounted
        {
            get { return mounted; }
            private set
            {
                mounted = value;

                try
                {
                    Anim.SetBool(Hash.Mount, Mounted);                           //Update Mount Parameter on the Animator
                }
                catch { }
            }
        }
        /// <summary>This is true (Finish Mounting) False (Finish Dismounting)</summary>
        public bool IsOnHorse { get; private set; }

        /// <summary>Check if we can call the Animal</summary>
        public bool CanCallAnimal { get; private set; }

        /// <summary> Speed Multiplier for the Speeds Changes while using other Animals</summary>
        public float SpeedMultiplier { get; set; }
        public float TargetSpeedMultiplier { get; set; }

        public bool ForceLateUpdateLink { get; set; }

        /// <summary> Store all the MonoBehaviours on this GameObject</summary>
        protected MonoBehaviour[] AllComponents { get; private set; }
        #endregion


        #region IK VARIABLES    
        protected float L_IKFootWeight = 0f;        //IK Weight for Left Foot
        protected float R_IKFootWeight = 0f;        //IK Weight for Right Foot
        #endregion

        internal int MountLayerIndex = -1;                    //Mount Layer Index
        protected AnimatorUpdateMode Default_Anim_UpdateMode;

        #region Properties
        /// <summary>Returns true if the Rider is on the horse and Mount animations are finished</summary>
        public bool IsRiding => IsOnHorse && Mounted;

        /// <summary>Returns true if the Rider is from the Start of the Mount to the End of the Dismount</summary>
        public bool IsMountingDismounting => IsOnHorse || Mounted;

        /// <summary>Returns true if the Rider is between the Start and the End of the Mount Animations</summary>
        public bool IsMounting => !IsOnHorse && Mounted;

        /// <summary>Returns true if the Rider is between the Start and the End of the Dismount Animations</summary>
        public bool IsDismounting => IsOnHorse && !Mounted;


        #region private vars
        /// <summary>Straight Spine Weight for the Bones</summary>
        protected float SP_Weight;
        // protected float SAim_Weight;
        protected RigidbodyConstraints DefaultConstraints;
        #region Re-Sync with Horse
        //Used this for Sync Animators
        private float RiderNormalizedTime;
        private float HorseNormalizedTime;
        private float LastSyncTime;
        private bool syncronize;
        #endregion
        #endregion

        #region References

        [SerializeField] private Animator animator;

        /// <summary>Reference for the Animator </summary>
        public Animator Anim { get => animator; private set => animator = value; }  //Reference for the Animator 
        /// <summary>Reference for the rigidbody</summary>
        protected Rigidbody RB { get; private set; }//Reference for this rigidbody
        #region Bones
        /// <summary>Spine Bone Transform</summary>
        public Transform Spine { get; private set; }
        //  public Transform Hips { get; private set; }
        public Transform Chest { get; private set; }

        /// <summary>Set from the Rider Combat that the Rider is Aiming</summary>
        public bool CombatAim { get; internal set; }


        #endregion


        /// <summary>Reference for all the colliders on this gameObject</summary>
        protected List<Collider> colliders;
        /// <summary>Reference for this transform</summary>
        protected Transform t;
        protected CapsuleCollider mountedCollider;  //For creating a collider when is mounted for Hit Porpouse
        #endregion

        #endregion

        void Awake()
        {
            t = transform;
            Anim = GetComponentInChildren<Animator>();
            RB = GetComponentInChildren<Rigidbody>();
            colliders = GetComponentsInChildren<Collider>().ToList();

            var CleanCol = new List<Collider>();

            foreach (var col in colliders)
            {
                if (col.enabled && !col.isTrigger)
                {
                    CleanCol.Add(col);
                }
            }
            colliders = new List<Collider>(CleanCol);

            if (Anim == null)
            {
                Anim = GetComponentInParent<Animator>() ?? GetComponentInChildren<Animator>();
            }
           
            RB = GetComponentInParent<Rigidbody>();

            if (Anim)
            {
                MountLayerIndex = Anim.GetLayerIndex(MountLayer);

                Spine = Anim.GetBoneTransform(HumanBodyBones.Spine);                   //Get the Rider Spine transform
                //Hips = Anim.GetBoneTransform(HumanBodyBones.Hips);                   //Get the Rider Hips transform
                Chest = Anim.GetBoneTransform(HumanBodyBones.Chest);                   //Get the Rider Chest transform

                Default_Anim_UpdateMode = Anim.updateMode;             //Gets the Update Mode of the Animator to restore later when dismounted.
            }


            SpeedMultiplier = 1f;
            TargetSpeedMultiplier = 1f;
        }

        void Start()
        {
            IsOnHorse = Mounted = false;
            ForceLateUpdateLink = false;


            if ((int)LinkUpdate == 0 || !Parent)
                LinkUpdate = UpdateMode.FixedUpdate | UpdateMode.LateUpdate;

            if (StartMounted) 
                Start_Mounted();                         //Set All if Started Mounted is Active   


            UpdateCanMountDismount();
        }

        void Update()
        {
            if (IsRiding) WhileIsMounted();                                       //Run Stuff While Mounted

            if ((LinkUpdate & UpdateMode.Update) == UpdateMode.Update) UpdateRiderTransform();
        }

        private void LateUpdate()
        {
            if ((LinkUpdate & UpdateMode.LateUpdate) == UpdateMode.LateUpdate || ForceLateUpdateLink) UpdateRiderTransform();
        }

        private void FixedUpdate()
        {
            if ((LinkUpdate & UpdateMode.FixedUpdate) == UpdateMode.FixedUpdate) UpdateRiderTransform();
        }

        public virtual void MountAnimal()
        {
            if (!CanMount) return;

            Anim?.SetLayerWeight(MountLayerIndex, 1);                     //Enable the Mounting layer  

            if (!Montura.InstantMount)                                                  //If is instant Mount play it      
            {
                Anim?.Play(MountTrigger.MountAnimation, MountLayerIndex);      //Play the Mounting Animations
            }
            else
            {
                Anim?.Play(Montura.MountIdle, MountLayerIndex);                //Ingore the Mounting Animations
                Anim?.Update(Time.fixedDeltaTime);                          //Update the Animator ????

                Start_Mounting();
                End_Mounting();
            }
        }

        public virtual void DismountAnimal()
        {
            if (!CanDismount) return;

            Montura.Mounted = Mounted = false;                                  //Unmount the Animal

            MountTriggers MTrigger = GetDismountTrigger();

            foreach (var mt in Montura.MountTriggers)
                if (mt.AutoMount) mt.WasAutomounted = true;                 //Set to all the Auto Mounted Triggers that it dismounting


            Anim.SetInteger(Hash.MountSide, MTrigger.DismountID);           //Update MountSide Parameter In the Animator

            if (Montura.InstantMount)                                       //Use for Instant mount
            {
                Anim.Play(Hash.Empty, MountLayerIndex);
                Anim.SetInteger(Hash.MountSide, 0);                          //Update MountSide Parameter In the Animator

                Start_Dismounting();
                End_Dismounting();

                t.position = MTrigger.transform.position + (MTrigger.transform.forward * -0.2f);   //Move the rider directly to the mounttrigger
            }
        }


        /// <summary>Return the Correct Mount Trigger using the DismountType</summary>
        private MountTriggers GetDismountTrigger()
        {
            switch (DismountType)
            {
                case DismountType.Last:
                    return MountTrigger;
                case DismountType.Input:
                    var MoveInput = Montura.Animal.MovementAxis;

                    MountTriggers close = MountTrigger;

                    float Diference = Vector3.Angle(MountTrigger.Direction, MoveInput);

                    foreach (var mt in Montura.MountTriggers)
                    {
                        var newDiff = Vector3.Angle(mt.Direction, MoveInput);

                        if (newDiff < Diference)
                        {
                            Diference = newDiff;
                            close = mt;
                        }
                    }

                    return close;

                case DismountType.Random:
                    int Randomindex = Random.Range(0, Montura.MountTriggers.Count);
                    return Montura.MountTriggers[Randomindex];
                default:
                    return MountTrigger;
            }
        }

        protected virtual void WhileIsMounted()
        {
            Animators_ReSync();                                         //Check the syncronization and fix it if is offset***
            SyncAnimator();
        }

        /// <summary>Set all the correct atributes and variables to Start Mounted on the next frame</summary>
        public void Start_Mounted()
        {
            if (MountStored)
            {
                Montura = MountStored;
                MonturaAI = Montura.GetComponent<IAIControl>();

                StopMountAI();

                Montura.Rider = this;

                if (MountTrigger == null)
                    Montura.transform.GetComponentInChildren<MountTriggers>(); //Save the first Mount trigger you found

                Start_Mounting();
                End_Mounting();

                Anim?.Play(Montura.MountIdle, MountLayerIndex);               //Play Mount Idle Animation Directly

                Montura.Mounted = Mounted = true;                                     //Send to the animalMount that mounted is active

                OnAlreadyMounted.Invoke();

                UpdateRiderTransform();
            }
        }

        public virtual void StopMountAI()
        {
            if (Montura.MountInput != null && MonturaAI != null)
            {
                MonturaAI.Stop();
                MonturaAI.SetTarget(null);
            }
        }

        /// <summary>Force the Rider to Dismount</summary>
        public virtual void ForceDismount()
        {
            DisconectRiderAnims();
            Anim?.Play(Hash.Empty, MountLayerIndex);
            Anim?.SetInteger(Hash.MountSide, 0);                           //Update MountSide Parameter In the Animator
            Start_Dismounting();
            End_Dismounting();
        }

        /// <summary>CallBack at the Start of the Mount Animations</summary>
        internal virtual void Start_Mounting()
        {
            Montura.Rider = this;                                   //Send to the Montura that it has a rider
            Montura.Mounted = Mounted = true;                       //Sync Mounted Values in Animal and Rider
            MountInput = Montura.GetComponent<IInputSource>();      //Get the Input of the controller

            StopMountAI();

            if (RB)                                                 //Deactivate stuffs for the Rider's Rigid Body
            {
                RB.useGravity = false;
                // RB.isKinematic = true;
                DefaultConstraints = RB.constraints;                //Store the Contraints before mounting
                RB.constraints = RigidbodyConstraints.FreezeAll;
            }

            ToogleColliders(false);                         //Deactivate All Colliders on the Rider IMPORTANT ... or the Rider will try to push the animal

            ToggleCall = false;                              //Set the Call to Stop Animal
            CallAnimal(false);                              //If is there an animal following us stop him

            MountStored = Montura;                          //Store the last animal you mounted

            if (Parent) transform.parent = Montura.MountPoint;

            if (!MountTrigger)
                MountTrigger = Montura.GetComponentInChildren<MountTriggers>();         //If null add the first mount trigger found

            if (DisableComponents)
            {
                ToggleComponents(false);                                                //Disable all Monobehaviours breaking the Riding System
            }

            OnStartMounting.Invoke();                                                   //Invoke UnityEvent for  Start Mounting

            Anim?.SetLayerWeight(MountLayerIndex, 1);                                   //Enable Mount Layer set the weight to 1
            
            if (!Anim) End_Mounting();                                                  //If is there no Animator  execute the End_Dismounting part

            UpdateCanMountDismount();
        }

        /// <summary>CallBack at the End of the Mount Animations </summary>
        internal virtual void End_Mounting()
        {
            Montura.Mounted = Mounted = IsOnHorse = true;                              //Sync Mounted Values in Animal and Rider again Double Check

            if (Parent)
            {
                transform.localPosition = Vector3.zero;                                    //Reset Position when PARENTED
                transform.localRotation = Quaternion.identity;                             //Reset Rotation when PARENTED
            }

            MountInput?.Enable(true);

            Montura.EnableInput(true);                                              //Enable Animal Controls

            if (CreateColliderMounted) MountingCollider(true);

            if (Anim)
            {
                Anim.updateMode = Montura.Anim.updateMode;                       //Use the Same UpdateMode from the Animal
                                                                                 // Anim.updateMode = AnimatorUpdateMode.Normal;                       //Use the Same UpdateMode from the Animal
                Anim.SetBool(Montura.Animal.hash_Grounded, Montura.Animal.Grounded);
                Anim.SetInteger(Montura.Animal.hash_State, Montura.Animal.ActiveStateID);
                Anim.SetInteger(Montura.Animal.hash_Mode, Montura.Animal.ModeAbility);

                Montura.Animal.OnGrounded.AddListener(AnimalGrounded);
                Montura.Animal.OnStateChange.AddListener(AnimalState);
                Montura.Animal.OnModeStart.AddListener(AnimalMode);


                Montura.Animal.OnStanceChange.AddListener(AnimalStance);

                Anim.SetInteger(Montura.Animal.hash_Stance, Montura.ID);
            }
            OnEndMounting.Invoke();

            UpdateCanMountDismount();
        }


        /// <summary> CallBack at the Start of the Dismount Animations</summary>
        internal virtual void Start_Dismounting()
        {
            if (CreateColliderMounted) MountingCollider(false);                    //Remove MountCollider

            transform.parent = null;

            Montura.Mounted = Mounted = false;                                      //Disable Mounted on everyone
            Montura.EnableInput(false);                                              //Disable Montura Controls

            OnStartDismounting.Invoke();

            if (Anim)
            {
                Anim.updateMode = Default_Anim_UpdateMode;                               //Restore Update mode to its original
                DisconectRiderAnims();
            }
            else
            {
                End_Dismounting();
            }

            UpdateCanMountDismount();
        }

        /// <summary>Disconnect the Animal Events from the Riders Methods (Grounded, State, Mode)</summary>
        void DisconectRiderAnims()
        {
            Montura.Animal.OnGrounded.RemoveListener(AnimalGrounded);
            Montura.Animal.OnStateChange.RemoveListener(AnimalState);
            Montura.Animal.OnModeStart.RemoveListener(AnimalMode);
            Montura.Animal.OnStanceChange.RemoveListener(AnimalStance);
        }

        /// <summary>CallBack at the End of the Dismount Animations</summary>
        internal virtual void End_Dismounting()
        {
            IsOnHorse = false;                                                              //Is no longer on the Animal

            if (Montura)
            {
                Montura.EnableInput(false);                                              //Disable Montura Controls
                Montura.Rider = null;
                Montura = null;                                                                 //Reset the Montura
                MonturaAI = null;
            }

            ToggleCall = false;                                                             //Reset the Call Animal

            if (RB)                                                                          //Reactivate stuffs for the Rider's Rigid Body
            {
                RB.useGravity = true;
                // RB.isKinematic = false;
                RB.constraints = DefaultConstraints;
            }

            if (Anim)
            {
                if (MountLayerIndex != -1)
                    Anim.SetLayerWeight(MountLayerIndex, 0);                                //Reset the Layer Weight to 0 when end dismounting

                Anim.speed = 1;                                                             //Reset AnimatorSpeed

                MalbersTools.ResetFloatParameters(Anim);
            }

            t.rotation = Quaternion.FromToRotation(t.up, -Physics.gravity) * t.rotation;    //Reset the Up Vector; ****IMPORTANT when  CHANGE THE GRAVIY

            ToogleColliders(true);                                                          //Enabled colliders

            if (DisableComponents) ToggleComponents(true);                                  //Enable all Monobehaviours breaking the Mount System
            OnEndDismounting.Invoke();                                                      //Invoke UnityEvent when is off Animal

            UpdateCanMountDismount();
        }
        
        internal virtual void MountTriggerEnter(Mount mount, MountTriggers mountTrigger)
        {
            Montura = mount;                                           //Set to Mount on this Rider    
            MonturaAI = Montura.GetComponent<IAIControl>();
            MountTrigger = mountTrigger;                               //Send the side transform to mount
            OnFindMount.Invoke(mount.transform.root.gameObject);       //Invoke Found Animal
            Montura.OnCanBeMounted.Invoke(Montura.CanBeMountedByState); //Invoke Can Be mounted to true ???
            Montura.NearbyRider = true;

            UpdateCanMountDismount();
        }

        internal virtual void MountTriggerExit()
        {
            MountTrigger = null;

            if (Montura)
            {
                Montura.EnableInput(false);
                Montura.OnCanBeMounted.Invoke(false);
                Montura.NearbyRider = false;
            }

            Montura = null;
            MonturaAI = null;
            MountInput = null;
            OnFindMount.Invoke(null);

            UpdateCanMountDismount();
        }

        /// <summary> Update the values Can Mount Can Dismount </summary>
        internal virtual void UpdateCanMountDismount()
        {
            bool canMount = Montura && !Mounted && !IsOnHorse && Montura.CanBeMountedByState;
            //  if (CanMount != canMount)
            {
                CanMount = canMount;
                OnCanMount.Invoke(CanMount);
            }


            bool canDismount = IsRiding && Montura.CanBeDismountedByState;
            // if (CanDismount != canDismount)
            {
                CanDismount = canDismount;
                OnCanDismount.Invoke(CanDismount);
            }


            bool canCallAnimal = !Montura && !Mounted && !IsOnHorse;
            //   if (CanCallAnimal != canCallAnimal)
            {
                CanCallAnimal = canCallAnimal;
                CanCallMount.Invoke(CanCallAnimal);
            }
        }

        /// <summary> Syncronize the Animal/Rider animations if Rider loose sync with the animal on the locomotion state </summary>
        protected virtual void Animators_ReSync()
        {
            if (!Anim) return;
            if (Montura.Animal.Stance != 0) return; //Skip if the we are not on the default stance                                                            
            if (Montura.ID != 0) return; // if is not the Horse (Wagon do not sync )                                                        

            if (Montura.Animal.ActiveStateID == StateEnum.Locomotion)                                               //Search for syncron the locomotion state on the animal
            {
                RiderNormalizedTime = Anim.GetCurrentAnimatorStateInfo(MountLayerIndex).normalizedTime;            //Get the normalized time from the Rider
                HorseNormalizedTime = Montura.Animal.Anim.GetCurrentAnimatorStateInfo(0).normalizedTime;           //Get the normalized time from the Horse

                syncronize = true;

                if (Mathf.Abs(RiderNormalizedTime - HorseNormalizedTime) > 0.1f && Time.time - LastSyncTime > 1f)   //Checking if the animal and the rider are unsync by 0.2
                {
                    Anim.CrossFade(AnimTag.Locomotion, 0.2f, MountLayerIndex, HorseNormalizedTime);                 //Normalized with blend
                    LastSyncTime = Time.time;
                }
            }
            else
            {
                syncronize = false;
                RiderNormalizedTime = HorseNormalizedTime = 0;
            }
        }

        /// <summary>Updates the Rider Position to the Mount Point</summary>
        public virtual void UpdateRiderTransform()
        {
            if (IsRiding)
            {
                transform.position = Montura.MountPoint.position;
                transform.rotation = Montura.MountPoint.rotation;
            }
        }


        /// <summary> Create a collider from hip to chest to check hits  when is on the horse  </summary>
        private void MountingCollider(bool create)
        {
            if (create)
            {
                mountedCollider = gameObject.AddComponent<CapsuleCollider>();
                mountedCollider.center = new Vector3(0, Col_Center);
                mountedCollider.radius = Col_radius;
                mountedCollider.height = Col_height;
                mountedCollider.isTrigger = Col_Trigger;
            }
            else
            {
                Destroy(mountedCollider);
            }
        }

        /// <summary> Enable Disable the Input for the Mount</summary>
        public virtual void EnableMountInput(bool value)
        {
            Montura?.EnableInput(value);
        }


        /// <summary> If the Animal has a IMountAI component it can be called</summary>
        public virtual void CallAnimal(bool call)
        {
            if (!CanCallAnimal) return;

            if (MountStored)                                                               //Call the animal Stored
            {
                MonturaAI = MountStored.GetComponent<IAIControl>();

                if (MonturaAI != null)
                {
                    ToggleCall = call;

                    if (ToggleCall)
                    {
                        MonturaAI.SetTarget(transform); //Set the Rider as the Target to follow
                        if (CallAnimalA)
                            RiderAudio.PlayOneShot(CallAnimalA);
                    }
                    else
                    {
                        StopMountAI();
                        if (StopAnimalA)
                            RiderAudio.PlayOneShot(StopAnimalA);
                    }
                }
            }
        }

        public virtual void CallAnimalToggle()
        {
            ToggleCall = !ToggleCall;
            CallAnimal(ToggleCall);
        }



        /// <summary>Enable/Disable The  Colliders in this gameobject </summary>
        protected virtual void ToogleColliders(bool active)
        {
            foreach (var col in colliders)
            {
                col.enabled = active;
            }
        }


        /// <summary>Toogle the MonoBehaviour Components Attached to this game Objects but the Riders Scripts </summary>
        protected virtual void ToggleComponents(bool enabled)
        {
            if (DisableList.Length == 0)
            {
                foreach (var component in AllComponents)
                {
                    if (component is MRider || component is RiderCombat) //Do not Disable or enable Rider or RiderCombat
                        continue;

                    component.enabled = enabled;
                }
            }
            else
            {
                foreach (var component in DisableList)
                {
                    if (component != null) component.enabled = enabled;
                }
            }
        }


        #region Link Animator


        protected virtual void SyncAnimator()
        {
            MAnimal animal = Montura.Animal;

            Anim.SetFloat(animal.hash_Vertical, animal.VerticalSmooth);
            Anim.SetFloat(animal.hash_Horizontal, animal.HorizontalSmooth);
            Anim.SetFloat(animal.hash_Slope, animal.SlopeNormalized);

            Anim.SetBool(animal.hash_Grounded, animal.Grounded);
            // Anim.SetInteger(animal.hash_State, animal.ActiveStateID);
            // Anim.SetInteger(animal.hash_Mode, animal.ModeID);


            Anim.SetInteger(animal.hash_IDInt, animal.IntID);
            Anim.SetFloat(animal.hash_IDFloat, animal.IDFloat);

            if (!Montura.UseSpeedModifiers) SpeedMultiplier = animal.SpeedMultiplier; //In case the Mount is not using Speed Modifiers

            SpeedMultiplier = Mathf.MoveTowards(SpeedMultiplier, TargetSpeedMultiplier, Time.deltaTime * 5f);
            Anim.SetFloat(animal.hash_SpeedMultiplier, SpeedMultiplier);
        }

        void AnimalGrounded(bool grounded)
        {
            if (Montura == null) return;
            Anim?.SetBool(Montura.Animal.hash_Grounded, grounded);
        }

        void AnimalState(int State)
        {
            if (Montura == null) return;
            Anim?.SetInteger(Montura.Animal.hash_State, State);
        }


        void AnimalStance(int stance)
        {
            if (Montura == null || (Montura.ID != 0)) return;//Skip if the Mount ID is Not  the Default
            Anim?.SetInteger(Montura.Animal.hash_Stance, stance);
        }
        void AnimalMode(int mode)
        {
            if (Montura == null) return;
            Anim?.SetInteger(Montura.Animal.hash_Mode, mode);
        }

        #endregion


        /// <summary> Checks and Execute  without Input if the Rider can Mount, Dismount or Call an Animal </summary>
        public virtual void CheckMountDismount()
        {
            UpdateCanMountDismount();

            if (CanMount)                       //if are near an animal and we are not already on an animal
            {
                MountAnimal();                  //Run mounting Animations
            }
            else if (CanDismount)               //if we are already mounted and the animal is not moving (Mounted && IsOnHorse && Montura.CanDismount)
            {
                DismountAnimal();               //Run Dismounting Animations
            }
            else if (CanCallAnimal)             //if there is no horse near, call the animal in the slot
            {
                CallAnimalToggle();
            }
        }

        /// <summary>IK Feet Adjustment while mounting</summary>
        void OnAnimatorIK()
        {
            if (Anim == null) return;           //If there's no animator skip

            if (IsMountingDismounting)
            {
                SolveStraightMount();
                //SolveStraightAIM();
                IKFeet();
            }
        }

        //void SolveStraightAIM()
        //{
        //    if (IsRiding  && CombatAim)
        //    {
        //        SAim_Weight = Mathf.MoveTowards(SAim_Weight, 1, Montura.Animal.DeltaTime * Montura.smoothSM / 2);
        //    }
        //    else 
        //    {
        //        SAim_Weight = Mathf.MoveTowards(SAim_Weight, 0, Montura.Animal.DeltaTime * Montura.smoothSM / 2);
        //    }

        //    if (SAim_Weight != 0)
        //    {
        //        if (Montura.UseStraightAim)
        //        {
        //            var MP = Montura.MountPoint;
        //            var TargetRotation = Quaternion.FromToRotation(MP.up, Montura.Animal.UpVector) * Montura.Animal.transform.rotation;
        //            var DeltaRotation = Quaternion.Lerp(Anim.bodyRotation, TargetRotation * Quaternion.Euler(Montura.AimOffset), SAim_Weight);
        //            var toWorld = Quaternion.Inverse(DeltaRotation) * Spine.rotation;
        //            //  Anim.bodyRotation = Quaternion.Lerp(Anim.bodyRotation, TargetRotation * Quaternion.Euler(Montura.AimOffset), SAim_Weight);
        //            Anim.SetBoneLocalRotation(HumanBodyBones.Spine, toWorld);
        //        }
        //    }
        //}


        private void SolveStraightMount()
        {
            if (IsRiding && Montura.StraightSpine && !CombatAim)
            {
                SP_Weight = Mathf.MoveTowards(SP_Weight, 1, Montura.Animal.DeltaTime * Montura.smoothSM / 2);

            }
            else
            {
                SP_Weight = Mathf.MoveTowards(SP_Weight, 0, Montura.Animal.DeltaTime * Montura.smoothSM / 2);
            }

            if (SP_Weight != 0)
            {
                //if (Montura.useMointPointOffset)
                //{
                //    var MP = Montura.MountPoint;
                //    var TargetRotation = Quaternion.FromToRotation(MP.up, Montura.Animal.UpVector) * Montura.Animal.transform.rotation;
                //    Anim.bodyRotation = Quaternion.Lerp(Anim.bodyRotation, TargetRotation * Quaternion.Euler(Montura.MPOffset),SP_Weight);
                //}


                Anim.SetLookAtPosition(Montura.MonturaSpineOffset);
                Anim.SetLookAtWeight(SP_Weight, 0.6f, 1);
            }
        }


        //private void SolveStraightMount()
        //{  
        //    if (IsRiding && Montura.StraightSpine)
        //    {
        //        var MP = Montura.MountPoint;

        //        SP_Weight = Mathf.MoveTowards(SP_Weight, 1, Montura.Animal.DeltaTime * Montura.smoothSM / 2);

        //        var TargetRotation = Quaternion.FromToRotation(MP.up, Montura.Animal.UpVector) * Montura.Animal.transform.rotation;     //Calculate the orientation to the Up Vector  

        //        TargetRotation  = Quaternion.Inverse(Spine.rotation) * TargetRotation;

        //        TargetRotation *= Montura.PointOffset;

        //        straightRotation = Quaternion.Lerp(straightRotation, TargetRotation, SP_Weight);
        //    }
        //    else if (IsDismounting || !Montura.StraightSpine)
        //    {
        //        SP_Weight = Mathf.MoveTowards(SP_Weight, 0, Montura.Animal.DeltaTime * Montura.smoothSM / 2);

        //        straightRotation = Quaternion.Lerp(Spine.localRotation, straightRotation, SP_Weight);
        //    }

        //    if (SP_Weight != 0)
        //    {
        //        Anim.SetBoneLocalRotation(HumanBodyBones.Spine, straightRotation);
        //    }
        //}


        internal virtual void IKFeet()
        {
            if (Montura.FootLeftIK == null || Montura.FootRightIK == null
                || Montura.KneeLeftIK == null || Montura.KneeRightIK == null) return;  //if is there missing an IK point do nothing

            //linking the weights to the animator
            if (Mounted || IsOnHorse)
            {
                L_IKFootWeight = 1f;
                R_IKFootWeight = 1f;

                int CurrentMountedHash = Anim.GetCurrentAnimatorStateInfo(MountLayerIndex).tagHash;

                if (Anim.IsInTransition(MountLayerIndex))
                {
                    CurrentMountedHash = Anim.GetNextAnimatorStateInfo(MountLayerIndex).tagHash;
                }

                if (CurrentMountedHash == Hash.Tag_Mount || CurrentMountedHash == Hash.Tag_Dismount)
                {
                    L_IKFootWeight = Anim.GetFloat(Hash.IKLeftFoot);
                    R_IKFootWeight = Anim.GetFloat(Hash.IKRightFoot);
                }

                //setting the weight
                Anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, L_IKFootWeight);
                Anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, R_IKFootWeight);

                Anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, L_IKFootWeight);
                Anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, R_IKFootWeight);

                //Knees
                Anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, L_IKFootWeight);
                Anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, R_IKFootWeight);

                //Set the IK Positions
                Anim.SetIKPosition(AvatarIKGoal.LeftFoot, Montura.FootLeftIK.position);
                Anim.SetIKPosition(AvatarIKGoal.RightFoot, Montura.FootRightIK.position);

                //Knees
                Anim.SetIKHintPosition(AvatarIKHint.LeftKnee, Montura.KneeLeftIK.position);    //Position
                Anim.SetIKHintPosition(AvatarIKHint.RightKnee, Montura.KneeRightIK.position);  //Position

                Anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, L_IKFootWeight);   //Weight
                Anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, R_IKFootWeight);  //Weight

                //setting the IK Rotations of the Feet
                Anim.SetIKRotation(AvatarIKGoal.LeftFoot, Montura.FootLeftIK.rotation);
                Anim.SetIKRotation(AvatarIKGoal.RightFoot, Montura.FootRightIK.rotation);
            }
            else
            {
                Anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
                Anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);

                Anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);
                Anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);
            }
        }


        /// <summary>Used for listening Message behaviour from the Animator</summary>
        public virtual void OnAnimatorBehaviourMessage(string message, object value)
        {
            this.InvokeWithParams(message, value);
        }

#if UNITY_EDITOR

        private void Reset()
        {
            animator = GetComponentInParent<Animator>() ?? GetComponentInChildren<Animator>();


            BoolVar CanMountV = MalbersTools.GetInstance<BoolVar>("Can Mount");
            BoolVar CanDismount = MalbersTools.GetInstance<BoolVar>("Can Dismount");


            MEvent CanMountE = MalbersTools.GetInstance<MEvent>("Rider Can Mount");
            MEvent CanDismountE = MalbersTools.GetInstance<MEvent>("Rider Can Dismount");
            MEvent RiderMountUI = MalbersTools.GetInstance<MEvent>("Rider Mount UI");

            BoolVar CanCallMountV = MalbersTools.GetInstance<BoolVar>("Can Call Mount");
            MEvent CanCallMountE = MalbersTools.GetInstance<MEvent>("Rider Can Call Mount");

            MEvent RiderisRiding = MalbersTools.GetInstance<MEvent>("Rider is Riding");
            MEvent SetCameraSettings = MalbersTools.GetInstance<MEvent>("Set Camera Settings");
            BoolVar RCWeaponInput = MalbersTools.GetInstance<BoolVar>("RC Weapon Input");

            OnCanMount = new BoolEvent();
            OnCanDismount = new BoolEvent();
            CanCallMount = new BoolEvent();
            OnStartMounting = new UnityEvent();
            OnEndMounting = new UnityEvent();
            OnStartMounting = new UnityEvent();
            OnStartDismounting = new UnityEvent();


            if (CanMountV != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnCanMount, CanMountV.SetValue);
            if (CanMountE != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnCanMount, CanMountE.Invoke);

            if (CanDismount != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnCanDismount, CanDismount.SetValue);
            if (CanDismountE != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnCanDismount, CanDismountE.Invoke);

            if (CanCallMountV != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(CanCallMount, CanCallMountV.SetValue);
            if (CanCallMountE != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(CanCallMount, CanCallMountE.Invoke);

            if (RiderMountUI != null) UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(OnStartMounting, RiderMountUI.Invoke, false);

            if (RiderisRiding != null)
            {
                UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(OnEndMounting, RiderisRiding.Invoke, true);
                UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(OnStartDismounting, RiderisRiding.Invoke, false);
            }

            if (SetCameraSettings != null) UnityEditor.Events.UnityEventTools.AddObjectPersistentListener<Transform>(OnStartDismounting, SetCameraSettings.Invoke, transform);

            if (RCWeaponInput != null)
            {
                UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(OnStartDismounting, RCWeaponInput.SetValue, false);
                UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(OnEndMounting, RCWeaponInput.SetValue, true);
            }


            var malbersinput = GetComponent<MalbersInput>();

            if (malbersinput)
            {
                UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(OnStartMounting, malbersinput.SetMoveCharacter, false);
                UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(OnEndDismounting, malbersinput.SetMoveCharacter, true);
            }
        }

        ///Editor Variables
        [HideInInspector] public int Editor_Tabs1;

        [ContextMenu("Create Mount Inputs")]
        void ConnectToInput()
        {
            MInput input = GetComponent<MInput>();

            if (input == null)
            { input = gameObject.AddComponent<MInput>(); }


            #region Mount Input
            var mountInput = input.FindInput("Mount");

            if (mountInput == null)
            {
                mountInput = new InputRow("Mount", "Mount", KeyCode.F, InputButton.Down, InputType.Key);
                input.inputs.Add(mountInput);

                mountInput.active.Variable = MalbersTools.GetInstance<BoolVar>("Can Mount");
                mountInput.active.UseConstant = false;

                UnityEditor.Events.UnityEventTools.AddPersistentListener(mountInput.OnInputDown, MountAnimal);


                Debug.Log("<B>Mount</B> Input created and connected to Rider.MountAnimal");
            }
            #endregion

            #region Dismount Input


            var DismountInput = input.FindInput("Dismount");

            if (DismountInput == null)
            {
                DismountInput = new InputRow("Dismount", "Dismount", KeyCode.F, InputButton.LongPress, InputType.Key);

                DismountInput.LongPressTime = 0.2f;

                input.inputs.Add(DismountInput);

                DismountInput.active.Variable = MalbersTools.GetInstance<BoolVar>("Can Dismount");
                DismountInput.active.UseConstant = false;

                var RiderDismountUI = MalbersTools.GetInstance<MEvent>("Rider Dismount UI");

                UnityEditor.Events.UnityEventTools.AddPersistentListener(DismountInput.OnLongPress, DismountAnimal);

                if (RiderDismountUI != null)
                {
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(DismountInput.OnLongPress, RiderDismountUI.Invoke);
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(DismountInput.OnPressedNormalized, RiderDismountUI.Invoke);
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(DismountInput.OnInputUp, RiderDismountUI.Invoke);
                    UnityEditor.Events.UnityEventTools.AddIntPersistentListener(DismountInput.OnInputDown, RiderDismountUI.Invoke, 0);
                }


                Debug.Log("<B>Dismount</B> Input created and connected to Rider.DismountAnimal");
            }

            #endregion

            #region CanCallMount Input


            var CanCallMount = input.FindInput("Call Mount");

            if (CanCallMount == null)
            {
                CanCallMount = new InputRow("Call Mount", "Call Mount", KeyCode.F, InputButton.Down, InputType.Key);
                input.inputs.Add(CanCallMount);

                CanCallMount.active.Variable = MalbersTools.GetInstance<BoolVar>("Can Call Mount");
                CanCallMount.active.UseConstant = false;

                UnityEditor.Events.UnityEventTools.AddPersistentListener(CanCallMount.OnInputDown, CallAnimalToggle);


                Debug.Log("<B>Call Mount</B> Input created and connected to Rider.CallAnimalToggle");
            }

            #endregion
        }

        [ContextMenu("Create Event Listeners")]
        void CreateEventListeners()
        {
            MEvent RiderSetMount = MalbersTools.GetInstance<MEvent>("Rider Set Mount");
            MEvent RiderSetDismount = MalbersTools.GetInstance<MEvent>("Rider Set Dismount");

            MEventListener listener = GetComponent<MEventListener>();

            if (listener == null)
            {
                listener = gameObject.AddComponent<MEventListener>();
            }

            if (listener.Events == null) listener.Events = new List<MEventItemListener>();

            if (listener.Events.Find(item => item.Event == RiderSetMount) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = RiderSetMount,
                    useVoid = true,
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.Response, MountAnimal);
                listener.Events.Add(item);

                Debug.Log("<B>Rider Set Mount</B> Added to the Event Listeners");
            }

            if (listener.Events.Find(item => item.Event == RiderSetDismount) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = RiderSetDismount,
                    useVoid = true,
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.Response, DismountAnimal);
                listener.Events.Add(item);

                Debug.Log("<B>Rider Set Dismount</B> Added to the Event Listeners");
            }

        }
#endif

        void OnDrawGizmos()
        {
            if (!debug) return;
            if (!Anim) return;

            if (syncronize && Mounted && Montura.debug)
            {
                Transform head = Anim.GetBoneTransform(HumanBodyBones.Head);

                if ((int)RiderNormalizedTime % 2 == 0)
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.white;
                }
                Gizmos.DrawSphere((head.position - transform.root.right * 0.2f), 0.05f);


                if ((int)HorseNormalizedTime % 2 == 0)
                {
                    Gizmos.color = new Color(0.11f, 1f, 0.25f); //Orange
                }
                else
                {
                    Gizmos.color = Color.white;
                }
                Gizmos.DrawSphere((head.position + transform.root.right * 0.2f), 0.05f);

#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.white;
                UnityEditor.Handles.Label(head.position + transform.up * 0.5f, "Sync Status");
#endif

            }
        }
    }
}