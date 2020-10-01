using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance; 

    public UI_EquipmentSlot helmet;
    public UI_EquipmentSlot chest;
    public UI_EquipmentSlot belt;
    public UI_EquipmentSlot gloves;
    public UI_EquipmentSlot pants;
    public UI_EquipmentSlot boots;
    public UI_EquipmentSlot necklace;
    public UI_EquipmentSlot ring;
    public UI_EquipmentSlot secondRing;
    public UI_EquipmentSlot mainHand;
    public UI_EquipmentSlot secondaryHand;
    public UI_EquipmentSlot bow;

    Characteristics characteristics;

    void Awake() {
        if (instance == null)
            instance = this;
    }

    void Start() {
        characteristics = Characteristics.instance;
    }

    void FixedUpdate() {
        ResetStats();

        Weapon w = (Weapon)mainHand.itemInSlot;
        if (w != null)
            AddStats(w);
        w = (Weapon)secondaryHand.itemInSlot;
        if (w != null)
            AddStats(w);
    }

    void AddStats (Weapon w) {
        characteristics.meleeAttackFromEquip += w.MeleeAttack;
        characteristics.rangedAttackFromEquip += w.RangedAttack;
        characteristics.magicPowerFromEquip += w.MagicPower;
        characteristics.healingPowerFromEquip += w.HealingPower;
        characteristics.defenseFromEquip += w.Defense;
    }

    void ResetStats () {
        characteristics.meleeAttackFromEquip = 0;
        characteristics.rangedAttackFromEquip = 0;
        characteristics.magicPowerFromEquip = 0;
        characteristics.healingPowerFromEquip = 0;
        characteristics.defenseFromEquip = 0;
    }
}
