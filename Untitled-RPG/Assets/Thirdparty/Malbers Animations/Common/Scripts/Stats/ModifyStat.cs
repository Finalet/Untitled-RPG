using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace MalbersAnimations 
{
    [AddComponentMenu("Malbers/Stats/Modify Stats")]

    public class ModifyStat : MonoBehaviour
    {
        public Stats stats;

        public List<StatModifier> modifiers = new List<StatModifier>();

        public virtual void SetStats(GameObject go) => stats = go.FindComponent<Stats>();
        public virtual void SetStats(Component go) => SetStats(go.gameObject);


        /// <summary> Apply All Modifiers to the Stats </summary>
        public virtual void Modify()
        {
            foreach (var statmod in modifiers)
                statmod.ModifyStat(stats);
        }

        public virtual void Modify(GameObject target)
        {
            SetStats(target);
            Modify();
        }
        public virtual void Modify(Component target) 
        {
            Modify(target.gameObject); 
        }

        /// <summary> Apply a Modifiers to the Stats using its Index</summary>
        public virtual void Modify(int index)
        {
            if (modifiers != null && index < modifiers.Count)
                modifiers[index]?.ModifyStat(stats);
        }
    }

    public enum StatOption
    {
        None,
        /// <summary>Add to the Stat Value </summary>
        AddValue,
        /// <summary>Set a new Stat Value </summary>
        SetValue,
        /// <summary>Remove to the Stat Value </summary>
        SubstractValue,
        /// <summary>Modify Add|Remove the Stat MAX Value </summary>
        ModifyMaxValue,
        /// <summary>Set a new Stat MAX Value </summary>
        SetMaxValue,
        /// <summary>Enable the Degeneration </summary>
        Degenerate,
        /// <summary>Disable the Degeneration </summary>
        StopDegenerate,
        /// <summary>Enable the Regeneration </summary>
        Regenerate,
        /// <summary>Disable the Regeneration </summary>
        StopRegenerate,
        /// <summary>Reset the Stat to the Default Min or Max Value </summary>
        Reset,
        /// <summary>Reduce the Value of the Stat by a percent</summary>
        ReduceByPercent,
        /// <summary>Increase the Value of the Stat by a percent</summary>
        IncreaseByPercent,
        /// <summary>Sets the multiplier of a stat</summary>
        Multiplier,
        /// <summary>Reset the Stat to the Max Value</summary>
        ResetToMax,
        /// <summary>Reset the Stat to the Min Value</summary>
        ResetToMin,
    }

    /// <summary> Modify a Stat usings its properties </summary>
    [System.Serializable]
    public class StatModifier
    {
        //public bool active = true;
        public StatID ID;
        public StatOption modify = StatOption.None;
        public FloatReference Value = new FloatReference(0);


       


        public StatModifier()
        {
            ID = null;
            modify = StatOption.None;
            Value = new FloatReference(0);
        }

        public StatModifier(StatModifier mod)
        {
            ID = mod.ID;
            modify = mod.modify;
            Value = new FloatReference(mod.Value.Value);
        }
        /// <summary>There's No ID stat</summary>
        public bool IsNull => ID == null;


        /// <summary>Modify the Stats on an animal </summary>
        public void ModifyStat(Stats stats)
        {
            if (stats != null && ID != null) ModifyStat(stats.Stat_Get(ID));
        }

        /// <summary>Modify the Stats on an animal </summary>
        public void ModifyStat(Stat s) => s?.Modify(Value, modify);
    }
}

//--------------------EDITOR----------------
#if UNITY_EDITOR

[CustomEditor(typeof(ModifyStat))]
public class ModifyStatEditor : Editor
{
    private UnityEditorInternal.ReorderableList RList_modifiers;

    SerializedProperty modifiers,stats;
    private ModifyStat m;
    private MonoScript script;
    public static GUIStyle StyleBlue => MTools.Style(new Color(0, 0.5f, 1f, 0.3f));


    private void OnEnable()
    {
        modifiers = serializedObject.FindProperty("modifiers");
        stats = serializedObject.FindProperty("stats");
        m = (ModifyStat)target;
        script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);

