using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public static class UI_General
{
    public static Color commonRarityColor = Color.white;
    public static Color rareRarityColor = new Color(0.16f, 0.37f, 0.63f);
    public static Color epicRarityColor = new Color(0.63f, 0.16f, 0.60f);
    public static Color legendaryRarityColor = new Color(0.9f, 0.65f, 0.11f);

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

    public static Color getRarityColor (Item item) {
        switch (item.itemRarity) {
            case ItemRarity.Common:
                return commonRarityColor;
            case ItemRarity.Rare:
                return rareRarityColor;
            case ItemRarity.Epic:
                return epicRarityColor;
            case ItemRarity.Legendary:
                return legendaryRarityColor;
            default:
                Debug.LogError("This could never happen, moron");
                return new Color();
        }
    }
    public static Color getRarityColor (ItemRarity itemRarity) {
        switch (itemRarity) {
            case ItemRarity.Common:
                return commonRarityColor;
            case ItemRarity.Rare:
                return rareRarityColor;
            case ItemRarity.Epic:
                return epicRarityColor;
            case ItemRarity.Legendary:
                return legendaryRarityColor;
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
            string text ="";
            switch (w.weaponType) {
                case WeaponType.OneHandedSword:
                    text = "One handed sword";
                    break;
                case WeaponType.OneHandedStaff:
                    text = "One handed staff";
                    break;
                case WeaponType.Bow:
                    text = "Bow";
                    break;
                case WeaponType.Shield:
                    text = "Shield";
                    break;
                case WeaponType.TwoHandedStaff:
                    text = "Two-handed staff";
                    break;
                case WeaponType.TwoHandedSword:
                    text = "Two-handed sword";
                    break;
                default:
                    text = "NOT IMPLEMENTED";
                    break;
            }
            return text;
        } else if (item is Armor) {
            Armor w = (Armor)item;
            return w.armorType.ToString();
        } else if (item is Skillbook) {
            return "Skillbook";
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
