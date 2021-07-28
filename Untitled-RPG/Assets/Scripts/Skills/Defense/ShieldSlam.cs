using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldSlam : Skill
{
    List<IDamagable> damagablesInTrigger = new List<IDamagable>();

    protected override void CustomUse()
    {
        animator.CrossFade("Attacks.Defense.ShieldSlam start", 0.25f);
        PlaySound(audioSource.clip, 0, characteristics.attackSpeed.x);
    }

    public void Hit (float hitNumber) {
        HitType ht = hitNumber == 0 ? HitType.Interrupt : HitType.Knockdown;
        for (int i = 0; i < damagablesInTrigger.Count; i++) {
            damagablesInTrigger[i].GetHit(CalculateDamage.damageInfo(damageType, baseDamagePercentage), skillName, true, true, ht);
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

    public override bool skillActive()
    {
        if (WeaponsController.instance.leftHandStatus != SingleHandStatus.Shield)
            return false;
        return base.skillActive();
    }

    public override string getDescription()
    {
        DamageInfo damageInfo = CalculateDamage.damageInfo(damageType, baseDamagePercentage, 0, 0);
        return $"Use your shield to make two short hits in succession, dealing {damageInfo.damage} {damageInfo.damageType} damage each and knocking the target on the ground.\n\nShield is required.";
    }
}
