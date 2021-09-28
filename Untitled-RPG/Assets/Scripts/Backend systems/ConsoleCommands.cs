using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using QFSW.QC;
using QFSW.QC.Utilities;
using QFSW.QC.Actions;
using Funly.SkyStudio;
using System;
using System.Reflection;
using Object = UnityEngine.Object;
using AwesomeTechnologies.VegetationSystem;
using Crest;
using UnityEngine.SceneManagement;
using Tayx.Graphy;
using DG.Tweening;

namespace Finale.ConsoleCommands {

[CommandPrefix("free-camera.")]
public static class FreeCameraControllerCommands
{
    [Command("disable-control")]
    static bool disableControl {
        get {
            FreeCameraController camController = Transform.FindObjectOfType<FreeCameraController>();
            if (!camController) throw new System.Exception("There is no active free camera.");

            return camController.disableControl;
        }
        set {
            FreeCameraController camController = Transform.FindObjectOfType<FreeCameraController>();
            if (!camController) throw new System.Exception("There is no active free camera.");

            camController.disableControl = value;
        }
    }

    [Command("speed")]
    static float speed {
        get {
            FreeCameraController camController = Transform.FindObjectOfType<FreeCameraController>();
            if (!camController) throw new System.Exception("There is no active free camera.");

            return camController.speed;
        }
        set {
            FreeCameraController camController = Transform.FindObjectOfType<FreeCameraController>();
            if (!camController) throw new System.Exception("There is no active free camera.");

            camController.speed = value;
        }
    }
}

[CommandPrefix("world.")]
public static class WorldCommands
{
    [Command("current-time")]
    static string currentTime => TimeOfDayController.instance.TimeStringFromPercent(TimeOfDayController.instance.timeOfDay);
    
    
    [Command("set-world-time", "Set day time between 0 and 1, which corresponds to 00:00 and 24:00 in game.")]
    static float SetTime  {
        set {
            if (value > 1 || value < 0) throw new System.Exception("Time can only be between 0 and 1.");
            TimeOfDayController.instance.skyTime = value;
        } 
    }

    [Command("set-world-time-speed", "Set the speed (between 0 and 1) with which the day-night cycle changes. Expressed in \"days per second\". Default: 0.00028 (1 in-game day ≈ 1 real hour)")]
    static float SetTimeSpeed {
        set {
            if (value > 1 || value < 0) throw new System.Exception("World time speed can only be between 0 and 1.");
            TimeOfDayController.instance.automaticIncrementSpeed = value;
        } 
    }
    [Command("reset-world-time-speed", "Set the world time speed to default 0.00028 (1 in-game day ≈ 1 real hour).")]
    static void ResetWorldSpeed () {
        SetTimeSpeed = 0.00028f;
    }

    [Command("match-fog-to", "Get scene fog color matching.")]
    static TimeOfDayController.MatchFogTo matchFogTo => TimeOfDayController.instance.matchFogTo;
    
    [Command("match-fog-to", "Set scene fog color to Horizon, Middle-sky, Upper-Sky, or Nothing.")]
    static void MatchFogTo (string matchToWhat) {
        if (matchToWhat == "Horizon") TimeOfDayController.instance.matchFogTo = TimeOfDayController.MatchFogTo.Horizon;
        else if (matchToWhat == "Middle") TimeOfDayController.instance.matchFogTo = TimeOfDayController.MatchFogTo.Middle;
        else if (matchToWhat == "Upper") TimeOfDayController.instance.matchFogTo = TimeOfDayController.MatchFogTo.Upper;
        else if (matchToWhat == "Nothing") TimeOfDayController.instance.matchFogTo = TimeOfDayController.MatchFogTo.Nothing;
        else Debug.LogError($"Cannot match fog color to {matchToWhat}. Available options are: Horizon, Middle, Upper, Nothing.");
    }
}

[CommandPrefix("items.")]
public static class ItemsCommands {
    
