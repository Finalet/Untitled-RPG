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

    protected override float RemainingDistanceToTarget()
    {
        return navAgent.remainingDistance;
    }
    protected override float StoppingDistance()
    {
        return enemyController.stoppingDistance;
    }
}
