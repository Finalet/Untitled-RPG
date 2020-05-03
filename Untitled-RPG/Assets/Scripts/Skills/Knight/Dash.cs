using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : Skill
{
    [Header("Custom Vars")]
    public float dashDistance;

    float hitID;

    public void CustomUse() {
        GenerateHitID();
        actualDamage = Mathf.RoundToInt(baseDamage * (float)characteristics.meleeAttack/100f);
        StartCoroutine(Using());
    }

    IEnumerator Using () {
        animator.CrossFade("Attacks.DoubleSwords.Dash", 0.25f);
        yield return new WaitForSeconds (castingTime + totalAttackTime * startAttackTime);
        canHit = true;
        playerControlls.independentFromInputFwd += dashDistance;
        yield return new WaitForSeconds (totalAttackTime * (stopAttackTime - startAttackTime));
        playerControlls.independentFromInputFwd -= dashDistance;
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
