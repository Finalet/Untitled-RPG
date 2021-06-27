using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hardening : Skill
{
    [Header("Custom Vars")]
    public ParticleSystem VFX;
    public Buff buff;
    
    protected override void CustomUse() {
        StartCoroutine(Using());

        buff.icon = icon;
        buff.associatedSkill = this;
        buff.healthBuff = characteristics.maxHealth;
    }

    IEnumerator Using () {
        animator.CrossFade("Attacks.Defense.Hardening start", 0.25f);
        yield return new WaitForSeconds(0.33f * (PlayerControlls.instance.GetComponent<Characteristics>().attackSpeed.y));
        audioSource.Play();
        VFX.Play();

        playerControlls.isAttacking = false;
        characteristics.AddBuff(buff);
    }

    public override string getDescription()
    {
        return $"Doubles max health for {buff.duration/60} minutes.";
    }
}
