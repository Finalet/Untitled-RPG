using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BuffIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Buff buff;

    public float timer;
    public TextMeshProUGUI timerText;

    public RectTransform toolTip;
    public Text tooltipNameLabel;
    public Text tooltipDescriptionLabel;
    public Text tooltipStatsLabel;

    Image image;
    bool noTimer;
    void Start() {
        image = GetComponent<Image>();
        image.sprite = buff.icon;

        timer = buff.duration;
        
        if (timer == 0) {
            noTimer = true;
            timerText.text = "";
            image.color = Color.white;
        } else {
            image.color = Color.gray;
        }
    }
    void Update() {
        if (!Characteristics.instance.activeBuffs.Contains(buff)) {
            Destroy(gameObject); //Not removing buff effects since they have already been removed, this is just for checking and removing the icons.
        }

        if (!noTimer) {
            if (timer <= 0) {
                RemoveBuff();
            } else {
                timer -= Time.deltaTime;
            }
            timerText.text = timer > 120 ? Mathf.RoundToInt(timer/60).ToString() + "m" : Mathf.RoundToInt(timer).ToString();
        }
    }

    void RemoveBuff () {
        Characteristics.instance.RemoveBuff(buff);
        Destroy(gameObject);
    }

    void ShowTooltip(){
        tooltipNameLabel.text = buff.name;
        tooltipDescriptionLabel.text = buff.description;
        tooltipStatsLabel.text = generateBuffStats();

        tooltipDescriptionLabel.rectTransform.sizeDelta = new Vector2(tooltipDescriptionLabel.rectTransform.sizeDelta.x, string.IsNullOrEmpty(tooltipDescriptionLabel.text) ? 0 : tooltipDescriptionLabel.preferredHeight);
        tooltipStatsLabel.rectTransform.sizeDelta = new Vector2(tooltipStatsLabel.rectTransform.sizeDelta.x, string.IsNullOrEmpty(tooltipStatsLabel.text) ? 0 : tooltipStatsLabel.preferredHeight);

        float height = 26 + tooltipDescriptionLabel.preferredHeight + tooltipStatsLabel.preferredHeight;
        toolTip.sizeDelta = new Vector2(toolTip.sizeDelta.x, height);
        toolTip.gameObject.SetActive(true);
    }
    void HideTooltip (){
        toolTip.gameObject.SetActive(false);
    }

    string generateBuffStats () {
        string stats = "";
        string color = $"#{ColorUtility.ToHtmlStringRGB(UI_General.highlightTextColor)}";

        if (buff.healthBuff != 0 || buff.staminaBuff != 0)
            stats += addSpace();

        if (buff.healthBuff != 0) stats += $"Health: <color={color}>+{buff.healthBuff}</color>\n";
        if (buff.staminaBuff != 0) stats += $"Stamina: <color={color}>+{buff.staminaBuff}</color>\n";

        if (buff.strengthBuff != 0 || buff.agilityBuff != 0 || buff.intellectBuff != 0)
            stats += addSpace();

        if (buff.strengthBuff != 0) stats += $"Strength: <color={color}>+{buff.strengthBuff}</color>\n";
        if (buff.agilityBuff != 0) stats += $"Agility: <color={color}>+{buff.agilityBuff}</color>\n";
        if (buff.intellectBuff != 0) stats += $"Intellect: <color={color}>+{buff.intellectBuff}</color>\n";

        if (buff.meleeAttackBuff != 0 || buff.rangedAttackBuff != 0 || buff.magicPowerBuff != 0 || buff.healingPowerBuff != 0 || buff.defenseBuff != 0)
            stats += addSpace();

        if (buff.meleeAttackBuff != 0) stats += $"Melee attack: <color={color}>+{buff.meleeAttackBuff*100}%</color>\n";
        if (buff.rangedAttackBuff != 0) stats += $"Ranged attack: <color={color}>+{buff.rangedAttackBuff*100}%</color>\n";
        if (buff.magicPowerBuff != 0) stats += $"Magic power: <color={color}>+{buff.magicPowerBuff*100}%</color>\n";
        if (buff.healingPowerBuff != 0) stats += $"Healing power: <color={color}>+{buff.healingPowerBuff*100}%</color>\n";
        if (buff.defenseBuff != 0) stats += $"Defense: <color={color}>+{buff.defenseBuff*100}%</color>\n";

        if (buff.castingSpeedBuff != 0 || buff.attackSpeedBuff != 0)
            stats += addSpace();

        if (buff.castingSpeedBuff != 0) stats += $"Casting speed: <color={color}>{buff.castingSpeedBuff*100}%</color>\n";
        if (buff.attackSpeedBuff != 0) stats += $"Attack speed: <color={color}>{buff.attackSpeedBuff*100}%</color>\n";

        if (buff.walkSpeedBuff != 0)
            stats += addSpace();

        if (buff.walkSpeedBuff != 0) stats += $"Walk speed: <color={color}>+{buff.walkSpeedBuff*100}%</color>\n";

        if (buff.skillDistanceBuff != 0)
            stats += addSpace();

        if (buff.skillDistanceBuff != 0) stats += $"Skill distance: <color={color}>+{buff.skillDistanceBuff}</color>\n";

        if (buff.immuneToDamage || buff.immuneToInterrupt)
            stats += addSpace();

        if (buff.immuneToDamage) stats += $"<color={color}>Immune to damage.</color>\n";
        if (buff.immuneToInterrupt) stats += $"<color={color}>Immune to interruptions.</color>\n";

        return stats;
    }

    string addSpace() {
        return "<size=8>\n</size>";
    }

    public virtual void OnPointerEnter (PointerEventData eventData) {
        ShowTooltip();
    }
    public virtual void OnPointerExit (PointerEventData eventData) {
        HideTooltip();
    }
    public virtual void OnPointerClick (PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right){
            RemoveBuff();
        }
    }
}
