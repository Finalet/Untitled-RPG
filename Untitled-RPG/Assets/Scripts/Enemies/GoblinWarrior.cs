using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoblinWarrior : Enemy
{
    [Space]
    public AudioClip[] hitSounds;

    protected override void Start()
    {
        base.Start();
        agrDelay = 1.7f;

        navAgent = GetComponent<NavMeshAgent>();
        navAgent.avoidancePriority = 50 + Random.Range(-20, 20);
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


    protected override void ApproachTarget () {
        base.ApproachTarget();

        navAgent.isStopped = isGettingInterrupted ? true : false;
        navAgent.destination = target.position;
    }

    protected override void FaceTarget (bool instant = false) {
        if (isAttacking || isKnockedDown || isRagdoll)
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
        base.ReturnToPosition();
        navAgent.destination = initialPos;
        navAgent.isStopped = false;
    }

    protected override void Idle () {
        base.Idle();
        navAgent.isStopped = true;
    }

    public override void Hit()
    {
        base.Hit();
        audioSource.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Length-1)]);
    }
}
