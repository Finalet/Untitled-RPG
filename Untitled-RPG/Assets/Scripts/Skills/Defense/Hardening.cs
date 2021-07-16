using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hardening : Skill
{
    [Header("Custom Vars")]
    public ParticleSystem VFX;
    public Buff buff;
    
    protected override void CustomUse() {
        buff.icon = icon;
        buff.associatedSkill = this;
        buff.healthBuff = characteristics.maxHealth;

        VFX.Play();
        animator.CrossFade("Attacks.Defense.Hardening", 0.25f);
        PlaySound(audioSource.clip, 0, characteristics.attackSpeed.x);
        playerControlls.isAttacking = false;
        characteristics.AddBuff(buff);
    }

    public override string getDescription()
    {
        return $"Doubles max health for {buff.duration/60} minutes.";
    }
}
