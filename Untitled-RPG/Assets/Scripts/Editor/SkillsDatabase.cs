using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SkillsDatabase : EditorWindow
{
    bool showAll;
    bool showKnight;
    bool showHunter;
    bool showMage;
    bool showAngel;
    bool showStealth;
    bool showDefense;
    bool showSummoner;
    bool showIndependent;

    Vector2 scrollPos;
    Vector2 selectedSkillScrollPos;

    Skill selectedSkill;
    Editor selectedWindow;

    [MenuItem("Finale/Skills Database")]
    static void Init () {
        SkillsDatabase window = (SkillsDatabase)GetWindow(typeof (SkillsDatabase), false, "Skills Database");
        window.Show();
    }

    public void OnGUI() {
        if (GameObject.Find("AssetHolder") == null) {
            EditorGUILayout.LabelField("Asset Holder not found in the scene.");
            return;
        }
        AssetHolder ah = GameObject.Find("AssetHolder").GetComponent<AssetHolder>();

        EditorGUILayout.BeginHorizontal();
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUI.indentLevel = 1;
        DrawTitles();

        EditorGUI.indentLevel = 0;
        showAll = EditorGUILayout.Foldout(showAll, "All skills");
        if (showAll) {
            DrawSkills();
            EditorGUILayout.Space(5, false);
        }

        EditorGUI.indentLevel = 0;
        showKnight = EditorGUILayout.Foldout(showKnight, "Knight");
        if (showKnight) {
            DrawSkills(SkillTree.Knight);
            EditorGUILayout.Space(5, false);
        }

        EditorGUI.indentLevel = 0;
        showHunter = EditorGUILayout.Foldout(showHunter, "Hunter");
        if (showHunter) {
            DrawSkills(SkillTree.Hunter);
            EditorGUILayout.Space(5, false);
        }

        EditorGUI.indentLevel = 0;
        showMage = EditorGUILayout.Foldout(showMage, "Mage");
        if (showMage) {
            DrawSkills(SkillTree.Mage);
            EditorGUILayout.Space(5, false);
        }

        EditorGUI.indentLevel = 0;
        showAngel = EditorGUILayout.Foldout(showAngel, "Angel");
        if (showAngel) {
            DrawSkills(SkillTree.Angel);
            EditorGUILayout.Space(5, false);
        }

        EditorGUI.indentLevel = 0;
        showStealth = EditorGUILayout.Foldout(showStealth, "Stealth");
        if (showStealth) {
            DrawSkills(SkillTree.Stealth);
            EditorGUILayout.Space(5, false);
        }

        EditorGUI.indentLevel = 0;
        showDefense = EditorGUILayout.Foldout(showDefense, "Defense");
        if (showDefense) {
            DrawSkills(SkillTree.Defense);
            EditorGUILayout.Space(5, false);
        }

        EditorGUI.indentLevel = 0;
        showSummoner = EditorGUILayout.Foldout(showSummoner, "Summoner");
        if (showSummoner) {
            DrawSkills(SkillTree.Summoner);
            EditorGUILayout.Space(5, false);
        }

        EditorGUI.indentLevel = 0;
        showIndependent = EditorGUILayout.Foldout(showIndependent, "Independent");
        if (showIndependent) {
            DrawSkills(SkillTree.Independent);
            EditorGUILayout.Space(5, false);
        }

        EditorGUILayout.EndScrollView();

        if (selectedSkill != null) {
            selectedSkillScrollPos = EditorGUILayout.BeginScrollView(selectedSkillScrollPos, GUILayout.Width(position.width/3));
            
            selectedWindow = Editor.CreateEditor(selectedSkill);
            selectedWindow.DrawDefaultInspector();
            
            EditorGUILayout.EndScrollView();
        }

        
        EditorGUILayout.EndHorizontal();
    }

    void DrawSkills () {
        AssetHolder ah = GameObject.Find("AssetHolder").GetComponent<AssetHolder>();
        DrawList(new List<Skill>(ah.Skills));
    }

    void DrawSkills (SkillTree skillTree) {
        AssetHolder ah = GameObject.Find("AssetHolder").GetComponent<AssetHolder>();
        List<Skill> skillsToDraw = new List<Skill>();
        
        foreach (Skill skill in ah.Skills) {
            if (skill.skillTree == skillTree)
                skillsToDraw.Add(skill);
        }

        DrawList(skillsToDraw);
    }
    
    void DrawList(List<Skill> list) {
        EditorGUI.indentLevel ++;
        
        if (list.Count == 0) {
            EditorGUILayout.LabelField("No skills to display");
            return;
        }
        
        foreach (Skill skill in list) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(skill.ID.ToString(), EditorStyles.boldLabel, GUILayout.Width(30 + EditorGUI.indentLevel * 8));
            GUILayout.Box(skill.icon.texture, GUILayout.Width(30), GUILayout.Height(30));
            EditorGUILayout.LabelField(skill.skillName, GUILayout.Width(120 + EditorGUI.indentLevel * 8));
            EditorGUILayout.LabelField(skill.skillTree.ToString(), GUILayout.Width(80 + EditorGUI.indentLevel * 8));
            EditorGUILayout.LabelField(skill.skillType.ToString(), GUILayout.Width(80 + EditorGUI.indentLevel * 8));
            EditorGUILayout.LabelField(skill.damageType.ToString(), GUILayout.Width(80 + EditorGUI.indentLevel * 8));
            string castingTIme = skill.castingTime == 0 ? "Instant" : skill.castingTime.ToString();
            EditorGUILayout.LabelField(castingTIme, GUILayout.Width(90 + EditorGUI.indentLevel * 8));
            string bdp = skill.skillType == SkillType.Damaging ? skill.baseDamagePercentage.ToString() : "-";
            EditorGUILayout.LabelField(bdp, GUILayout.Width(50 + EditorGUI.indentLevel * 8));
            EditorGUILayout.LabelField(skill.coolDown.ToString(), GUILayout.Width(50 + EditorGUI.indentLevel * 8));
            
            Skill skillPrefab = PrefabUtility.GetCorrespondingObjectFromSource(skill);

            EditorGUI.BeginDisabledGroup(true); EditorGUILayout.ObjectField("", skillPrefab, typeof(Item), false, GUILayout.Width(100)); EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Open", GUILayout.Width(70))) {
                selectedSkill = skillPrefab;
                Selection.activeObject = skillPrefab;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    void DrawTitles () {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ID", EditorStyles.boldLabel, GUILayout.Width(65 + EditorGUI.indentLevel * 8));
        EditorGUILayout.LabelField("Skill name", EditorStyles.boldLabel, GUILayout.Width(120 + EditorGUI.indentLevel * 8));
        EditorGUILayout.LabelField("Skilltree", EditorStyles.boldLabel, GUILayout.Width(80 + EditorGUI.indentLevel * 8));
        EditorGUILayout.LabelField("Skill type", EditorStyles.boldLabel, GUILayout.Width(80 + EditorGUI.indentLevel * 8));
        EditorGUILayout.LabelField("Damage type", EditorStyles.boldLabel, GUILayout.Width(80 + EditorGUI.indentLevel * 8));
        EditorGUILayout.LabelField("Casting time", EditorStyles.boldLabel, GUILayout.Width(90    + EditorGUI.indentLevel * 8));
        EditorGUILayout.LabelField(new GUIContent("BDP", "Base damage percentage"), EditorStyles.boldLabel, GUILayout.Width(50 + EditorGUI.indentLevel * 8));
        EditorGUILayout.LabelField(new GUIContent("CD", "Cooldown"), EditorStyles.boldLabel, GUILayout.Width(50 + EditorGUI.indentLevel * 8));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5, false);
    }
}