    [Command("get-item-info", "Get all information about the item with the specified ID")]
    static string getItemInfo (int itemID) {
        if (!AssetHolder.instance) throw new System.Exception("No active \"Asset holder\" instance. Cannot get item information.");

        var output = JsonUtility.ToJson(AssetHolder.instance.getItem(itemID), true);
        return output;
    }
    [Command("get-item-info-by-name", "Get all information about the item with the specified name")]
    static string getItemInfo (string itemName) {
        if (!AssetHolder.instance) throw new System.Exception("No active \"Asset holder\" instance. Cannot get item information.");

        var output = JsonUtility.ToJson(AssetHolder.instance.getItem(itemName), true);
        return output;
    }
    
    [Command("all-items")]
    static void PrintAllItems () {
        if (!AssetHolder.instance) throw new System.Exception("No active \"Asset holder\" instance. Cannot get items information.");

        string output = "";

        output += "\n--- Weapons ---\n";
        foreach (Item item in AssetHolder.instance.weapons) {
            output += $"ID: {item.ID}   |   {item.itemName}\n";
        }

        output += "\n--- Armor ---\n";
        foreach (Item item in AssetHolder.instance.armor) {
            output += $"ID: {item.ID}   |   {item.itemName}\n";
        }

        output += "\n--- Consumables ---\n";
        foreach (Item item in AssetHolder.instance.consumables) {
            output += $"ID: {item.ID}   |   {item.itemName}\n";
        }

        output += "\n--- Skillbooks ---\n";
        foreach (Item item in AssetHolder.instance.skillbooks) {
            output += $"ID: {item.ID}   |   {item.itemName}\n";
        }

        output += "\n--- Resources ---\n";
        foreach (Item item in AssetHolder.instance.resources) {
            output += $"ID: {item.ID}   |   {item.itemName}\n";
        }

        output += "\n--- Mounts ---\n";
        foreach (Item item in AssetHolder.instance.mounts) {
        output += $"ID: {item.ID}   |   {item.itemName}\n";
        }

        output += "\n--- Mount Equipment ---\n";
        foreach (Item item in AssetHolder.instance.mountEquipment) {
            output += $"ID: {item.ID}   |   {item.itemName}\n";
        }

        Debug.Log(output);
    }
}

[CommandPrefix("player.")]
public static class PlayerCommands {
    
    static AssetHolder assetHolder {
        get {
            if (!AssetHolder.instance) throw new Exception("Could not find \"Asset Holder\" instance.");
            return AssetHolder.instance;
        }
    }
    static PlayerControlls playerControlls {
        get {
            if (!PlayerControlls.instance) throw new Exception("Could not find \"Player Controlls\" instance.");
            return PlayerControlls.instance;
        }
    }
    static Combat combat {
        get {
            if (!Combat.instanace) throw new Exception("Could not find \"Combat\" instance.");
            return Combat.instanace;
        }
    }
    static PeaceCanvas peaceCanvas {
        get {
            if (!PeaceCanvas.instance) throw new Exception("Could not find \"Peace Canvas\" instance.");
            return PeaceCanvas.instance;
        }
    }
    static Characteristics characteristics {
        get {
            if (!Characteristics.instance) throw new Exception("Could not find \"Characteristics\" instance.");
            return Characteristics.instance;
        }
    }
    static InventoryManager inventoryManager {
        get {
            if (!InventoryManager.instance) throw new Exception("Could not find \"Inventory Manager\" instance.");
            return InventoryManager.instance;
        }
    }


    [CommandPrefix("skills.")]
    public static class SkillsCommands {

        [Command("learn", "Learns specified skill.")]
        static void LearnSkill (string skillName) {
            combat.LearnSkill(assetHolder.getSkill(skillName));
            
            peaceCanvas.CloseSkillsPanel();
        }

        [Command("learn-all", "Learns all skills.")]
        public static void LearnAllSkills () {
            foreach (Skill skill in assetHolder.Skills){
                if (skill.skillTree == SkillTree.Independent)
                    continue;
                combat.LearnSkill(skill);
            }

            peaceCanvas.CloseSkillsPanel();
        }

        [Command("forget", "Forgets specified skill.")]
        static void ForgetSkill (string skillName) {
            combat.ForgetSkill(AssetHolder.instance.getSkill(skillName));
            
            peaceCanvas.CloseSkillsPanel();
        }

        [Command("forget-all", "Forgets all skills.")]
        static void ForgetAllSkill () {
            foreach (Skill skill in assetHolder.Skills){
                if (skill.skillTree == SkillTree.Independent)
                    continue;
                combat.ForgetSkill(skill);
            }
            
            peaceCanvas.CloseSkillsPanel();
        }

