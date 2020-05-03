using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rage : Skill
{
    [Header("Custom Vars")]
    public float buffIncrease = 20;

     public void CustomUse() {
        StartCoroutine(Using());
    }

    IEnumerator Using () {
        animator.CrossFade("Attacks.DoubleSwords.Rage", 0.25f);
        yield return new WaitForSeconds(startAttackTime);
        playerControlls.isAttacking = false;
        playerControlls.isUsingSkill = false;
        characteristics.AddBuff(this);
    }
}
