using UnityEngine;
using System.Collections;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations.HAP
{
    /// <summary>This Enable the mounting System</summary> 
    [AddComponentMenu("Malbers/Riding/Mount Trigger")]
    public class MountTriggers : MonoBehaviour
    {

        [Tooltip("If true when the Rider enter the Trigger it will mount automatically")]
        public BoolReference AutoMount = new BoolReference(false);
        public bool WasAutomounted { get; internal set; }

        /// <summary>The name of the Animation we need to play to Mount the Animal</summary>
        [Tooltip("The name of the Animation we need to play to Mount the Animal")]
        public string MountAnimation = "Mount";

        /// <summary>The Transition ID value to dismount this kind of Montura.. (is Located on the Animator)</summary>
        [Tooltip("The Transition ID value to dismount this kind of Montura.. (is Located on the Animator)")]
        public IntReference DismountID;

        [Tooltip("the Transition ID value to dismount this kind of Montura.. (is Located on the Animator)")]
        /// <summary>The Local Direction of the Mount Trigger compared with the animal</summary>
        public Vector3Reference Direction;

        public TransformAnimation Adjustment;
      // Mountable Montura;
        Mount Montura;
        //Rider rider;
        MRider rider;

        // Use this for initialization
        void Awake()
        {
            Montura = GetComponentInParent<Mount>(); //Get the Mountable in the parents
        }

        void OnTriggerEnter(Collider other)
        {
            if (other != PlayerControlls.instance.GetComponent<CharacterController>())
                return;

            GetAnimal(other);
        }
        

        private void GetAnimal(Collider other)
        {
            if (!Montura)
            {
                Debug.LogError("No Mount Script Found... please add one");
                return;
            }
            if (!Montura.Mounted && Montura.CanBeMounted)                       //If there's no other Rider on the Animal or the the Animal isn't death
            {
                rider = other.GetComponentInChildren<MRider>();
                if (rider == null) rider = other.GetComponentInParent<MRider>();


                if (rider != null)
                {
                    if (rider.IsRiding) return;     //Means the Rider is already mounting an animal

                    rider.MountTriggerEnter(Montura,this); //Set Everything Requiered on the Rider in order to Mount

                    if (AutoMount.Value && !WasAutomounted)
                    {
                        rider.MountAnimal();
                    }
                }
            }
        }


        
        void OnTriggerExit(Collider other)
        {
            if (other != PlayerControlls.instance.GetComponent<CharacterController>())
                return;

            rider = other.GetComponentInChildren<MRider>();
            if (rider == null) rider = other.GetComponentInParent<MRider>();

            if (rider != null)
            {
                if (rider.IsRiding) return;                                         //You Cannot Mount if you are already mounted

                if (rider.MountTrigger == this && !Montura.Mounted)                 //When exiting if we are exiting From the Same Mount Trigger means that there's no mountrigger Nearby
                {
                    rider.MountTriggerExit();
                }

                rider = null;
                if (WasAutomounted) WasAutomounted = false;
            }

        }
    }
}