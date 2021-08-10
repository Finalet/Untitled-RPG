using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnglesBlessing : Skill
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

    public override string getDescription() {// instead of directly grabing values from buff.attackspeed, I hardcoded the description. Otherwise it would be "attack speed by -15%".
        return $"Increases your casting and attack speed by 15%, skill distance by {buff.skillDistanceBuff} meters, and walk speed by {buff.walkSpeedBuff*100}%.";
    }
}
