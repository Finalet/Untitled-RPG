using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleDrakeStudios.ModularCharacters;
using FIMSpace.FTail;

public class EquipmentManager : MonoBehaviour, ISavable
{
    public static EquipmentManager instance; 

    Transform TwohandedStaffSlot;
    Transform TwohandedSwordSlot;
    Transform MainHandSlot;
    Transform SecondaryHandSlot;
    Transform BowSlot;
    Transform ShieldSlot;

    Transform LeftHandTrans;
    Transform RightHandTrans;
    Transform LeftHandShieldTrans;

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
    public Buff uncommonSetBuff;
    public Buff rareSetBuff;
    public Buff epicSetBuff;
    public Buff legendarySetBuff;
    public Buff relicSetBuff;
    [Header("Weapons buffs")]
    public Buff shieldBuff;
    public Buff dualHandsBuff;
    public Buff axeBuff;
    public Buff TwoHandedSwordBuff;
    public Buff TwoHandedStaffBuff;

    [Header("Misc")]
    public TailAnimator2 capeBoneTail;

    Characteristics characteristics;
    ModularCharacterManager modularCharacterManager;

    void Awake() {
        if (instance == null)
            instance = this;
        
        modularCharacterManager = GetComponent<ModularCharacterManager>();
    }

    void Start() {
        SaveManager.instance.saveObjects.Add(this);

        characteristics = Characteristics.instance;
        TwohandedStaffSlot = WeaponsController.instance.twohandedStaffSlot;
        TwohandedSwordSlot = WeaponsController.instance.twohandedSwordSlot;
        BowSlot = WeaponsController.instance.bowSlot;
        ShieldSlot = WeaponsController.instance.shieldBackSlot;
        MainHandSlot = WeaponsController.instance.leftHipSlot;
        SecondaryHandSlot = WeaponsController.instance.rightHipSlot;
        LeftHandTrans = WeaponsController.instance.LeftHandTrans;
        RightHandTrans = WeaponsController.instance.RightHandTrans;
        LeftHandShieldTrans = WeaponsController.instance.LeftHandShieldTrans;
    }

    void LoadEquip(){
        helmet.LoadSlot();
        chest.LoadSlot();
        gloves.LoadSlot();
        pants.LoadSlot();
        boots.LoadSlot();
        back.LoadSlot();
        necklace.LoadSlot();
        ring.LoadSlot();
        secondRing.LoadSlot();
        mainHand.LoadSlot();
        secondaryHand.LoadSlot();
        bow.LoadSlot();

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

        characteristics.attackSpeedFromEquip.x *= (1-item.attackSpeed); 
        characteristics.castingSpeedFromEquip.x *= (1-item.castingTime);

        characteristics.attackSpeedFromEquip.y *= (1+item.attackSpeed); 
        characteristics.castingSpeedFromEquip.y *= (1+item.castingTime);

        characteristics.critChanceFromEquip += item.critChance;
        characteristics.critStrengthFromEquip += item.critStrength;
        characteristics.blockChanceFromEquip += item.blockChance;
        characteristics.walkSpeedFromEquipment *= (1+item.walkSpeed);
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

        characteristics.critChanceFromEquip = 0;
        characteristics.critStrengthFromEquip = 0;
        characteristics.blockChanceFromEquip = 0;
        characteristics.walkSpeedFromEquipment = 1;
    }

    public void EquipWeaponPrefab (Weapon weapon, bool secondary = false) {
        Transform parent = null;
        if (weapon.weaponHand == WeaponHand.TwoHanded) { //Two handed;
            if (weapon.weaponCategory == WeaponCategory.Sword) { //two handed sword
                if (!WeaponsController.instance.isWeaponOut) {
                    parent = TwohandedSwordSlot;
                } else {
                    parent = RightHandTrans;
                }
            } else if (weapon.weaponCategory == WeaponCategory.Staff) { //two handed staff
                if (!WeaponsController.instance.isWeaponOut) {
                    parent = TwohandedStaffSlot;
                } else {
                    parent = RightHandTrans;
                }
            }
        } else if (weapon.weaponHand == WeaponHand.OneHanded) { //One handed
            if (!secondary) {   //Main hand
                if (!WeaponsController.instance.isWeaponOut) {
                    parent = MainHandSlot;
                } else {
                    parent = RightHandTrans;
                }
            } else { //Secondary hand
                if (!WeaponsController.instance.isWeaponOut) {
                    parent = SecondaryHandSlot;
                } else {
                    parent = LeftHandTrans;
                }
            }
        } else if (weapon.weaponHand == WeaponHand.BowHand) { //Bow
            if (!WeaponsController.instance.isBowOut) {
                parent = BowSlot;
            } else {
                parent = LeftHandTrans;
            }
        } else if (weapon.weaponHand == WeaponHand.SecondaryHand) {
            if (weapon.weaponCategory == WeaponCategory.Shield) {
                if (!WeaponsController.instance.isWeaponOut) {
                    parent = ShieldSlot;
                } else {
                    parent = LeftHandShieldTrans;
                }
            } else {
                if (!WeaponsController.instance.isWeaponOut) {
                    parent = SecondaryHandSlot;
                } else {
                    parent = LeftHandTrans;
                }
            }
        } else {
            Debug.LogError($"Equiping prefab of {weapon.weaponHand} {weapon.weaponCategory} is not supported yet.");
            return;
        }

        GameObject w = Instantiate(weapon.weaponPrefab, parent);
        w.transform.localPosition = Vector3.zero;
        w.transform.localEulerAngles = Vector3.zero;
        w.transform.localScale = Vector3.one;
        if (weapon.weaponCategory == WeaponCategory.Bow) {
            WeaponsController.instance.BowObj = w;
            return;
        }
        if (parent == TwohandedSwordSlot || parent == TwohandedStaffSlot || parent == MainHandSlot || parent == RightHandTrans) {
            WeaponsController.instance.RightHandEquipObj = w;
        } else if (parent == SecondaryHandSlot || parent == LeftHandTrans || parent == ShieldSlot || parent == LeftHandShieldTrans) {
            WeaponsController.instance.LeftHandEquipObj = w;
        }
    }