        [Command("set-cooldown", "Set cooldown in seconds for a specified skill.")]
        static void SetCooldown (string skill, float value) {
            Skill s = assetHolder.getSkill(skill);
            if (!s) throw new Exception($"Could not find \"{skill}\" skill.");
    
            s.coolDown = value;
        }
        [Command("set-cooldown-all", "Set all skills' cooldown to a certain value.")]
        public static void SetAllSkillsCooldown (float value) {
            foreach (Skill s in assetHolder.Skills) {
                s.coolDown = value;
            }
        }

        [Command("set-damage-base-percentage", "Set base damage percentage for a specific skill.")]
        static void SetBDP (string skill, int value) {
            assetHolder.getSkill(skill).baseDamagePercentage = value;
        }

        [Command("set-damage-type", "Set damage type for a specified skill.")]
        static void SetDamageType (string skill, DamageType type) {
            assetHolder.getSkill(skill).damageType = type;
        }


    }

    [CommandPrefix("skill-trees.")]
    public static class SkilltreesCommands {

        [Command("unlock-all", "Unlocks all skilltrees.")]
        public static void UnlockAllSkilltrees() {
            foreach (SkillTree s in System.Enum.GetValues(typeof(SkillTree))) {
                if (s == SkillTree.Independent) continue;

                if (!combat.currentSkillTrees.Contains(s)) combat.currentSkillTrees.Add(s);
            }
        }

        [Command("unlock", "Unlocks specified skilltree.")]
        static void UnlockSkilltree (SkillTree skillTree) {
            if (!combat.currentSkillTrees.Contains(skillTree)) combat.currentSkillTrees.Add(skillTree);
        }

        [Command("forget", "Forgets specified skilltree.")]
        static void ForgetSkilltree (SkillTree skillTree) {
            if (combat.currentSkillTrees.Contains(skillTree)) combat.currentSkillTrees.Remove(skillTree);
        }

        [Command("forget-all", "Forgets all skilltrees.")]
        static void ForgetAllSkilltrees() {
            foreach (SkillTree s in System.Enum.GetValues(typeof(SkillTree))) {
                if (s == SkillTree.Independent) continue;

                if (combat.currentSkillTrees.Contains(s)) combat.currentSkillTrees.Remove(s);
            }
        }
    }

    [Command("suicide")]
    static void Suicide () {
        characteristics.health = 0;
        characteristics.Die(new DamageInfo(0, DamageType.Raw, false, "suicide."));
    }
    [Command("revive")]
    static void Revive () {
        characteristics.Revive();
    }

    [Command("give-item")]
    static void GiveItem (string itemName, int itemAmount = 1) {
        Item itemToGive = assetHolder.getItem(itemName);
        inventoryManager.AddItemToInventory(itemToGive, itemAmount, null);

        if (itemAmount > itemToGive.maxStackAmount) Debug.LogWarning($"Item amount was limited to the maximum stack size {itemToGive.maxStackAmount}");
    }

    [Command("give-item-by-id")]
    static void GiveItem (int itemID, int itemAmount = 1) {
        Item itemToGive = assetHolder.getItem(itemID);
        inventoryManager.AddItemToInventory(itemToGive, itemAmount, null);

        if (itemAmount > itemToGive.maxStackAmount) Debug.LogWarning($"Item amount was limited to the maximum stack size {itemToGive.maxStackAmount}");
    }

    [Command("godmode")]
    static void Godmode () {
        SkillsCommands.SetAllSkillsCooldown(0);
        UtilityCommands.CallVariable(playerControlls.GetType(), "staminaReqToRoll", "0");
        UtilityCommands.CallVariable(playerControlls.GetType(), "staminaReqToSprint", "0");
        SkillsCommands.LearnAllSkills();
        SkilltreesCommands.UnlockAllSkilltrees();

        Debug.Log("All skills cooldown set to 0.");
        Debug.Log("Stamina use set to 0.");
        Debug.Log("Unlocked all trees and learned all skills.");
    }

}

public static class UtilityCommands  {

