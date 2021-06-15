using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SkillbookPreview : MonoBehaviour
{
    public static SkillbookPreview instance;

    public Skill focusSkill;
    [Space]
    public Image skillIcon;
    public Text skillName;
    public Text skillTree;
    public Text skillDescription;
    public Text skillStats;
    [Space]

    public GameObject learnedSkillNotificationPrefab;

    void Start() {
        if (instance != null) {
            Destroy(SkillbookPreview.instance.gameObject);
        }
        instance = this;
        Init();
    }

    public void Init() {
        if (skillIcon.sprite != focusSkill.icon)
            skillIcon.sprite = focusSkill.icon;

        if (skillName.text != focusSkill.skillName)
            skillName.text = focusSkill.skillName;
        
        if (skillTree.text != focusSkill.skillTree.ToString())
            skillTree.text = focusSkill.skillTree.ToString();

        if (skillDescription.text != focusSkill.getDescription())
            skillDescription.text = focusSkill.getDescription();

        if (skillStats.text != generateSkillStats())
            skillStats.text = generateSkillStats();

        skillDescription.rectTransform.sizeDelta = new Vector2(skillDescription.rectTransform.sizeDelta.x, skillDescription.preferredHeight);
        skillStats.rectTransform.sizeDelta = new Vector2(skillStats.rectTransform.sizeDelta.x, skillStats.preferredHeight);

        float tooltipHeight = 311 + skillDescription.preferredHeight + skillStats.preferredHeight;
        GetComponent<RectTransform>().sizeDelta = new Vector2(400, tooltipHeight);
    }

    string generateSkillStats () {
        string stats = "";
        string castingTime = focusSkill.castingTime == 0 ? "instant" : $"{focusSkill.castingTime.ToString()} seconds";
        stats += $"Casting time: <color=white>{castingTime}</color>\n";
        stats += $"Cooldown: <color=white>{focusSkill.coolDown.ToString()}</color> seconds";
        return stats; 
    }

    public void LearnSkill (){
        if (Combat.instanace.learnedSkills.Contains(focusSkill)) {
            CanvasScript.instance.DisplayWarning($"You already know {focusSkill.skillName}");
        } else {
            StartCoroutine(learnSkill());
        }
    }
    IEnumerator learnSkill () {
        float learningDuration = 1;
        float learningStarted = Time.time;
        CanvasScript.instance.DisplayProgressBar(true);
        while (Time.time - learningStarted < learningDuration) {
            CanvasScript.instance.DisplayProgressBar(false, (Time.time-learningStarted)/learningDuration, false);
            yield return null;
        }
        CanvasScript.instance.DisplayProgressBar(false, 1, true);
        Combat.instanace.LearnSkill(focusSkill);

        GameObject notification = Instantiate(learnedSkillNotificationPrefab, PeaceCanvas.instance.transform);
        notification.transform.localScale = Vector3.zero;
        notification.GetComponentInChildren<Text>().text = $"You learned \"{focusSkill.skillName}\"";
        notification.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        notification.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.OutExpo).SetDelay(3);
        notification.SetActive(true);
        Destroy(notification, 3.8f);

        Destroy(gameObject);
    }
    
    public void Close () {
        Destroy(gameObject);
    }
}
