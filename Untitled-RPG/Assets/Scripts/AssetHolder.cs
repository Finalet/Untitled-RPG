using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AssetHolder : MonoBehaviour
{
    public static AssetHolder instance;
    
    public GameObject ddText;
    public Canvas canvas;

    public Skill[] Skills;

    public List<Item> consumables = new List<Item>();
    public List<Item> weapons = new List<Item>();
    public List<Consumable> consumablesCoolingDown = new List<Consumable>();

    void Awake() {
        if (instance == null)
            instance = this;
    }

    void FixedUpdate() {
        if (consumablesCoolingDown.Count >= 1)
            RunConsumablesCooldown();
    }
    void RunConsumablesCooldown() {
        for (int i = consumablesCoolingDown.Count-1; i >= 0; i--) {
            print(i);
            consumablesCoolingDown[i].cooldownTimer -= Time.fixedDeltaTime;
            consumablesCoolingDown[i].isCoolingDown = consumablesCoolingDown[i].cooldownTimer > 0 ? true : false;
            if (!consumablesCoolingDown[i].isCoolingDown) consumablesCoolingDown.RemoveAt(i);
        }
    }
    public void StartConsumableCooldown(Consumable item) {
        item.cooldownTimer = item.cooldownTime;
        consumablesCoolingDown.Add(item);
    }

    public Item getItem(int ID) {
        if (ID < 1000) { //Returns consumables 
            for (int i = 0; i < consumables.Count; i ++) {
                if (consumables[i].ID == ID)
                    return consumables[i];
            }
            Debug.LogError($"Item with ID = {ID} not found");
            return null;
        } else if (ID >= 1000) { //Returns weapons 
            for (int i = 0; i < weapons.Count; i ++) {
                if (weapons[i].ID == ID)
                    return weapons[i];
            }
            Debug.LogError($"Item with ID = {ID} not found");
            return null;
        } else {
            Debug.LogError($"ID = {ID} is out of range");
            return null;
        }
    }

    public Skill getSkill (int ID) {
        for (int i = 0; i < Skills.Length; i ++) {
            if (Skills[i].ID == ID)
                return Skills[i];
        }
        Debug.LogError($"Skill with ID = {ID} not found");
        return null;
    }
}