    [Command("call-field", "Get or set specified field in a specified class.")]
    public static string CallVariable ([CommandParameterDescription("Class containing the field.")] Type classType, [CommandParameterDescription("Field name")] string field, [CommandParameterDescription("Set value for the field.")] string value = null) {
        if (value == null) return ReadVariable(classType, field);
        else return SetVariable(classType, field, value);
    }

    static string ReadVariable (Type classType, string variable) {
        Object target = CommandsHelper.FindObject(classType);
        FieldInfo field = CommandsHelper.FindField(classType, variable);

        string value = field.GetValue(target).ToString().ColorText(QuantumConsole.Instance.Theme.DefaultReturnValueColor);

        return $"[{target.name} - {classType}] {field.Name}: {value.ColorText(QuantumConsole.Instance.Theme.DefaultReturnValueColor)}";
    }

    static string SetVariable (Type classType, string variable, string value) {
        Object target = CommandsHelper.FindObject(classType);
        FieldInfo field = CommandsHelper.FindField(classType, variable);

        object setValue = CommandsHelper.Parser.Parse(value, field.FieldType);
        
        field.SetValue(target, setValue);

        return $"[{target.name} - {classType}] Set {field.Name} to {field.GetValue(target).ToString().ColorText(QuantumConsole.Instance.Theme.DefaultReturnValueColor)}";        
    }

    [Command("call-field-static")]
    static string CallStaticVariable (Type classType, string field, string value = null) {
        if (value == null) return ReadStaticVariable(classType, field);
        else return SetStaticVariable(classType, field, value);
    }

    static string ReadStaticVariable (Type classType, string variable) {
        FieldInfo field = CommandsHelper.FindField(classType, variable);

        return $"[{classType}] {field.Name}: {field.GetValue(null).ToString().ColorText(QuantumConsole.Instance.Theme.DefaultReturnValueColor)}";
    }

    static string SetStaticVariable (Type classType, string variable, string value) {
        FieldInfo field = CommandsHelper.FindField(classType, variable);

        object setValue = CommandsHelper.Parser.Parse(value, field.FieldType);

        field.SetValue(null, setValue);
        
        return $"[{classType}] Set {field.Name} to {field.GetValue(null).ToString().ColorText(QuantumConsole.Instance.Theme.DefaultReturnValueColor)}";        
    }

    [Command("call-field-help", "Display all available fields in a class.")]
    public static string CallVariableHelp ([CommandParameterDescription("Class containings the fields.")] Type classType) {
        FieldInfo[] fields = CommandsHelper.FindAllFields(classType);

        string output = "\n--- Available Fields ---\n\n";

        foreach (FieldInfo field in fields) output += $"- {field.Name}\n";

        return output;
    }


    [Command("call-instance-help", "Display all available methods in a class.")]
    private static string CallInstanceHelp ([CommandParameterDescription("Class containing the methods.")] Type classType) {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | /*| BindingFlags.InvokeMethod | */
                                    BindingFlags.Static | BindingFlags.Instance;// | BindingFlags.FlattenHierarchy;
        
        MethodInfo[] methods = classType.GetMethods(flags);

        string output = "\n --- Available Methods ---\n";

        foreach (MethodInfo method in methods) {
            ParameterInfo[] parameters = method.GetParameters();
            output += $"\n- {method.Name}{ (parameters.Length > 0 ? ":" : "")}";

            for (int i = 0; i < parameters.Length; i++) {
                output += $" {parameters[i].GetType().ToString()}: {parameters[i].Name}{ (i != parameters.Length-1 ? "," : "") }";
            }
        }

        return output;
    }

    [Command("create-object")]
    static string CreateObject (string name) {
        return CreateObject(name, "");
    }

    [Command("create-object", "Creates an object with specified name and adds a component from an assembly.")]
    static string CreateObject ([CommandParameterDescription("Name to set")] string name, [CommandParameterDescription("Component to add")]string component) {
        GameObject createdObject = new GameObject();
        createdObject.name = name;

        string output = $"Created new object: {createdObject.ToString().ColorText(QuantumConsole.Instance.Theme.DefaultReturnValueColor)}";

        if (!string.IsNullOrEmpty(component)) {
            Type getType = GetTypeWithAssembly(component);
            if (getType == null) {
                GameObject.Destroy(createdObject);
                throw new Exception($"Could not get \"{component}\".");
            }

            createdObject.AddComponent(getType);

            output = $"Created new object: {createdObject.GetComponent(getType).ToString().ColorText(QuantumConsole.Instance.Theme.DefaultReturnValueColor)}.";
        }

        return output;
    }

