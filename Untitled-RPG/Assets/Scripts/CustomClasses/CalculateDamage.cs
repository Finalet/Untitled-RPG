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
    static float baseCritChance = 0.1f;
    static float baseCritMultiplier = 2;

    public static DamageInfo damageInfo (DamageType damageType, int baseDamagePercentage) {
        return damageInfo(damageType, baseDamagePercentage, baseCritChance);
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
            case DamageType.Enemy:
                skillTreeAdjustedDamage = Mathf.RoundToInt(baseDamagePercentage/100f);
                break;
            case DamageType.NoDamage:
                skillTreeAdjustedDamage = 0;
                break;
            default: 
                Debug.LogError("Fuck you this can never happen");
                skillTreeAdjustedDamage = 0;
                break;
        }
        int damageBeforeCrit = Mathf.RoundToInt(Random.Range(skillTreeAdjustedDamage * (1-damageVariation), skillTreeAdjustedDamage * (1+damageVariation)));
        bool isCritHit = isCrit(critChance);
        return new DamageInfo(isCritHit ? Mathf.RoundToInt(damageBeforeCrit * baseCritMultiplier) : damageBeforeCrit, damageType, isCritHit);
    }

    static bool isCrit(float critChance) {
        if (Random.value < critChance) {
            return true;
        } else {
            return false;
        }
    }

    public static DamageInfo enemyDamageInfo (int baseDamage) {
        return enemyDamageInfo(baseDamage, baseCritChance); 
    }
    public static DamageInfo enemyDamageInfo (int baseDamage, float critChance = 0, float damageVariation = 0.15f) {
        //appy players defense here
        int damageBeforeCrit = Mathf.RoundToInt(Random.Range(baseDamage * (1-damageVariation), baseDamage * (1+damageVariation)));
        bool isCritHit = isCrit(critChance);
        return new DamageInfo(isCritHit ? Mathf.RoundToInt(damageBeforeCrit * baseCritMultiplier) : damageBeforeCrit, DamageType.Enemy, isCritHit);
    }
}
