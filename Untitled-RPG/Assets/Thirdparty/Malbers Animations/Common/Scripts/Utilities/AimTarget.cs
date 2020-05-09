using MalbersAnimations.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.Utilities
{
    /// <summary>For when someone with LookAt enters it will set this transform as the target</summary>
    public class AimTarget : MonoBehaviour, IAimTarget
    {
        /// <summary>This will set AutoAiming for the Aim Logic</summary>
        [SerializeField,Tooltip("This will set AutoAiming for the Aim Logic")] 
        private bool aimAssist;

        public UnityEvent OnAimEnter = new UnityEvent();
        public UnityEvent OnAimExit = new UnityEvent();

        /// <summary>This will set AutoAiming for the Aim Logic</summary>
        public bool AimAssist { get => aimAssist; set => aimAssist = value; }

        /// <summary>Is the target been aimed by the Aim Ray of the Aim Script</summary>
        public void IsBeenAimed(bool enter) 
        {
            if (enter)
                OnAimEnter.Invoke();
            else
                OnAimExit.Invoke();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return; //Ignore if the Collider entering is a Trigger

            IAim Aimer = other.GetComponentInParent<IAim>();

            if (Aimer != null)
            {
                Aimer.AimTarget = transform;
                OnAimEnter.Invoke();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.isTrigger) return;                //Ignore if the Collider exiting is a Trigger

            IAim Aimer = other.GetComponentInParent<IAim>();

            if (Aimer != null)
            {
                Aimer.AimTarget = null;
                OnAimExit.Invoke();
            }
        }
    }
}