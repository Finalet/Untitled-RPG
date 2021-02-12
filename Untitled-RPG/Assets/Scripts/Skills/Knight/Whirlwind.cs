using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whirlwind : Skill
{
    List<Enemy> enemiesInTrigger = new List<Enemy>();

    [Header("CustomVars")]
    public float moveSpeed;
    public float duration;

    protected override void CustomUse() {
        StartCoroutine(Using());
    }

    protected override void Update() {
        base.Update();
        ClearTrigger();
    }
    bool wasFlying;
    IEnumerator Using () {
        animator.CrossFade("Attacks.Knight.Whirlwind", 0.25f);
        while (!animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Attacks")).IsName("Whirlwind_loop")) {
            yield return null;
        }

        if (!playerControlls.isFlying) {
            playerControlls.forceRigidbodyMovement = true;
            //I was setting speed/movement here before shifring to rigidbody
        } else {
            wasFlying = true;
        }

        Characteristics.instance.canGetHit = false;
        
        float timer = duration;
        float hitTimer = 0;
        while (timer > 0) {
            timer -= Time.fixedDeltaTime;
            if (hitTimer <= 0) {
                hitTimer = 0.2f * characteristics.attackSpeed.z;
                Hit();
                audioSource.time = 0.05f;
                audioSource.Play();
            } else {
                hitTimer -= Time.fixedDeltaTime;
            }
            //if was flying but the flight is over midway
            if (!playerControlls.isFlying && wasFlying == true) {
                //I was setting speed/movement here before shifring to rigidbody
                wasFlying = false;
            }
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        if (!wasFlying) {
            //I was setting speed/movement here before shifring to rigidbody
        }
        animator.CrossFade("Attacks.Knight.Whirlwind_end", 0.25f);
        playerControlls.forceRigidbodyMovement = false;
        Characteristics.instance.canGetHit = true;
    }   

    void OnTriggerEnter(Collider other) {
        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (en == null || other.isTrigger)
            return;

        enemiesInTrigger.Add(en);
    }
    void OnTriggerExit(Collider other) {
        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (en == null || other.isTrigger)
            return;

        enemiesInTrigger.Remove(en);
    }

    public void Hit () {
        for (int i = 0; i < enemiesInTrigger.Count; i++) {
            enemiesInTrigger[i].GetHit(CalculateDamage.damageInfo(skillTree, baseDamagePercentage), skillName, false, true);
        }
    }

    void ClearTrigger () {
        for (int i = 0; i < enemiesInTrigger.Count; i++) {
            if (enemiesInTrigger[i] == null) {
                enemiesInTrigger.RemoveAt(i);
            }
        }
    }

}
