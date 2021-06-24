using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DEBUG_TooManyItems : MonoBehaviour
{
    public GameObject slotPrefab;
    public InputField goldAmounInputField;

    public Transform grid;

    void OnEnable() {
        transform.SetAsLastSibling();

        if (grid.childCount != 0)
            return;

        List<Item> allItems = new List<Item>();
        allItems.AddRange(AssetHolder.instance.consumables);
        allItems.AddRange(AssetHolder.instance.weapons);
        allItems.AddRange(AssetHolder.instance.armor);
        allItems.AddRange(AssetHolder.instance.skillbooks);

        foreach (Item item in allItems) {
            DEBUG_UI_TooManyItemsSlot slot = Instantiate(slotPrefab, grid).GetComponent<DEBUG_UI_TooManyItemsSlot>();
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

    public void AutoFillInventory () {
        foreach (Item item in AssetHolder.instance.weapons)
            InventoryManager.instance.AddItemToInventory(item, 1, null);
        
        foreach (Item item in AssetHolder.instance.armor)
            InventoryManager.instance.AddItemToInventory(item, 1, null);
        
        foreach (Item item in AssetHolder.instance.consumables)
            InventoryManager.instance.AddItemToInventory(item, 50, null);
        
        foreach (Skill skill in AssetHolder.instance.Skills){
            if (skill.skillTree == SkillTree.Independent)
                continue;
            Combat.instanace.LearnSkill(skill);
        }

        InventoryManager.instance.AddGold(50000);
    }
}
