using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Scriptables;
using UnityEngine.Events;
using MalbersAnimations.Utilities;
using MalbersAnimations.Events;

namespace MalbersAnimations.Controller
{

    /// <summary>When an animal Enter a Zone this will activate a new State or a new Mode </summary>
    public class Zone : MonoBehaviour, IDestination , IInteractable
    { 
        /// <summary>Set the Action Zone to Automatic</summary>
        public bool automatic;
        /// <summary>if Automatic is set to true this will be the time to disable temporarly the Trigger</summary>
        public float AutomaticDisabled = 10f;
        /// <summary>Use the Trigger for Heads only</summary>
        public bool HeadOnly;
        public string HeadName = "Head";


        public ZoneType zoneType = ZoneType.Mode;
        public StateAction stateAction = StateAction.Activate;
        public StanceAction stanceAction = StanceAction.Enter;
        public ModeID modeID;
        public StateID stateID;
        public StanceID stanceID;
        public Action ActionID;
        /// <summary> Mode Index Value</summary>
        [SerializeField] private IntReference modeIndex =  new IntReference(0);

        /// <summary> Mode Index Value</summary>
        public int ModeIndex => modeID.ID == 4 ? ActionID.ID : modeIndex.Value;

        /// <summary>Current Animal the Zone is using </summary>
        public MAnimal CurrentAnimal { get; internal set; }
        protected List<Collider> animal_Colliders = new List<Collider>();

        public float ActionDelay = 0;
        public bool RemoveAnimalOnActive = false;

        [UnityEngine.Serialization.FormerlySerializedAs("StatModifier")]
        public StatModifier StatModifierOnActive;
        public StatModifier StatModifierOnEnter;
        public StatModifier StatModifierOnExit;
        [HideInInspector] public bool ShowStatModifiers = true;
        public AnimalEvent OnEnter = new AnimalEvent();
        public AnimalEvent OnExit = new AnimalEvent();
        public AnimalEvent OnZoneActivation = new AnimalEvent();

        protected Collider ZoneCollider;
        protected Stats AnimalStats;
 
        /// <summary>Keep a Track of all the Zones on the Scene </summary>
        public static List<Zone> Zones;
 
        /// <summary>Retuns the ID of the Zone regarding the Type of Zone(State,Stance,Mode) </summary>
        public int GetID
        {
            get
            {
                switch (zoneType)
                {
                    case ZoneType.Mode:
                        return modeID;
                    case ZoneType.State:
                        return stateID;
                    case ZoneType.Stance:
                        return stateID;
                    default:
                        return 0;
                }
            }
        }

        /// <summary>Is the zone a Mode Zone</summary>
        public bool IsMode => zoneType == ZoneType.Mode;

        /// <summary>Is the zone a Mode Zone</summary>
        public bool IsState => zoneType == ZoneType.State;

        /// <summary>Is the zone a Mode Zone</summary>
        public bool IsStance => zoneType == ZoneType.Stance; 

        void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return;

            if (!MalbersTools.CollidersLayer(other, LayerMask.GetMask("Animal"))) return;           //Just accept animal layer only
            if (HeadOnly && !other.name.Contains(HeadName)) return;                                 //If is Head Only and no head was found Skip

            MAnimal newAnimal = other.GetComponentInParent<MAnimal>();                              //Get the animal on the entering collider

            if (!newAnimal) return;                                                                 //If there's no animal do nothing

            if (animal_Colliders.Find(coll => coll == other) == null)                               //if the entering collider is not already on the list add it
            {
                animal_Colliders.Add(other);
            }

