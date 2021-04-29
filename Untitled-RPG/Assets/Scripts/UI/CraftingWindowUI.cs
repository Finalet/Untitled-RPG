using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingWindowUI : MonoBehaviour
{
    public CraftingNPC ownerNPC;
    [Space]
    public GameObject storeItemTemplate;
    public GameObject resourceTemplate;
    public GameObject itemsTab;
    public GameObject resourcesContainer;
    public Image itemIcon;
    public TextMeshProUGUI itemTypeLabel;
    public TextMeshProUGUI itemNameLabel;
    public TextMeshProUGUI itemRarityLabel;
    public TextMeshProUGUI itemDescriptionLabel;
    public Button craftButton;
    public Button cancelButton;
    public Button quantityUpButton;
    public Button quantityDownButton;
    public TMP_InputField quanitityInputField;

    List<UI_CraftingResourceSlot> resourcesSlots = new List<UI_CraftingResourceSlot>();

    public void Init() {
        for (int i = 0; i < ownerNPC.craftingItems.Length; i++) {
            CraftingItemUI itemUI = Instantiate(storeItemTemplate, itemsTab.transform).GetComponent<CraftingItemUI>();
            itemUI.item = ownerNPC.craftingItems[i];
            itemUI.parentCraftingNPC = ownerNPC;
            itemUI.Init();
        }
        Destroy(storeItemTemplate);
        craftButton.onClick.AddListener(delegate{ownerNPC.CraftItem();});
        cancelButton.onClick.AddListener(delegate{ownerNPC.CancelCraft();});
        quantityUpButton.onClick.AddListener(delegate{QuantityUp();});
        quantityDownButton.onClick.AddListener(delegate{QuantityDown();});
        quanitityInputField.onValueChanged.AddListener(delegate{UpdateQuantityFronInput();});
        quanitityInputField.text = ownerNPC.craftQuanitity.ToString();
    }

    public void DisplaySelectedItem () {
        itemIcon.sprite = ownerNPC.selectedItem.itemIcon;
        itemTypeLabel.text = UI_General.getItemType(ownerNPC.selectedItem);
        itemNameLabel.text = ownerNPC.selectedItem.itemName;
        itemRarityLabel.text = ownerNPC.selectedItem.itemRarity.ToString();
        itemRarityLabel.color = UI_General.getRarityColor(ownerNPC.selectedItem.itemRarity);
        itemDescriptionLabel.text = ownerNPC.selectedItem.itemDesctription;
        
        resourceTemplate.SetActive(true);
        if (resourcesSlots.Count < ownerNPC.selectedItem.craftingRecipe.Length) {
            for (int i = ownerNPC.selectedItem.craftingRecipe.Length - resourcesSlots.Count; i > 0; i--) {
                UI_CraftingResourceSlot slot = Instantiate(resourceTemplate, resourcesContainer.transform).GetComponent<UI_CraftingResourceSlot>();
                resourcesSlots.Add(slot);
            }
        } else if (resourcesSlots.Count > ownerNPC.selectedItem.craftingRecipe.Length) {
            for (int i = resourcesSlots.Count; i > ownerNPC.selectedItem.craftingRecipe.Length; i--) {
                Destroy(resourcesSlots[i-1].gameObject);
                resourcesSlots.RemoveAt(i-1);
            }
        }
        resourceTemplate.SetActive(false);

        UpdateResources();
    }
    void UpdateResources () {
        if (resourcesSlots.Count <= 0 || ownerNPC == null || ownerNPC.selectedItem == null)
            return;

        for (int i = 0; i < ownerNPC.selectedItem.craftingRecipe.Length; i++) {
            resourcesSlots[i].itemInSlot = ownerNPC.selectedItem.craftingRecipe[i].resource;
            resourcesSlots[i].itemAmount = ownerNPC.selectedItem.craftingRecipe[i].requiredAmount * ownerNPC.craftQuanitity;
            resourcesSlots[i].UpdateResourceDisplay();
        }
    }

    void UpdateQuantityFronInput () {
        ownerNPC.craftQuanitity = quanitityInputField.text != "" ? Mathf.Clamp(int.Parse(quanitityInputField.text), 1, 100) : 1;
        quanitityInputField.text = ownerNPC.craftQuanitity.ToString();
        UpdateResources();
    }

    void QuantityUp () {
        ownerNPC.craftQuanitity ++;
        quanitityInputField.text = ownerNPC.craftQuanitity.ToString();
    }
    void QuantityDown () {
        ownerNPC.craftQuanitity --;
        quanitityInputField.text = ownerNPC.craftQuanitity.ToString();
    }
}
