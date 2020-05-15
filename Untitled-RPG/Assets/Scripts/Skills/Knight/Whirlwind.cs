using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whirlwind : Skill
{
    List<GameObject> enemiesInTrigger = new List<GameObject>();

    [Header("CustomVars")]
    public float moveSpeed;

    public void CustomUse() {

        actualDamage = Mathf.RoundToInt( baseDamage * (float)characteristics.meleeAttack/100f);

        StartCoroutine(Using());
    }

    public override void Update() {
        base.Update();
        ClearTrigger();
    }

    IEnumerator Using () {
        animator.CrossFade("Attacks.DoubleSwords.Whirlwind", 0.25f);
        while (!animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Attacks")).IsName("Whirlwind_loop")) {
            yield return null;
        }
        playerControlls.fwd += moveSpeed;
        playerControlls.sideways += moveSpeed;
        
        float timer = totalAttackTime;
        float hitTimer = 0.2f * 1/characteristics.attackSpeedPercentage;
        while (timer > 0) {
            timer -= Time.fixedDeltaTime;
            if (hitTimer <= 0) {
                hitTimer = 0.2f * 1/characteristics.attackSpeedPercentage;
                Hit();
            } else {
                hitTimer -= Time.fixedDeltaTime;
            }
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        playerControlls.fwd -= moveSpeed;
        playerControlls.sideways -= moveSpeed;
        animator.CrossFade("Attacks.DoubleSwords.Whirlwind_end", 0.25f);
        
    }

    int damage () {
        return Mathf.RoundToInt(Random.Range(actualDamage*0.85f, actualDamage*1.15f));
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
            enemiesInTrigger[i].GetComponent<Enemy>().GetHit(damage());
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
