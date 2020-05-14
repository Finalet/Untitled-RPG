using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GettingHitState : StateMachineBehaviour
{
    public bool isGoblin;
    public bool isPlayer;

    override public void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        if (isGoblin) 
            animator.gameObject.GetComponent<Goblin>().isGettingHit = false;
        else if (isPlayer)
            animator.gameObject.GetComponent<PlayerControlls>().isGettingHit = false;
    }
    override public void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        if (isGoblin) 
            animator.gameObject.GetComponent<Goblin>().isGettingHit = true;
        else if (isPlayer)
            animator.gameObject.GetComponent<PlayerControlls>().isGettingHit = true;
    }
}
