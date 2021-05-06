using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BigGoblin : Enemy
{

    public GameObject projectile;
    public ParticleSystem diggingVFX;
    public Transform boulderSpawnPos;
    bool forceFaceTarget;

    protected override void Start()
    {
        base.Start();
        agrDelay = 2.7f;

        navAgent = GetComponent<NavMeshAgent>();
        navAgent.avoidancePriority = 50 + Random.Range(-20, 20);
    }

    protected override void Update()
    {   
        if (PlayerControlls.instance == null)
            return;

        if (distanceToPlayer >= 10)
            attackRange = 10;

        animator.SetBool("Agr", agr);
        animator.SetBool("KnockedDown", isKnockedDown);
        
        base.Update();

        if (isKnockedDown || isDead || PlayerControlls.instance == null) { //Player instance is null when level is only loading.
            if (navAgent.enabled) navAgent.isStopped = true;
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
    }


    protected override void ApproachTarget () {
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

    protected override void AttackTarget () {
        isAttacking = true;
        animator.SetTrigger("Attack");
        if (distanceToPlayer <= 2) { // Close attack
            animator.SetInteger("AttackID", 0); 
            hitType = HitType.Interrupt;
            coolDownTimer = 3;
            forceFaceTarget = false;
        } else if (distanceToPlayer <= 4) { //Medium Attack
            animator.SetInteger("AttackID", 1); //Medium attack
            hitType = HitType.Interrupt;
            coolDownTimer = 4;
            forceFaceTarget = false;
        } else if (distanceToPlayer <= 7) {
            animator.SetInteger("AttackID", 2); //Dash attack
            hitType = HitType.Interrupt;
            coolDownTimer = 5;
            forceFaceTarget = true;
        } else if (distanceToPlayer <= 10) {
            animator.SetInteger("AttackID", 3); //Throw stone attack
            hitType = HitType.Interrupt;
            coolDownTimer = 7;
            StartCoroutine(YeetBoulder());
            attackRange = 6;
        }
        //play attack sound
    }

    protected override void ReturnToPosition () {
        base.ReturnToPosition();
        navAgent.destination = initialPos;
    }

    protected override void Idle () {
        base.Idle();
        navAgent.isStopped = true;
    }
    
    IEnumerator YeetBoulder () {
        immuneToInterrupt = true;
        immuneToKnockDown = true;
        while(!animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Attacks")).IsName("Throw Stone")) {
            yield return null;
        }
        while (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Attacks")).normalizedTime < 0.15f) {
            if (isDead)
                yield break;
            yield return null;
        }
        diggingVFX.Play();
        while (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Attacks")).normalizedTime < 0.18f) {
            if (isDead) {
                yield break;
            }
            yield return null;
        }
        Quaternion randRot = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        GameObject go = Instantiate(projectile, boulderSpawnPos);
        go.SetActive(true);
        go.GetComponent<EnemyProjectile>().baseDamage = Mathf.RoundToInt(baseDamage*1.5f);
        go.GetComponent<EnemyProjectile>().hitType = hitType;
        while (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Attacks")).normalizedTime < 0.3f) {
            if (isDead) {
                Destroy(go);
                yield break;
            }
            yield return null;
        }
        diggingVFX.Stop();
        while (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Attacks")).normalizedTime < 0.67f) {
            forceFaceTarget = true;
            FaceTarget();
            if (isDead) {
                Destroy(go);
                yield break;
            }
            yield return null;
        }
        Vector3 dir = PlayerControlls.instance.transform.position + Vector3.up * 2.5f + PlayerControlls.instance.rb.velocity/0.8f - transform.position;
        go.transform.parent = null;
        go.GetComponent<Rigidbody>().isKinematic = false;
        dir = Vector3.ClampMagnitude(dir, 15);
        go.GetComponent<Rigidbody>().AddForce(dir, ForceMode.Impulse);
        go.GetComponent<EnemyProjectile>().shot = true;
        forceFaceTarget = false;

        immuneToInterrupt = false;
        immuneToKnockDown = false;
    }

    public override void FootStep()
    {
        base.FootStep();
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.15f, 0.8f, 0.1f, transform.position);
    }
    public override void PlayAttackSound(AnimationEvent animationEvent)
    {
        if (animationEvent.intParameter == 1)
            audioSource.loop = true;
        else
            audioSource.loop = false;
        base.PlayAttackSound(animationEvent);
    }
}
