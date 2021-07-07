using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public static class UI_General
{
    public static Color commonRarityColor = Color.white;
    public static Color uncommonRarityColor = new Color(0.0f, 0.8f, 0);
    public static Color rareRarityColor = new Color(0.2f, 0.45f, 0.76f);
    public static Color epicRarityColor = new Color(0.69f, 0.18f, 0.66f);
    public static Color legendaryRarityColor = new Color(0.9f, 0.65f, 0.11f);
    public static Color relicRarityColor = new Color(0.8f, 0, 0);

    public static Color mainTextColor = Color.white;
    public static Color secondaryTextColor = new Color (0.74f, 0.74f, 0.74f);
    public static Color highlightTextColor = new Color (1, 0.74f, 0.35f);
    public static Color secondaryHighlightTextColor = new Color (0, 0.9f, 0);

    public static IEnumerator PressAnimation (Image image, KeyCode pressedKey) {
        float animationDepth = 0.85f;
        float animationSpeed = 10f;
        Vector2 currentSize = image.GetComponent<RectTransform>().localScale;
        while (currentSize.x > animationDepth) {
            image.GetComponent<RectTransform>().localScale = Vector2.MoveTowards(currentSize, Vector2.one * animationDepth, Time.deltaTime * animationSpeed);
            currentSize = image.GetComponent<RectTransform>().localScale;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        while (Input.GetKey(pressedKey)) {
            yield return null;
        }

        while (currentSize.x < 1) {
            image.GetComponent<RectTransform>().localScale = Vector2.MoveTowards(currentSize, Vector2.one, Time.deltaTime * animationSpeed);
            currentSize = image.GetComponent<RectTransform>().localScale;
            yield return new WaitForSeconds(Time.deltaTime);
        } 
        image.GetComponent<RectTransform>().localScale = Vector2.one;
    }

       public static Color getRarityColor (ItemRarity itemRarity) {
        switch (itemRarity) {
            case ItemRarity.Common:
                return commonRarityColor;
            case ItemRarity.Uncommon:
                return uncommonRarityColor;
            case ItemRarity.Rare:
                return rareRarityColor;
            case ItemRarity.Epic:
                return epicRarityColor;
            case ItemRarity.Legendary:
                return legendaryRarityColor;
            case ItemRarity.Relic:
                return relicRarityColor;
            default:
                Debug.LogError("This could never happen, moron");
                return new Color();
        }
    }

    public static string getItemType (Item item) {
        if (item is Consumable) {
            return "Consumable";
        } else if (item is Weapon) {
            Weapon w = (Weapon)item;
            string hand ="";
            switch (w.weaponHand) {
                case WeaponHand.OneHanded:
                    hand = "One handed";
                    break;
                case WeaponHand.TwoHanded:
                    hand = "Two handed";
                    break;
                case WeaponHand.BowHand:
                    hand = "";
                    break;
                case WeaponHand.SecondaryHand:
                    hand = "";
                    break;
            }
            return $"{hand} {w.weaponCategory}";
        } else if (item is Armor) {
            Armor w = (Armor)item;
            return w.armorType.ToString();
        } else if (item is Skillbook) {
            return "Skillbook";
        } else if (item is Resource) {
            Resource r = (Resource)item;
            switch (r.resourceType) {
                case ResourceType.CraftingResource: return "Crafting resource";
                case ResourceType.QuestItem: return "Quest item";
                case ResourceType.Misc: return "Misc";
            }
        }
        return $"NOT IMPLEMENTED";
    }

    public static int getClickAmount (int maxAmount = 100) {
        int amount = 1;
        if(maxAmount > 1)
            amount = Input.GetKey(KeyCode.LeftControl) ? (maxAmount >= 100 ? 100 : maxAmount) : Input.GetKey(KeyCode.LeftShift) ? (maxAmount >= 10 ? 10 : maxAmount) : amount;
        return amount;
    }
}
