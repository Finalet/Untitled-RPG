using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AssetHolder : MonoBehaviour
{
    public static AssetHolder instance;
    
    public GameObject ddText;
    
    public Skill[] Skills;

    public List<Item> consumables = new List<Item>();
    public List<Item> weapons = new List<Item>();
    public List<Item> armor = new List<Item>();
    public List<Item> skillbooks = new List<Item>();
    public List<Item> resources = new List<Item>();
    [Space]
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
        } else if (ID >= 1000 && ID < 2000) { //Returns weapons 
            for (int i = 0; i < weapons.Count; i ++) {
                if (weapons[i].ID == ID)
                    return weapons[i];
            }
            Debug.LogError($"Weapon with ID = {ID} not found");
            return null;
        } else if (ID >= 2000 && ID < 3000) { //Returns armor 
            for (int i = 0; i < armor.Count; i ++) {
                if (armor[i].ID == ID)
                    return armor[i];
            }
            Debug.LogError($"Armor with ID = {ID} not found");
            return null;
        } else if (ID >= 3000 && ID < 4000) { //Returns skillbooks 
            for (int i = 0; i < skillbooks.Count; i ++) {
                if (skillbooks[i].ID == ID)
                    return skillbooks[i];
            }
            Debug.LogError($"Skillbook with ID = {ID} not found");
            return null;
        } else if (ID >= 4000 && ID < 5000) { //Returns resources 
            for (int i = 0; i < resources.Count; i ++) {
                if (resources[i].ID == ID)
                    return resources[i];
            }
            Debug.LogError($"Resource with ID = {ID} not found");
            return null;
        } else {
            Debug.LogError($"ID {ID} is out of range");
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

    public void SortAllByID () {
        consumables.Sort((x, y) => x.ID.CompareTo(y.ID));
        weapons.Sort((x, y) => x.ID.CompareTo(y.ID));
        armor.Sort((x, y) => x.ID.CompareTo(y.ID));
        skillbooks.Sort((x, y) => x.ID.CompareTo(y.ID));
        Skills = Skills.OrderBy(x => x.ID).ToArray();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AssetHolder))]
public class AssetHolderInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        AssetHolder assetHolder = (AssetHolder)target;
        GUILayout.Space(10);
        if(GUILayout.Button("Sort all by ID")) {
            assetHolder.SortAllByID();
        }
    }
}

#endif
