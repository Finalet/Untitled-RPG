using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeTree : Skill
{
    [Header("Custom vars")]
    public float duration;
    public Transform treePrefab;
    public float skillDistance = 15;
    public Buff buff;
    public RecurringEffect effect;
    [Space]
    public AudioClip throwSound;

    protected override void Start() {
        base.Start();
        buff.name = skillName;
        buff.icon = icon;

        effect.name = skillName;
        effect.baseEffectPercentage = baseDamagePercentage;
    }

    protected override float actualDistance()
    {
        return skillDistance + characteristics.skillDistanceIncrease;
    }

    protected override void CustomUse()
    {
        animator.CrossFade("Attacks.Angel.LifeTree", 0.25f);
        PlaySound(throwSound, 0, characteristics.attackSpeed.x, 0.7f);
        Instantiate(treePrefab.gameObject, playerControlls.rightHandWeaponSlot).GetComponent<LifeTreePrefab>().Throw(this, playerControlls.rightHandWeaponSlot, pickedPosition -Vector3.up*0.3f);
    }

    public override string getDescription() {
        DamageInfo damageInfo = CalculateDamage.damageInfo(effect.damageType, effect.baseEffectPercentage);
        return $"Grow a life tree which restores your health by {damageInfo.damage} points {Mathf.RoundToInt(effect.frequencyPerSecond)} times per second when you are nearby. The tree decays and dies after {effect.duration} seconds.";
    }
}
