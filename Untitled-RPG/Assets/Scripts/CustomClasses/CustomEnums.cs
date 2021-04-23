using UnityEngine;

public enum ConsumableType {Health, Stamina, Buff};

public enum SkillTree {Knight, Hunter, Mage, Agnel, Stealth, Shield, Summoner };
public enum SkillType {Damaging, Healing, Buff };

public enum EquipmentSlotType {Helmet, Chest, Gloves, Pants, Boots, Back, Necklace, Ring, MainHand, SecondaryHand, Bow};
public enum WeaponType {OneHandedSword, OneHandedStaff, TwoHandedSword, TwoHandedStaff, Bow, Shield}
public enum ArmorType {Helmet, Chest, Gloves, Pants, Boots, Back, Necklace, Ring}

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

public class CustomEnums : MonoBehaviour
{}
