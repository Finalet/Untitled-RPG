using UnityEngine;

namespace MalbersAnimations.Controller
{
    public class AttackTriggerBehaviour : StateMachineBehaviour
    {
        //[Header("MANIMAL")]
        public int AttackTrigger = 1;                           //ID of the Attack Trigger to Enable/Disable during the Attack Animation

        [Tooltip("Range on the Animation that the Attack Trigger will be Active")]
        [MinMaxRange(0, 1)]
        public RangedFloat AttackActivation = new RangedFloat(0.3f, 0.6f);

        private bool isOn, isOff;
        private MAnimal animal;


        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animal = animator.GetComponent<MAnimal>();
            isOn = isOff = false;                                   //Reset the ON/OFF parameters (to be used on the Range of the animation
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!isOn && (stateInfo.normalizedTime % 1) >= AttackActivation.minValue)
            {
                animal.AttackTrigger(AttackTrigger);
                isOn = true;
            }

            if (!isOff && (stateInfo.normalizedTime % 1) >= AttackActivation.maxValue)
            {
                animal.AttackTrigger(0);
                isOff = true;
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animator.GetCurrentAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash)
            {
               // Debug.Log("SELF");
                return;
            } //means is transitioning to it self
            
            animal.AttackTrigger(0);                //Disable all Attack Triggers
            isOn = isOff = false;                   //Reset the ON/OFF variables
        }
    }
}