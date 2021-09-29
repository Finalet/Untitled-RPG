using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class NavAgentEnemy : Enemy
{

    protected NavMeshAgent navAgent;

    protected override void Start() {
        base.Start();
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.avoidancePriority = 50 + Random.Range(-20, 20);
    }
    protected override void Update()
    {
        base.Update();
        if (isRagdoll || isKnockedDown || isDead || PlayerControlls.instance == null) { //Player instance is null when level is only loading.
            if (navAgent.isActiveAndEnabled) navAgent.isStopped = true;
        }
    }

    protected override void Idle()
    {
        base.Idle();
        navAgent.isStopped = true;
    }

    protected override void TryAttackTarget()
    {
        base.TryAttackTarget();
        navAgent.isStopped = true;
    }
    protected override void AttackTarget()
    {   
        //
    }

    protected override void ReturnToPosition()
    {
        base.ReturnToPosition();
        navAgent.destination = initialPos;
        navAgent.isStopped = false;
    }

    protected override void Die()
    {
        base.Die();
        navAgent.enabled = false;
    }

    protected override void ApproachTarget () {
        base.ApproachTarget();

        navAgent.isStopped = isGettingInterrupted ? true : false;
        navAgent.destination = target.position;

        plannedAttack = ClosestAttack();
    }

    protected override float DistanceToInitialPos()
    {
        return Vector3.Distance(navAgent.nextPosition, initialPos);
    }
    protected override float StoppingDistance()
    {
        return enemyController.stoppingDistance;
    }

    protected override void Stun()
    {
        base.Stun();
        navAgent.isStopped = true;
    }

    protected override string ExtraDebug()
    {
        return $"\n<color=#7f7f7f>Remaining dis: <color=white>{System.Math.Round(navAgent.remainingDistance, 3)}";
    }
}
