using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrongAttack : Skill
{
    List<Enemy> enemiesInTrigger = new List<Enemy>();

    protected override void Update() {
        base.Update();
        ClearTrigger();
    }

    protected override void CustomUse() {
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHandedSword)
            animator.CrossFade("Attacks.Knight.StrongAttack Two handed", 0.25f);
        else 
            animator.CrossFade("Attacks.Knight.StrongAttack", 0.25f);

        audioSource.PlayDelayed(0.35f * characteristics.attackSpeed.z);
    }  

    void OnTriggerEnter(Collider other) {
        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (en == null || other.isTrigger)
            return;

        enemiesInTrigger.Add(en);
    }
    void OnTriggerExit(Collider other) {
        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (en == null || other.isTrigger)
            return;

        enemiesInTrigger.Remove(en);
    }

    public void Hit () {
        for (int i = 0; i < enemiesInTrigger.Count; i++) {
            enemiesInTrigger[i].GetHit(CalculateDamage.damageInfo(skillTree, baseDamagePercentage, 0.5f), skillName, true, true, HitType.Kickback);
        }
    }

    void ClearTrigger () {
        for (int i = 0; i < enemiesInTrigger.Count; i++) {
            if (enemiesInTrigger[i] == null) {
                enemiesInTrigger.RemoveAt(i);
            }
        }
    }
}
