using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffIcon : MonoBehaviour
{
    public Skill skill;

    public float timer;
    TextMeshProUGUI timerText;

    bool noTimer;
    void Start() {
        GetComponent<Image>().sprite = skill.icon;
        timerText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        if (timer == 0) {
            noTimer = true;
            timerText.text = "";
        }
    }
    void Update() {
        if (!Characteristics.instance.activeBuffs.Contains(skill)) {
            Destroy(gameObject); //Not removing buff effects since they have already been removed, this is just for checking and removing the icons.
        }

        if (!noTimer) {
            if (timer <= 0) {
                RemoveBuff();
            } else {
                timer -= Time.deltaTime;
            }
            timerText.text = Mathf.RoundToInt(timer).ToString();
        }
    }

    void RemoveBuff () {
        Characteristics.instance.RemoveBuff(skill);
        Destroy(gameObject);
    }
}
