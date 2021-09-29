using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public struct Loot {
    public Item item;
    public int amount;
    [Range(0,1)] public float amountVariability;
}

[System.Serializable] public struct RandomLoot {
    public WeightedRandomBag<Loot> possibleLoot;
    public int minLoot;
    public int maxLoot;
    [System.NonSerialized] public double ev;
    [DisplayWithoutEdit, SerializeField] string expectedValue;

    public void OnValidate() {
        minLoot = Mathf.Clamp(minLoot, 0, 1000);
        maxLoot = Mathf.Clamp(maxLoot, 0, 1000);
        if (minLoot > maxLoot) minLoot = maxLoot;
        ev = GetExpectedValue();
        expectedValue = UI_General.FormatPrice(Mathf.RoundToInt((float)ev));
    }

    public ItemAmountPair[] GetLoot() {
        int numberOfLoot = Random.Range(minLoot, maxLoot);

        List<ItemAmountPair> list = new List<ItemAmountPair>();

        for (int i = 0; i < numberOfLoot; i++) {
            Loot reward = possibleLoot.GetRandom();
            int amount = Mathf.RoundToInt( Random.Range(reward.amount * (1-reward.amountVariability), reward.amount * (1+reward.amountVariability)));
            if (amount <= 0) continue;
            list.Add(new ItemAmountPair(reward.item, amount));
        }

        return list.ToArray();
    }

    double GetExpectedValue () {
        if (possibleLoot == null) return 0;
        
        double ev = 0;
        double totalWeight = possibleLoot.totalWeight;

        if (totalWeight <= 0) return 0;

        for (int i = 0; i < possibleLoot.WeightedEntries.Count; i++) {
            if (!possibleLoot.WeightedEntries[i].item.item) continue;

            ev += possibleLoot.WeightedEntries[i].item.item.itemBasePrice * possibleLoot.WeightedEntries[i].item.amount * possibleLoot.WeightedEntries[i].weight / totalWeight;
        }

        return ev * (minLoot+maxLoot)/2;
    }
}

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


public interface ILootable {
    bool canDropLoot { get; set; }
    
    void DropLoot();
}