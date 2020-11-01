using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GettingInterruptedState : StateMachineBehaviour //Placed on animation GetHit
{

    override public void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        animator.gameObject.GetComponent<Enemy>().isGettingInterrupted = false;
    }
    override public void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        animator.gameObject.GetComponent<Enemy>().isGettingInterrupted = true;
    }
}
