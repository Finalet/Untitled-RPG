using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScript : MonoBehaviour
{
    public static CanvasScript instance;

    public Image healthBar;
    public Image staminaBar;

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
        healthBar.fillAmount = (float)characteristics.HP/characteristics.maxHP;
        healthBar.transform.GetChild(0).GetComponent<Text>().text = characteristics.HP.ToString();
        staminaBar.fillAmount = (float)characteristics.Stamina/characteristics.maxStamina;
        staminaBar.transform.GetChild(0).GetComponent<Text>().text = characteristics.Stamina.ToString();
    }
}
