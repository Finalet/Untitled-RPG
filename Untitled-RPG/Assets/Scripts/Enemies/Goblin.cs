using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goblin : Enemy
{

    [Header("Custom vars")]
    public GameObject healthBar;
    public bool playerWithinReach;

    public float attackCoolDown = 2;
    public bool isCoolingDown;
    float cooldownTimer;

    protected override void Start() {
        base.Start();
    }

    protected override void Update() {
        base.Update();

        ShowHealthBar();
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
        cooldownTimer = attackCoolDown;
    }

    void CoolDown () {
        if (cooldownTimer > 0) {
            cooldownTimer -= Time.deltaTime;
            isCoolingDown = true;
        } else {
            isCoolingDown = false;
            isAttacking = false;
        }
    }

    void ShowHealthBar () {
        if (isDead) {
            healthBar.SetActive(false);
            return;
        }

        if (Vector3.Distance(transform.position, PlayerControlls.instance.transform.position) <= PlayerControlls.instance.playerCamera.GetComponent<LookingTarget>().viewDistance / 1.5f) {
            healthBar.transform.GetChild(0).localScale = new Vector3((float)health/maxHealth, transform.localScale.y, transform.localScale.z);
            healthBar.transform.LookAt (PlayerControlls.instance.playerCamera.transform);
            healthBar.SetActive(true);
        } else {
            healthBar.SetActive(false);
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
        if (isGettingHit || !playerWithinReach)
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
