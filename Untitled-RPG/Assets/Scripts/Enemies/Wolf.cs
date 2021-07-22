using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Wolf : Enemy
{
    bool forceFaceTarget;
    float lastLookAround;
    public GameObject jawCollider;

    protected override void Start()
    {
        base.Start();
        agrDelay = 1.33f;

        navAgent = GetComponent<NavMeshAgent>();
        navAgent.avoidancePriority = 50 + Random.Range(-20, 20);
    }

    protected override void Update()
    {
        animator.SetBool("Agr", agr);
        animator.SetBool("KnockedDown", isKnockedDown);

        base.Update();

        if (isKnockedDown || isDead || PlayerControlls.instance == null) { //Player instance is null when level is only loading.
            if (navAgent.enabled) navAgent.isStopped = true;
            return;
        }

        animator.SetBool("Approaching", currentState == EnemyState.Approaching ? true : false);
        animator.SetBool("Returning", currentState == EnemyState.Returning ? true : false);

        if (currentState == EnemyState.Idle && Time.realtimeSinceStartup - lastLookAround >= 5) {
            float x = Random.Range(0.0f, 1.0f);
            if (x >= 0.5f){
                animator.SetTrigger("Look Around");
            }
            lastLookAround = Time.realtimeSinceStartup;
        }
    }

    protected override void AttackTarget()
    {
        isAttacking = true;
        forceFaceTarget = true;
        if (distanceToPlayer <= 2) {
            StartCoroutine(BiteAttack());
        } else if (distanceToPlayer <= 4.5f) {
            animator.SetTrigger("Pound Attack");
            hitType = HitType.Interrupt;
        }
    }
    IEnumerator BiteAttack () {
        animator.SetTrigger("Bite Attack");
        hitType = HitType.Normal;
        jawCollider.SetActive(false);
        while (animator.GetFloat("CanHit") < 0.9f) { //delay because otherwise the player is allways inside the collider and the hit is not triggered
            yield return null;
        }
        jawCollider.SetActive(true);
    }

    protected override void ApproachTarget()
    {
        base.ApproachTarget();

        navAgent.isStopped = isGettingInterrupted ? true : false;
        navAgent.destination = target.position;
    }

    protected override void FaceTarget (bool instant = false) {
        if (isAttacking && !forceFaceTarget)
            return;

        if (instant) {
            StartCoroutine(InstantFaceTarget());
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }
}
