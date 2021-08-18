using UnityEngine;
using System.Collections;
using System;

namespace MalbersAnimations.HAP
{
    /// <summary>Used for Linking the Reins to the hand of the Ride </summary>
   // [RequireComponent(typeof(Mount))]
   // [AddComponentMenu("Malbers/Riding/IK Reins")]
    public class IKReins : MonoBehaviour
    {
        [HelpBox] public string desc = "-OBSOLETE-";

      /*  [SerializeField, RequiredField]
        protected Mount Montura;

       [RequiredField]  public Transform LeftHandRein;
        public Vector3 LeftHandOffset = new Vector3(-0.125f, 0.02f, 0.015f);
        [Space]
        [RequiredField] public Transform RightHandRein;
        public Vector3 RightHandOffset = new Vector3(-0.125f, 0.02f, -0.015f);

        /// <summary>Left Rein Handle Default Local Position  </summary>
        protected Vector3 DefaultLeftReinPos;
        /// <summary>Right Rein Handle Default Local Position  </summary>
        protected Vector3 DefaultRightReinPos;
        protected bool freeRightHand = true;
        protected bool freeLeftHand = true;

      
        void Start()
        {
            if (Montura == null)
                Montura = this.FindComponent<Mount>();

            if (LeftHandRein && RightHandRein)
            {
                DefaultLeftReinPos = LeftHandRein.localPosition;             //Set the Reins Local Values Values
                DefaultRightReinPos = RightHandRein.localPosition;            //Set the Reins Local Values Values
            }
            else
            {
                Debug.LogWarning("Some of the Reins has not been set on the inspector. Please fill the values");
                enabled = false;
            }
        }

       

        /// <summary>Free the Right Hand (True :The reins will not be on the Hand)</summary>
        public void FreeRightHand(bool value)
        {
            freeRightHand = !value;

            if (freeRightHand && RightHandRein)
            {
                RightHandRein.localPosition = DefaultRightReinPos;
            }
        }

        public void FreeBothHands()
        {
            FreeRightHand(false);
            FreeLeftHand(false);
        }


        public void WeaponInHands()
        {
            FreeRightHand(true);
            FreeLeftHand(true);
        }


        /// <summary>Free the Left Hand (True :The reins will not be on the Hand)</summary>
        public void FreeLeftHand(bool value)
        {
            freeLeftHand = !value;
            if (freeLeftHand && LeftHandRein)
            {
                LeftHandRein.localPosition = DefaultLeftReinPos;
            }
        } 

        void LateUpdate()
        {
            if (Montura.Rider && Montura.Rider.IsRiding)
            {
                if (!LeftHandRein || !RightHandRein) return; //There's no Reins Reference
                if (Montura.Rider.LeftHand == null || Montura.Rider.RightHand == null) return; //There's no Rider Hands References Reference

                var New_L_ReinPos = Montura.Rider.LeftHand.TransformPoint(LeftHandOffset);
                var New_R_ReinPos = Montura.Rider.RightHand.TransformPoint(RightHandOffset);

                if (!freeLeftHand && !freeRightHand) //When Both hands are free
                {
                    LeftHandRein.localPosition = DefaultLeftReinPos;
                    RightHandRein.localPosition = DefaultRightReinPos;
                    return;
                }

                if (freeLeftHand)
                {
                    LeftHandRein.position = New_L_ReinPos;     //Put it in the middle o the left hand
                }
                else
                {
                    if (freeRightHand)
                    {
                        LeftHandRein.position = New_R_ReinPos; //if the right hand is holding a weapon put the right rein to the Right hand
                    }
                }

                if (freeRightHand)
                {
                    RightHandRein.position = New_R_ReinPos; //Put it in the middle o the RIGHT hand
                }
                else
                {
                    if (freeLeftHand)
                    {
                        RightHandRein.position = New_L_ReinPos; //if the right hand is holding a weapon put the right rein to the Left hand
                    }
                }
            }
            else
            {
                LeftHandRein.localPosition = DefaultLeftReinPos;
                RightHandRein.localPosition = DefaultRightReinPos;
            }
        }

        private void Reset()
        {
            if (Montura == null)
                Montura = this.FindComponent<Mount>();
        }*/
    } 
}
