using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoblinWarrior : Enemy
{
    float baseAngularSpeed;
    float approachDelay = 1.7f;
    float approachDelayTimer;

    protected override void Start()
    {
        base.Start();
        navAgent = GetComponent<NavMeshAgent>();
        baseAngularSpeed = navAgent.angularSpeed;
    }

    protected override void Update()
    {
        animator.SetBool("Agr", agr);
        animator.SetBool("KnockedDown", isKnockedDown);
        base.Update();
        if (isDead || PlayerControlls.instance == null || isKnockedDown) { //Player instance is null when level is only loading.
            navAgent.isStopped = true;
            return;
        }

        if (currentState == EnemyState.Approaching) {
            animator.SetBool("Approaching", true);
        } else {
            animator.SetBool("Approaching", false);
        }
        if (currentState == EnemyState.Returning) {
            animator.SetBool("Returning", true);
        } else {
            animator.SetBool("Returning", false);
        }

        //Stops from rotating when attacking
        if (isAttacking){
            navAgent.angularSpeed = 0;
        } else {
            navAgent.angularSpeed = baseAngularSpeed;
        }
    }


    protected override void ApproachTarget () {
        if (approachDelayTimer > 0) {
            approachDelayTimer -= Time.deltaTime;
            return;
        }

        navAgent.isStopped = isGettingInterrupted ? true : false;
        navAgent.destination = target.position;
    }

    protected override void FaceTarget (bool instant = false) {
        if (isAttacking)
            return;

        if (instant) {
            StartCoroutine(InstantFaceTarget());
            return;
        }

        navAgent.isStopped = isGettingInterrupted ? true : false;

        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    protected override void AttackTarget () {
        animator.SetTrigger("Attack");
        if (distanceToPlayer <= 1.5f) {
            animator.SetInteger("AttackID", 0);
            hitType = HitType.Interrupt;
        } else {
            animator.SetInteger("AttackID", 1);
            hitType = HitType.Normal;
        }
        //play attack sound
    }

    protected override void ReturnToPosition () {
        navAgent.destination = initialPos;
    }

    protected override void Idle () {
        navAgent.isStopped = true;
        approachDelayTimer = approachDelay;
    }

    IEnumerator InstantFaceTarget () {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        while (Quaternion.Angle(transform.rotation, lookRotation) > 2) {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 30f);
            yield return null;
        }
    }
}
