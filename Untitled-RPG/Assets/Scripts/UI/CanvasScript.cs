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
    public GameObject buffs;
    public GameObject castingBar;

    [Header("Overall UI")]
    public TextMeshProUGUI warningText;

    [Header("Enemy")]
    public Image enemyHealthBar;
    public TextMeshProUGUI enemyName;
    public TextMeshProUGUI enemyHealth;

    [Header("Middle Skill Panel")]
    public UI_MiddleSkillPanelButtons LMB;
    public UI_MiddleSkillPanelButtons RMB;
    public Sprite cancelSprite;

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
        healthBar.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = characteristics.HP.ToString();
        staminaBar.fillAmount = Mathf.Lerp(staminaBar.fillAmount, (float)characteristics.Stamina/characteristics.maxStamina, 0.1f);
        staminaBar.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = characteristics.Stamina.ToString();
    }

    public void DisplayEnemyInfo (string name, float healthFillAmount, int health) {
        if (!enemyHealthBar.gameObject.activeInHierarchy) {
            enemyHealthBar.gameObject.SetActive(true);
        } else if (!enemyName.gameObject.activeInHierarchy) {
            enemyName.gameObject.SetActive(true);
        }
        enemyHealthBar.transform.GetChild(0).GetComponent<Image>().fillAmount = Mathf.Lerp(enemyHealthBar.transform.GetChild(0).GetComponent<Image>().fillAmount, healthFillAmount, 0.1f);
        enemyName.text = name;
        if (enemyName.text != "Training Dummy")
            enemyHealth.text = health.ToString();
        else 
            enemyHealth.text = "";
    }
    public void StopDisplayEnemyInfo () {
        if (enemyHealthBar.gameObject.activeInHierarchy) {
            enemyHealthBar.gameObject.SetActive(false);
        } else if (enemyName.gameObject.activeInHierarchy) {
            enemyName.gameObject.SetActive(false);
        }
    }
    float warningTextAlpha;
    IEnumerator warning;
    public void DisplayWarning(string text) {
        warning = WarningTextIenum(text);
        if (warning != null)
            StartCoroutine(warning);   
    }
    IEnumerator WarningTextIenum (string text){
        warningText.text = text;
        warningText.gameObject.SetActive(true);
        while (warningText.color.a <= 1) {
            warningTextAlpha += Time.deltaTime * 5;
            warningText.color = new Color(warningText.color.r, warningText.color.g, warningText.color.b, warningTextAlpha);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        yield return new WaitForSeconds(2);
        while (warningText.color.a >= 0) {
            warningTextAlpha -= Time.deltaTime * 5;
            warningText.color = new Color(warningText.color.r, warningText.color.g, warningText.color.b, warningTextAlpha);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        warningText.text = null;
        warningText.gameObject.SetActive(false);
        warning = null;
    }

    public void DisplayCastingBar (float castingTime) {
        StartCoroutine(DisplayCastinBarIenum(castingTime));
    }
    IEnumerator DisplayCastinBarIenum (float castEndNormalizedTime) {
        castingBar.SetActive(true);
        castingBar.transform.GetChild(0).GetComponent<Image>().fillAmount = 0;
        while (castingBar.transform.GetChild(0).GetComponent<Image>().fillAmount != 1) {
            if (PlayerControlls.instance.castInterrupted) {
                break;
            }
            AnimatorStateInfo cc = PlayerControlls.instance.animator.GetCurrentAnimatorStateInfo(PlayerControlls.instance.animator.GetLayerIndex("Attacks"));
            if (cc.IsName("Empty"))
                cc = PlayerControlls.instance.animator.GetNextAnimatorStateInfo(PlayerControlls.instance.animator.GetLayerIndex("Attacks")); 

            castingBar.transform.GetChild(0).GetComponent<Image>().fillAmount = cc.normalizedTime % 1 / castEndNormalizedTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.25f);
        castingBar.SetActive(false);
    }

    public void PickAreaForSkill (Skill skill) {
        LMB.areaPickerSkill = skill;
        RMB.areaPickerSkill = skill;
    }
}
