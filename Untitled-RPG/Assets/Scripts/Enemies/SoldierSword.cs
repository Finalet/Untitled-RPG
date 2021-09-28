using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierSword : NavAgentEnemy
{

    protected override void Start()
    {
        base.Start();
        agrDelay = 1.7f;
    }

    protected override void Update()
    {
        animator.SetBool("Agr", agr);
        animator.SetBool("KnockedDown", isKnockedDown);
        
        base.Update();

        if (isRagdoll || isKnockedDown || isDead || PlayerControlls.instance == null) { //Player instance is null when level is only loading.
            if (navAgent.enabled) navAgent.isStopped = true;
            return;
        }

        animator.SetBool("Approaching", currentState == EnemyState.Approaching ? true : false);
        animator.SetBool("Returning", currentState == EnemyState.Returning ? true : false);
        animator.SetBool("Celebrating", currentState == EnemyState.Celebrating ? true : false);
    }

    protected override void Idle () {
        base.Idle();
        navAgent.isStopped = true;
    }

    protected override void AttackTarget()
    {
        UseAttack(plannedAttack);
        plannedAttack = ClosestAttack();
    }


    protected override void ApproachTarget () {
        base.ApproachTarget();

        navAgent.isStopped = isGettingInterrupted ? true : false;
        navAgent.destination = target.position;

        plannedAttack = ClosestAttack();
    }
}
