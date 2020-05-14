using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whirlwind : Skill
{
    [Header("CustomVars")]
    public float moveSpeed;

    float hitID;

    public void CustomUse() {

        animator.CrossFade("Attacks.DoubleSwords.Whirlwind", 0.25f);

        startAttackTime = 0.5f/totalAttackTime;
        stopAttackTime = (totalAttackTime-0.5f)/totalAttackTime;

        GenerateHitID();
        actualDamage = Mathf.RoundToInt( baseDamage * (float)characteristics.meleeAttack/100f);

        StartCoroutine(Using());
    }


    IEnumerator Using () {
        yield return new WaitForSeconds (castingTime + totalAttackTime * startAttackTime);
        playerControlls.fwd += moveSpeed;
        playerControlls.sideways += moveSpeed;
        canHit = true;
        float timer = totalAttackTime * (stopAttackTime - startAttackTime);
        float hitTimer = 0.2f * 1/characteristics.attackSpeedPercentage;
        while (timer > 0) {
            timer -= Time.fixedDeltaTime;
            if (hitTimer <= 0) {
                hitTimer = 0.2f * 1/characteristics.attackSpeedPercentage;
                GenerateHitID();
            } else {
                hitTimer -= Time.fixedDeltaTime;
            }
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        playerControlls.fwd -= moveSpeed;
        playerControlls.sideways -= moveSpeed;
        animator.CrossFade("Attacks.DoubleSwords.Whirlwind_end", 0.25f);
        canHit = false;
    }

    int damage () {
        return Mathf.RoundToInt(Random.Range(actualDamage*0.85f, actualDamage*1.15f));
    }    

    void OnTriggerStay(Collider other) {
        if (other.gameObject.GetComponent<Enemy>() != null && !other.isTrigger && canHit) {
            other.GetComponent<Enemy>().GetHit(damage(), hitID);
        }
    }

    void GenerateHitID () {
        hitID = Random.Range(-100.00f, 100.00f);
    }
}
