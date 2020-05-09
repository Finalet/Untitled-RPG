using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    public class ModifierBehaviour : StateMachineBehaviour
    {
        public AnimalModifier EnterModifier;
        public AnimalModifier ExitModifier;
        private MAnimal animal;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animal = animator.GetComponent<MAnimal>();

            EnterModifier.Modify(animal);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            ExitModifier.Modify(animal);
        }
    }
}