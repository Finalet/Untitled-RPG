using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Item item;
    public CraftingNPC parentCraftingNPC;

    [Space]
    public Image itemIcon;
    public TextMeshProUGUI itemTypeLabel;
    public TextMeshProUGUI itemNameLabel;
    public TextMeshProUGUI itemRarityLabel;

    public void Init () {
        if (item == null)
            return;

        itemIcon.sprite = item.itemIcon;
        itemTypeLabel.text = UI_General.getItemType(item);
        itemNameLabel.text = item.itemName;
        itemRarityLabel.text = item.itemRarity.ToString();
        itemRarityLabel.color = UI_General.getRarityColor(item.itemRarity);
        GetComponent<Button>().onClick.AddListener(delegate{parentCraftingNPC.Select(item);});

        if (item.specialFrameMat) {
            RectTransform specialFrame = new GameObject().AddComponent<RectTransform>();
            specialFrame.SetParent(itemIcon.transform);
            specialFrame.anchoredPosition3D = Vector3.zero;
            specialFrame.localScale = Vector2.one;
            specialFrame.anchorMin = Vector2.zero;
            specialFrame.anchorMax = Vector2.one;
            specialFrame.offsetMin = Vector2.zero;
            specialFrame.offsetMax = Vector2.zero;
            specialFrame.gameObject.AddComponent<Image>().material = item.specialFrameMat;
        }
    }

    public virtual void OnPointerEnter (PointerEventData pointerData) {
        TooltipsManager.instance.RequestTooltip(item, gameObject);
    }
    public virtual void OnPointerExit (PointerEventData pointerData) {
        TooltipsManager.instance.CancelTooltipRequest(gameObject);
    }
    void OnDisable() {
        TooltipsManager.instance.CancelTooltipRequest(gameObject);
    }
}
