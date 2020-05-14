using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goblin : Enemy
{
    public GameObject healthBar;
    public bool playerWithinReach;

    public override void Start() {
        base.Start();
    }

    public override void Update() {
        base.Update();

        ShowHealthBar();
        AI();
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

    bool one;
    void AI (){
        if (distanceToPlayer <= attackRadius && !one) {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack () {
        if (isGettingHit || isKnockedDown || isDead) 
            yield break;

        agent.isStopped = true;
        one = true;
        animator.CrossFade("Main.Attack", 0.25f);
        float timer = 1;
        while (timer > 0) {
            yield return new WaitForSeconds(Time.fixedDeltaTime);
            timer -= Time.fixedDeltaTime;
            if (isGettingHit) {
                break;
            }
        }
        one = false;
        agent.isStopped = false;
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
        if (isGettingHit)
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
