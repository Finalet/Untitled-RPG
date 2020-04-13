using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAnimStartStop : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        animator.gameObject.GetComponent<PlayerControlls>().isAttacking = false;
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        animator.gameObject.GetComponent<PlayerControlls>().isAttacking = true;
    }
}