            if (newAnimal == CurrentAnimal) return;                                    //if the animal is the same do nothing (when entering two animals on the same Zone)
            else
            {
                if (CurrentAnimal)
                    animal_Colliders = new List<Collider>();                            //Clean the colliders

                CurrentAnimal = newAnimal;                                             //Set a new Animal
                AnimalStats = CurrentAnimal.GetComponentInParent<Stats>();
                
                StatModifierOnEnter.ModifyStat(AnimalStats);                         //Modify the stats when exit
                OnEnter.Invoke(CurrentAnimal);
                ActivateZone();
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (other.isTrigger) return;
            if (HeadOnly && !other.name.Contains(HeadName)) return;         //if is only set to head and there's no head SKIP

            MAnimal existing_animal = other.GetComponentInParent<MAnimal>();

            if (!existing_animal) return;                                            //If there's no animal script found skip all
            if (existing_animal != CurrentAnimal) return;                            //If is another animal exiting the zone SKIP

            if (animal_Colliders.Find(item => item == other))                       //Remove the collider from the list that is exiting the zone.
            {
                animal_Colliders.Remove(other);
            }

            if (animal_Colliders.Count == 0)                                        //When all the collides are removed from the list..
            {
                OnExit.Invoke(CurrentAnimal);                                      //Invoke On Exit when all animal's colliders has exited the Zone
                StatModifierOnExit.ModifyStat(AnimalStats);                         //Modify the stats when exit

                if (zoneType == ZoneType.Stance && stanceAction == StanceAction.Stay && CurrentAnimal.Stance == stanceID.ID)
                {
                    CurrentAnimal.Stance_Reset();
                }

                ResetStoredAnimal();
            }
        }


        /// <summary>Activate the Zone depending the Zone Type</summary>
        /// <param name="forced"></param>
        public virtual void ActivateZone(bool forced = false)
        {
           if (CurrentAnimal) CurrentAnimal.IsOnZone = true;

            switch (zoneType)
            {
                case ZoneType.Mode:
                    ActivateModeZone(forced);
                    break;
                case ZoneType.State:
                    ActivateStateZone(); //State Zones does not require to be delay or prepared to be activated
                    break;
                case ZoneType.Stance:
                    ActivateStanceZone(); //State Zones does not require to be delay or prepared to be activated
                    break;
            }
        }

        public virtual void ResetStoredAnimal()
        {
            CurrentAnimal.IsOnZone = false;

            if (zoneType == ZoneType.Mode)
            {
                var PreMode = CurrentAnimal.Mode_Get(modeID);
                if (PreMode != null)
                {
                    PreMode.ResetAbilityIndex();
                    PreMode.GlobalProperties.OnEnter.RemoveListener(OnZONEActive);
                }
            }

            CurrentAnimal = null;
            AnimalStats = null;
            animal_Colliders = new List<Collider>();                            //Clean the colliders
        }


        /// <summary>Enables the Zone using the State</summary>
        private void ActivateStateZone()
        {
            switch (stateAction)
            {
                case StateAction.Activate:
                    CurrentAnimal.State_Activate(stateID);
                    break;
                case StateAction.AllowExit:
                    if (CurrentAnimal.ActiveStateID == stateID) CurrentAnimal.ActiveState.AllowExit();
                    break;
                case StateAction.ForceActivate:
                    CurrentAnimal.State_Force(stateID);
                    break;
                case StateAction.Enable:
                    CurrentAnimal.State_Enable(stateID);
                    break;
                case StateAction.Disable:
                    CurrentAnimal.State_Disable(stateID);
                    break;
                default:
                    break;
            }

            StatModifierOnActive.ModifyStat(AnimalStats);
            OnZoneActivation.Invoke(CurrentAnimal);
        }

        /// <summary>Enables the Zone using the Stance</summary>
        private void ActivateStanceZone()
        {
           // CurrentAnimal.Mode_Interrupt(); //in case the Animal is doing a mode Interrupt it

            switch (stanceAction)
            { 
                case StanceAction.Enter:
                    CurrentAnimal.Stance_Set(stanceID);
                    break;
                case StanceAction.Exit:
                    CurrentAnimal.Stance_Reset();
                    break;
                case StanceAction.Toggle:
                    CurrentAnimal.Stance_Toggle(stanceID);
                    break;
                case StanceAction.Stay:
                    CurrentAnimal.Stance_Set(stanceID);
                    break;
                default:
                    break;
            }
           
            StatModifierOnActive.ModifyStat(AnimalStats);
            OnZoneActivation.Invoke(CurrentAnimal);
        }

