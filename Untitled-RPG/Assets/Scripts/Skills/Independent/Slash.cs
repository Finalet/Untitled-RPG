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
    public Vector3[] colliderSizeDualHands;
    public Vector3[] colliderSizeTwohanded;
    public Vector3[] colliderSizeOnahanded;

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
            if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded)
                timing = 0.5f * characteristics.attackSpeed.y;
            else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualOneHanded)
                timing = 0.5f * characteristics.attackSpeed.y;
            else 
                timing = 0.5f * characteristics.attackSpeed.y;

        } else if (hits == 2) {
            if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded)
                timing = 0.6f * characteristics.attackSpeed.y;
            else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualOneHanded)
                timing = 0.5f * characteristics.attackSpeed.y;
            else 
                timing = 0.5f * characteristics.attackSpeed.y;
        } else {
            if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded)
                timing = 1.2f * characteristics.attackSpeed.y;
            else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualOneHanded)
                timing = 0.74f * characteristics.attackSpeed.y;
            else 
                timing = 0.74f * characteristics.attackSpeed.y;
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
            if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded)
                animator.CrossFade("Attacks.Knight.Slash_1 Two handed", 0.2f);
            else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualOneHanded)
                animator.CrossFade("Attacks.Knight.Slash_1 Dual swords", 0.2f);
            else 
                animator.CrossFade("Attacks.Knight.Slash_1 One handed", 0.2f);
        } else if (hits == 2) {
            if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded)
                animator.CrossFade("Attacks.Knight.Slash_2 Two handed", 0.2f);
            else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualOneHanded)
                animator.CrossFade("Attacks.Knight.Slash_2 Dual swords", 0.2f);
            else
                animator.CrossFade("Attacks.Knight.Slash_2 One handed", 0.2f);
            
        } else if (hits == 3) {
            if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded)
                animator.CrossFade("Attacks.Knight.Slash_3 Two handed", 0.2f);
            else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualOneHanded)
                animator.CrossFade("Attacks.Knight.Slash_3 Dual swords", 0.2f);
            else
                animator.CrossFade("Attacks.Knight.Slash_3 One handed", 0.2f);
        }
    }
    void PlaySounds () {
        float delayMult = WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded || WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualOneHanded ? 1 : 0.8f;
        if (hits == 1) {
            PlaySound(sounds[0], 0, 1, 0.15f * characteristics.attackSpeed.y);
        } else if (hits == 2) {
            PlaySound(sounds[1], 0, 1, 0.2f * characteristics.attackSpeed.y * delayMult);
        } else if (hits == 3) {
            PlaySound(sounds[2], 0, 1, 0.3f * characteristics.attackSpeed.y * delayMult);
            if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualOneHanded)
                Invoke("PlayLastDualSwordsSound", 0.45f * characteristics.attackSpeed.y); //Invoke because otherwise the sound does not play
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

    void OnTriggerStay(Collider other) {
        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (en == null || other.isTrigger)
            return;

        if (!enemiesInCombatTrigger.Contains(en)) enemiesInCombatTrigger.Add(en);
    }
    void OnTriggerExit(Collider other) {
        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (en == null || other.isTrigger)
            return;

        if (enemiesInCombatTrigger.Contains(en)) enemiesInCombatTrigger.Remove(en);
    }

    public void Hit () {
        for (int i = 0; i < enemiesInCombatTrigger.Count; i++) {
            enemiesInCombatTrigger[i].GetHit(CalculateDamage.damageInfo(damageType, baseDamagePercentage), skillName, true, true, hitType);
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
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded) {
            colliderSize = colliderSizeTwohanded;
        } else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualOneHanded) { 
            colliderSize = colliderSizeDualHands;
        } else {
            colliderSize = colliderSizeOnahanded;
        }
    }
    void ChooseSounds() {
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded) {
            sounds = TwoHandedSwordSounds;
        } else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualOneHanded) { 
            sounds = dualSwordSounds;
        } else {
            sounds = dualSwordSounds;
        }
    }

    public override string getDescription() {
        DamageInfo dmg = CalculateDamage.damageInfo(damageType, baseDamagePercentage, 0, 0);
        return $"Simple slashes that deal {dmg.damage} {dmg.damageType} damage.";
    }
}
