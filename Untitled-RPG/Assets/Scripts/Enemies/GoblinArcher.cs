using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoblinArcher : Enemy
{
    [Header("Custom vars")]
    public GameObject arrowPrefab;
    public float shootStrength;
    public Transform bowTransform;
    public Transform rightHandTransform;
    
    EnemyArrow newArrow;
    LayerMask ignoreEnemy;
    Vector3 shootPoint;
    bool grabBowstring;

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

        if (isKnockedDown || isDead || PlayerControlls.instance == null) { //Player instance is null when level is only loading.
            if (navAgent.enabled) navAgent.isStopped = true;
            return;
        }    
    }

    protected override void FaceTarget (bool instant = false) {
        if (isAttacking)
            return;

        if (instant) {
            StartCoroutine(InstantFaceTarget());
            return;
        }
        
        if (navAgent.enabled) navAgent.isStopped = isGettingInterrupted ? true : false;

        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    protected override void AttackTarget () {
        animator.SetTrigger("Attack");
        hitType = HitType.Interrupt;
        grabBowstring = true;
        StartCoroutine(GrabBowstringIE());
        StartCoroutine(SpawnArrowIE());
        //play attack sound
    }
    public void Shoot() {
        if (newArrow == null)
            newArrow = Instantiate(arrowPrefab, bowTransform).GetComponent<EnemyArrow>();

        ignoreEnemy =~ LayerMask.GetMask("Enemy");

        shootPoint = PlayerControlls.instance.transform.position + Vector3.up * 1.5F + PlayerControlls.instance.rb.velocity * 0.5f * distanceToPlayer/30;

        coolDownTimer = attackCoolDown;
        newArrow.Shoot(shootStrength, shootPoint, CalculateDamage.enemyDamageInfo(baseDamage), enemyName);
        newArrow.hitType = hitType;
        bowTransform.GetComponent<Bow>().ReleaseString();
        grabBowstring = false;

        newArrow = null;

    }

    protected override void ReturnToPosition () {
        base.ReturnToPosition();
        if(navAgent.enabled) navAgent.destination = initialPos;
    }

    protected override void Idle () {
        base.Idle();
        if (navAgent.enabled) navAgent.isStopped = true;
    }

    IEnumerator GrabBowstringIE () {
        while (grabBowstring) {
            bowTransform.GetComponent<Bow>().bowstring.position = rightHandTransform.transform.position;
            yield return null;
        }
    }
    IEnumerator SpawnArrowIE () {
        newArrow = Instantiate(arrowPrefab, bowTransform).GetComponent<EnemyArrow>();
        while (newArrow != null) {
            newArrow.transform.position = rightHandTransform.position + 0.03f * newArrow.transform.forward;
            newArrow.transform.LookAt(bowTransform.transform);
            yield return null;
        }
    }
}
