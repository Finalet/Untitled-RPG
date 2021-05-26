using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffIcon : MonoBehaviour
{
    public Buff buff;

    public float timer;
    public TextMeshProUGUI timerText;

    bool noTimer;
    void Start() {
        GetComponent<Image>().sprite = buff.skill.icon;

        timer = buff.duration;
        
        if (timer == 0) {
            noTimer = true;
            timerText.text = "";
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
            timerText.text = Mathf.RoundToInt(timer).ToString();
        }
    }

    void RemoveBuff () {
        Characteristics.instance.RemoveBuff(buff);
        Destroy(gameObject);
    }
}
