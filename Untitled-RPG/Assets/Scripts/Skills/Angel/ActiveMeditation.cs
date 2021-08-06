using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveMeditation : Skill
{
    [Header("Custom Vars")]
    public ParticleSystem VFX;
    public Buff buff;
    
    protected override void CustomUse() {
        buff.icon = icon;
        buff.associatedSkill = this;

        VFX.Play();
        animator.CrossFade("Attacks.Angel.Angles Blessing", 0.25f);
        PlaySound(audioSource.clip, 0, characteristics.attackSpeed.x);
        playerControlls.isAttacking = false;
        characteristics.AddBuff(buff);
    }

    public override string getDescription() {
        return $"Increase all your attacks power and defense by {buff.meleeAttackBuff*100}% for {buff.duration} seconds.";
    }
}
