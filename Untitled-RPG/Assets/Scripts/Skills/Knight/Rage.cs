using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rage : Skill
{
    [Header("Custom Vars")]
    public float buffIncrease = 20;

     protected override void CustomUse() {
        StartCoroutine(Using());
    }

    IEnumerator Using () {
        animator.CrossFade("Attacks.DoubleSwords.Rage", 0.25f);
        yield return new WaitForSeconds(0.25f * (1-PlayerControlls.instance.GetComponent<Characteristics>().attackSpeedPercentageAdjustement));
        audioSource.Play();
        transform.GetChild(0).GetComponent<ParticleSystem>().Play();

        playerControlls.isAttacking = false;
        playerControlls.isUsingSkill = false;
        characteristics.AddBuff(this);
    }
}
