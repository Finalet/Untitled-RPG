using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance; 

    [Header("Transforms")]
    Transform TwoHandedWeaponSlot;
    Transform MainHandSlot;
    Transform SecondaryHandSlot;

    [Header("Slots")]
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
        TwoHandedWeaponSlot = WeaponsController.instance.twohandedWeaponSlot;
        MainHandSlot = WeaponsController.instance.leftHipSlot;
        SecondaryHandSlot = WeaponsController.instance.rightHipSlot;
        LoadEquip();
    }

    void LoadEquip(){
        helmet.Load();
        chest.Load();
        belt.Load();
        gloves.Load();
        pants.Load();
        boots.Load();
        necklace.Load();
        ring.Load();
        secondRing.Load();
        mainHand.Load();
        secondaryHand.Load();
        bow.Load();
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

    public void EquipWeaponPrefab (Weapon weapon, bool secondary = false) {
        Transform parent;
        if (weapon.weaponType == WeaponType.TwoHanded) { //Two handed weapon
            parent = TwoHandedWeaponSlot;
        } else if (!secondary) {  // Main hand
            parent = MainHandSlot;
        } else { //Secondary hand
            parent = SecondaryHandSlot;
        }
        GameObject w = Instantiate(weapon.itemPrefab, parent);
        if (parent == TwoHandedWeaponSlot || parent == MainHandSlot) {
            WeaponsController.instance.RightHandEquipObj = w;
        } else if (parent == SecondaryHandSlot) {
            WeaponsController.instance.LeftHandEquipObj = w;
        }
    }

    public void UnequipWeaponPrefab (bool twoHanded, bool secondary = false) {
        Transform slot;
        if (twoHanded) {
            slot = TwoHandedWeaponSlot;
        } else if (!secondary) {
            slot = MainHandSlot;
        } else {
            slot = SecondaryHandSlot;
        }
        foreach (Transform child in slot) {
            Destroy(child.gameObject);
        }
    }
}
