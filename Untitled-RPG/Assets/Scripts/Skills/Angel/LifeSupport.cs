using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeSupport : Skill
{
    [Header("Custom Vars")]
    public AudioClip sound;
    public RecurringEffect recurringEffect;
    public Buff buff;
    public ParticleSystem circleVFX;

    bool matchVFX;
    Vector3 matchedPos;

    protected override void Update() {
        base.Update();

        if(matchVFX) {
            matchedPos.x = circleVFX.transform.position.x;
            matchedPos.y = Mathf.Lerp(matchedPos.y, playerControlls.leftHandWeaponSlot.transform.position.y, Time.deltaTime * 10) ;
            matchedPos.z = circleVFX.transform.position.z;
            circleVFX.transform.position = matchedPos;
        }
    }

    protected override void CustomUse() {
    }
    
    protected override void CastingAnim() {
        var main = circleVFX.main;
        main.startLifetime = characteristics.castingSpeed.y;
        
        buff.name = skillName;
        buff.icon = icon;
        buff.duration = recurringEffect.duration;
        buff.associatedSkill = this;

        recurringEffect.name = skillName;
        recurringEffect.damageType = damageType;
        recurringEffect.baseEffectPercentage = baseDamagePercentage;
        recurringEffect.recurringEffectType = RecurringEffectType.Healing;
        
        animator.CrossFade("Attacks.Angel.Life Support", 0.25f);     
        PlaySound(sound, 0.05f, 0.95f  * characteristics.castingSpeed.x);
        matchVFX = true;
        circleVFX.Play();
    }

    protected override void InterruptCasting()
    {
        base.InterruptCasting();
        matchVFX = false;
    }

    public void AddRecurringEffect () {
        characteristics.AddRecurringEffect(recurringEffect);
        characteristics.AddBuff(buff);
        matchVFX = false;
        finishedCast = true;
    }

    public override void OnBuffRemove()
    {
        base.OnBuffRemove();
        characteristics.RemoveRecurringEffect(recurringEffect);
    }

    public override string getDescription() {
        DamageInfo damageInfo = CalculateDamage.damageInfo(recurringEffect.damageType, recurringEffect.baseEffectPercentage, skillName, 0,0);
        return $"Restores {damageInfo.damage} health {recurringEffect.frequencyPerSecond} time per second for {recurringEffect.duration} seconds.";
    }
}
