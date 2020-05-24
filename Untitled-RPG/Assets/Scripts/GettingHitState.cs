using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GettingHitState : StateMachineBehaviour //Placed on animation GetHit
{
    public bool isEnemy;
    public bool isPlayer;

    override public void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        if (isEnemy) 
            animator.gameObject.GetComponent<Enemy>().isGettingHit = false;
        else if (isPlayer)
            animator.gameObject.GetComponent<PlayerControlls>().isGettingHit = false;
    }
    override public void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        if (isEnemy) 
            animator.gameObject.GetComponent<Enemy>().isGettingHit = true;
        else if (isPlayer)
            animator.gameObject.GetComponent<PlayerControlls>().isGettingHit = true;
    }
}
