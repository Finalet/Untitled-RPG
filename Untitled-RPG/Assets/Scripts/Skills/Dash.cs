using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : Skill
{
    [Header("Custom Vars")]
    public float dashDistance;

    float hitID;

    public override void Use() {
        if (isCoolingDown) {
            print(skillName +" is still cooling down. " + Mathf.RoundToInt(coolDownTimer) + " seconds left.");
            return;
        }
        if (playerControlls.GetComponent<Characteristics>().Stamina < staminaRequired) {
            CanvasScript.instance.DisplayWarning("Not enough stamina!");
            return;
        }
        if (!playerControlls.isWeaponOut || playerControlls.isRolling || playerControlls.isUsingSkill)
            return;

        base.Use();
        animator.CrossFade("Attacks.DoubleSwords.Dash", 0.25f);

        hitID = generateHitID();

        StartCoroutine(Using());
    }

    IEnumerator Using () {
        yield return new WaitForSeconds (castingTime + totalAttackTime * startAttackTime);
        canHit = true;
        playerControlls.independentFromInputFwd += dashDistance;
        yield return new WaitForSeconds (totalAttackTime * (stopAttackTime - startAttackTime));
        playerControlls.independentFromInputFwd -= dashDistance;
        canHit = false;
    }

    int damage () {
        return Mathf.RoundToInt(Random.Range(baseDamage*0.7f, baseDamage*1.3f));
    }    

    void OnTriggerStay(Collider other) {
        if (other.gameObject.GetComponent<Enemy>() != null && canHit) {
            other.GetComponent<Enemy>().GetHit(damage(), hitID);
        }
    }

    float generateHitID () {
        float x = Random.Range(-100.00f, 100.00f);
        return x;
    }
}
