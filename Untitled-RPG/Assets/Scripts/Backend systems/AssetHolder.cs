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
    public GameObject genericLootItemPrefab;
    public GameObject goldLootItemPrefab;

    public Skill[] Skills;

    public List<Item> consumables = new List<Item>();
    public List<Item> weapons = new List<Item>();
    public List<Item> armor = new List<Item>();
    public List<Item> skillbooks = new List<Item>();
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

    public void DropItem (Item item, int amount, Vector3 worldPosition) {
        GameObject prefab = item.itemPrefab != null ? item.itemPrefab : genericLootItemPrefab;
        LootItem li = Instantiate(prefab, worldPosition, Quaternion.identity).GetComponent<LootItem>();
        li.itemAmount = amount < 5 ? amount : Mathf.RoundToInt(Random.Range(0.8f*amount, 1.2f*amount));
        if (li.item == null)
            li.item = item;
        li.Drop();
    }
    public void DropGold (int amount, Vector3 worldPosition) {
        LootItem li = Instantiate(goldLootItemPrefab, worldPosition, Quaternion.identity).GetComponent<LootItem>();
        li.isGold = true;
        li.itemAmount = Mathf.RoundToInt(Random.Range(0.8f*amount, 1.2f*amount));
        li.Drop();
    }

    public void SortAllByID () {
        consumables.Sort((x, y) => x.ID.CompareTo(y.ID));
        weapons.Sort((x, y) => x.ID.CompareTo(y.ID));
        armor.Sort((x, y) => x.ID.CompareTo(y.ID));
        skillbooks.Sort((x, y) => x.ID.CompareTo(y.ID));
        Skills = Skills.OrderBy(x => x.ID).ToArray();
    }

     public void CheckUsedLayers()
     {
        Debug.Log("Check used layers");
        Dictionary<string, int> layerCount = new Dictionary<string, int>();
        List<GameObject> gameObjects = FindObjectsOfType<GameObject>().ToList();

        // iterate objects and save to dictionary
        for (int i = 0; i < gameObjects.Count; ++i)
        {
            string layerName = LayerMask.LayerToName(gameObjects[i].layer);
            if (layerCount.ContainsKey(layerName))
            {
                layerCount[layerName]++;
            }
            else
            {
                layerCount.Add(layerName, 1);
            }
        }

        // log to console
        foreach (KeyValuePair<string, int> entry in layerCount)
        {
            Debug.Log(entry.Key + ": " + entry.Value);
        }

        // unused layers
        List<string> layerNames = new List<string>();
        for (int i = 8; i <= 31; i++) //user defined layers start with layer 8 and unity supports 31 layers
        {
            var layerN = LayerMask.LayerToName(i); //get the name of the layer
            if (layerN.Length > 0) //only add the layer if it has been named (comment this line out if you want every layer)
                layerNames.Add(layerN);
        }

        List<string> listOfKeys = layerCount.Keys.ToList();
        List<string> unusedLayers = layerNames.Except(listOfKeys).ToList();
        string joined = string.Join(", ", unusedLayers);
        Scene scene = SceneManager.GetActiveScene();
        Debug.Log("Unused layers in " + scene.name + ": " + joined);

        Debug.Log("Check used layers done");
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
        if (GUILayout.Button("Check used Layers")) {
            assetHolder.CheckUsedLayers();
        }
    }
}

#endif
