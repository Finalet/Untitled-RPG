using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rage : Skill
{
    [Header("Custom Vars")]
    public float buffIncrease = 20;
    public float duration;
    
    protected override void CustomUse() {
        StartCoroutine(Using());
    }

    IEnumerator Using () {
        animator.CrossFade("Attacks.Knight.Rage", 0.25f);
        yield return new WaitForSeconds(0.25f * (1-PlayerControlls.instance.GetComponent<Characteristics>().attackSpeed.y));
        audioSource.Play();
        transform.GetChild(0).GetComponent<ParticleSystem>().Play();

        playerControlls.isAttacking = false;
        characteristics.AddBuff(this);
    }
}