    [Command("cursor.show")]
    static void ShowCursor (bool _lock = false) {
        ToggleCursor(true, _lock ? CursorLockMode.Locked : CursorLockMode.None);
    }
    [Command("cursor.hide")]
    static void HideCursor (bool _lock = true) {
        ToggleCursor(true, _lock ? CursorLockMode.Locked : CursorLockMode.None);
    }


    [Command("capture-screenshot")]
    [CommandDescription("Captures a screenshot and saves it to the supplied file path as a PNG.\n" +
                        "If superSize is supplied the screenshot will be captured at a higher than native resolution.")]
    private static IEnumerator<ICommandAction> CaptureScreenshot(
        [CommandParameterDescription("The name of the file to save the screenshot in")] string filename,
        [CommandParameterDescription("Factor by which to increase resolution")] int superSize = 1
    )
    {
        QuantumConsole.Instance.ToggleVisibility(false);
        yield return new WaitFrame();

        if (!Directory.Exists(Application.persistentDataPath + "/Screenshots")) Directory.CreateDirectory(Application.persistentDataPath + "/Screenshots");
        
        ScreenCapture.CaptureScreenshot(Application.persistentDataPath + "/Screenshots/" + filename + ".png", superSize);
        Debug.Log("Saved screenshot to: " + Application.persistentDataPath + "/Screenshots/" + filename + ".png");
        yield return new WaitFrame();
        QuantumConsole.Instance.ToggleVisibility(true);
    }

    [Command("get-transform-hierarchy")]
    static string GetTransformHierarchy (string parent = null) {
        string output = $"\n--- {(!string.IsNullOrEmpty(parent) ? parent : "Scene")} Hierarchy ---\n";
        
        if (string.IsNullOrEmpty(parent)) {
            List<GameObject> sceneObjects = new List<GameObject>(); 
            for (int i = 0; i < SceneManager.sceneCount; i++){
                sceneObjects.AddRange(SceneManager.GetSceneAt(i).GetRootGameObjects());
            }

            for (int i = 0; i < sceneObjects.Count; i++) {
                output += $"\n{sceneObjects[i].transform.name}";
            }

            return output;
        }

        Transform p = null;

        if (parent.Contains("-")){
            string[] words = parent.Split('-');
            
            for (int i = 0; i<words.Length; i++) {
                if (i == 0) {
                    p = GameObject.Find(words[i]).transform;
                    continue;
                }
                p = p.Find(words[i]).transform;
            }
        } else {
            p = GameObject.Find(parent).transform;
        }

        for (int i = 0; i < p.childCount; i++) {
            output += $"\n{p.GetChild(i).name}";
        }

        return output;
    }

    [Command("performance-monitor", "Show or hide the performance monitor.")]
    static void TogglePerformanceMonitor () {
        if (!GraphyManager.Instance) GameObject.Instantiate(Resources.Load<GameObject>("Performance Monitor"));
        else GameObject.Destroy(GraphyManager.Instance.gameObject);
    }

    [Command("teleport-to")]
    public static void Teleport (Transform teleportee, Transform destination) {
        teleportee.transform.position = destination.transform.position;
    }

    static void ToggleCursor (bool visible, CursorLockMode lockMode) {
        Cursor.visible = visible;
        Cursor.lockState = lockMode;
    }

    public static Type GetTypeWithAssembly( string TypeName ) {
    
        // Try Type.GetType() first. This will work with types defined
        // by the Mono runtime, etc.
        var type = Type.GetType( TypeName );
    
        // If it worked, then we're done here
        if( type != null )
            return type;
    
        // Get the name of the assembly (Assumption is that we are using
        // fully-qualified type names)
        string assemblyName = "";
        try {
            assemblyName = TypeName.Substring( 0, TypeName.LastIndexOf( '.' ) );
        }
        catch {
            throw new Exception($"Could not find type {TypeName}. Try specifying an assembly.");
        }

        // Attempt to load the indicated Assembly
        var assembly = Assembly.Load(assemblyName);// LoadWithPartialName( assemblyName );
        if( assembly == null ) {
            throw new Exception($"Could not load {assembly} assembly.");
        }
    
        // Ask that assembly to return the proper Type
        return assembly.GetType( TypeName );
    
    }   
}

[CommandPrefix("vegetation.")]
public static class VegetationCommands {

