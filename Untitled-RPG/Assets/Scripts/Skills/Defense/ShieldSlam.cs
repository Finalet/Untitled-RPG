using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldSlam : Skill
{
    List<Enemy> enemiesInTrigger = new List<Enemy>();

    public AudioClip[] wooshes;

    protected override void CustomUse()
    {
        animator.CrossFade("Attacks.Defense.ShieldSlam start", 0.25f);
        Invoke("PlayWooshSound", 0.35f * characteristics.attackSpeed.y);
        Invoke("PlayWooshSound", 0.8f * characteristics.attackSpeed.y);
    }

    public void Hit (float hitNumber) {
        HitType ht = hitNumber == 0 ? HitType.Interrupt : HitType.Knockdown;
        for (int i = 0; i < enemiesInTrigger.Count; i++) {
            enemiesInTrigger[i].GetHit(CalculateDamage.damageInfo(damageType, baseDamagePercentage), skillName, true, true, ht);
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

    int x;
    void PlayWooshSound() {
        if (x == 0) {
            PlaySound(wooshes[0]);
            x = 1;
        } else if (x == 1) {
            PlaySound(wooshes[1]);
            x = 0;
        }
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
