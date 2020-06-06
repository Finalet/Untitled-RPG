using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsAttackingCheck : StateMachineBehaviour
{
    public bool Player;
    public bool Enemy;

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        if (Player){
            animator.gameObject.GetComponent<PlayerControlls>().isAttacking = false;
        } else {
            animator.gameObject.GetComponent<Enemy>().isAttacking = true;
        }
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        if (Player){
            if (!animator.gameObject.GetComponent<PlayerControlls>().isCastingSkill)
                animator.gameObject.GetComponent<PlayerControlls>().isAttacking = true;
        } else {
            animator.gameObject.GetComponent<Enemy>().isAttacking = false;
        }
    }
}
