using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.Utilities
{
    /// <summary>For when someone with LookAt enters it will set this transform as the target</summary>
    [AddComponentMenu("Malbers/Utilities/Aiming/Aim Target")]
    public class AimTarget : MonoBehaviour, IAimTarget
    {
        /// <summary>This will set AutoAiming for the Aim Logic</summary>
        [SerializeField,Tooltip("It will center the Aim Ray into this gameObject's collider")] 
        private bool aimAssist;

        [SerializeField, Tooltip("Transform Point for the Aim Assist")]
        private Transform m_AimPoint;

        public UnityEvent OnAimEnter = new UnityEvent();
        public UnityEvent OnAimExit = new UnityEvent();

        /// <summary>This will set AutoAiming for the Aim Logic</summary>
        public bool AimAssist { get => aimAssist; set => aimAssist = value; }
        public Transform AimPoint => m_AimPoint;


        private void Start()
        {
            if (m_AimPoint == null) m_AimPoint = transform;
        }

        private void OnValidate()
        {
            if (m_AimPoint == null) m_AimPoint = transform;
        }



        /// <summary>Is the target been aimed by the Aim Ray of the Aim Script</summary>
        public void IsBeenAimed(bool enter) 
        {
            if (enter)
                OnAimEnter.Invoke();
            else
                OnAimExit.Invoke();
        }

        

        ///  
        ///   Aim Targets can be also used as Trigger Enter Exit 
        void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return; //Ignore if the Collider entering is a Trigger

            IAim Aimer = other.transform.root.FindInterface<IAim>();

            if (Aimer != null)
            {
                Aimer.AimTarget = transform;
                OnAimEnter.Invoke();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.isTrigger) return;                //Ignore if the Collider exiting is a Trigger

            IAim Aimer = other.transform.root.FindInterface<IAim>();

            if (Aimer != null)
            {
                Aimer.AimTarget = null;
                OnAimExit.Invoke();
            }
        }


    }
}