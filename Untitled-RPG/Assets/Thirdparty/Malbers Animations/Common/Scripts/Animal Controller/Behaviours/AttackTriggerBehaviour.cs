using UnityEngine;

namespace MalbersAnimations.Controller
{
    public class AttackTriggerBehaviour : StateMachineBehaviour
    {
        [Tooltip("0: Disable All Attack Triggers\n-1: Enable All Attack Triggers\nx: Enable the Attack Trigger by its index")]
        public int AttackTrigger = 1;                           //ID of the Attack Trigger to Enable/Disable during the Attack Animation

        [Tooltip("Range on the Animation that the Attack Trigger will be Active")]
        [MinMaxRange(0, 1)]
        public RangedFloat AttackActivation = new RangedFloat(0.3f, 0.6f);

        private bool isOn, isOff;
        private IMDamagerSet[] damagers;


        override public void OnStateEnter(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (damagers == null) damagers = anim.GetComponents<IMDamagerSet>();
            isOn = isOff = false;                                   //Reset the ON/OFF parameters (to be used on the Range of the animation
        }

        override public void OnStateUpdate(Animator anim, AnimatorStateInfo state, int layer)
        {
            var time = state.normalizedTime % 1;

            if (!isOn && (time >= AttackActivation.minValue))
            {
                foreach (var d in damagers) d.ActivateDamager(AttackTrigger);
                isOn = true;
            }

            if (!isOff && (time >= AttackActivation.maxValue))
            {
                if (anim.IsInTransition(layer) && anim.GetNextAnimatorStateInfo(layer).fullPathHash == state.fullPathHash) return; //means is transitioning to it self so do not OFF it
                foreach (var d in damagers) d.ActivateDamager(0);
                isOff = true;
            }
        }

        override public void OnStateExit(Animator anim, AnimatorStateInfo state, int layer)
        {
            if (anim.GetCurrentAnimatorStateInfo(layer).fullPathHash == state.fullPathHash) return; //means is transitioning to it self

            if (!isOff)
                foreach (var d in damagers) d.ActivateDamager(0);  //Double check that the Trigger is OFF


            isOn = isOff = false;                                               //Reset the ON/OFF variables
        }
    }
}