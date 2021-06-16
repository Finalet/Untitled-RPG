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

    public static DamageInfo damageInfo (SkillTree skillTree, int baseDamagePercentage) {
        return damageInfo(skillTree, baseDamagePercentage, baseCritChance);
    }
    public static DamageInfo damageInfo (SkillTree skillTree, int baseDamagePercentage, float critChance, float damageVariation = 0.15f) {
        int skillTreeAdjustedDamage;
        DamageType damageType;
        switch (skillTree) {
            case SkillTree.Knight:
                skillTreeAdjustedDamage = Mathf.RoundToInt(baseDamagePercentage/100f * (float)Characteristics.instance.meleeAttack);
                damageType = DamageType.Melee;
                break;
            case SkillTree.Mage:
                skillTreeAdjustedDamage = Mathf.RoundToInt(baseDamagePercentage/100f * (float)Characteristics.instance.magicPower);
                damageType = DamageType.Magic;
                break;
            case SkillTree.Hunter:
                skillTreeAdjustedDamage = Mathf.RoundToInt(baseDamagePercentage/100f * (float)Characteristics.instance.rangedAttack);
                damageType = DamageType.Ranged;
                break;
            default: 
                Debug.LogError("Fuck you this can never happen");
                damageType = DamageType.Melee;
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
    public static DamageInfo enemyDamageInfo (int baseDamage, float critChance, float damageVariation = 0.15f) {
        //appy players defense here
        int damageBeforeCrit = Mathf.RoundToInt(Random.Range(baseDamage * (1-damageVariation), baseDamage * (1+damageVariation)));
        bool isCritHit = isCrit(critChance);
        return new DamageInfo(isCritHit ? Mathf.RoundToInt(damageBeforeCrit * baseCritMultiplier) : damageBeforeCrit, DamageType.Enemy, isCritHit);
    }
}
