using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KO : Skill
{
    List<IDamagable> damagablesInTrigger = new List<IDamagable>();

    [Header("Custom vars")]
    public AudioClip[] sounds;
    public ParticleSystem twohandedSwordVFX;

    Vector3 colliderSize;
    public Vector3 colliderSizeDualSwords;
    public Vector3 colliderSizeTwohandedSword;

    protected override void Update() {
        base.Update();
        ClearTrigger();
    }

    protected override void CustomUse() { 
        ChooseColliderSize();   
        PlayAnimation();
        PlaySounds();
    }

    
    void PlayAnimation () {
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded)
            animator.CrossFade("Attacks.Knight.KO Two handed", 0.25f);
        else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualOneHanded) 
            animator.CrossFade("Attacks.Knight.KO Dual swords", 0.25f);
        else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.OneHandedPlusShield) 
            animator.CrossFade("Attacks.Knight.KO OneHanded", 0.25f);
        else 
            animator.CrossFade("Attacks.Knight.KO OneHanded", 0.25f);
    }
    void PlaySounds() {
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded) {
            PlaySound(sounds[2], 0, characteristics.attackSpeed.x);
        } else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualOneHanded) {
            Invoke("PlaySoundDualSwords", 0.15f * characteristics.attackSpeed.y);
            Invoke("PlaySoundDualSwords", 0.6f * characteristics.attackSpeed.y);
        } else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.OneHandedPlusShield) {
            Invoke("PlaySoundDualSwords", 0.25f * characteristics.attackSpeed.y);
        } else {
            Invoke("PlaySoundDualSwords", 0.25f * characteristics.attackSpeed.y);
        }
    }
    
    float x = 0;
    void PlaySoundDualSwords(){
        if (x == 0) {
            audioSource.clip = sounds[0];
            audioSource.Play();
            x = 1;
        } else {
            audioSource.clip = sounds[1];
            audioSource.Play();
            x = 0;
        }
    } 

    void OnTriggerStay(Collider other) {
        IDamagable en = other.transform.GetComponentInParent<IDamagable>();
        if (en == null || other.isTrigger)
            return;

        if (!damagablesInTrigger.Contains(en)) damagablesInTrigger.Add(en);
    }
    void OnTriggerExit(Collider other) {
        IDamagable en = other.transform.GetComponentInParent<IDamagable>();
        if (en == null || other.isTrigger)
            return;

        if (damagablesInTrigger.Contains(en)) damagablesInTrigger.Remove(en);
    }

    public void Hit (float knockDown) {
        for (int i = 0; i < damagablesInTrigger.Count; i++) {
            if (knockDown == 1) {
                damagablesInTrigger[i].GetHit(CalculateDamage.damageInfo(damageType, baseDamagePercentage), skillName, true, true, HitType.Knockdown);
            } else {
                damagablesInTrigger[i].GetHit(CalculateDamage.damageInfo(damageType, baseDamagePercentage), skillName, true, true, HitType.Normal);
            }
        }
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded)
            twohandedSwordVFX.Play();
    }

    void ClearTrigger () {
        for (int i = 0; i < damagablesInTrigger.Count; i++) {
            if (damagablesInTrigger[i] == null) {
                damagablesInTrigger.RemoveAt(i);
            }
        }
    }

    void ChooseColliderSize() {
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded) {
            colliderSize = colliderSizeTwohandedSword;
        } else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualOneHanded) { 
            colliderSize = colliderSizeDualSwords;
        } else {
            colliderSize = colliderSizeDualSwords;
        }
        GetComponent<BoxCollider>().size = colliderSize;
    }

    public override string getDescription() {
        DamageInfo dmg = CalculateDamage.damageInfo(damageType, baseDamagePercentage, 0, 0);
        return $"Knock down enemies and deal {dmg.damage} {dmg.damageType} damage.";
    }
}
