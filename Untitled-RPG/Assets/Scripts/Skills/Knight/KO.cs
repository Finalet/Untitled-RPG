using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KO : Skill
{
    List<Enemy> enemiesInTrigger = new List<Enemy>();

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
        else 
            animator.CrossFade("Attacks.Knight.KO Dual swords", 0.25f);
    }
    void PlaySounds() {
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded) {
            PlaySound(sounds[2], 0, characteristics.attackSpeed.x);
        } else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualOneHanded) {
            Invoke("PlaySoundDualSwords", 0.15f * characteristics.attackSpeed.y);
            Invoke("PlaySoundDualSwords", 0.6f * characteristics.attackSpeed.y);
        } else {
            Invoke("PlaySoundDualSwords", 0.15f * characteristics.attackSpeed.y);
            Invoke("PlaySoundDualSwords", 0.6f * characteristics.attackSpeed.y);
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
        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (en == null || other.isTrigger)
            return;

        if (!enemiesInTrigger.Contains(en)) enemiesInTrigger.Add(en);
    }
    void OnTriggerExit(Collider other) {
        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (en == null || other.isTrigger)
            return;

        if (enemiesInTrigger.Contains(en)) enemiesInTrigger.Remove(en);
    }

    public void Hit (float knockDown) {
        for (int i = 0; i < enemiesInTrigger.Count; i++) {
            if (knockDown == 1) {
                enemiesInTrigger[i].GetHit(CalculateDamage.damageInfo(damageType, baseDamagePercentage), skillName, true, true, HitType.Knockdown);
            } else {
                enemiesInTrigger[i].GetHit(CalculateDamage.damageInfo(damageType, baseDamagePercentage), skillName, true, true, HitType.Normal);
            }
        }
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded)
            twohandedSwordVFX.Play();
    }

    void ClearTrigger () {
        for (int i = 0; i < enemiesInTrigger.Count; i++) {
            if (enemiesInTrigger[i] == null) {
                enemiesInTrigger.RemoveAt(i);
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
