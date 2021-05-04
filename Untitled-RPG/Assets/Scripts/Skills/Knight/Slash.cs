using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash : Skill
{
    AudioClip[] sounds;
    [Header("Custom vars")]
    public AudioClip[] dualSwordSounds;
    public AudioClip[] TwoHandedSwordSounds;

    List<Enemy> enemiesInCombatTrigger = new List<Enemy>();

    int hits;
    float lastHitTime;
    float timing;

    BoxCollider hitCollider;
    HitType hitType;

    Vector3[] colliderSize;
    [Header("Collider sizes")]
    public Vector3[] colliderSizeDualSwords;
    public Vector3[] colliderSizeTwohandedSword;

    protected override void Start() {
        base.Start();
        hitCollider = GetComponent<BoxCollider>();
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
        ChooseColliderSize();
        ChooseSounds();

        hits++;
        lastHitTime = Time.time;
        hitCollider.size = colliderSize[hits-1];

        SetHitType();
        PlayAnimation();
        PlaySounds();
    }

    void PlayLastDualSwordsSound () {
        PlaySound(sounds[3]);
    }

    void PlayAnimation () {
        if (hits == 1) {
            if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHandedSword)
                animator.CrossFade("Attacks.Knight.Slash_1 Two handed", 0.2f);
            else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualSwords)
                animator.CrossFade("Attacks.Knight.Slash_1 Dual swords", 0.2f);
            else 
                animator.CrossFade("Attacks.Knight.Slash_1 Dual swords", 0.2f);
        } else if (hits == 2) {
            if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHandedSword)
                animator.CrossFade("Attacks.Knight.Slash_2 Two handed", 0.2f);
            else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualSwords)
                animator.CrossFade("Attacks.Knight.Slash_2 Dual swords", 0.2f);
            else
                animator.CrossFade("Attacks.Knight.Slash_2 Dual swords", 0.2f);
            
        } else if (hits == 3) {
            if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHandedSword)
                animator.CrossFade("Attacks.Knight.Slash_3 Two handed", 0.2f);
            else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualSwords)
                animator.CrossFade("Attacks.Knight.Slash_3 Dual swords", 0.2f);
            else
                animator.CrossFade("Attacks.Knight.Slash_3 Dual swords", 0.2f);
        }
    }
    void PlaySounds () {
        if (hits == 1) {
            PlaySound(sounds[0], 0, 1, 0.15f * characteristics.attackSpeed.z);
        } else if (hits == 2) {
            PlaySound(sounds[1], 0, 1, 0.2f * characteristics.attackSpeed.z);
        } else if (hits == 3) {
            PlaySound(sounds[2], 0, 1, 0.3f * characteristics.attackSpeed.z);
            if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualSwords)
                Invoke("PlayLastDualSwordsSound", 0.45f * characteristics.attackSpeed.z); //Invoke because otherwise the sound does not play
        }
    }
    void SetHitType () {
        if (hits == 1) {
            hitType = HitType.Interrupt;
        } else if (hits == 2) {
            hitType = HitType.Interrupt;
        } else if (hits == 3) {
            hitType = HitType.Kickback;
        }  
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

    void ChooseColliderSize() {
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHandedSword) {
            colliderSize = colliderSizeTwohandedSword;
        } else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualSwords) { 
            colliderSize = colliderSizeDualSwords;
        } else {
            colliderSize = colliderSizeDualSwords;
        }
    }
    void ChooseSounds() {
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHandedSword) {
            sounds = TwoHandedSwordSounds;
        } else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualSwords) { 
            sounds = dualSwordSounds;
        } else {
            sounds = dualSwordSounds;
        }
    }
}
