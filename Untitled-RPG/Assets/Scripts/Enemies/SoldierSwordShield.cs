using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierSwordShield : NavAgentEnemy
{

    [Header("Custom vars")]
    public float defenseDamageMultiplier = 0.05f;
    [Space]
    public Transform sword;
    public Transform shield;
    public Transform swordHandSpot;
    public Transform shieldHandSpot;
    public Transform swordBackSpot;
    public Transform shiedBackSpot;

    public AudioClip breakDefenseSFX;
    public ParticleSystem breakDefenseVFX;

    bool shouldntReturn;

    protected override void Start()
    {
        base.Start();
        agrDelay = 1.7f;
    }

    protected override void ReturnToPosition()
    {
        base.ReturnToPosition();
        isDefending = false;
        StartCoroutine(DelayReturnWhileSheathing());
        navAgent.isStopped = shouldntReturn;
    }
    IEnumerator DelayReturnWhileSheathing() {
        shouldntReturn = true;
        while(!animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Main")).IsName("Returning")) 
            yield return null;
        shouldntReturn = false;
    }

    protected override void Update()
    {
        base.Update();
        enemyController.speed = currentState == EnemyState.Returning ? 1.5f : isDefending ? 1.5f : 4.8f;
        immuneToInterrupt = isDefending || isStunned;
    }

    protected override void AttackTarget()
    {
        isDefending = false;
        UseAttack(plannedAttack);
        StartCoroutine(WaitForAttackOver());
    }
    IEnumerator WaitForAttackOver () {
        while (!isAttacking) yield return null;
        while (isAttacking) yield return null;
        isDefending = true;
    }

    protected override void SyncAnimator()
    {
        base.SyncAnimator();
        animator.SetBool("isDefending", isDefending);
        animator.SetBool("isStun", isStunned);
    }

    protected override int calculateActualDamage(int damage)
    {
        return Mathf.RoundToInt(base.calculateActualDamage(damage) * (isDefending ? defenseDamageMultiplier : 1) );
    }

    IEnumerator BreakDefense () {
        animator.CrossFade("GetHit.Defense Break", 0.1f);
        isDefending = false;
        isStunned = true;
        audioSource.PlayOneShot(breakDefenseSFX);
        breakDefenseVFX.Play();
        yield return new WaitForSeconds(5);
        isStunned = false;
        isDefending = true;
    }

    protected override void KickBack (float kickBackStrength = 50) {
        if (isDefending) {
            StartCoroutine(BreakDefense());
        } else {
            base.KickBack(kickBackStrength/3);
        }
    }

    protected override void KnockedDown () {
        if (isDefending) {
            StartCoroutine(BreakDefense());
        } else {
            base.KnockedDown();
        }
    }

    public void Unshethe () {
        sword.SetParent(swordHandSpot);
        sword.transform.localPosition = Vector3.zero;
        sword.transform.localRotation = Quaternion.identity;

        shield.SetParent(shieldHandSpot);
        shield.transform.localPosition = Vector3.zero;
        shield.transform.localRotation = Quaternion.identity;
    }
    public void Sheathe () {
        sword.SetParent(swordBackSpot);
        sword.transform.localPosition = Vector3.zero;
        sword.transform.localRotation = Quaternion.identity;

        shield.SetParent(shiedBackSpot);
        shield.transform.localPosition = Vector3.zero;
        shield.transform.localRotation = Quaternion.identity;
    }

    protected override void OnStateChange()
    {
        base.OnStateChange();
        if (currentState == EnemyState.Returning) animator.SetTrigger("Return");

        if (currentState == EnemyState.Approaching && previousState == EnemyState.Idle) isDefending = true;
    }

    protected override void PlayGetHitSounds () {
        if (!isDefending) base.PlayGetHitSounds();
    }
    protected override void PlayStabSounds() {
        if (isDefending) base.PlayStabSounds();
    }
}
