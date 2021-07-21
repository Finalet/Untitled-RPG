using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Golem : Boss
{
    bool isDefending;
    bool walkForward;

    float defendingDuration = 3;
    float stunDuration = 5;
    float stunHitTime;
    float startedDefending;

    protected override void Update() {
        base.Update();

        if (isKnockedDown || isDead || PlayerControlls.instance == null) { //Player instance is null when level is only loading.
            if (navAgent.enabled) navAgent.isStopped = true;
            return;
        }

        isStunned = Time.time - stunHitTime < stunDuration;
        isDefending = (Time.time - startedDefending < defendingDuration) && !isStunned;
        walkForward = !isDefending && !isStunned && (currentState == EnemyState.Approaching || currentState == EnemyState.Returning);

        animator.SetBool("Defend", isDefending);
        animator.SetBool("isStunned", isStunned);
        animator.SetBool("Walk Forward", walkForward);
    }

    protected override void Start()
    {
        base.Start();

        navAgent = GetComponent<NavMeshAgent>();
        navAgent.avoidancePriority = 50 + Random.Range(-20, 20);

        //Doing this cause otherwise it will trigger at start;
        stunHitTime -= stunDuration;
        startedDefending -= defendingDuration;
    }

    protected override void AttackTarget () {
        isAttacking = true;
        string attack = distanceToPlayer < 3 ? "Hit Ground Attack" : Random.value < 0.5f ? "Punch Attack" : "Double Punch Attack";
        animator.SetTrigger(attack);
        coolDownTimer = 3;
        Invoke("TurnOffIsAttacking", 1.2f);
    }
    void TurnOffIsAttacking () {
        isAttacking = false;
    }

    protected override void ApproachTarget () {
        base.ApproachTarget();

        navAgent.isStopped = isGettingInterrupted || isDefending ? true : false;
        navAgent.destination = target.position;
    }

    protected override void FaceTarget(bool instant = false)
    {
        if (isAttacking)
            return;

        if (instant) {
            StartCoroutine(InstantFaceTarget());
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    public override void Agr() {
        if (!agr) {
            target = PlayerControlls.instance.transform;
            agr = true;
            agrDelayTimer = agrDelay;
            delayingAgr = true;
            FaceTarget();
            if (TryGetComponent(out EnemyAlarmNetwork eam)) eam.TriggerAlarm();
        }
        agrTimer = agrTime;
    }

    public override void FootStep()
    {
        base.FootStep();
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.15f, 2.5f, 0.1f, transform.position);
    }

    public override void Hit () {
        PlayerControlls.instance.GetComponent<Characteristics>().GetHit(damage(), enemyName, hitType, 0.2f, 2.5f);
    }

    public override void GetHit(DamageInfo damageInfo, string damageSourceName, bool stopHit = false, bool cameraShake = false, HitType hitType = HitType.Normal, Vector3 damageTextPos = default, float kickBackStrength = 50)
    {
        DamageInfo adjForDefenseDI = damageInfo;
        adjForDefenseDI.damage = Mathf.RoundToInt(adjForDefenseDI.damage * (isDefending ? 0.1f : 1));
        base.GetHit(adjForDefenseDI, damageSourceName, stopHit, cameraShake, hitType, damageTextPos, kickBackStrength);
        startedDefending = Time.time;
    }

    public override void OnWeakSpotHit()
    {
        base.OnWeakSpotHit();
        stunHitTime = Time.time;
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.15f, 1.5f, 0.1f, transform.position);
    }

    public void ShakeCamera () {
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.15f, 2.5f, 0.1f, transform.position);
    }
}
