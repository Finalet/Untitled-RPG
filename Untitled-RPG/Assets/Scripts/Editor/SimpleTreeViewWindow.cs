// Create a foldable menu that hides/shows the selected transform position.
// If no Transform is selected, the Foldout item will be folded until
// a transform is selected.
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemsDatabase : EditorWindow
{
    bool showWeapons;
    bool showArmor;
    bool showSkillbooks;
    bool showConsumables;

    bool showAllArmor;
    bool showHelmets;
    bool showChests;
    bool showGloves;
    bool showPants;
    bool showBoots;
    bool shotBacks;
    bool showNecklaces;
    bool showRings;

    Vector2 scrollPos;
    Vector2 scrollPosSelection;
    Item selectedItem;
    Editor selectedWindow;
    bool changingSelectedItemName = false;

    [MenuItem("Window/Items Database")]
    static void Init()
    {
        ItemsDatabase window = (ItemsDatabase)GetWindow(typeof(ItemsDatabase), false, "Items Database");
        window.Show();
    }

    public void OnGUI()
    {
        if (GameObject.Find("AssetHolder") == null) {
            EditorGUILayout.LabelField("Asset Holder not found in the scene.");
            return;
        }
        AssetHolder ah = GameObject.Find("AssetHolder").GetComponent<AssetHolder>();
        
        EditorGUILayout.BeginHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUI.indentLevel = 0;
        showWeapons = EditorGUILayout.Foldout(showWeapons, "Weapons");
        if (showWeapons) {
            DrawList(ah.weapons);
            EditorGUILayout.Space(5, false);
        }

        EditorGUI.indentLevel = 0;
        showArmor = EditorGUILayout.Foldout(showArmor, "Armor");
        if (showArmor) {
            DrawArmorList(ah.armor);
            EditorGUILayout.Space(5, false);
        }

        EditorGUI.indentLevel = 0;
        showSkillbooks = EditorGUILayout.Foldout(showSkillbooks, "Skillbooks");
        if (showSkillbooks) {
            DrawList(ah.skillbooks);
            EditorGUILayout.Space(5, false);
        }
        
        EditorGUI.indentLevel = 0;
        showConsumables = EditorGUILayout.Foldout(showConsumables, "Consumable");
        if (showConsumables) {
            DrawList(ah.consumables);
            EditorGUILayout.Space(5, false);
        }
        
        EditorGUILayout.EndScrollView();
        
        //---------------Selection View--------------/
        if (selectedItem != null) {
            scrollPosSelection = EditorGUILayout.BeginScrollView(scrollPosSelection, GUILayout.Width(position.width/3.5f));
            
            EditorGUILayout.BeginHorizontal();
            
            EditorGUI.BeginDisabledGroup(!changingSelectedItemName);
            selectedItem.name = EditorGUILayout.TextField(selectedItem.name);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button(changingSelectedItemName ? "Save" : "Edit name")) {
                if (changingSelectedItemName){
                    string assetPath =  AssetDatabase.GetAssetPath(selectedItem.GetInstanceID());
                    AssetDatabase.RenameAsset(assetPath, selectedItem.name);
                }
                changingSelectedItemName = !changingSelectedItemName;
            }
            
            EditorGUILayout.EndHorizontal();

            selectedWindow = Editor.CreateEditor(selectedItem);
            selectedWindow.DrawDefaultInspector();
            
            EditorGUILayout.EndScrollView();
        } else {
            changingSelectedItemName = false;
        }


        EditorGUILayout.EndHorizontal();
    }

    void DrawTitles (List<Item> list) {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ID", EditorStyles.boldLabel, GUILayout.Width(50 + EditorGUI.indentLevel * 8));
        EditorGUILayout.LabelField("Item name", EditorStyles.boldLabel, GUILayout.Width(230 + EditorGUI.indentLevel * 8));
        EditorGUILayout.LabelField("Item rarity", EditorStyles.boldLabel, GUILayout.Width(100 + EditorGUI.indentLevel * 8));
        DrawAdditionalFieldsTitles(list);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5, false);
    }

    void DrawList (List<Item> list, bool hideTitles = false) {
        EditorGUI.indentLevel ++;
        if (!hideTitles) {
            DrawTitles(list);
        }
        foreach (Item item in list){
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(item.ID.ToString(), EditorStyles.boldLabel, GUILayout.Width(50 + EditorGUI.indentLevel * 8));
            EditorGUILayout.LabelField(item.name, GUILayout.Width(230 + EditorGUI.indentLevel * 8));
            EditorGUILayout.LabelField(item.itemRarity.ToString(), GUILayout.Width(100 + EditorGUI.indentLevel * 8));
            DrawAdditionalFields(item);
            EditorGUI.BeginDisabledGroup(true); EditorGUILayout.ObjectField("", item, typeof(Item), false, GUILayout.Width(150)); EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Open", GUILayout.Width(70)))
                selectedItem = item;
            if (GUILayout.Button("-", GUILayout.Width(15))){
                DeleteItem(list, item);
                return;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20 * EditorGUI.indentLevel);
        if (GUILayout.Button("+", GUILayout.Width(20))) {
            AddNewItem(list);
        }
        EditorGUILayout.EndHorizontal();
    }
    void DrawArmorList (List<Item> list) {
        List<Item> helmets = new List<Item>();
        List<Item> chests = new List<Item>();
        List<Item> gloves = new List<Item>();
        List<Item> pants = new List<Item>();
        List<Item> boots = new List<Item>();
        List<Item> backs = new List<Item>();
        List<Item> necklaces = new List<Item>();
        List<Item> rings = new List<Item>();
        
        Armor e;
        foreach (Item item in list){
            e = (Armor)item;
            switch (e.armorType) {
                case ArmorType.Helmet:
                    helmets.Add(e);
                    break;
                case ArmorType.Chest:
                    chests.Add(e);
                    break;
                case ArmorType.Gloves:
                    gloves.Add(e);
                    break;
                case ArmorType.Pants:
                    pants.Add(e);
                    break;
                case ArmorType.Boots:
                    boots.Add(e);
                    break;
                case ArmorType.Back:
                    backs.Add(e);
                    break;
                case ArmorType.Necklace:
                    necklaces.Add(e);
                    break;
                case ArmorType.Ring:
                    rings.Add(e);
                    break;
            }
        }

        EditorGUI.indentLevel = 2;
        DrawTitles(list);
        
        EditorGUI.indentLevel = 1;
        showAllArmor = EditorGUILayout.Foldout(showAllArmor, "All armor");
        if(showAllArmor) {
            DrawList(list, true);
        }

        EditorGUI.indentLevel = 1;
        showHelmets = EditorGUILayout.Foldout(showHelmets, "Helmets");
        if (showHelmets) {
            DrawList(helmets, true);
        }
        EditorGUI.indentLevel = 1;
        showChests = EditorGUILayout.Foldout(showChests, "Chests");
        if (showChests) {
            DrawList(chests, true);
        }
        EditorGUI.indentLevel = 1;
        showGloves = EditorGUILayout.Foldout(showGloves, "Gloves");
        if (showGloves) {
            DrawList(gloves, true);
        }
        EditorGUI.indentLevel = 1;
        showPants = EditorGUILayout.Foldout(showPants, "Pants");
        if (showPants) {
            DrawList(pants, true);
        }
        EditorGUI.indentLevel = 1;
        showBoots = EditorGUILayout.Foldout(showBoots, "Boots");
        if (showBoots) {
            DrawList(boots, true);
        }
        EditorGUI.indentLevel = 1;
        shotBacks = EditorGUILayout.Foldout(shotBacks, "Backs");
        if (shotBacks) {
            DrawList(backs, true);
        }
        EditorGUI.indentLevel = 1;
        showNecklaces = EditorGUILayout.Foldout(showNecklaces, "Necklaces");
        if (showNecklaces) {
            DrawList(necklaces, true);
        }
        EditorGUI.indentLevel = 1;
        showRings = EditorGUILayout.Foldout(showRings, "Rings");
        if (showRings) {
            DrawList(rings, true);
        }
    }

    void DrawAdditionalFieldsTitles (List<Item> list) {
        AssetHolder ah = GameObject.Find("AssetHolder").GetComponent<AssetHolder>();
        
        if (list[0] is Weapon) {
            EditorGUILayout.LabelField("Weapon type", EditorStyles.boldLabel, GUILayout.Width(150));
        } else if (list[0] is Armor) {
            EditorGUILayout.LabelField("Armor type", EditorStyles.boldLabel, GUILayout.Width(150));
        } else if (list[0] is Skillbook) {
            EditorGUILayout.LabelField("Skill tree", EditorStyles.boldLabel, GUILayout.Width(150));
        } else if (list[0] is Consumable) {
            EditorGUILayout.LabelField("Consumable type", EditorStyles.boldLabel, GUILayout.Width(150));
        }
    }
    void DrawAdditionalFields (Item item) {
        if (item is Weapon) {
            Weapon w = (Weapon)item;
            EditorGUILayout.LabelField(w.weaponType.ToString(), GUILayout.Width(150));
        } else if (item is Armor) {
            Armor a = (Armor)item;
            EditorGUILayout.LabelField(a.armorType.ToString(), GUILayout.Width(150));
        } else if (item is Consumable) {
            Consumable c = (Consumable)item;
            EditorGUILayout.LabelField(c.consumableType.ToString(), GUILayout.Width(150));
        } else if (item is Skillbook) {
            Skillbook s = (Skillbook)item;
            EditorGUILayout.LabelField(s.learnedSkill.skillTree.ToString(), GUILayout.Width(150));
        }
    }

    void AddNewItem (List<Item> list) {
        AssetHolder ah = GameObject.Find("AssetHolder").GetComponent<AssetHolder>();
        
        if (list == ah.weapons) {
            Weapon w = ScriptableObject.CreateInstance<Weapon>();
            name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Items/Weapons/New Weapon.asset");
            AssetDatabase.CreateAsset(w, name);
            w.ID = list[list.Count-1].ID + 1;
            w.itemName = "New Weapon";
            ah.weapons.Add(w);
            selectedItem = w;
        } else if (list == ah.armor) {
            Armor a = ScriptableObject.CreateInstance<Armor>();
            name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Items/Armor/New Armor.asset");
            AssetDatabase.CreateAsset(a, name);
            a.ID = list[list.Count-1].ID + 1;
            a.itemName = "New Armor";
            ah.armor.Add(a);
            selectedItem = a;
        } else if (list == ah.skillbooks) {
            Skillbook s = ScriptableObject.CreateInstance<Skillbook>();
            name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Items/Skillbooks/New Skillbook.asset");
            AssetDatabase.CreateAsset(s, name);
            s.ID = list[list.Count-1].ID + 1;
            s.itemName = "New Skillbook";
            ah.skillbooks.Add(s);
            selectedItem = s;
        } else if (list == ah.consumables) {
            Consumable c = ScriptableObject.CreateInstance<Consumable>();
            name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Items/Consumables/New Consumable.asset");
            AssetDatabase.CreateAsset(c, name);
            c.ID = list[list.Count-1].ID + 1;
            c.itemName = "New Consumable";
            ah.consumables.Add(c);
            selectedItem = c;
        } else {
            //-------------Different types of Armor-------------//
            if (list != ah.armor && list[0] is Armor) {
                Armor existingArmor = (Armor)list[0];
                Armor a = ScriptableObject.CreateInstance<Armor>();
                name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath($"Assets/Items/Armor/New {existingArmor.armorType}.asset");
                AssetDatabase.CreateAsset(a, name);
                a.ID = list[list.Count-1].ID + 1;
                a.itemName = $"New {existingArmor.armorType}";
                a.armorType = existingArmor.armorType;
                ah.armor.Add(a);
                selectedItem = a;
            }
        }
    }

    void DeleteItem (List<Item> list, Item itemToDelete) {
        AssetHolder ah = GameObject.Find("AssetHolder").GetComponent<AssetHolder>();
        
        if (!EditorUtility.DisplayDialog($"Delete {itemToDelete.name}?", $"Are you sure you want to delete {itemToDelete.name}? This action is irreversable.", "Cancel", "Delete")) {          
            string folder = "";
            if (list == ah.weapons) {
                folder = "Weapons";
                ah.weapons.Remove(itemToDelete);
            } else if (list == ah.armor) {
                folder = "Armor";
                ah.armor.Remove(itemToDelete);
            } else if (list == ah.skillbooks) {
                Skillbook s = (Skillbook)itemToDelete;
                folder = "Skillbooks";
                ah.skillbooks.Remove(itemToDelete);
            } else if (list == ah.consumables) {
                folder = "Consumables";
                ah.consumables.Remove(itemToDelete);
            } else {
                //-------------Different types of Armor-------------//
                if (list != ah.armor && list[0] is Armor) {
                    folder = "Armor";
                    ah.armor.Remove(itemToDelete);
                }
            }
            AssetDatabase.DeleteAsset($"Assets/Items/{folder}/{itemToDelete.name}.asset");
        }
    }

    public void OnInspectorUpdate()
    {
        this.Repaint();
    }
}