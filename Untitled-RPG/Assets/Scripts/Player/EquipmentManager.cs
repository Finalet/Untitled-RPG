using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleDrakeStudios.ModularCharacters;
using FIMSpace.FTail;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance; 

    Transform TwohandedStaffSlot;
    Transform TwohandedSwordSlot;
    Transform MainHandSlot;
    Transform SecondaryHandSlot;
    Transform BowSlot;

    Transform LeftHandTrans;
    Transform RightHandTrans;

    [Header("Slots")]
    public UI_EquipmentSlot helmet;
    public UI_EquipmentSlot chest;
    public UI_EquipmentSlot gloves;
    public UI_EquipmentSlot pants;
    public UI_EquipmentSlot boots;
    public UI_EquipmentSlot back;
    public UI_EquipmentSlot necklace;
    public UI_EquipmentSlot ring;
    public UI_EquipmentSlot secondRing;
    public UI_EquipmentSlot mainHand;
    public UI_EquipmentSlot secondaryHand;
    public UI_EquipmentSlot bow;

    [Header("Misc")]
    public TailAnimator2 capeBoneTail;

    Characteristics characteristics;
    ModularCharacterManager modularCharacterManager;

    void Awake() {
        if (instance == null)
            instance = this;

        InventoryManager.instance.Init();
    }

    void Start() {
        characteristics = Characteristics.instance;
        modularCharacterManager = GetComponent<ModularCharacterManager>();
        TwohandedStaffSlot = WeaponsController.instance.twohandedStaffSlot;
        TwohandedSwordSlot = WeaponsController.instance.twohandedSwordSlot;
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
        gloves.Load();
        pants.Load();
        boots.Load();
        back.Load();
        necklace.Load();
        ring.Load();
        secondRing.Load();
        mainHand.Load();
        secondaryHand.Load();
        bow.Load();

        AddAllStats();
        Characteristics.instance.StatsCalculations();
        Characteristics.instance.health = Characteristics.instance.maxHealth;
        Characteristics.instance.stamina = Characteristics.instance.maxStamina;
    }

    void FixedUpdate() {
        ResetStats();
        AddAllStats();
    
        capeBoneTail.enabled = back.itemInSlot == null ? false : true;
    }
    void AddAllStats () {
        if (mainHand.itemInSlot != null) AddStats((Equipment)mainHand.itemInSlot);
        if (secondaryHand.itemInSlot != null) AddStats((Equipment)secondaryHand.itemInSlot);
        if (bow.itemInSlot != null) AddStats((Equipment)bow.itemInSlot);
        if (helmet.itemInSlot != null) AddStats((Equipment)helmet.itemInSlot);
        if (chest.itemInSlot != null) AddStats((Equipment)chest.itemInSlot);
        if (gloves.itemInSlot != null) AddStats((Equipment)gloves.itemInSlot);
        if (pants.itemInSlot != null) AddStats((Equipment)pants.itemInSlot);
        if (boots.itemInSlot != null) AddStats((Equipment)boots.itemInSlot);
        if (back.itemInSlot != null) AddStats((Equipment)back.itemInSlot);
        if (necklace.itemInSlot != null) AddStats((Equipment)necklace.itemInSlot);
        if (ring.itemInSlot != null) AddStats((Equipment)ring.itemInSlot);
        if (secondRing.itemInSlot != null) AddStats((Equipment)secondRing.itemInSlot);
        //ADD ALL OTHER ITEMS
    }

    void AddStats (Equipment item) {
        characteristics.meleeAttackFromEquip += item.MeleeAttack;
        characteristics.rangedAttackFromEquip += item.RangedAttack;
        characteristics.magicPowerFromEquip += item.MagicPower;
        characteristics.healingPowerFromEquip += item.HealingPower;
        characteristics.defenseFromEquip += item.Defense;
        characteristics.strengthFromEquip += item.strength;
        characteristics.agilityFromEquip += item.agility;
        characteristics.intellectFromEquip += item.intellect;
        characteristics.healthFromEquip += item.Health;
        characteristics.staminaFromEquip += item.Stamina;
    }

    void ResetStats () {
        characteristics.meleeAttackFromEquip = 0;
        characteristics.rangedAttackFromEquip = 0;
        characteristics.magicPowerFromEquip = 0;
        characteristics.healingPowerFromEquip = 0;
        characteristics.defenseFromEquip = 0;
        characteristics.strengthFromEquip = 0;
        characteristics.agilityFromEquip = 0;
        characteristics.intellectFromEquip = 0;
        characteristics.healthFromEquip = 0;
        characteristics.staminaFromEquip = 0;
    }

    public void EquipWeaponPrefab (Weapon weapon, bool secondary = false) {
        Transform parent;
        if (weapon.weaponType == WeaponType.TwoHandedSword) { //Two handed sword
            if (!WeaponsController.instance.isWeaponOut) {
                parent = TwohandedSwordSlot;
            } else {
                parent = RightHandTrans;
            }
        } else if (weapon.weaponType == WeaponType.TwoHandedStaff) { //Two handed staff
            if (!WeaponsController.instance.isWeaponOut) {
                parent = TwohandedStaffSlot;
            } else {
                parent = RightHandTrans;
            }
        } else if (!secondary && weapon.weaponType != WeaponType.Bow) {  // Main hand
            if (!WeaponsController.instance.isWeaponOut) {
                parent = MainHandSlot;
            } else {
                parent = RightHandTrans;
            }
        } else if (secondary && weapon.weaponType != WeaponType.Bow) { //Secondary hand
            if (!WeaponsController.instance.isWeaponOut) {
                parent = SecondaryHandSlot;
            } else {
                parent = LeftHandTrans;
            }
        } else if (weapon.weaponType == WeaponType.Bow) {   //Bow
            if (!WeaponsController.instance.isBowOut) {
                parent = BowSlot;
            } else {
                parent = LeftHandTrans;
            }
        } else {
            Debug.LogError("Weapon type not yet supported");
            return;
        }
        GameObject w = Instantiate(weapon.weaponPrefab, parent);
        w.transform.localPosition = Vector3.zero;
        w.transform.localEulerAngles = Vector3.zero;
        w.transform.localScale = Vector3.one;
        if (weapon.weaponType == WeaponType.Bow) {
            WeaponsController.instance.BowObj = w;
            return;
        }
        if (parent == TwohandedSwordSlot || parent == TwohandedStaffSlot || parent == MainHandSlot || parent == RightHandTrans) {
            WeaponsController.instance.RightHandEquipObj = w;
        } else if (parent == SecondaryHandSlot || parent == LeftHandTrans) {
            WeaponsController.instance.LeftHandEquipObj = w;
        }
    }

    public void UnequipWeaponPrefab (bool twoHandedStaff, bool secondary = false, bool bow = false, bool twoHandedSword = false) {
        Transform slot;
        if (twoHandedStaff) {
            if (!WeaponsController.instance.isWeaponOut) {
                slot = TwohandedStaffSlot;
            } else {
                slot = RightHandTrans;
            }
        } else if (twoHandedSword) {
            if (!WeaponsController.instance.isWeaponOut) {
                slot = TwohandedSwordSlot;
            } else {
                slot = RightHandTrans;
            }
        } else if (!secondary && !bow) {
            if (!WeaponsController.instance.isWeaponOut) {
                slot = MainHandSlot;
            } else {
                slot = RightHandTrans;
            }
        } else if (secondary && !bow) {
            if (!WeaponsController.instance.isWeaponOut) {
                slot = SecondaryHandSlot;
            } else {
                slot = LeftHandTrans;
            }
        } else if (bow) { //Bow
            if (!WeaponsController.instance.isBowOut) {
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

    public void EquipArmorVisual(Armor item) {
        if (item.armorType == ArmorType.Helmet && !SettingsManager.instance.displayHelmet)
            return;
        if (item.armorType == ArmorType.Back && !SettingsManager.instance.displayCape)
            return;

        foreach (var part in item.modularArmor.armorParts) {
            if (part.partID > -1) {
                modularCharacterManager.ActivatePart(part.bodyType, part.partID);
                ColorPropertyLinker[] armorColors = item.modularArmor.armorColors;
                for (int i = 0; i < armorColors.Length; i++) {
                    modularCharacterManager.SetPartColor(part.bodyType, part.partID, armorColors[i].property, armorColors[i].color);
                }
            } else {
                modularCharacterManager.DeactivatePart(part.bodyType);
            }
        }
        foreach (var part in item.modularArmor.partsToDeactivateWhileWearing) {
            modularCharacterManager.DeactivatePart(part);
        }
    }
    public void UnequipArmorVisual (Armor item) {
        foreach (var part in item.modularArmor.armorParts) {
            if (part.bodyType.IsBaseBodyPart())
                modularCharacterManager.ActivatePart(part.bodyType, 0); //If its human body part, then activate naked skin
            else 
                modularCharacterManager.DeactivatePart(part.bodyType);
        }
        foreach (var part in item.modularArmor.partsToDeactivateWhileWearing) { 
            modularCharacterManager.ReturnToBaseBodypart(part);
        }
    }

    public void CheckDisplayHelmet () {
        if (helmet.itemInSlot == null) //If we dont have a helmet, then there is nothing to do
            return;

        if (SettingsManager.instance.displayHelmet) {
            EquipArmorVisual((Armor)helmet.itemInSlot);
        } else {
            UnequipArmorVisual((Armor)helmet.itemInSlot);
        }
    }
    public void CheckDisplayCape () {
        if (back.itemInSlot == null) //If we dont have a cape, then there is nothing to do
            return;

        if (SettingsManager.instance.displayCape) {
            EquipArmorVisual((Armor)back.itemInSlot);
        } else {
            UnequipArmorVisual((Armor)back.itemInSlot);
        }
    }
}
