using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Wolf : NavAgentEnemy
{
    float lastLookAround;
    public GameObject jawCollider;

    protected override void Start()
    {
        base.Start();
        agrDelay = 1.33f;
    }

    protected override void Update()
    {
        base.Update();

        if (isKnockedDown || isDead || PlayerControlls.instance == null) { //Player instance is null when level is only loading.
            if (navAgent.enabled) navAgent.isStopped = true;
            return;
        }

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
        forceFaceTarget = true;
        isAttacking = true;
        if (plannedAttack.attackName == "Bite Attack") StartCoroutine(BiteAttack());
        UseAttack(plannedAttack);
    }
    IEnumerator BiteAttack () {
        hitType = HitType.Normal;
        jawCollider.SetActive(false);
        while (animator.GetFloat("CanHit") < 0.9f) { //delay because otherwise the player is allways inside the collider and the hit is not triggered
            yield return null;
        }
        jawCollider.SetActive(true);
    }

    protected override bool blockFaceTarget()
    {
        return isAttacking && !forceFaceTarget;
    }
}
