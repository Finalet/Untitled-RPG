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

    public GameObject rockShard;
    public ParticleSystem weakSpotHitPS;

    float kaiteDuration;
    float maxAllowedKaite = 7;

    [Space]
    public AudioClip weakSpotHitSound;
    public AudioClip hitGroundAttackSound;
    public AudioClip attack1Sound;
    public AudioClip attack2Sound;
    public AudioClip rockShardLoop;
    public AudioClip rockShardExplosion;

    protected override void Update() {
        base.Update();

        if (isKnockedDown || isDead || PlayerControlls.instance == null) { //Player instance is null when level is only loading.
            if (navAgent.enabled) navAgent.isStopped = true;
            return;
        }

        if (distanceToPlayer >= 20) {
            attackRange = 20;
            kaiteDuration = 0;
        }

        CheckForKaite();

        isStunned = Time.time - stunHitTime < stunDuration;
        isDefending = (Time.time - startedDefending < defendingDuration) && !isStunned;
        walkForward = !isDefending && !isStunned && (currentState == EnemyState.Approaching || currentState == EnemyState.Returning);

        animator.SetBool("Defend", isDefending);
        animator.SetBool("isStunned", isStunned);
        animator.SetBool("Walk Forward", walkForward);
    }

    void CheckForKaite () {
        if (!agr) {
            kaiteDuration = 0;
            return;
        }
        if (distanceToPlayer < 20 && distanceToPlayer > 4.5) {
            kaiteDuration += Time.deltaTime;
        }
        if (kaiteDuration > maxAllowedKaite) {
            attackRange = 20;
            kaiteDuration = 0;
        }
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
        if (distanceToPlayer < 3) {
            StartCoroutine(HitGroundAttack());
        } else if (distanceToPlayer < 4.5f) {
            if (Random.value < 0.5f) {
                StartCoroutine(PunchAttack());
            } else {
                StartCoroutine(DoublePunchAttack());
            }
        } else if (distanceToPlayer < 20) {
            StartCoroutine(CastSpellAttack());
            attackRange = 4.5f;
        } else {
            StartCoroutine(CastSpellAttack());
        }
    }
    void TurnOffIsAttacking () {
        isAttacking = false;
    }
    IEnumerator CastSpellAttack() {
        animator.SetTrigger("Cast Spell");
        yield return new WaitForSeconds(0.4f);
        ShootRockShard();
        yield return new WaitForSeconds(0.2f);
        isAttacking = false;
    }
    IEnumerator HitGroundAttack() {
        animator.SetTrigger("Hit Ground Attack");
        yield return new WaitForSeconds(0.4f);
        PlaySound(hitGroundAttackSound);
        yield return new WaitForSeconds(0.2f);
        isAttacking = false;
    }
    IEnumerator PunchAttack() {
        animator.SetTrigger("Punch Attack");
        yield return new WaitForSeconds(0.15f);
        PlaySound(attack1Sound);
        yield return new WaitForSeconds(0.2f);
        isAttacking = false;
    }
    IEnumerator DoublePunchAttack() {
        animator.SetTrigger("Double Punch Attack");
        yield return new WaitForSeconds(0.15f);
        PlaySound(attack1Sound);
        yield return new WaitForSeconds(0.4f);
        PlaySound(attack2Sound);
        yield return new WaitForSeconds(0.2f);
        isAttacking = false;
    }

    public void ShootRockShard(){
        Vector3 adjPlayerPos = target.transform.position + PlayerControlls.instance.rb.velocity * 0.5f;
        Quaternion rot = Quaternion.LookRotation(adjPlayerPos - transform.position, Vector3.up);
        GolemRockShard r = Instantiate(rockShard, transform.position, rot, null).GetComponent<GolemRockShard>();
        float dot = Vector3.Dot(PlayerControlls.instance.rb.velocity, transform.forward) * 0.5f;
        r.Init(distanceToPlayer + dot, this);
    }

    protected override void ApproachTarget () {
        base.ApproachTarget();

        navAgent.isStopped = isGettingInterrupted || isDefending || isStunned || isAttacking ? true : false;
        navAgent.destination = target.position;
    }

    protected override void FaceTarget(bool instant = false)
    {
        if (isAttacking || isStunned || isDefending)
            return;

        if (instant) {
            StartCoroutine(InstantFaceTarget());
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
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
        adjForDefenseDI.damage = Mathf.RoundToInt(adjForDefenseDI.damage * (isDefending ? 0.05f : 1));
        base.GetHit(adjForDefenseDI, damageSourceName, stopHit, cameraShake, hitType, damageTextPos, kickBackStrength);
        if (!isStunned) startedDefending = Time.time;
    }

    public override void OnWeakSpotHit()
    {
        base.OnWeakSpotHit();
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.15f, 1.5f, 0.1f, transform.position);
        if (isStunned) return;

        PlaySound(weakSpotHitSound);
        weakSpotHitPS.Play();
        stunHitTime = Time.time;
    }

    public void ShakeCamera () {
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.15f, 2.5f, 0.1f, transform.position);
    }
}
