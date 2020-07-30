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

    void Awake() {
        if (instance == null)
            instance = this;
    }

    public Item getItem(int ID) {
        if (ID < 1000) { //Returns consumables 
            for (int i = 0; i < consumables.Count; i ++) {
                if (consumables[i].ID == ID)
                    return consumables[i];
            }
            Debug.LogError($"Item with ID = {ID} is not found");
            return null;
        } else {
            Debug.LogError($"ID = {ID} is out of range");
            return null;
        }
    }
}
