using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrongAttack : Skill
{
    float hitID;

    public void CustomUse() {
        GenerateHitID();
        actualDamage = Mathf.RoundToInt( baseDamage * (float)characteristics.meleeAttack/100f);
        StartCoroutine(Using());
    }

    IEnumerator Using () {
        animator.CrossFade("Attacks.DoubleSwords.StrongAttack", 0.25f);
        yield return new WaitForSeconds (castingTime + totalAttackTime * startAttackTime);
        canHit = true;
        yield return new WaitForSeconds (totalAttackTime * (stopAttackTime - startAttackTime));
        canHit = false;
    }

    int damage () {
        return Mathf.RoundToInt(Random.Range(actualDamage*0.85f, actualDamage*1.15f));
    }    

    void OnTriggerStay(Collider other) {
        if (other.gameObject.GetComponent<Enemy>() != null && canHit) {
            other.GetComponent<Enemy>().GetHit(damage(), hitID);
        }
    }

    void GenerateHitID () {
        hitID = Random.Range(-100.00f, 100.00f);
    }
}
