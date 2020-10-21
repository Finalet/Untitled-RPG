using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsAttackingCheck : StateMachineBehaviour
{
    public bool Player;
    public bool Enemy;

    public byte ID;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) { //This was Update, i dont remember why but i remember it was important
        if (Player){
            animator.gameObject.GetComponent<PlayerControlls>().isAttacking = false;
        } else {
            animator.gameObject.GetComponent<Enemy>().isAttacking = true;
        }
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        if (Player){
            PlayerControlls.instance.emptyAttackAnimatorStates[ID] = false;
            
            if (!animator.gameObject.GetComponent<PlayerControlls>().isCastingSkill)
                animator.gameObject.GetComponent<PlayerControlls>().isAttacking = true;
        } else {
            animator.gameObject.GetComponent<Enemy>().isAttacking = false;
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerControlls.instance.emptyAttackAnimatorStates[ID] = true;
    }
}
