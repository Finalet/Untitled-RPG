using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFSW.QC;
using QFSW.QC.Utilities;
using Funly.SkyStudio;
using System;
using System.Reflection;
using Object = UnityEngine.Object;
using AwesomeTechnologies.VegetationSystem;

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
    
    [CommandPrefix("skills.")]
    public static class SkillsCommands {

        [Command("learn", "Learns specified skill.")]
        static void LearnSkill (string skillName) {
            if (!PlayerControlls.instance || !Combat.instanace) throw new System.Exception("No player instance was found.");

            Combat.instanace.LearnSkill(AssetHolder.instance.getSkill(skillName));
            
            PeaceCanvas.instance.CloseSkillsPanel();
        }

        [Command("learn-all", "Learns all skills.")]
        static void LearnAllSkills () {
            if (!PlayerControlls.instance || !Combat.instanace) throw new System.Exception("No player instance was found.");

            foreach (Skill skill in AssetHolder.instance.Skills){
                if (skill.skillTree == SkillTree.Independent)
                    continue;
                Combat.instanace.LearnSkill(skill);
            }

            PeaceCanvas.instance.CloseSkillsPanel();
        }

        [Command("forget", "Forgets specified skill.")]
        static void ForgetSkill (string skillName) {
            if (!PlayerControlls.instance || !Combat.instanace) throw new System.Exception("No player instance was found.");
            
            Combat.instanace.ForgetSkill(AssetHolder.instance.getSkill(skillName));
            
            PeaceCanvas.instance.CloseSkillsPanel();
        }

        [Command("forget-all", "Forgets all skills.")]
        static void ForgetAllSkill () {
            if (!PlayerControlls.instance || !Combat.instanace) throw new System.Exception("No player instance was found.");
            
            foreach (Skill skill in AssetHolder.instance.Skills){
                if (skill.skillTree == SkillTree.Independent)
                    continue;
                Combat.instanace.ForgetSkill(skill);
            }
            
            PeaceCanvas.instance.CloseSkillsPanel();
        }

    }

    [CommandPrefix("skill-trees.")]
    public static class SkilltreesCommands {

        [Command("unlock-all", "Unlocks all skilltrees.")]
        static void UnlockAllSkilltrees() {
            if (!PlayerControlls.instance || !Combat.instanace) throw new System.Exception("No player instance was found.");

            foreach (SkillTree s in System.Enum.GetValues(typeof(SkillTree))) {
                if (s == SkillTree.Independent) continue;

                if (!Combat.instanace.currentSkillTrees.Contains(s)) Combat.instanace.currentSkillTrees.Add(s);
            }
        }

        [Command("unlock", "Unlocks specified skilltree.")]
        static void UnlockSkilltree (SkillTree skillTree) {
            if (!PlayerControlls.instance || !Combat.instanace) throw new System.Exception("No player instance was found.");

            if (!Combat.instanace.currentSkillTrees.Contains(skillTree)) Combat.instanace.currentSkillTrees.Add(skillTree);
        }

        [Command("forget", "Forgets specified skilltree.")]
        static void ForgetSkilltree (SkillTree skillTree) {
            if (!PlayerControlls.instance || !Combat.instanace) throw new System.Exception("No player instance was found.");
            
            if (Combat.instanace.currentSkillTrees.Contains(skillTree)) Combat.instanace.currentSkillTrees.Remove(skillTree);
        }

        [Command("forget-all", "Forgets all skilltrees.")]
        static void ForgetAllSkilltrees() {
            if (!PlayerControlls.instance || !Combat.instanace) throw new System.Exception("No player instance was found.");

            foreach (SkillTree s in System.Enum.GetValues(typeof(SkillTree))) {
                if (s == SkillTree.Independent) continue;

                if (Combat.instanace.currentSkillTrees.Contains(s)) Combat.instanace.currentSkillTrees.Remove(s);
            }
        }
    }

    [Command("suicide")]
    static void Suicide () {
        if (!PlayerControlls.instance || !Characteristics.instance) throw new System.Exception("No player instance was found.");

        Characteristics.instance.health = 0;
        Characteristics.instance.Die();
    }

    [Command("give-item")]
    static void GiveItem (string itemName, int itemAmount = 1) {
        if (!PlayerControlls.instance || !InventoryManager.instance) throw new System.Exception("No player instance was found.");
        if (!AssetHolder.instance) throw new System.Exception("No Asset Holder was found.");
            
        Item itemToGive = AssetHolder.instance.getItem(itemName);
        InventoryManager.instance.AddItemToInventory(itemToGive, itemAmount, null);

        if (itemAmount > itemToGive.maxStackAmount) Debug.LogWarning($"Item amount was limited to the maximum stack size {itemToGive.maxStackAmount}");
    }

    [Command("give-item-by-id")]
    static void GiveItem (int itemID, int itemAmount = 1) {
        if (!PlayerControlls.instance || !InventoryManager.instance) throw new System.Exception("No player instance was found.");
        if (!AssetHolder.instance) throw new System.Exception("No Asset Holder was found.");
        
        Item itemToGive = AssetHolder.instance.getItem(itemID);
        InventoryManager.instance.AddItemToInventory(itemToGive, itemAmount, null);

        if (itemAmount > itemToGive.maxStackAmount) Debug.LogWarning($"Item amount was limited to the maximum stack size {itemToGive.maxStackAmount}");
    }

}

