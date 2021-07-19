using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInfoDrawer : MonoBehaviour
{
    public TextMeshProUGUI playerName;
    [Space]
    public TextMeshProUGUI maxHealth;
    public TextMeshProUGUI maxStamina;
    [Space]
    public TextMeshProUGUI meleeAttack;
    public TextMeshProUGUI rangedAttack;
    public TextMeshProUGUI magicPower;
    public TextMeshProUGUI healingPower;
    public TextMeshProUGUI defensePower;
    [Space]
    public TextMeshProUGUI strength;
    public TextMeshProUGUI agility;
    public TextMeshProUGUI intellect;
    [Space]
    public TextMeshProUGUI castingSpeed;
    public TextMeshProUGUI attackSpeed;
    [Space]
    public TextMeshProUGUI criticalChance;
    public TextMeshProUGUI criticalStrength;
    public TextMeshProUGUI blockChance;
    public TextMeshProUGUI walkSpeed;

    void Start() {
        playerName.text = Characteristics.instance.playerName;
    }

    void Update() {
        UpdateStats();
    }

    void UpdateStats() {
        string highlightColor = "#" + ColorUtility.ToHtmlStringRGB(UI_General.highlightTextColor);
        maxHealth.text = $"<color={highlightColor}>{Characteristics.instance.maxHealth}</color>\n";
        maxStamina.text = $"<color={highlightColor}>{Characteristics.instance.maxStamina}</color>\n";
        
        meleeAttack.text = $"<color={highlightColor}>{Characteristics.instance.meleeAttack}</color>\n";
        rangedAttack.text = $"<color={highlightColor}>{Characteristics.instance.rangedAttack}</color>\n";
        magicPower.text = $"<color={highlightColor}>{Characteristics.instance.magicPower}</color>\n";
        healingPower.text = $"<color={highlightColor}>{Characteristics.instance.healingPower}</color>\n";
        defensePower.text = $"<color={highlightColor}>{Characteristics.instance.defense}</color>\n";
        
        strength.text = $"<color={highlightColor}>{Characteristics.instance.strength}</color>\n";
        agility.text = $"<color={highlightColor}>{Characteristics.instance.agility}</color>\n";
        intellect.text = $"<color={highlightColor}>{Characteristics.instance.intellect}</color>\n";
        
        castingSpeed.text = $"<color={highlightColor}>{Mathf.Round(Characteristics.instance.castingSpeed.y*1000f)/10f}%</color>\n";
        attackSpeed.text = $"<color={highlightColor}>{Mathf.Round(Characteristics.instance.attackSpeed.y*1000f)/10f}%</color>\n";
        
        criticalChance.text = $"<color={highlightColor}>{Mathf.Round(Characteristics.instance.critChance*1000f)/10f}%</color>\n";
        criticalStrength.text = $"<color={highlightColor}>{Mathf.Round(Characteristics.instance.critStrength*1000f)/10f}%</color>\n";
        blockChance.text = $"<color={highlightColor}>{Mathf.Round(Characteristics.instance.blockChance*1000f)/10f}%</color>\n";
        walkSpeed.text = $"<color={highlightColor}>{Mathf.Round(Characteristics.instance.walkSpeed*1000f)/10f}%</color>\n";
    }
}