    private static readonly QuantumParser Parser = new QuantumParser();

    [Command("settings", "Get or set settings of the global vegitation system.")]
    static string VegetationSetting ([CommandParameterDescription("Which settings to get or set.")] string setting, [CommandParameterDescription("Value of the settings to set.")] string value = null) {
        if (value == null) return ReadVegetationSettings(setting);
        else return SetVegetationSettings(setting, value);
    }

    static string SetVegetationSettings (string setting, string value) {
        VegetationSystemPro vsp = Object.FindObjectOfType<VegetationSystemPro>();

        if (!vsp) throw new Exception("No vegetation system found.");

        VegetationSettings vs = vsp.VegetationSettings;

        FieldInfo field = CommandsHelper.FindField(vs.GetType(), setting);
        
        object setValue = CommandsHelper.Parser.Parse(value, field.FieldType);

        field.SetValue(vs, setValue);

        vsp.ClearCache();

        return $"[Vegetation Settings] Set {field.Name} to {field.GetValue(vs).ToString().ColorText(QuantumConsole.Instance.Theme.DefaultReturnValueColor)}";        
    }

    static string ReadVegetationSettings (string setting) {
        VegetationSystemPro vsp = Object.FindObjectOfType<VegetationSystemPro>();

        if (!vsp) throw new Exception("No vegetation system found.");

        VegetationSettings vs = vsp.VegetationSettings;

        FieldInfo field = CommandsHelper.FindField(vs.GetType(), setting);

        return $"[Vegetation Settings] {field.Name}: {field.GetValue(vs).ToString().ColorText(QuantumConsole.Instance.Theme.DefaultReturnValueColor)}";     
    }

    [Command("settings-help", "Display all available settings.")]
    static string GetAllSettings () {
        return UtilityCommands.CallVariableHelp(new VegetationSettings().GetType());
    }

}

[CommandPrefix("ocean.")]
public static class OceanCommands {

    static OceanRenderer oceanRenderer {
        get {
            if (!OceanRenderer.Instance) throw new Exception("Could not find \"Ocean Renderer\" instance.");
            return OceanRenderer.Instance;
        }
    }

    [Command("create-camera")]
    static string CreateOceanCamera (string name = "") {
        Camera newCam = new GameObject().AddComponent<Camera>();
        newCam.gameObject.name = string.IsNullOrEmpty(name) ? "Ocean Camera" : name;

        oceanRenderer.ViewCamera = newCam;

        return $"Created new camera {newCam.ToString().ColorText(QuantumConsole.Instance.Theme.DefaultReturnValueColor)} and assigned it to the ocean renderer.";
    }
    [Command("assign-camera")]
    static string AssignOceanCamera (GameObject camera) {
        Camera newCam = camera.GetComponent<Camera>();
        if (!newCam) throw new Exception ($"{camera} does not contain a camera component.");

        oceanRenderer.ViewCamera = newCam;

        return $"Assigned {newCam.ToString().ColorText(QuantumConsole.Instance.Theme.DefaultReturnValueColor)} to the ocean renderer.";
    }

}

[CommandPrefix("console.")]
public static class ConsoleCommands {
    
    [Command("keybinds-help")]
    static string ConsoleKeysHelp () {
        string output = "";

        FieldInfo[] fields = CommandsHelper.FindAllFields(QuantumConsole.Instance.KeyConfig.GetType());
        
        output += "\n--- Console Keybinds ---\n";
        Type keycodeType = new KeyCode().GetType(); 
        Type keyConfigtype = new ModifierKeyCombo().GetType();
        foreach (FieldInfo field in fields){
            if (field.FieldType == keycodeType) output += $"\n- {field.Name}: {field.GetValue(QuantumConsole.Instance.KeyConfig).ToString()}";

            if (field.FieldType == keyConfigtype) {
                ModifierKeyCombo k = (ModifierKeyCombo)field.GetValue(QuantumConsole.Instance.KeyConfig);
                string keyText = $"{(k.Ctrl ? "Ctrl+" : "")}{(k.Alt ? "Alt+" : "")}{(k.Shift ? "Shift+" : "")}{k.Key}";
                output += $"\n- {field.Name}: {keyText}";
            }
        }
        return output;
    }
}

[CommandPrefix("watchers.")]
public static class WatcherCommans {