        RList_modifiers = new UnityEditorInternal.ReorderableList(serializedObject, modifiers, true, true, true , true)
        {
            drawElementCallback = Draw_Element_Reo,
            drawHeaderCallback = Draw_Header_Reo,
            onAddCallback = OnAdd_Modify
        };
    }

    private void OnAdd_Modify(ReorderableList list)
    {
        if (m.modifiers == null) m.modifiers = new List<StatModifier>();
        m.modifiers.Add(new StatModifier());
        EditorUtility.SetDirty(target);
        Undo.RecordObject(target, "Stat Modify Add");
    }

    private void Draw_Header_Reo(Rect rect)
    {
        var idRet = new Rect(rect);
        var ID_Rect = new Rect(rect);
        var  oRect = new Rect(rect);
        idRet.width = 65;

        EditorGUI.LabelField(idRet, new GUIContent("  Index", "Index of the Array"));
        ID_Rect.x += 60;
        ID_Rect.width = 60;
        EditorGUI.LabelField(ID_Rect, new GUIContent("Stat ID", "ID of the Stat to Modify"));

        oRect.x += 45+ rect.width / 3 + 5; 
        oRect.width = (rect.width / 3 + 5)-15;

        EditorGUI.LabelField(oRect, new GUIContent("Option", "ID of the Stat to Modify"));
        var Value_REct = new Rect(oRect);
        Value_REct.x += rect.width / 3 + 18;
        Value_REct.width -= 38;

        EditorGUI.LabelField(Value_REct, new GUIContent("Value", "Value to Apply to the modification"));
    }

    private void Draw_Element_Reo(Rect rect, int index, bool isActive, bool isFocused)
    {
        rect.y += 2;
        rect.width -= 20;

        var property = modifiers.GetArrayElementAtIndex(index);
        var ID = property.FindPropertyRelative("ID");
        var modify = property.FindPropertyRelative("modify");
        var Value = property.FindPropertyRelative("Value");

        var line = new Rect(rect); 

        var IndexRect = new Rect(rect);

        IndexRect.x = rect.x - 2;
        IndexRect.width = 30;

        EditorGUI.LabelField(IndexRect, "[" + index + "]");
        line.height = EditorGUIUtility.singleLineHeight;
        line.x += 45;

        line.width = rect.width / 3 + 5;
        EditorGUI.PropertyField(line, ID, new GUIContent(string.Empty, "ID for the Stat to modify"));
        line.x += rect.width / 3 + 5;
        line.width += -15;
        EditorGUI.PropertyField(line, modify, new GUIContent(string.Empty, "Option to Modify"));
        line.x += rect.width / 3 + 18;
        line.width -= 38;
        EditorGUI.PropertyField(line, Value, new GUIContent(string.Empty, "Value to Apply"));
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
      
        EditorGUILayout.PropertyField(stats);
        RList_modifiers.DoLayoutList();

        EditorGUI.BeginDisabledGroup(!m.stats);
        if (Application.isPlaying && GUILayout.Button("Modify Stat"))
        {
            m.Modify();
        }
        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
    }
}


[CustomPropertyDrawer(typeof(StatModifier))]
public class StatModifierDrawer : PropertyDrawer
{
    private readonly string[] Tooltips = {
          "[None] Skips the stat modification",
          "Adds to the stat Value",
          "Sets the stat value",
          "Substracts from the stat value",
          "Modifies the Stat Max Value",
          "Set the Stat MAX Value",
          "Enables the Degeneration and sets the Degen Rate Value. If the value is 0, the rate wont be changed",
          "Stops the Degeneration",
          "Enables the Regeneration and sets the Regen Rate Value.  If the value is 0, the rate wont be changed",
          "Stops the Regeneration",
          "Reset the Stat to the Default Min or Max Value",
          "Reduce the Value of the Stat by a percent",
          "Increase the Value of the Stat by a percent",
          "Sets the multiplier value of the stat",
          "Reset the Stat to the Max Value",
          "Reset the Stat to the Min Value",
    };


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

       // label.tooltip = Tooltips[modify.intValue]; 

        EditorGUI.PrefixLabel(position, label);

        position.y += 2;
        position.x += 15;
        position.width -= 12;


        var indent = EditorGUI.indentLevel;
        var height = EditorGUIUtility.singleLineHeight;


        var ID = property.FindPropertyRelative("ID");
        var Value = property.FindPropertyRelative("Value");
        var modify = property.FindPropertyRelative("modify");


        var line = position;

        line.y += height;
        line.height = height;




        line.x += 5;
        line.width = position.width / 3 + 5;
        EditorGUI.PropertyField(line, ID, new GUIContent(string.Empty, "ID for the Stat to modify"));


        line.x += position.width / 3 + 10;
        line.width += -15;
        EditorGUI.PropertyField(line, modify, new GUIContent(string.Empty, "Option to Modify"));
        var LN = new Rect(line);
       
        LN.y -= height;
        EditorGUI.LabelField(LN, new GUIContent("Type", Tooltips[modify.intValue]));
        LN.y += height;
        EditorGUI.LabelField(LN, new GUIContent("             ", Tooltips[modify.intValue]));


        line.x += position.width / 3 + 17;
        line.width -= 25;


        EditorGUI.PropertyField(line, Value, new GUIContent(string.Empty, "Value to Apply"));
        LN = new Rect(line);
        LN.y -= height;
        EditorGUI.LabelField(LN, new GUIContent("Value"));

        EditorGUIUtility.labelWidth = 0;
        property.serializedObject.ApplyModifiedProperties();

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)   { return 16 * 2 + 6; }
 
}
#endif