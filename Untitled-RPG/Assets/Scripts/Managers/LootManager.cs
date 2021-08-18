using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    public static LootManager instance;
    [Header("Generic")]
    public GameObject genericLootPrefab;
    public GameObject skillbookLootPrefab;
    public GameObject consumableLootPrefab;
    public GameObject goldLootItemPrefab;
    [Header("Armor")]
    public GameObject helmentLootPrefab;
    public GameObject chestLootPrefab;
    public GameObject glovesLootPrefab;
    public GameObject pantsLootPrefab;
    public GameObject bootsLootPrefab;
    [Header("Weapons")]
    public GameObject bowPrefab;
    public GameObject shieldPrefab;
    public GameObject oneHandedSwordPrefab;
    public GameObject twoHandedSwordPrefab;
    public GameObject oneHandedAxePrefab;
    public GameObject oneHandedStaffPrefab;
    public GameObject twoHandedStaffPrefab;

    void Awake() {
        instance = this;
    }

    public void DropItem (Item item, int amount, Vector3 worldPosition) {
        LootItem li = Instantiate(getLootPrefab(item), worldPosition, Quaternion.identity).GetComponent<LootItem>();
        li.itemAmount = amount;
        if (li.item == null)
            li.item = item;
        li.Drop();
    }
    public void DropGold (int amount, Vector3 worldPosition) {
        LootItem li = Instantiate(goldLootItemPrefab, worldPosition, Quaternion.identity).GetComponent<LootItem>();
        li.itemAmount = Mathf.RoundToInt(Random.Range(0.8f*amount, 1.2f*amount));
        li.Drop();
    }

    GameObject getLootPrefab (Item item) {
        if (item is Weapon) {
            Weapon w = (Weapon)item;
            if (w.weaponHand == WeaponHand.OneHanded || w.weaponHand == WeaponHand.SecondaryHand) {
                switch (w.weaponCategory){
                    case WeaponCategory.Sword: return oneHandedSwordPrefab;
                    case WeaponCategory.Staff: return oneHandedStaffPrefab;
                    case WeaponCategory.Axe: return oneHandedAxePrefab;
                    case WeaponCategory.Shield: return shieldPrefab;
                    default: break;
                }
            } else if (w.weaponHand == WeaponHand.TwoHanded) {
                switch (w.weaponCategory){
                    case WeaponCategory.Sword: return twoHandedSwordPrefab;
                    case WeaponCategory.Staff: return twoHandedStaffPrefab;
                    default: break;
                }
            } else if (w.weaponHand == WeaponHand.BowHand) {
                return bowPrefab;
            }
        } else if (item is Armor) {
            Armor a = (Armor)item;
            switch (a.armorType) {
                case ArmorType.Helmet: return helmentLootPrefab;
                case ArmorType.Chest: return chestLootPrefab;
                case ArmorType.Gloves: return glovesLootPrefab;
                case ArmorType.Pants: return pantsLootPrefab;
                case ArmorType.Boots: return bootsLootPrefab;
                default: break;
            }
        } else if (item is Consumable) {
            return consumableLootPrefab;
        } else if (item is Skillbook) {
            return skillbookLootPrefab;
        }

        return genericLootPrefab;
    }
}
