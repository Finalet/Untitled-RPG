using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whirlwind : Skill
{
    List<GameObject> enemiesInTrigger = new List<GameObject>();

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
            playerControlls.fwd += moveSpeed;
            playerControlls.sideways += moveSpeed;
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
                playerControlls.fwd += moveSpeed;
                playerControlls.sideways += moveSpeed;
                wasFlying = false;
            }
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        if (!wasFlying) {
            playerControlls.fwd -= moveSpeed;
            playerControlls.sideways -= moveSpeed;
        }
        animator.CrossFade("Attacks.Knight.Whirlwind_end", 0.25f);
        Characteristics.instance.canGetHit = true;
    }   

    void OnTriggerEnter(Collider other) {
        if (other.GetComponent<Enemy>() != null && !other.isTrigger) {
            enemiesInTrigger.Add(other.gameObject);
        }
    }
    void OnTriggerExit(Collider other) {
        if (other.GetComponent<Enemy>() != null && !other.isTrigger) {
            enemiesInTrigger.Remove(other.gameObject);
        }
    }

    public void Hit () {
        for (int i = 0; i < enemiesInTrigger.Count; i++) {
            enemiesInTrigger[i].GetComponent<Enemy>().GetHit(damage(), skillName, false, true);
        }
    }

    void ClearTrigger () {
        for (int i = 0; i < enemiesInTrigger.Count; i++) {
            if (enemiesInTrigger[i].gameObject == null) {
                enemiesInTrigger.RemoveAt(i);
            }
        }
    }

}
