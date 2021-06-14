using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New skillbook", menuName = "Item/Skillbook")]
public class Skillbook : Item
{
    [Header("Skillbook")]
    public Skill learnedSkill;

    public override void Use()
    {
        Instantiate(PeaceCanvas.instance.skillbookPreviewPanel, PeaceCanvas.instance.transform).GetComponent<SkillbookPreview>().focusSkill = AssetHolder.instance.getSkill(learnedSkill.ID);
    }
    public override void Use(UI_InventorySlot initialSlot) {}

    public override string getItemDescription()
    {
        string color = "#" + ColorUtility.ToHtmlStringRGB(UI_General.highlightTextColor);
        return $"Teaches <color={color}>{learnedSkill.skillTree}'s</color> skill <color={color}>\"{learnedSkill.skillName}\"</color>";
    }
}
