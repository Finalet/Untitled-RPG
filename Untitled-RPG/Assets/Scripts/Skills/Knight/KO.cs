using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KO : Skill
{
     float hitID;

    public void CustomUse() {
        actualDamage = Mathf.RoundToInt( baseDamage * (float)characteristics.meleeAttack/100f);
        StartCoroutine(Using());
    }

    bool knockDown;
    IEnumerator Using () {
        GenerateHitID();
        animator.CrossFade("Attacks.DoubleSwords.KO", 0.25f);
        yield return new WaitForSeconds(0.4f);
        canHit = true;
        yield return new WaitForSeconds(0.1f);
        canHit = false;
        yield return new WaitForSeconds(0.3f);
        GenerateHitID();
        canHit = true;
        knockDown = true;
        yield return new WaitForSeconds(0.1f);
        canHit = false;
        knockDown = false;
    }
    int damage () {
        return Mathf.RoundToInt(Random.Range(actualDamage*0.85f, actualDamage*1.15f));
    }    

    void OnTriggerStay(Collider other) {
        if (other.GetComponent<Enemy>() != null && !other.isTrigger && canHit) {
            if (knockDown) {
                other.GetComponent<Enemy>().GetKnockedDown();
            }
            other.GetComponent<Enemy>().GetHit(damage(), hitID);
        }
    }

    void GenerateHitID () {
        hitID = Random.Range(-100.00f, 100.00f);
    }
}
