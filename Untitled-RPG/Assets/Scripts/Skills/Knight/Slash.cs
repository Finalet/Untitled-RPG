using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash : Skill
{
    [Header("Custom vars")]
    public AudioClip[] sounds;

    List<Enemy> enemiesInCombatTrigger = new List<Enemy>();

    int hits;
    float lastHitTime;
    float timing;

    Vector3 baseColliderSize;
    HitType hitType;

    protected override void Start() {
        base.Start();
        baseColliderSize = GetComponent<BoxCollider>().size;
    }

    public override void Use() {
        if (playerControlls.GetComponent<Characteristics>().stamina < staminaRequired) {
            CanvasScript.instance.DisplayWarning("Not enough stamina!");
            return;
        }
        if (!skillActive() || isCoolingDown || playerControlls.isRolling || playerControlls.isGettingHit || playerControlls.isCastingSkill)
            return;

        StartCoroutine(StartUse());
    }

    protected override void CustomUse() {
        if (hits == 1) {
            timing = WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHandedSword ?  0.5f * characteristics.attackSpeed.z : 0.5f * characteristics.attackSpeed.z;
        } else if (hits == 2) {
            timing = WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHandedSword ?  0.6f * characteristics.attackSpeed.z : 0.5f * characteristics.attackSpeed.z;
        } else {
            timing = WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHandedSword ?  1.2f * characteristics.attackSpeed.z : 0.74f * characteristics.attackSpeed.z;
        }

        if (Time.time - lastHitTime > timing)
            Attack();
    }

    void Attack() {
        hits++;

        lastHitTime = Time.time;

        if (hits == 1) {
            if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHandedSword)
                animator.CrossFade("Attacks.Knight.Slash_1 Two handed", 0.2f);
            else 
                animator.CrossFade("Attacks.Knight.Slash_1", 0.2f);

            hitType = HitType.Interrupt;
            PlaySound(sounds[0], 0, 1, 0.15f * characteristics.attackSpeed.z);
            GetComponent<BoxCollider>().size = baseColliderSize;
        } else if (hits == 2) {
            if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHandedSword)
                animator.CrossFade("Attacks.Knight.Slash_2 Two handed", 0.2f);
            else 
                animator.CrossFade("Attacks.Knight.Slash_2", 0.2f);

            hitType = HitType.Interrupt;
            PlaySound(sounds[1], 0, 1, 0.2f * characteristics.attackSpeed.z);
            GetComponent<BoxCollider>().size = baseColliderSize;
        } else if (hits == 3) {
            if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHandedSword)
                animator.CrossFade("Attacks.Knight.Slash_3 Two handed", 0.2f);
            else 
                animator.CrossFade("Attacks.Knight.Slash_3", 0.2f);

            hitType = HitType.Kickback;
            PlaySound(sounds[2], 0, 1, 0.3f * characteristics.attackSpeed.z);
            Invoke("PlayLastSound", 0.45f * characteristics.attackSpeed.z); //Invoke because otherwise the sound does not play

            GetComponent<BoxCollider>().size += Vector3.right;
        }      
    }

    void PlayLastSound () {
        PlaySound(sounds[3]);
    }

    protected override void Update() {
        base.Update();
        ClearTrigger();

        if (hits >= 3 || Time.time - lastHitTime > 1.5f) {
            hits = 0;
        }
    }

    void OnTriggerEnter(Collider other) {
        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (en == null || other.isTrigger)
            return;

        enemiesInCombatTrigger.Add(en);
    }
    void OnTriggerExit(Collider other) {
        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (en == null || other.isTrigger)
            return;

        enemiesInCombatTrigger.Remove(en);
    }

    public void Hit () {
        for (int i = 0; i < enemiesInCombatTrigger.Count; i++) {
            enemiesInCombatTrigger[i].GetHit(CalculateDamage.damageInfo(skillTree, baseDamagePercentage), skillName, true, true, hitType);
        }
    }

    void ClearTrigger () {
        for (int i = 0; i < enemiesInCombatTrigger.Count; i++) {
            if (enemiesInCombatTrigger[i] == null) {
                enemiesInCombatTrigger.RemoveAt(i);
            }
        }
    }
}
