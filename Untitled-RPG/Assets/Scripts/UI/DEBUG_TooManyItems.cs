using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DEBUG_TooManyItems : MonoBehaviour
{
    public GameObject slotPrefab;
    public InputField goldAmounInputField;

    public Transform allGrid;
    public Transform weaponsGrid;
    public Transform armorGrid;
    public Transform skillbooksGrid;
    public Transform consumablesGrid;
    public Transform resourcesGrid;
    public Transform mountsGrid;
    public Transform mountsEquipmentGrid;

    void Start() {
        transform.SetAsLastSibling();

        List<Item> allItems = new List<Item>();
        allItems.AddRange(AssetHolder.instance.weapons);
        allItems.AddRange(AssetHolder.instance.armor);
        allItems.AddRange(AssetHolder.instance.skillbooks);
        allItems.AddRange(AssetHolder.instance.consumables);
        allItems.AddRange(AssetHolder.instance.resources);
        allItems.AddRange(AssetHolder.instance.mounts);
        allItems.AddRange(AssetHolder.instance.mountEquipment);

        foreach (Item item in allItems) {
            DEBUG_UI_TooManyItemsSlot slot = Instantiate(slotPrefab, allGrid).GetComponent<DEBUG_UI_TooManyItemsSlot>();
            slot.AddItem(item, 1, null);
        }
        foreach (Item item in AssetHolder.instance.weapons) {
            DEBUG_UI_TooManyItemsSlot slot = Instantiate(slotPrefab, weaponsGrid).GetComponent<DEBUG_UI_TooManyItemsSlot>();
            slot.AddItem(item, 1, null);
        }
        foreach (Item item in AssetHolder.instance.armor) {
            DEBUG_UI_TooManyItemsSlot slot = Instantiate(slotPrefab, armorGrid).GetComponent<DEBUG_UI_TooManyItemsSlot>();
            slot.AddItem(item, 1, null);
        }
        foreach (Item item in AssetHolder.instance.skillbooks) {
            DEBUG_UI_TooManyItemsSlot slot = Instantiate(slotPrefab, skillbooksGrid).GetComponent<DEBUG_UI_TooManyItemsSlot>();
            slot.AddItem(item, 1, null);
        }
        foreach (Item item in AssetHolder.instance.consumables) {
            DEBUG_UI_TooManyItemsSlot slot = Instantiate(slotPrefab, consumablesGrid).GetComponent<DEBUG_UI_TooManyItemsSlot>();
            slot.AddItem(item, 1, null);
        }
        foreach (Item item in AssetHolder.instance.resources) {
            DEBUG_UI_TooManyItemsSlot slot = Instantiate(slotPrefab, resourcesGrid).GetComponent<DEBUG_UI_TooManyItemsSlot>();
            slot.AddItem(item, 1, null);
        }
        foreach (Item item in AssetHolder.instance.mounts) {
            DEBUG_UI_TooManyItemsSlot slot = Instantiate(slotPrefab, mountsGrid).GetComponent<DEBUG_UI_TooManyItemsSlot>();
            slot.AddItem(item, 1, null);
        }
        foreach (Item item in AssetHolder.instance.mountEquipment) {
            DEBUG_UI_TooManyItemsSlot slot = Instantiate(slotPrefab, mountsEquipmentGrid).GetComponent<DEBUG_UI_TooManyItemsSlot>();
            slot.AddItem(item, 1, null);
        }
    }

    void Update() {
        if (goldAmounInputField.text != "" && ( Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) )
            AddGold();
    }

    public void AddGold () {
        InventoryManager.instance.AddGold(int.Parse(goldAmounInputField.text));
        goldAmounInputField.text = "";
    }

    public void LearnAllSkills () {
        foreach (Skill skill in AssetHolder.instance.Skills){
            if (skill.skillTree == SkillTree.Independent)
                continue;
            Combat.instanace.LearnSkill(skill);
        }
    }
}
