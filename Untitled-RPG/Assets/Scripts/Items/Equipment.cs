using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Equipment : Item
{
    [Header("Attack Stats")]
    public int MeleeAttack;
    public int RangedAttack;
    public int MagicPower;
    public int HealingPower;
    public int Defense;
    [Space]
    public int Health;
    public int Stamina;
    [Header("Secondary Stats")]
    public int strength;
    public int agility;
    public int intellect;
    [Header("Thirdary Stats")]
    public float castingTime;
    public float attackSpeed;
    [Header("Misc")]
    public float critChance;
    public float blockChance;
    [Space]
    public Skill grantedSkill;

    public override void Use (){}
    public override void Use (UI_InventorySlot initialSlot){
        if (!(initialSlot is UI_EquipmentSlot))
            Equip(initialSlot);
        else
            Unequip(initialSlot);
    }

    protected virtual void Equip (UI_InventorySlot initialSlot) {
    }
    protected virtual void Unequip (UI_InventorySlot initialSlot) {
        InventoryManager.instance.AddItemToInventory(this, 1);
        initialSlot.ClearSlot();
    }

    public override int getItemValue()
    {
        float attacksValue = 10;
        float healthStaminaValue = 5;
        float statsValue = 50;
        float speedStatsValue = -100000;
        float critBlockValue = 100000;
        float grantedSkillValue = 5000;
        float rarityValue = 2000;

        float value = 0;
        value += attacksValue * (MeleeAttack + RangedAttack + MagicPower + HealingPower + Defense);
        value += healthStaminaValue * (Health + Stamina);
        value += statsValue * (strength + agility + intellect);
        value += speedStatsValue * (castingTime + attackSpeed);
        value += critBlockValue  * (critChance + blockChance);
        value += grantedSkill == null ? 0 : grantedSkillValue;
        value += rarityValue * (int)itemRarity;
        
        return Mathf.RoundToInt(value);
    }

    protected override void OnValidate() {
        base.OnValidate();
        itemBasePrice = getItemValue();
    }
}
