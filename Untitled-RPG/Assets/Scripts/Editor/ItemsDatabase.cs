using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemsDatabase : EditorWindow
{
    bool showIconsToggle = true;

    bool showWeapons;
    bool showArmor;
    bool showSkillbooks;
    bool showConsumables;
    bool showResources;
    bool showMounts;
    bool showMountEquipment;
    bool showShipAttachements;

    bool showAllWeapons;
    bool showSwords;
    bool showAxes;
    bool showStaffs;
    bool showBows;
    bool showShields;

    bool showAllArmor;
    bool showHelmets;
    bool showChests;
    bool showGloves;
    bool showPants;
    bool showBoots;
    bool shotBacks;
    bool showNecklaces;
    bool showRings;

    bool showAllResources;
    bool showCraftingResources;
    bool showQuestItems;
    bool showMisc;

    bool showAllMountEquipment;
    bool showMountSaddles;
    bool showMountArmor;

    bool showAllShipAttachements;
    bool showCannons;
    bool showSails;
    bool showHelms;
    bool showFlags;

    Vector2 scrollPos;
    Vector2 scrollPosSelection;
    Item selectedItem;
    Editor selectedWindow;
    bool changingSelectedItemName = false;

    [MenuItem("Finale/Items Database")]
    static void Init()
    {
        ItemsDatabase window = (ItemsDatabase)GetWindow(typeof(ItemsDatabase), false, "Items Database");
        window.Show();
    }

    public void OnGUI()
    {
        if (GameObject.Find("Managers") == null) {
            EditorGUILayout.LabelField("Asset Holder not found in the scene.");
            return;
        }
        AssetHolder ah = GameObject.Find("Managers").GetComponent<AssetHolder>();
        
        EditorGUILayout.BeginHorizontal();


        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        showIconsToggle = EditorGUILayout.ToggleLeft("Show icons", showIconsToggle);
        EditorGUILayout.Space(10);

        EditorGUI.indentLevel = 0;
        showWeapons = EditorGUILayout.Foldout(showWeapons, "Weapons");
        if (showWeapons) {
            DrawWeaponsList(ah);
            EditorGUILayout.Space(5, false);
        }

        EditorGUI.indentLevel = 0;
        showArmor = EditorGUILayout.Foldout(showArmor, "Armor");
        if (showArmor) {
            DrawArmorList(ah);
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

        EditorGUI.indentLevel = 0;
        showResources = EditorGUILayout.Foldout(showResources, "Resources");
        if (showResources) {
            DrawResourcesList(ah);
            EditorGUILayout.Space(5, false);
        }

        EditorGUI.indentLevel = 0;
        showMounts = EditorGUILayout.Foldout(showMounts, "Mounts");
        if (showMounts) {
            DrawList(ah.mounts);
            EditorGUILayout.Space(5, false);
        }

        EditorGUI.indentLevel = 0;
        showMountEquipment = EditorGUILayout.Foldout(showMountEquipment, "Mount Equipment");
        if (showMountEquipment) {
            DrawMountEquipmentList(ah);
            EditorGUILayout.Space(5, false);
        }

        EditorGUI.indentLevel = 0;
        showShipAttachements = EditorGUILayout.Foldout(showShipAttachements, "Ship Attachements");
        if (showShipAttachements) {
            DrawShipAttachementsList(ah);
            EditorGUILayout.Space(5, false);
        }
        
        EditorGUI.indentLevel = 0;
        EditorGUILayout.Space(10);
        if (GUILayout.Button("Sort all by ID", GUILayout.Width(200))) {
            ah.SortAllByID();
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
            if (changingSelectedItemName && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)){
                string assetPath =  AssetDatabase.GetAssetPath(selectedItem.GetInstanceID());
                AssetDatabase.RenameAsset(assetPath, selectedItem.name);
                changingSelectedItemName = false;
            }
            
            EditorGUILayout.EndHorizontal();

            selectedWindow = Editor.CreateEditor(selectedItem);
            selectedWindow.OnInspectorGUI();

            EditorGUILayout.EndScrollView();
        } else {
            changingSelectedItemName = false;
        }


        EditorGUILayout.EndHorizontal();
    }

    void DrawTitles (List<Item> list) {
        EditorGUILayout.BeginHorizontal();
        int iconSpace = showIconsToggle ? 35 : 0;
        EditorGUILayout.LabelField("ID", EditorStyles.boldLabel, GUILayout.Width(50 + iconSpace + EditorGUI.indentLevel * 8));
        EditorGUILayout.LabelField("Item name", EditorStyles.boldLabel, GUILayout.Width(230 + EditorGUI.indentLevel * 8));
        EditorGUILayout.LabelField("Item rarity", EditorStyles.boldLabel, GUILayout.Width(100 + EditorGUI.indentLevel * 8));
        EditorGUILayout.LabelField("Item price", EditorStyles.boldLabel, GUILayout.Width(100 + EditorGUI.indentLevel * 8));
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
            if (showIconsToggle) DrawTexturePreview(EditorGUILayout.GetControlRect(GUILayout.Width(30), GUILayout.Height(30)), item.itemIcon);
            EditorGUILayout.LabelField(item.name, GUILayout.Width(230 + EditorGUI.indentLevel * 8));
            EditorGUILayout.LabelField(item.itemRarity.ToString(), GUILayout.Width(100 + EditorGUI.indentLevel * 8));
            EditorGUILayout.LabelField(item.itemBasePrice.ToString(), GUILayout.Width(100 + EditorGUI.indentLevel * 8));
            DrawAdditionalFields(item);
            EditorGUI.BeginDisabledGroup(true); EditorGUILayout.ObjectField("", item, typeof(Item), false, GUILayout.Width(100)); EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Open", GUILayout.Width(70))) {
                selectedItem = item;
                GUI.FocusControl(null);
            }
            if (GUILayout.Button("-", GUILayout.Width(15))){
                DeleteItem(list, item);
                return;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20 * EditorGUI.indentLevel);
        if (GUILayout.Button("+", GUILayout.Width(20))) {
            AddNewItem(list);
        }
        EditorGUILayout.EndHorizontal();
    }
    
    void DrawArmorList (AssetHolder ah) {
        List<Item> helmets = new List<Item>();
        List<Item> chests = new List<Item>();
        List<Item> gloves = new List<Item>();
        List<Item> pants = new List<Item>();
        List<Item> boots = new List<Item>();
        List<Item> backs = new List<Item>();
        List<Item> necklaces = new List<Item>();
        List<Item> rings = new List<Item>();
        


        Armor e;
        foreach (Item item in ah.armor){
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
        DrawTitles(ah.armor);
        
        EditorGUI.indentLevel = 1;
        showAllArmor = EditorGUILayout.Foldout(showAllArmor, "All armor");
        if(showAllArmor) {
            DrawList(ah.armor, true);
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

    void DrawWeaponsList (AssetHolder ah) {
        List<Item> Swords = new List<Item>();
        List<Item> Staffs = new List<Item>();
        List<Item> Bows = new List<Item>();
        List<Item> Shields = new List<Item>();
        List<Item> Axes = new List<Item>();

        Weapon w;
        foreach (Item item in ah.weapons){
            w = (Weapon)item;
            switch (w.weaponCategory) {
                case WeaponCategory.Sword:
                    Swords.Add(w);
                    break;
                case WeaponCategory.Staff:
                    Staffs.Add(w);
                    break;
                case WeaponCategory.Bow:
                    Bows.Add(w);
                    break;
                case WeaponCategory.Shield:
                    Shields.Add(w);
                    break;
                case WeaponCategory.Axe:
                    Axes.Add(w);
                    break;
            }
        }

        EditorGUI.indentLevel = 2;
        DrawTitles(ah.weapons);
        
        EditorGUI.indentLevel = 1;
        showAllWeapons = EditorGUILayout.Foldout(showAllWeapons, "All weapons");
        if(showAllWeapons) {
            DrawList(ah.weapons, true);
        }

        EditorGUI.indentLevel = 1;
        showSwords = EditorGUILayout.Foldout(showSwords, "Swords");
        if (showSwords) {
            DrawList(Swords, true);
        }
        EditorGUI.indentLevel = 1;
        showAxes = EditorGUILayout.Foldout(showAxes, "Axes");
        if (showAxes) {
            DrawList(Axes, true);
        }
        EditorGUI.indentLevel = 1;
        showStaffs = EditorGUILayout.Foldout(showStaffs, "Staffs");
        if (showStaffs) {
            DrawList(Staffs, true);
        }
        EditorGUI.indentLevel = 1;
        showBows = EditorGUILayout.Foldout(showBows, "Bows");
        if (showBows) {
            DrawList(Bows, true);
        }
        EditorGUI.indentLevel = 1;
        showShields = EditorGUILayout.Foldout(showShields, "Shields");
        if (showShields) {
            DrawList(Shields, true);
        }
    }

    void DrawResourcesList (AssetHolder ah) {
        List<Item> CraftingResources = new List<Item>();
        List<Item> QuestItems = new List<Item>();
        List<Item> Misc = new List<Item>();

        Resource r;
        foreach (Item item in ah.resources){
            r = (Resource)item;
            switch (r.resourceType) {
                case ResourceType.CraftingResource:
                    CraftingResources.Add(r);
                    break;
                case ResourceType.QuestItem:
                    QuestItems.Add(r);
                    break;
                case ResourceType.Misc:
                    Misc.Add(r);
                    break;
            }
        }

        EditorGUI.indentLevel = 2;
        DrawTitles(ah.resources);
        
        EditorGUI.indentLevel = 1;
        showAllResources = EditorGUILayout.Foldout(showAllResources, "All resources");
        if(showAllResources) {
            DrawList(ah.resources, true);
        }

        EditorGUI.indentLevel = 1;
        showCraftingResources = EditorGUILayout.Foldout(showCraftingResources, "Crafting resources");
        if (showCraftingResources) {
            DrawList(CraftingResources, true);
        }
        EditorGUI.indentLevel = 1;
        showQuestItems = EditorGUILayout.Foldout(showQuestItems, "Quest items");
        if (showQuestItems) {
            DrawList(QuestItems, true);
        }
        EditorGUI.indentLevel = 1;
        showMisc = EditorGUILayout.Foldout(showMisc, "Misc");
        if (showMisc) {
            DrawList(Misc, true);
        }
    }

    void DrawMountEquipmentList (AssetHolder ah) {
        List<Item> Saddles = new List<Item>();
        List<Item> Armor = new List<Item>();

        MountEquipment me;
        foreach (Item item in ah.mountEquipment){
            me = (MountEquipment)item;
            switch (me.equipmentType) {
                case MountEquipmentType.Saddle:
                    Saddles.Add(me);
                    break;
                case MountEquipmentType.Armor:
                    Armor.Add(me);
                    break;
            }
        }

        EditorGUI.indentLevel = 2;
        DrawTitles(ah.mountEquipment);
        
        EditorGUI.indentLevel = 1;
        showAllMountEquipment = EditorGUILayout.Foldout(showAllMountEquipment, "All mount equipment");
        if(showAllMountEquipment) {
            DrawList(ah.mountEquipment, true);
        }

        EditorGUI.indentLevel = 1;
        showMountSaddles = EditorGUILayout.Foldout(showMountSaddles, "Saddles");
        if (showMountSaddles) {
            DrawList(Saddles, true);
        }
        EditorGUI.indentLevel = 1;
        showMountArmor = EditorGUILayout.Foldout(showMountArmor, "Armor");
        if (showMountArmor) {
            DrawList(Armor, true);
        }
    }

    void DrawShipAttachementsList (AssetHolder ah) {
        List<Item> Cannons = new List<Item>();
        List<Item> Sails = new List<Item>();
        List<Item> Helms = new List<Item>();
        List<Item> Flags = new List<Item>();

        ShipAttachement sa;
        foreach (Item item in ah.shipAttachements){
            sa = (ShipAttachement)item;
            switch (sa.attachementType) {
                case ShipAttachementType.Cannons:
                    Cannons.Add(sa);
                    break;
                case ShipAttachementType.Sails:
                    Sails.Add(sa);
                    break;
                case ShipAttachementType.Helm:
                    Helms.Add(sa);
                    break;
                case ShipAttachementType.Flag:
                    Flags.Add(sa);
                    break;
            }
        }

        EditorGUI.indentLevel = 2;
        DrawTitles(ah.shipAttachements);
        
        EditorGUI.indentLevel = 1;
        showAllShipAttachements = EditorGUILayout.Foldout(showAllShipAttachements, "All ship attachements");
        if(showAllShipAttachements) {
            DrawList(ah.shipAttachements, true);
        }

        EditorGUI.indentLevel = 1;
        showCannons = EditorGUILayout.Foldout(showCannons, "Cannons");
        if (showCannons) {
            DrawList(Cannons, true);
        }
        EditorGUI.indentLevel = 1;
        showSails = EditorGUILayout.Foldout(showSails, "Sails");
        if (showSails) {
            DrawList(Sails, true);
        }
        EditorGUI.indentLevel = 1;
        showHelms = EditorGUILayout.Foldout(showHelms, "Helms");
        if (showHelms) {
            DrawList(Helms, true);
        }
        EditorGUI.indentLevel = 1;
        showFlags = EditorGUILayout.Foldout(showFlags, "Flags");
        if (showFlags) {
            DrawList(Flags, true);
        }
    }

    void DrawAdditionalFieldsTitles (List<Item> list) {
        AssetHolder ah = GameObject.Find("Managers").GetComponent<AssetHolder>();
        
        if (list[0] is Weapon) {
            EditorGUILayout.LabelField("Category", EditorStyles.boldLabel, GUILayout.Width(100));
            EditorGUILayout.LabelField("Hand", EditorStyles.boldLabel, GUILayout.Width(150));
        } else if (list[0] is Armor) {
            EditorGUILayout.LabelField("Armor type", EditorStyles.boldLabel, GUILayout.Width(150));
        } else if (list[0] is Skillbook) {
            EditorGUILayout.LabelField("Skill tree", EditorStyles.boldLabel, GUILayout.Width(150));
        } else if (list[0] is Consumable) {
            EditorGUILayout.LabelField("Consumable type", EditorStyles.boldLabel, GUILayout.Width(150));
        } else if (list[0] is Resource) {
            EditorGUILayout.LabelField("Resource type", EditorStyles.boldLabel, GUILayout.Width(150));
        } else if (list[0] is Mount) {
            EditorGUILayout.LabelField("Movement speed", EditorStyles.boldLabel, GUILayout.Width(150));
            EditorGUILayout.LabelField("Stamina", EditorStyles.boldLabel, GUILayout.Width(80));
        } else if (list[0] is MountEquipment) {
            EditorGUILayout.LabelField("Type", EditorStyles.boldLabel, GUILayout.Width(80));
        } else if (list[0] is ShipAttachement) {
            EditorGUILayout.LabelField("Type", EditorStyles.boldLabel, GUILayout.Width(100));
        }
    }
    void DrawAdditionalFields (Item item) {
        if (item is Weapon) {
            Weapon w = (Weapon)item;
            EditorGUILayout.LabelField(w.weaponCategory.ToString(), GUILayout.Width(100));
            EditorGUILayout.LabelField(w.weaponHand.ToString(), GUILayout.Width(150));
        } else if (item is Armor) {
            Armor a = (Armor)item;
            EditorGUILayout.LabelField(a.armorType.ToString(), GUILayout.Width(150));
        } else if (item is Consumable) {
            Consumable c = (Consumable)item;
            EditorGUILayout.LabelField(c.consumableType.ToString(), GUILayout.Width(150));
        } else if (item is Skillbook) {
            Skillbook s = (Skillbook)item;
            EditorGUILayout.LabelField(s.learnedSkill != null ? s.learnedSkill.skillTree.ToString() : "No skills assigned", GUILayout.Width(150));
        } else if (item is Resource) {
            Resource r = (Resource)item;
            EditorGUILayout.LabelField(r.resourceType.ToString(), GUILayout.Width(150));
        } else if (item is Mount) {
            Mount m = (Mount)item;
            EditorGUILayout.LabelField(m.movementSpeed.ToString(), GUILayout.Width(150));
            EditorGUILayout.LabelField(m.maxStamina.ToString(), GUILayout.Width(80));
        } else if (item is MountEquipment) {
            MountEquipment m = (MountEquipment)item;
            EditorGUILayout.LabelField(m.equipmentType.ToString(), GUILayout.Width(80));
        } else if (item is ShipAttachement) {
            ShipAttachement sa = (ShipAttachement)item;
            EditorGUILayout.LabelField(sa.attachementType.ToString(), GUILayout.Width(100));
        }
    }

    void AddNewItem (List<Item> list) {
        AssetHolder ah = GameObject.Find("Managers").GetComponent<AssetHolder>();
        
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
            name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Items/Armor/AR_New Armor.asset");
            AssetDatabase.CreateAsset(a, name);
            a.ID = list[list.Count-1].ID + 1;
            a.itemName = "New Armor";
            ah.armor.Add(a);
            selectedItem = a;
        } else if (list == ah.skillbooks) {
            Skillbook s = ScriptableObject.CreateInstance<Skillbook>();
            name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Items/Skillbooks/SB_New Skillbook.asset");
            AssetDatabase.CreateAsset(s, name);
            s.ID = list[list.Count-1].ID + 1;
            s.itemName = "New Skillbook";
            ah.skillbooks.Add(s);
            selectedItem = s;
        } else if (list == ah.consumables) {
            Consumable c = ScriptableObject.CreateInstance<Consumable>();
            name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Items/Consumables/CM_New Consumable.asset");
            AssetDatabase.CreateAsset(c, name);
            c.ID = list[list.Count-1].ID + 1;
            c.itemName = "New Consumable";
            ah.consumables.Add(c);
            selectedItem = c;
        } else if (list == ah.resources) {
            Resource r = ScriptableObject.CreateInstance<Resource>();
            name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Items/Resources/RE_New Resource.asset");
            AssetDatabase.CreateAsset(r, name);
            r.ID = list[list.Count-1].ID + 1;
            r.itemName = "New Resource";
            ah.resources.Add(r);
            selectedItem = r;
        } else if (list == ah.mounts) {
            Mount m = ScriptableObject.CreateInstance<Mount>();
            name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Items/Mounts/MO_New Mount.asset");
            AssetDatabase.CreateAsset(m, name);
            m.ID = list[list.Count-1].ID + 1;
            m.itemName = "New Mount";
            ah.mounts.Add(m);
            selectedItem = m;
        } else if (list == ah.mountEquipment) {
            MountEquipment me = ScriptableObject.CreateInstance<MountEquipment>();
            name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Items/Mount Equipment/ME_New Mount Equipment.asset");
            AssetDatabase.CreateAsset(me, name);
            me.ID = list[list.Count-1].ID + 1;
            me.itemName = "New Mount Equipment";
            ah.mountEquipment.Add(me);
            selectedItem = me;
        } else {
            //-------------Different types of Armor-------------//
            if (list != ah.armor && list[0] is Armor) {
                Armor existingArmor = (Armor)list[0];
                Armor a = ScriptableObject.CreateInstance<Armor>();
                name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath($"Assets/Items/Armor/AR_New {existingArmor.armorType}.asset");
                AssetDatabase.CreateAsset(a, name);
                a.ID = list[list.Count-1].ID + 1;
                a.itemName = $"New {existingArmor.armorType}";
                a.armorType = existingArmor.armorType;
                ah.armor.Add(a);
                selectedItem = a;
            }

            //-------------Different types of Weapons-------------//
            if (list != ah.weapons && list[0] is Weapon) {
                Weapon existingWeapon = (Weapon)list[0];
                Weapon w = ScriptableObject.CreateInstance<Weapon>();
                name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath($"Assets/Items/Weapons/WP_New {existingWeapon.weaponCategory}.asset");
                AssetDatabase.CreateAsset(w, name);
                w.ID = list[list.Count-1].ID + 1;
                w.itemName = $"New {existingWeapon.weaponCategory}";
                w.weaponCategory = existingWeapon.weaponCategory;
                ah.weapons.Add(w);
                selectedItem = w;
            }

            //-------------Different types of Resources-------------//
            if (list != ah.resources && list[0] is Resource) {
                Resource existingResource = (Resource)list[0];
                Resource r = ScriptableObject.CreateInstance<Resource>();
                name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath($"Assets/Items/Weapons/RE_New {existingResource.resourceType}.asset");
                AssetDatabase.CreateAsset(r, name);
                r.ID = list[list.Count-1].ID + 1;
                r.itemName = $"New {existingResource.resourceType}";
                r.resourceType = existingResource.resourceType;
                ah.resources.Add(r);
                selectedItem = r;
            }

            //-------------Different types of Mount Equipment-------------//
            if (list != ah.mountEquipment && list[0] is MountEquipment) {
                MountEquipment existingMountEquipment = (MountEquipment)list[0];
                MountEquipment me = ScriptableObject.CreateInstance<MountEquipment>();
                name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath($"Assets/Items/Mount Equipment/RE_New {existingMountEquipment.equipmentType}.asset");
                AssetDatabase.CreateAsset(me, name);
                me.ID = list[list.Count-1].ID + 1;
                me.itemName = $"New {existingMountEquipment.equipmentType}";
                me.equipmentType = existingMountEquipment.equipmentType;
                ah.mountEquipment.Add(me);
                selectedItem = me;
            }
        }

        ah.SortAllByID();
    }

    void DeleteItem (List<Item> list, Item itemToDelete) {
        AssetHolder ah = GameObject.Find("Managers").GetComponent<AssetHolder>();
        
        if (EditorUtility.DisplayDialog($"Delete {itemToDelete.name}?", $"Are you sure you want to delete {itemToDelete.name}? This action is irreversable.", "Delete", "Cancel")) {          
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
            } else if (list == ah.resources) {
                folder = "Resources";
                ah.resources.Remove(itemToDelete);
            } else if (list == ah.mounts) {
                folder = "Mounts";
                ah.mounts.Remove(itemToDelete);
            } else if (list == ah.mountEquipment) {
                folder = "Mount Equipment";
                ah.mountEquipment.Remove(itemToDelete);
            } else {
                //-------------Different types of Armor-------------//
                if (list != ah.armor && list[0] is Armor) {
                    folder = "Armor";
                    ah.armor.Remove(itemToDelete);
                }

                //-------------Different types of Weapons-------------//
                if (list != ah.weapons && list[0] is Weapon) {
                    folder = "Weapons";
                    ah.weapons.Remove(itemToDelete);
                }

                //-------------Different types of Resources-------------//
                if (list != ah.resources && list[0] is Resource) {
                    folder = "Resources";
                    ah.resources.Remove(itemToDelete);
                }

                //-------------Different types of Mount Equipment-------------//
                if (list != ah.mountEquipment && list[0] is MountEquipment) {
                    folder = "Mount Equipment";
                    ah.mountEquipment.Remove(itemToDelete);
                }
            }
            AssetDatabase.DeleteAsset($"Assets/Items/{folder}/{itemToDelete.name}.asset");
        }
    }

    public void OnInspectorUpdate()
    {
        this.Repaint();
    }

    void DrawTexturePreview(Rect position, Sprite sprite)
        {
            if (sprite == null)
                return;
            Vector2 fullSize = new Vector2(sprite.texture.width, sprite.texture.height);
            Vector2 size = new Vector2(sprite.textureRect.width, sprite.textureRect.height);
 
            Rect coords = sprite.textureRect;
            coords.x /= fullSize.x;
            coords.width /= fullSize.x;
            coords.y /= fullSize.y;
            coords.height /= fullSize.y;
 
            Vector2 ratio;
            ratio.x = position.width / size.x;
            ratio.y = position.height / size.y;
            float minRatio = Mathf.Min(ratio.x, ratio.y);
 
            Vector2 center = position.center;
            position.width = size.x * minRatio;
            position.height = size.y * minRatio;
            position.center = center;
 
            GUI.DrawTextureWithTexCoords(position, sprite.texture, coords);
        }
}