public static class UtilityCommands  {

    [Command("call-variable")]
    static string CallVariable (Type type, string variable, string value = null) {
        if (value == null) return ReadVariable(type, variable);
        else return SetVariable(type, variable, value);
    }

    static string ReadVariable (Type type, string variable) {
        Object target = CommandsHelper.FindObject(type);
        FieldInfo field = CommandsHelper.FindField(type, variable);

        string value = field.GetValue(target).ToString().ColorText(QuantumConsole.Instance.Theme.DefaultReturnValueColor);

        return $"[{target.name} - {type}] {field.Name}: {value.ColorText(QuantumConsole.Instance.Theme.DefaultReturnValueColor)}";
    }

    static string SetVariable (Type type, string variable, string value) {
        Object target = CommandsHelper.FindObject(type);
        FieldInfo field = CommandsHelper.FindField(type, variable);

        object setValue = CommandsHelper.Parser.Parse(value, field.FieldType);
        
        field.SetValue(target, setValue);

        return $"[{target.name} - {type}] Set {field.Name} to {field.GetValue(target).ToString().ColorText(QuantumConsole.Instance.Theme.DefaultReturnValueColor)}";        
    }

    [Command("call-variable-static")]
    static string CallStaticVariable (Type type, string variable, string value = null) {
        if (value == null) return ReadStaticVariable(type, variable);
        else return SetStaticVariable(type, variable, value);
    }

    static string ReadStaticVariable (Type type, string variable) {
        FieldInfo field = CommandsHelper.FindField(type, variable);

        return $"[{type}] {field.Name}: {field.GetValue(null).ToString().ColorText(QuantumConsole.Instance.Theme.DefaultReturnValueColor)}";
    }

    static string SetStaticVariable (Type type, string variable, string value) {
        FieldInfo field = CommandsHelper.FindField(type, variable);

        object setValue = CommandsHelper.Parser.Parse(value, field.FieldType);

        field.SetValue(null, setValue);
        
        return $"[{type}] Set {field.Name} to {field.GetValue(null).ToString().ColorText(QuantumConsole.Instance.Theme.DefaultReturnValueColor)}";        
    }

    [Command("call-variable-help", "Display all available fields in a class.")]
    public static string CallVariableHelp (Type type) {
        FieldInfo[] fields = CommandsHelper.FindAllFields(type);

        string output = "\n--- Available Fields ---\n\n";

        foreach (FieldInfo field in fields) output += $"- {field.Name}\n";

        return output;
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

public static class CommandsHelper {

    public static readonly QuantumParser Parser = new QuantumParser();

    public static FieldInfo FindField (Type type, string variable) {

        const BindingFlags flags =  BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod |
                                       BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        
        FieldInfo field = type.GetField(variable, flags);

        if (field == null) throw new Exception($"Could not find variable \"{variable}\" inside \"{type}\".");

        return field;
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
}

} 

