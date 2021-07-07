using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LootNotificationManager : MonoBehaviour
{
    public static LootNotificationManager instance;
    public float spacing;
    public float duration = 3;

    [Space]
    public GameObject lootNotificationUIPrefab;
    public List<RectTransform> currentLootNotificaitonsUI = new List<RectTransform>();

    float animSpeed = 0.2f;
    Ease ease = Ease.OutQuad;

    void Awake() {
        instance = this;
    }

    public void ShowLootNotification (LootItem lootItem) {
        if (lootItem.item == null)
            return;

        GameObject go = Instantiate(lootNotificationUIPrefab, transform);
        RectTransform rect = go.GetComponent<RectTransform>();

        Color rarityColor = lootItem.item.itemRarity == ItemRarity.Common ? Color.black : UI_General.getRarityColor(lootItem.item.itemRarity);
        rarityColor.a = rarityColor == Color.black ? 1 : 0.5f;
        go.transform.GetComponent<Image>().color = rarityColor;

        go.transform.GetChild(0).GetComponent<Image>().sprite = lootItem.item.itemIcon;
        go.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = lootItem.item.itemName + " x" + lootItem.itemAmount;

        rect.anchoredPosition = new Vector2(-rect.sizeDelta.x, 0);
        rect.DOAnchorPosX(0, animSpeed).SetEase(ease);

        currentLootNotificaitonsUI.Insert(0, rect);
        for (int i = 0; i < currentLootNotificaitonsUI.Count; i++) {
            currentLootNotificaitonsUI[i].DOAnchorPosY(-i*(rect.sizeDelta.y + spacing), animSpeed).SetEase(ease);
            if (i > 2) 
                currentLootNotificaitonsUI[i].GetComponent<CanvasGroup>().DOFade( (6-i)*0.25f, animSpeed);
        }
        StartCoroutine(RemoveNotification(rect));
    }

    IEnumerator RemoveNotification (RectTransform notificationUI) {
        yield return new WaitForSeconds(duration);
        notificationUI.DOAnchorPosX(-notificationUI.sizeDelta.x, animSpeed).SetEase(ease);
        currentLootNotificaitonsUI.Remove(notificationUI);
        Destroy(notificationUI.gameObject, animSpeed);
    }
}
