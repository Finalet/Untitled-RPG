using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goblin : Enemy
{

    [Header("Custom vars")]
    public bool playerWithinReach;

    public float attackCoolDown = 2;
    public bool isCoolingDown;
    float cooldownTimer;

    public AudioSource attackAudioSource;
    public AudioClip[] attackSounds; 
    
    protected override void Update() {
        if (PlayerControlls.instance == null) // Player instnace is null when loading level;
            return;

        base.Update();
        AI();
    }

    void AI () {
        if (distanceToPlayer <= attackRadius && !isCoolingDown && !isDead && !isKnockedDown) {
            Attack();
        }
        CoolDown();
    }

    void Attack() {
        isAttacking = true;
        animator.CrossFade("Main.Attack", 0.25f);
        PlayAttackSounds();
        cooldownTimer = attackCoolDown;
    }

    void PlayAttackSounds() {
        if (attackSounds.Length == 0)
            return;

        int x = Random.Range(0, attackSounds.Length);
        attackAudioSource.clip = attackSounds[x];
        attackAudioSource.pitch = 1 + Random.Range(-0.1f, 0.1f);
        attackAudioSource.PlayDelayed(0.3f);
    }

    void CoolDown () {
        if (cooldownTimer > 0) {
            cooldownTimer -= Time.deltaTime;
            isCoolingDown = true;
        } else {
            isCoolingDown = false;
        }
    }

    void OnTriggerStay(Collider other) {
        if (other.CompareTag("Player")) {
            playerWithinReach = true;
        }    
    }
    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            playerWithinReach = false;
        }    
    }

    void Hit () {
        if (!canHit || !playerWithinReach)
            return;

        PlayerControlls.instance.GetComponent<Characteristics>().GetHit(damage());
    }

    int damage () {
        return Mathf.RoundToInt(Random.Range(baseDamage*0.85f, baseDamage*1.15f));
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, playerDetectRadius);
    }
}
