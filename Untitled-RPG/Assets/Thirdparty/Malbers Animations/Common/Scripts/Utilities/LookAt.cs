using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    /// <summary>Used for Animal that have Animated Physics enabled </summary>
    [DefaultExecutionOrder(500)]
    [RequireComponent(typeof(Aim))]
    public class LookAt : MonoBehaviour, IAnimatorListener 
    {
        [System.Serializable]
        public class BoneRotation
        {
            public Transform bone;                                          //The bone
            public Vector3 offset = new Vector3(0, -90, -90);               //The offset for the look At
            [Range(0, 1)]
            public float weight = 1;                                        //the Weight of the look a
            internal Quaternion nextRotation;
        }

        public BoolReference active = new BoolReference(true);     //For Activating and Deactivating the HeadTrack

        private IGravity a_UpVector;
        private Animator Anim;
        private IAim aimer;

        /// <summary>Max Angle to LookAt</summary>
        [Space, Tooltip("Max Angle to LookAt")]
        public FloatReference LimitAngle = new FloatReference(80f);                              
        /// <summary>Smoothness between Enabled and Disable</summary>
        [Tooltip("Smoothness between Enabled and Disable")]
        public FloatReference Smoothness = new FloatReference(5f);

        [Tooltip("Uses Quaternion.Lerp for the Bones... (Experimental)")]
        public BoolReference UseLerp;
        /// <summary>Smoothness between Enabled and Disable</summary>
        [Tooltip("Use the LookAt only when there's a Force Target on the Aim... use this when the Animal is AI Controlled")]
        [SerializeField] private BoolReference onlyTargets = new BoolReference(false)    ;
            
        

        [Space]
        public BoneRotation[] Bones;                                //Bone Chain    

        public bool debug = true;

        public float SP_Weight { get; private set; }
        /// <summary>Angle created between the transform.Forward and the LookAt Point   </summary>
        protected float angle;


        /// <summary>Means there's a camera or a Target to look At</summary>
        public bool HasTarget { get; set; }
        public Vector3 UpVector { get { return a_UpVector != null ? a_UpVector.UpVector : Vector3.up; } }


        private Transform EndBone;

        /// <summary>Direction Stored on the Aim Script</summary>
        public Vector3 AimDirection { get { return aimer.AimDirection; } }

        /// <summary>Check if is on the Angle of Aiming</summary>
        public bool IsAiming
        {
            get { return Active && ActiveByAnimation && (angle < LimitAngle); }
        }

        /// <summary> Enable Disable the Look At </summary>
        public bool Active
        {
            get { return active; }
            set
            {
                active.Value = value;        //enable disable also the Aimer
                aimer?.SetActive(value);     //enable disable also the Aimer
            }
        }

        /// <summary> Enable/Disable the LookAt by the Animator</summary>
        public bool ActiveByAnimation { get; set; }


        /// <summary>When set to True the Look At will only function with Targets gameobjects only instead of the Camera forward Direction</summary>
        public bool OnlyTargets { get => onlyTargets.Value; set => onlyTargets.Value = value; }

        void Awake()
        {
            a_UpVector = GetComponent<IGravity>();     //Get the main camera
            aimer = GetComponent<IAim>();              //Get the main camera
            Anim = GetComponent<Animator>();              //Get the main camera
            aimer.IgnoreTransform = transform;

            ActiveByAnimation = true;
        }

        void Start()
        {
            if (Bones != null && Bones.Length > 0) EndBone = Bones[Bones.Length - 1].bone;

            if (aimer.AimOrigin == null) aimer.AimOrigin = EndBone; 
        }


        void LateUpdate()
        {
            if (Time.time < float.Epsilon) return;

            if (OnlyTargets) Active = aimer.AimTarget != null;

            angle = Vector3.Angle(transform.forward, AimDirection);
            SP_Weight = Mathf.MoveTowards(SP_Weight, IsAiming ? 1 : 0, Time.deltaTime * Smoothness / 2);
            aimer.Limited = !IsAiming;


            if (UseLerp)
            LookAtBoneSet_AnimatePhysics_Lerp();            //Rotate the bones
            else
            LookAtBoneSet_AnimatePhysics();            //Rotate the bones

        }

        /// <summary>Enable or Disable this script functionality by the Animator </summary>
        public void EnableLookAt(bool value)
        {
            if (!OverridePriority)
            {
                ActiveByAnimation = value;
            }
            lastActivation = value;
        }

        public virtual void SetTargetOnly(bool val)
        {
            OnlyTargets = val;
        }

        bool OverridePriority;
        bool lastActivation;

        public void DisableLookAt(bool value)
        {
            OverridePriority = value;
            ActiveByAnimation = OverridePriority ? false : lastActivation;
        }

        /// <summary>Rotates the bones to the Look direction for FIXED UPTADE ANIMALS</summary>
        void LookAtBoneSet_AnimatePhysics()
        {
            for (int i = 0; i < Bones.Length; i++)
            {
                var bn = Bones[i];

                if (!bn.bone) continue;

                if (IsAiming)
                {
                    var BoneAim = Vector3.Slerp(transform.forward, AimDirection, bn.weight).normalized;
                    var TargetTotation = Quaternion.LookRotation(BoneAim, UpVector) * Quaternion.Euler(bn.offset);
                    bn.nextRotation = Quaternion.Lerp(bn.nextRotation, TargetTotation, SP_Weight);
                }
                else
                {
                    bn.nextRotation = Quaternion.Lerp(bn.bone.rotation, bn.nextRotation, SP_Weight);
                }

                if (SP_Weight != 0)
                {
                    bn.bone.rotation = bn.nextRotation;
                }
            }
        }

        void LookAtBoneSet_AnimatePhysics_Lerp()
        {
            Anim.Update(0);

            for (int i = 0; i < Bones.Length; i++)
            {
                var bn = Bones[i];

                if (!bn.bone) continue;
             
                if (IsAiming)
                {
                    var TargetTotation = Quaternion.LookRotation(AimDirection, UpVector) * Quaternion.Euler(bn.offset);
                    bn.nextRotation = Quaternion.Lerp(bn.bone.rotation, TargetTotation, bn.weight);
                }

                if (SP_Weight != 0)
                {
                    bn.bone.rotation = Quaternion.Lerp(bn.bone.rotation, bn.nextRotation,SP_Weight);
                }
            }
        }

        /// <summary>This is used to listen the Animator asociated to this gameObject </summary>
        public virtual void OnAnimatorBehaviourMessage(string message, object value)
        {
            this.InvokeWithParams(message, value);
        }

        void OnValidate()
        {
            if (Bones != null && Bones.Length>0)
            {
                EndBone = Bones[Bones.Length - 1].bone;
            }
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            bool AppIsPlaying = Application.isPlaying;

            if (debug)
            {
                UnityEditor.Handles.color = IsAiming || !AppIsPlaying ? new Color(0, 1, 0, 0.1f) : new Color(1, 0, 0, 0.1f);
 
                if (EndBone != null)
                {
                    UnityEditor.Handles.DrawSolidArc(EndBone.position, UpVector, Quaternion.Euler(0, -LimitAngle, 0) * transform.forward, LimitAngle * 2, 1);
                    UnityEditor.Handles.color = IsAiming || !AppIsPlaying ? Color.green : Color.red;
                    UnityEditor.Handles.DrawWireArc(EndBone.position, UpVector, Quaternion.Euler(0, -LimitAngle, 0) * transform.forward, LimitAngle * 2, 1);
                }
            }
        }
#endif
    }
}