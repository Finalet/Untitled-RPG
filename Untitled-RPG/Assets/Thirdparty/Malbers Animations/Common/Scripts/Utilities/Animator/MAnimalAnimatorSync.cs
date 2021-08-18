using MalbersAnimations.Controller;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations
{
    [RequireComponent(typeof(Animator))]
    /// <summary>Sync the Parameters of an alave Animator to a Master Animator</summary>
    [AddComponentMenu("Malbers/Utilities/Animator/Animator Sync")]

    public class MAnimalAnimatorSync : MonoBehaviour
    {
        [RequiredField,Tooltip("Master Animator Reference to get the parameters values")] 
        public Animator MasterAnimator;
        public int Layer = 0;
        public bool Re_Sync = true;
        [Hide("Re_Sync",true,false),Tooltip("Threshold to check if the Slave animator is unsyc")]
        public float Threshold = 0.1f;
        [Hide("Re_Sync", true, false), Tooltip("Which State will be synced again")]
        public List<int> StateCheck = new List<int>();

        private IMAnimator listenTo;
        private IAnimatorStateCycle StateCycle;
        private Animator MyAnimator;

        private Hashtable animatorParams;

        const float crossFade = 0.2f;

        private void Awake()
        {
            animatorParams = new Hashtable();
            if (MasterAnimator != null)
            {
                listenTo = MasterAnimator.GetComponent<IMAnimator>();
                StateCycle = MasterAnimator.GetComponent<IAnimatorStateCycle>();
            }

            MyAnimator = GetComponent<Animator>();

            foreach (AnimatorControllerParameter parameter in MyAnimator.parameters)
                animatorParams.Add(parameter.nameHash, parameter.name);
        }

        private void OnEnable()
        {
            listenTo.SetBoolParameter += SetAnimParameter;
            listenTo.SetIntParameter += SetAnimParameter;
            listenTo.SetFloatParameter += SetAnimParameter;
            if (Re_Sync) StateCycle.StateCycle += SyncStateCycle;
        }

        private void OnDisable()
        {
            listenTo.SetBoolParameter -= SetAnimParameter;
            listenTo.SetIntParameter -= SetAnimParameter;
            listenTo.SetFloatParameter -= SetAnimParameter;
            if (Re_Sync) StateCycle.StateCycle -= SyncStateCycle;
        }

        private void SyncStateCycle(int currentState)
        {
            if (MasterAnimator.IsInTransition(0) || MyAnimator.IsInTransition(Layer)) return; //Do not Resync when is on  transition

            if (HasStateCheck(currentState))
            {
                var MasterStateInfo = MasterAnimator.GetCurrentAnimatorStateInfo(0);
                var SlaveStateInfo = MyAnimator.GetCurrentAnimatorStateInfo(Layer);


                var MainTime = MasterStateInfo.normalizedTime;            //Get the normalized time from the Rider
                var SlaveTime = SlaveStateInfo.normalizedTime;            //Get the normalized time from the Horse

                if (Mathf.Abs(MainTime - SlaveTime) >= Threshold)   //Checking if the animal and the rider are unsync by 0.2
                {
                    MyAnimator.CrossFade(SlaveStateInfo.fullPathHash, crossFade, Layer, MainTime);                 //Normalized with blend
                    //Debug.Log("Resync");
                }
            }
        }


        public bool HasStateCheck(int check)
        {
            if (StateCheck.Count == 0) return false;

            foreach (var st in StateCheck)
                if (st == check) return true;

            return false;
        }

        /// <summary>Set a Int on the Animator</summary>
        public void SetAnimParameter(int hash, int value) 
        {
            if (animatorParams.ContainsKey(hash)) MyAnimator.SetInteger(hash, value);
        }

        /// <summary>Set a float on the Animator</summary>
        public void SetAnimParameter(int hash, float value)
        {
            if (animatorParams.ContainsKey(hash)) MyAnimator.SetFloat(hash, value);
        }

        /// <summary>Set a Bool on the Animator</summary>
        public void SetAnimParameter(int hash, bool value) 
        {
            if (animatorParams.ContainsKey(hash)) MyAnimator.SetBool(hash, value); 
        }
    }
}
