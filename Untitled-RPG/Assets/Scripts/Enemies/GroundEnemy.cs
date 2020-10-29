using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GroundEnemy : Enemy
{
    float baseAngularSpeed;

    protected override void Start()
    {
        base.Start();
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.updatePosition = false;
        baseAngularSpeed = navAgent.angularSpeed;
    }

    protected override void Update()
    {
        base.Update();
        navAgent.nextPosition = transform.position;

        //Stops from rotating when attacking
        if (isAttacking){
            navAgent.angularSpeed = 0;
        } else {
            navAgent.angularSpeed = baseAngularSpeed;
        }
    }


    protected override void ApproachTarget () {
        animator.SetBool("Moving", true);
        navAgent.destination = target.position;
    }

    protected override void FaceTarget () {
        if (isAttacking)
            return;

        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    protected override void AttackTarget () {
        animator.SetBool("Moving", false);
        animator.CrossFade("Main.Attack", 0.1f);
        //play attack sound
    }

    protected override void ReturnToPosition () {
        animator.SetBool("Moving", true);   
        navAgent.destination = initialPos;
    }

    protected override void Idle () {
        animator.SetBool("Moving", false);
    }
}
