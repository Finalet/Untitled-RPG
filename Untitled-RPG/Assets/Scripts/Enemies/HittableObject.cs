using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HittableObject : Enemy
{
    protected override void AttackTarget()
    {
        throw new System.NotImplementedException();
    }

    protected override void FaceTarget(bool instant = false)
    {
        //
    }

    protected override void Update()
    {
        RunRecurringEffects();
    }
    protected override void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;
    }

    public override void GetHit (DamageInfo damageInfo, bool stopHit = false, bool cameraShake = false, HitType hitType = HitType.Normal, Vector3 damageTextPos = new Vector3 (), float kickBackStrength = 50) {
        if (isDead || !canGetHit)
            return;
        
        int actualDamage = calculateActualDamage(damageInfo.damage);

        PlayHitParticles(); 
        PlayGetHitSounds();
        PlayStabSounds();
        
        if (stopHit || damageInfo.isCrit) StartCoroutine(HitStop(damageInfo.isCrit));
        if (cameraShake) PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.2f, 1*(1+actualDamage/3000), 0.1f, transform.position);
        
        damageTextPos = damageTextPos == Vector3.zero ? transform.position + Vector3.up * 1.5f : damageTextPos;
        Combat.instanace.DisplayDamageNumber(new DamageInfo(actualDamage, damageInfo.damageType, damageInfo.isCrit, damageInfo.sourceName), damageTextPos);

        string criticalDEBUGtext = damageInfo.isCrit ? "CRITICAL " : "";
        PeaceCanvas.instance.DebugChat($"[{System.DateTime.Now.Hour}:{System.DateTime.Now.Minute}:{System.DateTime.Now.Second}] <color=blue>{enemyName}</color> was hit with<color=red>{criticalDEBUGtext} {actualDamage} {damageInfo.damageType} damage</color> by <color=#80FFFF>{damageInfo.sourceName}</color>.");
    }
    
    protected override void PlayStabSounds () {
        int playID;
        float x = Random.Range(0f, 1f);
        if (x<0.7f) {
            playID = 0;
        } else {
            playID = 1;
        }
        audioSource.pitch = 1 + Random.Range(-0.1f, 0.1f);
        audioSource.PlayOneShot(stabSounds[playID]);
    } 
}
