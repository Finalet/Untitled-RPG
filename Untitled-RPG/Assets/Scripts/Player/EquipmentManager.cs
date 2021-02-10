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
    Transform BowSlot;

    Transform LeftHandTrans;
    Transform RightHandTrans;

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
        BowSlot = WeaponsController.instance.bowSlot;
        MainHandSlot = WeaponsController.instance.leftHipSlot;
        SecondaryHandSlot = WeaponsController.instance.rightHipSlot;
        LeftHandTrans = WeaponsController.instance.LeftHandTrans;
        RightHandTrans = WeaponsController.instance.RightHandTrans;
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
        if (weapon.weaponType == WeaponType.TwoHandedSword || weapon.weaponType == WeaponType.TwoHandedStaff) { //Two handed weapon
            if (!PlayerControlls.instance.isWeaponOut) {
                parent = TwoHandedWeaponSlot;
            } else {
                parent = RightHandTrans;
            }
        } else if (!secondary && weapon.weaponType != WeaponType.Bow) {  // Main hand
            if (!PlayerControlls.instance.isWeaponOut) {
                parent = MainHandSlot;
            } else {
                parent = RightHandTrans;
            }
        } else if (secondary && weapon.weaponType != WeaponType.Bow) { //Secondary hand
            if (!PlayerControlls.instance.isWeaponOut) {
                parent = SecondaryHandSlot;
            } else {
                parent = LeftHandTrans;
            }
        } else if (weapon.weaponType == WeaponType.Bow) {   //Bow
            parent = BowSlot; //NOT YET IMPLEMENTED IF BOW IS SUPPOSED TO BE OUT IN HAND
        } else {
            Debug.LogError("Weapon type not yet supported");
            return;
        }
        GameObject w = Instantiate(weapon.itemPrefab, parent);
        w.transform.localPosition = Vector3.zero;
        w.transform.localEulerAngles = Vector3.zero;
        w.transform.localScale = Vector3.one;
        if (parent == TwoHandedWeaponSlot || parent == MainHandSlot || parent == RightHandTrans) {
            WeaponsController.instance.RightHandEquipObj = w;
        } else if (parent == SecondaryHandSlot || parent == LeftHandTrans) {
            WeaponsController.instance.LeftHandEquipObj = w;
        } else if (parent == BowSlot) {
            WeaponsController.instance.BowObj = w;
        }
    }

    public void UnequipWeaponPrefab (bool twoHanded, bool secondary = false, bool bow = false) {
        Transform slot;
        if (twoHanded) {
            if (!PlayerControlls.instance.isWeaponOut) {
                slot = TwoHandedWeaponSlot;
            } else {
                slot = RightHandTrans;
            }
        } else if (!secondary && !bow) {
            if (!PlayerControlls.instance.isWeaponOut) {
                slot = MainHandSlot;
            } else {
                slot = RightHandTrans;
            }
        } else if (secondary && !bow) {
            if (!PlayerControlls.instance.isWeaponOut) {
                slot = SecondaryHandSlot;
            } else {
                slot = LeftHandTrans;
            }
        } else if (bow) { //Bow
            if (!PlayerControlls.instance.isWeaponOut) {
                slot = BowSlot;
            } else {
                slot = LeftHandTrans;
            }
        } else {
            Debug.LogError("Weapon type unsupported");
            return;
        }
        foreach (Transform child in slot) {
            Destroy(child.gameObject);
        }
    }
}
