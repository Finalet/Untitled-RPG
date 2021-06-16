using UnityEngine;

public enum ConsumableType {Health, Stamina, Buff};

public enum SkillTree {Knight, Hunter, Mage, Agnel, Stealth, Defense, Summoner, Independent}
public enum SkillType {Damaging, Healing, Buff }
public enum DamageType {Melee, Ranged, Magic, Enemy}


public enum EquipmentSlotType {Helmet, Chest, Gloves, Pants, Boots, Back, Necklace, Ring, MainHand, SecondaryHand, Bow}
public enum WeaponType {OneHandedSword, OneHandedStaff, TwoHandedSword, TwoHandedStaff, Bow, Shield}
public enum ArmorType {Helmet, Chest, Gloves, Pants, Boots, Back, Necklace, Ring}
public enum ItemRarity {Common, Rare, Epic, Legendary}

[System.Serializable]
public class RecurringEffect {
    public string name;
    public SkillTree skillTree;
    public int baseEffectPercentage;
    public float frequencyPerSecond;
    public float duration;
    public ParticleSystem vfx;
    [System.NonSerialized] public float frequencyTimer; 
    [System.NonSerialized] public float durationTimer;

    public RecurringEffect (string _name, SkillTree _skillTree, int _baseEffectPercentage, float _frequencyPerSecond, float _duration, ParticleSystem _vfx, float _frequencyTimer, float _durationTimer) {
        this.name = _name;
        this.skillTree = _skillTree;
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
    public Skill skill;
    public float duration;
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

    public Buff (string _name, Skill _skill, float _duration, float _meleeAttackBuff, float _rangedAttackBuff, float _magicPowerBuff, float _healingPowerBuff, float _defenseBuff, float _castingSpeedBuff, float _attackSpeedBuff, float _walkSpeedBuff, int _skillDistanceBuff) {
        this.name = _name;
        this.skill = _skill;
        this.duration = _duration;
        this.meleeAttackBuff = _meleeAttackBuff;
        this.rangedAttackBuff = _rangedAttackBuff;
        this.magicPowerBuff = _magicPowerBuff;
        this.healingPowerBuff = _healingPowerBuff;
        this.defenseBuff = _defenseBuff;
        this.castingSpeedBuff = _castingSpeedBuff;
        this.attackSpeedBuff = _attackSpeedBuff;
        this.walkSpeedBuff = _walkSpeedBuff;
        this.skillDistanceBuff = _skillDistanceBuff;
    }
}

public class CustomEnums : MonoBehaviour
{}