    public void UnequipWeaponPrefab (Weapon weapon, bool secondary = false) {
        Transform slot = null;
        if (weapon.weaponHand == WeaponHand.TwoHanded) {
            if (weapon.weaponCategory == WeaponCategory.Sword) {
                if (!WeaponsController.instance.isWeaponOut) {
                    slot = TwohandedSwordSlot;
                } else {
                    slot = RightHandTrans;
                }
            } else if (weapon.weaponCategory == WeaponCategory.Staff) {
                if (!WeaponsController.instance.isWeaponOut) {
                    slot = TwohandedStaffSlot;
                } else {
                    slot = RightHandTrans;
                }
            }
        } else if (weapon.weaponHand == WeaponHand.OneHanded) {
            if (!secondary) {
                if (!WeaponsController.instance.isWeaponOut) {
                    slot = MainHandSlot;
                } else {
                    slot = RightHandTrans;
                }
            } else {
                if (!WeaponsController.instance.isWeaponOut) {
                    slot = SecondaryHandSlot;
                } else {
                    slot = LeftHandTrans;
                }
            }
        } else if (weapon.weaponHand == WeaponHand.BowHand) {
            if (!WeaponsController.instance.isBowOut) {
                slot = BowSlot;
            } else {
                slot = LeftHandTrans;
            }
        } else if (weapon.weaponHand == WeaponHand.SecondaryHand) {
            if (weapon.weaponCategory == WeaponCategory.Shield) {
                if (!WeaponsController.instance.isWeaponOut) {
                    slot = ShieldSlot;
                } else {
                    slot = LeftHandShieldTrans;
                }
            } else {
                if (!WeaponsController.instance.isWeaponOut) {
                    slot = SecondaryHandSlot;
                } else {
                    slot = LeftHandTrans;
                }
            }
        } else {
            Debug.LogError($"{weapon.weaponHand} {weapon.weaponCategory} unsupported");
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
    public bool isSlotEquiped (WeaponHand weaponHand, out Item equipedItem, bool secondary = false) {
        bool equiped = false;
        equipedItem = null;
        switch (weaponHand) {
            case WeaponHand.BowHand:
                equiped = bow.itemInSlot == null ? false : true;
                equipedItem = equiped ? bow.itemInSlot : null;
                break;
            case WeaponHand.TwoHanded:
                equiped = mainHand.itemInSlot == null ? false : true;
                equipedItem = equiped ? mainHand.itemInSlot : null;
                break;
            case WeaponHand.OneHanded:
                equiped = secondary ? secondaryHand.itemInSlot == null ? false : true : mainHand.itemInSlot == null ? false : true;
                equipedItem = equiped ? secondary ? secondaryHand.itemInSlot : mainHand.itemInSlot : null;
                break;
            case WeaponHand.SecondaryHand:
                equiped = secondaryHand.itemInSlot == null ? false : true;
                equipedItem = equiped ? secondaryHand.itemInSlot : null;
                break;
            default:
                Debug.LogError($"Can't check if slot is equiped: wrong weaponHand: {weaponHand}");
                equiped = false;
                equipedItem = null;
                break;
        }
        return equiped;
    }

    public void CheckEquipmentBuffs (){
        int numberOfCommonArmor = 0;
        int numberOfUncommonArmor = 0;
        int numberOfRareArmor = 0;
        int numberOfEpicArmor = 0;
        int numberOfLegendaryArmor = 0;
        int numberOfRelicArmor = 0;

        if (helmet.itemInSlot != null) CheckItemForRarity(helmet.itemInSlot, ref numberOfCommonArmor, ref numberOfUncommonArmor, ref numberOfRareArmor, ref numberOfEpicArmor, ref numberOfLegendaryArmor, ref numberOfRelicArmor);
        if (chest.itemInSlot != null) CheckItemForRarity(chest.itemInSlot, ref numberOfCommonArmor, ref numberOfUncommonArmor, ref numberOfRareArmor, ref numberOfEpicArmor, ref numberOfLegendaryArmor, ref numberOfRelicArmor);
        if (gloves.itemInSlot != null) CheckItemForRarity(gloves.itemInSlot, ref numberOfCommonArmor, ref numberOfUncommonArmor, ref numberOfRareArmor, ref numberOfEpicArmor, ref numberOfLegendaryArmor, ref numberOfRelicArmor);
        if (pants.itemInSlot != null) CheckItemForRarity(pants.itemInSlot, ref numberOfCommonArmor, ref numberOfUncommonArmor, ref numberOfRareArmor, ref numberOfEpicArmor, ref numberOfLegendaryArmor, ref numberOfRelicArmor);
        if (boots.itemInSlot != null) CheckItemForRarity(boots.itemInSlot, ref numberOfCommonArmor, ref numberOfUncommonArmor, ref numberOfRareArmor, ref numberOfEpicArmor, ref numberOfLegendaryArmor, ref numberOfRelicArmor);
        if (back.itemInSlot != null) CheckItemForRarity(back.itemInSlot, ref numberOfCommonArmor, ref numberOfUncommonArmor, ref numberOfRareArmor, ref numberOfEpicArmor, ref numberOfLegendaryArmor, ref numberOfRelicArmor);
    
        if (numberOfCommonArmor >= 5) characteristics.AddBuff(commonSetBuff); else characteristics.RemoveBuff(commonSetBuff);
        if (numberOfUncommonArmor >= 5) characteristics.AddBuff(uncommonSetBuff); else characteristics.RemoveBuff(uncommonSetBuff);
        if (numberOfRareArmor >= 5) characteristics.AddBuff(rareSetBuff); else characteristics.RemoveBuff(rareSetBuff);
        if (numberOfEpicArmor >= 5) characteristics.AddBuff(epicSetBuff); else characteristics.RemoveBuff(epicSetBuff);
        if (numberOfLegendaryArmor >= 5) characteristics.AddBuff(legendarySetBuff); else characteristics.RemoveBuff(legendarySetBuff);
        if (numberOfRelicArmor >= 5) characteristics.AddBuff(relicSetBuff); else characteristics.RemoveBuff(relicSetBuff);

        CheckWeaponBuffs();
    }

    void CheckItemForRarity (Item item, ref int common, ref int uncommon, ref int rare, ref int epic, ref int legendary, ref int relic) {
        switch (item.itemRarity) {
            case ItemRarity.Common: common++; break;
            case ItemRarity.Uncommon: uncommon++; break;
            case ItemRarity.Rare: rare++; break;
            case ItemRarity.Epic: epic++; break;
            case ItemRarity.Legendary: legendary++; break;
            case ItemRarity.Relic: relic++; break;
        }
    }

    void CheckWeaponBuffs () {
        characteristics.RemoveBuff(TwoHandedStaffBuff);
        characteristics.RemoveBuff(TwoHandedSwordBuff);
        characteristics.RemoveBuff(shieldBuff);
        characteristics.RemoveBuff(axeBuff);
        characteristics.RemoveBuff(dualHandsBuff);
        
        Weapon mh = null;
        Weapon sh = null;
        if (isSlotEquiped(WeaponHand.OneHanded, out Item mainHandItem)) {
            mh = (Weapon)mainHandItem;
        }
        if (isSlotEquiped(WeaponHand.OneHanded, out Item secondaryHandItem, true)) {
            sh = (Weapon)secondaryHandItem;
        }
        if (mh != null) {
            if (mh.weaponHand == WeaponHand.TwoHanded) {
                if (mh.weaponCategory == WeaponCategory.Sword) characteristics.AddBuff(TwoHandedSwordBuff);
                else if (mh.weaponCategory == WeaponCategory.Staff) characteristics.AddBuff(TwoHandedStaffBuff);
            }
            if (mh.weaponCategory == WeaponCategory.Axe) characteristics.AddBuff(axeBuff);
        }
        if (sh != null) {
            if (sh.weaponCategory == WeaponCategory.Shield) characteristics.AddBuff(shieldBuff);
            else if (sh.weaponCategory == WeaponCategory.Axe) characteristics.AddBuff(axeBuff);

            if(mh != null) {
                if (mh.weaponHand == WeaponHand.OneHanded && sh.weaponHand == WeaponHand.OneHanded) characteristics.AddBuff(dualHandsBuff);
            }
        }
    }

#region  ISavable

    public LoadPriority loadPriority {
        get {
            return LoadPriority.First;
        }
    }

    public void Save () {
        helmet.SaveSlot();
        chest.SaveSlot();
        gloves.SaveSlot();
        pants.SaveSlot();
        boots.SaveSlot();
        back.SaveSlot();
        necklace.SaveSlot();
        ring.SaveSlot();
        secondRing.SaveSlot();
        mainHand.SaveSlot();
        secondaryHand.SaveSlot();
        bow.SaveSlot();
    }
    public void Load () {
        LoadEquip();
    }

#endregion
}
