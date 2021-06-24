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

    [Header("Rarity set buffs")]
    public Buff commonSetBuff;
    public Buff rareSetBuff;
    public Buff epicSetBuff;
    public Buff legendarySetBuff;

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

        CalculateSetRarityBuffs();
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

        characteristics.attackSpeedFromEquip.x *= (1-item.attackSpeed); 
        characteristics.castingSpeedFromEquip.x *= (1-item.castingTime);

        characteristics.attackSpeedFromEquip.y *= (1+item.attackSpeed); 
        characteristics.castingSpeedFromEquip.y *= (1+item.castingTime);
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

        characteristics.attackSpeedFromEquip = Vector2.one;
        characteristics.castingSpeedFromEquip = Vector2.one;
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

    public bool isSlotEquiped (ArmorType armorType, out Item equipedItem, bool secondary = false) {
        bool equiped = false;
        equipedItem = null;
        switch (armorType) {
            case ArmorType.Helmet:
                equiped = helmet.itemInSlot == null ? false : true;
                equipedItem = equiped ? helmet.itemInSlot : null;
                break;
            case ArmorType.Chest:
                equiped = chest.itemInSlot == null ? false : true;
                equipedItem = equiped ? chest.itemInSlot : null;
                break;
            case ArmorType.Gloves:
                equiped = gloves.itemInSlot == null ? false : true;
                equipedItem = equiped ? gloves.itemInSlot : null;
                break;
            case ArmorType.Pants:
                equiped = pants.itemInSlot == null ? false : true;
                equipedItem = equiped ? pants.itemInSlot : null;
                break;
            case ArmorType.Boots:
                equiped = boots.itemInSlot == null ? false : true;
                equipedItem = equiped ? boots.itemInSlot : null;
                break;
            case ArmorType.Back:
                equiped = back.itemInSlot == null ? false : true;
                equipedItem = equiped ? back.itemInSlot : null;
                break;
            case ArmorType.Necklace:
                equiped = necklace.itemInSlot == null ? false : true;
                equipedItem = equiped ? necklace.itemInSlot : null;
                break;
            case ArmorType.Ring:
                equiped = secondary ? secondRing.itemInSlot == null ? false : true : ring.itemInSlot == null ? false : true;
                equipedItem = equiped ? secondary ? secondRing.itemInSlot : ring.itemInSlot : null;
                break;
            default:
                Debug.LogError("Can't check if slot is equiped: wrong armor type");
                equiped = false;
                equipedItem = null;
                break;
        }
        return equiped;
    }
    public bool isSlotEquiped (WeaponType weaponType, out Item equipedItem, bool secondary = false) {
        bool equiped = false;
        equipedItem = null;
        switch (weaponType) {
            case WeaponType.Bow:
                equiped = bow.itemInSlot == null ? false : true;
                equipedItem = equiped ? bow.itemInSlot : null;
                break;
            case WeaponType.OneHandedStaff:
                equiped = mainHand.itemInSlot == null ? false : true;
                equipedItem = equiped ? mainHand.itemInSlot : null;
                break;
            case WeaponType.OneHandedSword:
                equiped = secondary ? secondaryHand.itemInSlot == null ? false : true : mainHand.itemInSlot == null ? false : true;
                equipedItem = equiped ? secondary ? secondaryHand.itemInSlot : mainHand.itemInSlot : null;
                break;
            case WeaponType.Shield:
                equiped = secondaryHand.itemInSlot == null ? false : true;
                equipedItem = equiped ? secondaryHand.itemInSlot : null;
                break;
            case WeaponType.TwoHandedStaff:
                equiped = mainHand.itemInSlot == null ? false : true;
                equipedItem = equiped ? mainHand.itemInSlot : null;
                break;
            case WeaponType.TwoHandedSword:
                equiped = mainHand.itemInSlot == null ? false : true;
                equipedItem = equiped ? mainHand.itemInSlot : null;
                break;
            default:
                Debug.LogError("Can't check if slot is equiped: wrong weapon type");
                equiped = false;
                equipedItem = null;
                break;
        }
        return equiped;
    }

    void CalculateSetRarityBuffs (){
        int numberOfCommonArmor = 0;
        int numberOfRareArmor = 0;
        int numberOfEpicArmor = 0;
        int numberOfLegendaryArmor = 0;
        if (helmet.itemInSlot != null) CheckItemForRarity(helmet.itemInSlot, ref numberOfCommonArmor, ref numberOfRareArmor, ref numberOfEpicArmor, ref numberOfLegendaryArmor);
        if (chest.itemInSlot != null) CheckItemForRarity(chest.itemInSlot, ref numberOfCommonArmor, ref numberOfRareArmor, ref numberOfEpicArmor, ref numberOfLegendaryArmor);
        if (gloves.itemInSlot != null) CheckItemForRarity(gloves.itemInSlot, ref numberOfCommonArmor, ref numberOfRareArmor, ref numberOfEpicArmor, ref numberOfLegendaryArmor);
        if (pants.itemInSlot != null) CheckItemForRarity(pants.itemInSlot, ref numberOfCommonArmor, ref numberOfRareArmor, ref numberOfEpicArmor, ref numberOfLegendaryArmor);
        if (boots.itemInSlot != null) CheckItemForRarity(boots.itemInSlot, ref numberOfCommonArmor, ref numberOfRareArmor, ref numberOfEpicArmor, ref numberOfLegendaryArmor);
        if (back.itemInSlot != null) CheckItemForRarity(back.itemInSlot, ref numberOfCommonArmor, ref numberOfRareArmor, ref numberOfEpicArmor, ref numberOfLegendaryArmor);
    
        if (numberOfCommonArmor >= 5) characteristics.AddBuff(commonSetBuff); else characteristics.RemoveBuff(commonSetBuff);
        if (numberOfRareArmor >= 5) characteristics.AddBuff(rareSetBuff); else characteristics.RemoveBuff(rareSetBuff);
        if (numberOfEpicArmor >= 5) characteristics.AddBuff(epicSetBuff); else characteristics.RemoveBuff(epicSetBuff);
        if (numberOfLegendaryArmor >= 5) characteristics.AddBuff(legendarySetBuff); else characteristics.RemoveBuff(legendarySetBuff);
    }

    void CheckItemForRarity (Item item, ref int common, ref int rare, ref int epic, ref int legendary) {
        switch (item.itemRarity) {
            case ItemRarity.Common: common++; break;
            case ItemRarity.Rare: rare++; break;
            case ItemRarity.Epic: epic++; break;
            case ItemRarity.Legendary: legendary++; break;
        }
    }
}
