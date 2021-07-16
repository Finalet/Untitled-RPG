using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DamageInfo {
    public int damage;
    public DamageType damageType;
    public bool isCrit;

    public DamageInfo(int _damage, DamageType _damageType, bool _isCrit) {
        this.damage = _damage;
        this.damageType = _damageType;
        this.isCrit = _isCrit;
    }
}

public static class CalculateDamage
{
    public static DamageInfo damageInfo(float rawDamage) {
        return damageInfo(rawDamage, Characteristics.instance.critChance);
    }
    public static DamageInfo damageInfo(float rawDamage, float critChance, float damageVariation = 0.15f) {
        bool isCritHit = isCrit(critChance);
        int damageBeforeCrit = Mathf.RoundToInt(Random.Range(rawDamage * (1-damageVariation), rawDamage * (1+damageVariation)));
        return new DamageInfo(isCritHit ? Mathf.RoundToInt(damageBeforeCrit * Characteristics.instance.critMultiplier) : Mathf.RoundToInt(damageBeforeCrit), DamageType.Raw, isCritHit);
    }

    public static DamageInfo damageInfo (DamageType damageType, int baseDamagePercentage) {
        return damageInfo(damageType, baseDamagePercentage, Characteristics.instance.critChance);
    }
    public static DamageInfo damageInfo (DamageType damageType, int baseDamagePercentage, float critChance, float damageVariation = 0.15f) {
        int skillTreeAdjustedDamage;
        switch (damageType) {
            case DamageType.Melee:
                skillTreeAdjustedDamage = Mathf.RoundToInt(baseDamagePercentage/100f * (float)Characteristics.instance.meleeAttack);
                break;
            case DamageType.Magic:
                skillTreeAdjustedDamage = Mathf.RoundToInt(baseDamagePercentage/100f * (float)Characteristics.instance.magicPower);
                break;
            case DamageType.Ranged:
                skillTreeAdjustedDamage = Mathf.RoundToInt(baseDamagePercentage/100f * (float)Characteristics.instance.rangedAttack);
                break;
            case DamageType.Defensive:
                skillTreeAdjustedDamage = Mathf.RoundToInt(baseDamagePercentage/100f * (float)Characteristics.instance.defense);
                break;
            case DamageType.Enemy:
                skillTreeAdjustedDamage = Mathf.RoundToInt(baseDamagePercentage/100f);
                break;
            case DamageType.NoDamage:
                skillTreeAdjustedDamage = 0;
                break;
            case DamageType.Raw:
                skillTreeAdjustedDamage = Mathf.RoundToInt(baseDamagePercentage);
                break;
            default: 
                Debug.LogError("Fuck you this can never happen");
                skillTreeAdjustedDamage = 0;
                break;
        }
        int damageBeforeCrit = Mathf.RoundToInt(Random.Range(skillTreeAdjustedDamage * (1-damageVariation), skillTreeAdjustedDamage * (1+damageVariation)));
        bool isCritHit = isCrit(critChance);
        return new DamageInfo(isCritHit ? Mathf.RoundToInt(damageBeforeCrit * Characteristics.instance.critMultiplier) : damageBeforeCrit, damageType, isCritHit);
    }

    static bool isCrit(float critChance) {
        return Random.value < critChance;
    }

    public static DamageInfo enemyDamageInfo (int baseDamage) {
        return enemyDamageInfo(baseDamage, 0); 
    }
    public static DamageInfo enemyDamageInfo (int baseDamage, float critChance = 0, float damageVariation = 0.15f) {
        int damageBeforeCrit = Mathf.RoundToInt(Random.Range(baseDamage * (1-damageVariation), baseDamage * (1+damageVariation)));
        bool isCritHit = isCrit(critChance);
        return new DamageInfo(isCritHit ? Mathf.RoundToInt(damageBeforeCrit * 2) : damageBeforeCrit, DamageType.Enemy, isCritHit);
    }
}