        public virtual void Animal_StopMoving()
        {
            if (CurrentAnimal)
            {
                CurrentAnimal.MovementAxis = CurrentAnimal.MovementAxisSmoothed = Vector3.zero;
            }
        }

        private void ActivateModeZone(bool forced)
        {
            if (forced || automatic)
            {
                CurrentAnimal.Mode_Activate(modeID.ID, ModeIndex); //Current animal was empty  ??!?!??!
                OnZONEActive();

                if (automatic)
                {
                    StartCoroutine(ZoneColliderONOFF());
                }
            }
            else
            {  //In Case the Zone is not Automatic
                var PreMode = CurrentAnimal.Mode_Get(modeID);

                if (PreMode != null)
                {
                    PreMode.AbilityIndex = ModeIndex;
                    PreMode.GlobalProperties.OnEnter.AddListener(OnZONEActive);
                }
            }
        }


        void OnZONEActive()
        { 
            StatModifierOnActive.ModifyStat(AnimalStats);
            OnZoneActivation.Invoke(CurrentAnimal);

            if (RemoveAnimalOnActive)
            {
                ResetStoredAnimal();
            }
        }
         
        /// <summary> Destroy the Zone after x Time</summary>
        public virtual void Zone_Destroy(float time)
        {
            if (time == 0)
                Destroy(gameObject);
            else
            {
                Destroy(gameObject, time);
            }
        }

        /// <summary> Enable Disable the Zone COllider for and X time</summary>
        IEnumerator ZoneColliderONOFF() //For Automatic only 
        {
            yield return null;

            if (AutomaticDisabled > 0)
            {
                ZoneCollider.enabled = false;
                CurrentAnimal?.ActiveMode?.ResetAbilityIndex();       //Reset the Ability Index when Set to automatic and the Collider is off
                yield return new WaitForSeconds(AutomaticDisabled);
                ZoneCollider.enabled = true;
            }
            CurrentAnimal = null;                           //clean animal
            animal_Colliders = new List<Collider>();        //Reset Colliders
            yield return null;
        }

        void OnEnable()
        {
            if (Zones == null) Zones = new List<Zone>();
            ZoneCollider = GetComponent<Collider>();                                   //Get the reference for the collider
            Zones.Add(this);                                                  //Save the the Action Zones on the global Action Zone list
        }
        void OnDisable()
        {
            Zones.Remove(this);                                              //Remove the the Action Zones on the global Action Zone list

            if (CurrentAnimal)
                ResetStoredAnimal();
        }

        public void ResetInteraction() {/* Do nothing  */}

        public void Interact() { ActivateZone(true); }

        public void TargetArrived(GameObject target)
        {
            CurrentAnimal = target.GetComponent<MAnimal>();
            ActivateZone(true);
        }

     
        [HideInInspector] public bool EditorShowEvents = true;
    }

    public enum StateAction
    {
        /// <summary>Tries to Activate the State of the Zone</summary>
        Activate,
        /// <summary>If the Animal is already on the state of the zone it will allow to exit and activate states below the Active one</summary>
        AllowExit,
        /// <summary>Force the State of the Zone to be enable even if it cannot be activate at the moment</summary>
        ForceActivate,
        /// <summary>Enable a  Disabled State </summary>
        Enable,
        /// <summary>Disable State </summary>
        Disable
    }
    public enum StanceAction
    {
        /// <summary>Enters a Stance</summary>
        Enter,
        /// <summary>Exits a Stance</summary>
        Exit,
        /// <summary>Toggle a Stance</summary>
        Toggle,
        /// <summary>While the Animal is inside the collider the Animal will stay on the Stance</summary>
        Stay,
    }
    public enum ZoneType
    {
        Mode,
        State,
        Stance
    }
}
