using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpingHand : Skill
{
    [Header("Custom Vars")]
    public ParticleSystem VFX;
    public ParticleSystem circleVFX;
    public AudioClip sound;

    bool matchVFX;
    Vector3 matchedPos;

    protected override void Update() {
        base.Update();

        if(matchVFX) {
            matchedPos.x = circleVFX.transform.position.x;
            matchedPos.y = Mathf.Lerp(matchedPos.y, playerControlls.leftHandWeaponSlot.transform.position.y -0.5f, Time.deltaTime * 10) ;
            matchedPos.z = circleVFX.transform.position.z;
            circleVFX.transform.position = matchedPos;
        }
    }

    protected override void CustomUse() {
    }
    
    protected override void CastingAnim() {
        var main = circleVFX.main;
        main.startLifetime = characteristics.castingSpeed.y * 1.05f;

        animator.CrossFade("Attacks.Angel.Helping hand", 0.25f);     
        PlaySound(sound, 0.2f, 0.8f * characteristics.castingSpeed.x);
        matchVFX = true;
        circleVFX.Play();
    }
    protected override void InterruptCasting()
    {
        base.InterruptCasting();
        matchVFX = false;
    }

    public void Heal() {
        characteristics.GetHealed(CalculateDamage.damageInfo(damageType, baseDamagePercentage, skillName));
        playerControlls.isCasting = false;
        VFX.Play();
        matchVFX = false;
    }


    public override string getDescription() {
        DamageInfo damageInfo = CalculateDamage.damageInfo(damageType, baseDamagePercentage, skillName, 0,0);
        return $"Restored {damageInfo.damage} health.";
    }
}
