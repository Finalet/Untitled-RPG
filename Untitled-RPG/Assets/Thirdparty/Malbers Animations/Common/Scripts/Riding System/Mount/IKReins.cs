using UnityEngine;
using System.Collections;
using System;

namespace MalbersAnimations.HAP
{
    /// <summary>Used for Linking the Reins to the hand of the Ride </summary>
    [RequireComponent(typeof(Mount))]
    public class IKReins : MonoBehaviour
    {
        public Transform LeftHandRein;
        public Vector3 LeftHandOffset  = new Vector3(-0.125f, 0.02f,  0.015f);
        [Space]
        public Transform RightHandRein;
        public Vector3 RightHandOffset = new Vector3(-0.125f, 0.02f, -0.015f);

        /// <summary>Left Rein Handle Default Local Position  </summary>
        protected Vector3 DefaultLeftReinPos;
        /// <summary>Right Rein Handle Default Local Position  </summary>
        protected Vector3 DefaultRightReinPos;

        protected Transform Rider_L_Hand, Rider_R_Hand;
        protected Mount Montura;

        protected bool freeRightHand = true;
        protected bool freeLeftHand = true;

        private void Awake()
        {
            Montura = GetComponent<Mount>();
        }

        void Start()
        {
            if (LeftHandRein && RightHandRein)
            {
                DefaultLeftReinPos = LeftHandRein.localPosition;             //Set the Reins Local Values Values
                DefaultRightReinPos = RightHandRein.localPosition;            //Set the Reins Local Values Values
            }
            else
            {
                Debug.LogWarning("Some of the Reins has not been set on the inspector. Please fill the values");
            }
        }

        private void OnEnable()
        {
            Montura.OnMounted.AddListener(OnRiderMounted);
            Montura.OnDismounted.AddListener(OnRiderDismounted);
        }



        private void OnDisable()
        {
            Montura.OnMounted.RemoveListener(OnRiderMounted);
            Montura.OnDismounted.RemoveListener(OnRiderDismounted);
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

        /// <summary>Free the Left Hand (True :The reins will not be on the Hand)</summary>
        public void FreeLeftHand(bool value)
        {
            freeLeftHand = !value;
            if (freeLeftHand && LeftHandRein)
            {
                LeftHandRein.localPosition = DefaultLeftReinPos;
            }
        }

        void OnRiderMounted()
        {
            Animator RiderAnim = Montura.Rider.Anim;  //Get the Rider Animator

            if (RiderAnim)
            {
                Rider_L_Hand = RiderAnim.GetBoneTransform(HumanBodyBones.LeftHand);
                Rider_R_Hand = RiderAnim.GetBoneTransform(HumanBodyBones.RightHand);
            }
        }

        private void OnRiderDismounted()
        {
            Rider_L_Hand = null;
            Rider_R_Hand = null;

            LeftHandRein.localPosition = DefaultLeftReinPos;
            RightHandRein.localPosition = DefaultRightReinPos;
        }


        void LateUpdate()
        {
            if (!LeftHandRein || !RightHandRein) return; //There's no Reins Reference
            if (!Rider_L_Hand || !Rider_R_Hand) return; //There's no Rider Hands References Reference

            if (Montura.Rider && Montura.Rider.IsRiding)
            {
                var New_L_ReinPos =  Rider_L_Hand.TransformPoint(LeftHandOffset);
                var New_R_ReinPos =  Rider_R_Hand.TransformPoint(RightHandOffset);

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
    }
}
