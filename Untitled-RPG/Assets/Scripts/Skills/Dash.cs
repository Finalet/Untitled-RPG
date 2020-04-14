using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : Skill
{
    List<Enemy> enemiesInRange;

    [Header("Custom Vars")]
    public float dashDistance;

    float hitID;

    public override void Use() {
        if (isCoolingDown) {
            print(skillName +" is still cooling down. " + Mathf.RoundToInt(coolDownTimer) + " seconds left.");
            return;
        }

        base.Use();
        animator.CrossFade("Attacks.DoubleSwords.Dash", 0.25f);

        hitID = generateHitID();

        StartCoroutine(Using());
    }

    IEnumerator Using () {
        yield return new WaitForSeconds (castingTime + totalAttackTime * startAttackTime);
        canHit = true;
        Vector3 dashVec = playerControlls.transform.forward * dashDistance;
        playerControlls.additional += dashVec;
        yield return new WaitForSeconds (totalAttackTime * (stopAttackTime - startAttackTime));
        playerControlls.additional -= dashVec;
        canHit = false;
    }

    void OnTriggerStay(Collider other) {
        if (other.gameObject.GetComponent<Enemy>() != null && canHit) {
            other.GetComponent<Enemy>().GetHit(PlayerControlls.instance.GetComponent<Combat>().currentSkillDamage, hitID);
        }
    }

    float generateHitID () {
        float x = Random.Range(-100.00f, 100.00f);
        print(x);
        return x;
    }
}
