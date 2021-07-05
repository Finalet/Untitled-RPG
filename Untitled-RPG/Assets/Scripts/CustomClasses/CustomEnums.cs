using UnityEngine;

public enum ConsumableType {Health, Stamina, Buff};

public enum SkillTree {Knight, Hunter, Mage, Angel, Stealth, Defense, Summoner, Independent}
public enum SkillType {Damaging, Healing, Buff }
public enum DamageType {Melee, Ranged, Magic, Enemy, NoDamage, Raw}


public enum EquipmentSlotType {Helmet, Chest, Gloves, Pants, Boots, Back, Necklace, Ring, MainHand, SecondaryHand, Bow}
public enum WeaponCategory {Sword, Staff, Bow, Shield, Axe}
public enum WeaponHand {OneHanded, TwoHanded, SecondaryHand, BowHand};
public enum ArmorType {Helmet, Chest, Gloves, Pants, Boots, Back, Necklace, Ring}
public enum ItemRarity {Common, Uncommon, Rare, Epic, Legendary, Relic}

[System.Serializable]
public class RecurringEffect {
    public string name;
    public SkillTree skillTree;
    public DamageType damageType;
    public int baseEffectPercentage;
    public float frequencyPerSecond;
    public float duration;
    public ParticleSystem vfx;
    [System.NonSerialized] public float frequencyTimer; 
    [System.NonSerialized] public float durationTimer;

    public RecurringEffect (string _name, SkillTree _skillTree, DamageType _damageType, int _baseEffectPercentage, float _frequencyPerSecond, float _duration, ParticleSystem _vfx, float _frequencyTimer, float _durationTimer) {
        this.name = _name;
        this.skillTree = _skillTree;
        this.damageType = _damageType;
        this.baseEffectPercentage = _baseEffectPercentage;
        this.frequencyPerSecond = _frequencyPerSecond;
        this.duration = _duration;
        this.vfx = _vfx;
        this.frequencyTimer = _frequencyTimer;
        this.durationTimer = _durationTimer;
    }
}

[System.Serializable]
public class Buff {
    public string name;
    public string description;
    public Sprite icon;
    public float duration;
    [Space]
    public int healthBuff;
    public int staminaBuff;
    [Space]
    public int strengthBuff;
    public int agilityBuff;
    public int intellectBuff;
    [Space]
    public float meleeAttackBuff;
    public float rangedAttackBuff;
    public float magicPowerBuff;
    public float healingPowerBuff;
    public float defenseBuff;
    [Space]
    public float castingSpeedBuff;
    public float attackSpeedBuff;
    [Space]
    public float walkSpeedBuff;
    [Space]
    public int skillDistanceBuff;
    [Space]
    public bool immuneToDamage;
    public bool immuneToInterrupt;
    [Space]
    public Skill associatedSkill;

    public Buff(string name, string description, Sprite icon, float duration, int healthBuff, int staminaBuff, int strengthBuff, int agilityBuff, int intellectBuff, float meleeAttackBuff, float rangedAttackBuff, float magicPowerBuff, float healingPowerBuff, float defenseBuff, float castingSpeedBuff, float attackSpeedBuff, float walkSpeedBuff, int skillDistanceBuff, bool immuneToDamage, bool immuneToInterrupt, Skill associatedSkill)
    {
        this.name = name;
        this.description = description;
        this.icon = icon;
        this.duration = duration;
        this.healthBuff = healthBuff;
        this.staminaBuff = staminaBuff;
        this.strengthBuff = strengthBuff;
        this.agilityBuff = agilityBuff;
        this.intellectBuff = intellectBuff;
        this.meleeAttackBuff = meleeAttackBuff;
        this.rangedAttackBuff = rangedAttackBuff;
        this.magicPowerBuff = magicPowerBuff;
        this.healingPowerBuff = healingPowerBuff;
        this.defenseBuff = defenseBuff;
        this.castingSpeedBuff = castingSpeedBuff;
        this.attackSpeedBuff = attackSpeedBuff;
        this.walkSpeedBuff = walkSpeedBuff;
        this.skillDistanceBuff = skillDistanceBuff;
        this.immuneToDamage = immuneToDamage;
        this.immuneToInterrupt = immuneToInterrupt;
        this.associatedSkill = associatedSkill;
    }
}
