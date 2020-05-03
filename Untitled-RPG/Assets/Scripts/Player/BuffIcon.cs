using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffIcon : MonoBehaviour
{
    public Skill skill;
    TextMeshProUGUI timerText;


    float timer;
    void Start() {
        timer = skill.totalAttackTime-skill.startAttackTime;
        GetComponent<Image>().sprite = skill.icon;
        timerText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }
    void Update() {
        if (timer <= 0) {
            Characteristics.instance.RemoveBuff(skill);
            Destroy(gameObject);
        } else {
            timer -= Time.deltaTime;
        }
        timerText.text = Mathf.RoundToInt(timer).ToString();
    }
}