    [Command("create", "Create a watcher that will show a field value updated each frame.")]
    public static void CreateWatcher ([CommandParameterDescription("Target object to watch")] GameObject target, [CommandParameterDescription("Class of the field to watch")] Type classType, [CommandParameterDescription("Which field to watch")] string field, [CommandParameterDescription("Name of the watcher")] string watcherName = "") {
        Object target1 = target ? CommandsHelper.FindObjectOnGameobject(target, classType) : CommandsHelper.FindObject(classType);
        
        FieldInfo field1 = null;
        PropertyInfo propertyInfo1 = null;
        try {
            field1 = CommandsHelper.FindField(classType, field);
        } catch {
            try {
            propertyInfo1 = CommandsHelper.FindProperty(classType, field);
            } catch {
                throw new Exception($"Could not find neither field nor property called \"{field}\" in \"{classType}\".");
            }
        }

        DEBUG_Watchers watcherPrefab = GameObject.FindObjectOfType<DEBUG_Watchers>();
        if (!watcherPrefab) watcherPrefab = GameObject.Instantiate(Resources.Load<GameObject>("WatcherPrefab"), QuantumConsole.Instance.transform).GetComponent<DEBUG_Watchers>();
        
        
        if (field1 != null) watcherPrefab.CreateWatcher(watcherName, field1, target1);
        else if (propertyInfo1 != null) watcherPrefab.CreateWatcher(watcherName, propertyInfo1, target1);
        
    }

    [Command("create")]
    public static void CreateWatcher (Type classType, string field, string watcherName = "") {
        CreateWatcher(null, classType, field, watcherName);
    }

    [Command("delete")]
    public static void DeleteWatcher (string watcher) {
        DEBUG_Watchers watcherPrefab = GameObject.FindObjectOfType<DEBUG_Watchers>();
        if (!watcherPrefab) throw new Exception("There are no active watchers.");

        watcherPrefab.DeleteWatcher(watcher);
    }

    [Command("delete-all")]
    public static void DeleteAllWatchers () {
        DEBUG_Watchers watcherPrefab = GameObject.FindObjectOfType<DEBUG_Watchers>();
        if (!watcherPrefab) throw new Exception("There are no active watchers.");

        GameObject.Destroy(watcherPrefab.gameObject);
    }

}

public static class CommandsHelper {

    public static readonly QuantumParser Parser = new QuantumParser();

    public static FieldInfo FindField (Type type, string field) {

        const BindingFlags flags =  BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod |
                                       BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        
        FieldInfo field1 = type.GetField(field, flags);

        if (field1 == null) throw new Exception($"Could not find field \"{field}\" inside \"{type}\".");

        return field1;
    }

    public static FieldInfo[] FindAllFields (Type type) {
        const BindingFlags flags =  BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod |
                                       BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        
        FieldInfo[] fields = type.GetFields(flags);

        if (fields.Length <= 0) throw new Exception($"Could not find any fields in \"{type}\".");

        return fields;
    }

    public static Object FindObject (Type type) {
        Object target = Object.FindObjectOfType(type);
        if (!target) throw new Exception($"No target of type \"{type}\" was found."); 

        return target;
    }

    public static Object FindObjectOnGameobject (GameObject gameObject, Type type) {
        Object target = gameObject.GetComponent(type);
        if (!target) throw new Exception($"No target of type \"{type}\" was found."); 

        return target;
    }

    public static PropertyInfo FindProperty (Type type, string property) {

        const BindingFlags flags =  BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod |
                                       BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        
        PropertyInfo property1 = type.GetProperty(property, flags);

        if (property1 == null) throw new Exception($"Could not find property \"{property1}\" inside \"{type}\".");

        return property1;
    }
}

} 

