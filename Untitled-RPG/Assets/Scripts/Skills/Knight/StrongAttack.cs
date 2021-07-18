using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrongAttack : Skill
{
    List<IDamagable> damagablesInTrigger = new List<IDamagable>();

    Vector3 colliderSize;

    [Header("Custom vars")]
    public float critChance = 0.5f;
    public AudioClip[] sounds;
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

    void PlayAnimation() {
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded)
            animator.CrossFade("Attacks.Knight.StrongAttack Two handed", 0.25f);
        else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualOneHanded)
            animator.CrossFade("Attacks.Knight.StrongAttack Dual swords", 0.25f);
        else 
            animator.CrossFade("Attacks.Knight.StrongAttack Dual swords", 0.25f);
    }
    void PlaySounds () {
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded) {
            PlaySound(sounds[1], 0, characteristics.attackSpeed.x, 0, 0.4f);
        } else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualOneHanded) {
            PlaySound(sounds[0], 0, 1, 0.35f * characteristics.attackSpeed.y);
        } else {
            PlaySound(sounds[0], 0, 1, 0.35f * characteristics.attackSpeed.y);
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

    public void Hit () {
        for (int i = 0; i < damagablesInTrigger.Count; i++) {
            damagablesInTrigger[i].GetHit(CalculateDamage.damageInfo(damageType, baseDamagePercentage, critChance), skillName, true, true, HitType.Kickback);
        }
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

    public override string getDescription()
    {
        DamageInfo dmg = CalculateDamage.damageInfo(damageType, baseDamagePercentage, 0, 0);
        return $"Smash emeies with extra force and deal {dmg.damage} {dmg.damageType} damage. This skill has a {critChance*100}% chance of landing critical hits";
    }
}
