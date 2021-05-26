using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrongAttack : Skill
{
    List<Enemy> enemiesInTrigger = new List<Enemy>();

    Vector3 colliderSize;

    [Header("Custom vars")]
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
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHandedSword)
            animator.CrossFade("Attacks.Knight.StrongAttack Two handed", 0.25f);
        else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualSwords)
            animator.CrossFade("Attacks.Knight.StrongAttack Dual swords", 0.25f);
        else 
            animator.CrossFade("Attacks.Knight.StrongAttack Dual swords", 0.25f);
    }
    void PlaySounds () {
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHandedSword) {
            PlaySound(sounds[1], 0, 1, 1f * characteristics.attackSpeed.y);
        } else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualSwords) {
            PlaySound(sounds[0], 0, 1, 0.35f * characteristics.attackSpeed.y);
        } else {
            PlaySound(sounds[0], 0, 1, 0.35f * characteristics.attackSpeed.y);
        }
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

    void ChooseColliderSize() {
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHandedSword) {
            colliderSize = colliderSizeTwohandedSword;
        } else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualSwords) { 
            colliderSize = colliderSizeDualSwords;
        } else {
            colliderSize = colliderSizeDualSwords;
        }
        GetComponent<BoxCollider>().size = colliderSize;
    }
}
