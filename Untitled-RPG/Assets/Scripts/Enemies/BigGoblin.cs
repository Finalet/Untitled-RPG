using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BigGoblin : NavAgentEnemy
{

    public GameObject projectile;
    public ParticleSystem diggingVFX;
    public Transform boulderSpawnPos;

    public CapsuleCollider[] collidersToDisableWhenAttacking;

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
        
        base.Update();
    }

    protected override bool blockFaceTarget()
    {
        return isAttacking && !forceFaceTarget;
    }

    protected override void AttackTarget () {
        if (plannedAttack.attackName == "Dash Attack") {
            forceFaceTarget = true;
        } else if (plannedAttack.attackName == "Throw Stone") {
            StartCoroutine(YeetBoulder());
        }
        StartCoroutine(DisableColliders());
        UseAttack(plannedAttack);
    }


    protected override void Idle () {
        base.Idle();
        navAgent.isStopped = true;
    }
    
    IEnumerator DisableColliders () {
        for (int i = 0; i < collidersToDisableWhenAttacking.Length; i++) {
            collidersToDisableWhenAttacking[i].enabled = false;
        }
        while (!isAttacking) yield return null;
        while (isAttacking) yield return null;
            
        for (int i = 0; i < collidersToDisableWhenAttacking.Length; i++) {
            collidersToDisableWhenAttacking[i].enabled = true;
        }
    }

    IEnumerator YeetBoulder () {
        immuneToInterrupt = true;
        immuneToKnockDown = true;
        // for (int i = 0; i < collidersToDisableWhenAttacking.Length; i++) {
        //     collidersToDisableWhenAttacking[i].enabled = false;
        // }
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
        EnemyProjectile enProjectile = go.GetComponent<EnemyProjectile>();
        enProjectile.enemyDamageInfo = CalculateDamage.enemyDamageInfo(Mathf.RoundToInt(finalDamage*1.5f), enemyName);
        enProjectile.hitType = hitType;
        enProjectile.enemyName = enemyName;
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
        Vector3 dir = PlayerControlls.instance.transform.position + Vector3.up * 2.5f + PlayerControlls.instance.rb.velocity/0.8f -PlayerControlls.instance.transform.right - transform.position;
        
        go.transform.parent = null;
        go.GetComponent<Rigidbody>().isKinematic = false;
        dir = Vector3.ClampMagnitude(dir, 15);
        go.GetComponent<Rigidbody>().AddForce(dir, ForceMode.Impulse);
        enProjectile.shot = true;
        forceFaceTarget = false;

        // for (int i = 0; i < collidersToDisableWhenAttacking.Length; i++) {
        //     collidersToDisableWhenAttacking[i].enabled = true;
        // }

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
