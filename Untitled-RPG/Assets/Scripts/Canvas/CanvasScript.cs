using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasScript : MonoBehaviour
{
    public static CanvasScript instance;

    [Header("Player")]
    public Image healthBar;
    public Image staminaBar;

    [Header("Enemy")]
    public Image enemyHealthBar;
    public TextMeshProUGUI enemyName;

    Characteristics characteristics;

    void Awake() {
        if (instance == null) 
            instance = this;
    }

    void Start() {
        characteristics = PlayerControlls.instance.GetComponent<Characteristics>();
    }

    void Update() {
        DisplayHPandStamina();
    }

    void DisplayHPandStamina () {
        healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, (float)characteristics.HP/characteristics.maxHP, 0.1f);
        healthBar.transform.GetChild(0).GetComponent<Text>().text = characteristics.HP.ToString();
        staminaBar.fillAmount = Mathf.Lerp(staminaBar.fillAmount, (float)characteristics.Stamina/characteristics.maxStamina, 0.1f);
        staminaBar.transform.GetChild(0).GetComponent<Text>().text = characteristics.Stamina.ToString();
    }

    public void DisplayEnemyInfo (string name, float healthFillAmount) {
        if (!enemyHealthBar.gameObject.activeInHierarchy) {
            enemyHealthBar.gameObject.SetActive(true);
        } else if (!enemyName.gameObject.activeInHierarchy) {
            enemyName.gameObject.SetActive(true);
        }
        enemyHealthBar.transform.GetChild(0).GetComponent<Image>().fillAmount = healthFillAmount;
        enemyName.text = name;
    }
    public void StopDisplayEnemyInfo () {
        if (enemyHealthBar.gameObject.activeInHierarchy) {
            enemyHealthBar.gameObject.SetActive(false);
        } else if (enemyName.gameObject.activeInHierarchy) {
            enemyName.gameObject.SetActive(false);
        }
    }
}
