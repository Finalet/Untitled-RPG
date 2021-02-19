using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DamageInfo {
    public int damage;
    public bool isCrit;

    public DamageInfo(int _damage, bool _isCrit) {
        this.damage = _damage;
        this.isCrit = _isCrit;
    }
}

public static class CalculateDamage
{
    static float damageVariation = 0.15f; //How much the damage can variate in %. I.E. damge of 100 can be 85 or 115.
    static float baseCritChance = 0.1f;
    static float baseCritMultiplier = 2;

    public static DamageInfo damageInfo (SkillTree skillTree, int baseDamagePercentage) {
        return damageInfo(skillTree, baseDamagePercentage, baseCritChance);
    }
    public static DamageInfo damageInfo (SkillTree skillTree, int baseDamagePercentage, float critChance) {
        int skillTreeAdjustedDamage;
        switch (skillTree) {
            case SkillTree.Knight:
                skillTreeAdjustedDamage = Mathf.RoundToInt(baseDamagePercentage/100f * (float)Characteristics.instance.meleeAttack);
                break;
            case SkillTree.Mage:
                skillTreeAdjustedDamage = Mathf.RoundToInt(baseDamagePercentage/100f * (float)Characteristics.instance.magicPower);
                break;
            case SkillTree.Hunter:
                skillTreeAdjustedDamage = Mathf.RoundToInt(baseDamagePercentage/100f * (float)Characteristics.instance.rangedAttack);
                break;
            default: 
                Debug.LogError("Fuck you this can never happen");
                skillTreeAdjustedDamage = 0;
                break;
        }
        int damageBeforeCrit = Mathf.RoundToInt(Random.Range(skillTreeAdjustedDamage * (1-damageVariation), skillTreeAdjustedDamage * (1+damageVariation)));
        bool isCritHit = isCrit(critChance);
        return new DamageInfo(isCritHit ? Mathf.RoundToInt(damageBeforeCrit * baseCritMultiplier) : damageBeforeCrit, isCritHit);
    }

    static bool isCrit(float critChance) {
        if (Random.value < critChance) {
            return true;
        } else {
            return false;
        }
    }
}
