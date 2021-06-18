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
    public Text warningText;

    [Header("Enemy")]
    public Image enemyHealthBar;
    public TextMeshProUGUI enemyName;
    public TextMeshProUGUI enemyHealth;

    [Header("Middle Skill Panel")]
    public UI_MiddleSkillPanelButtons LMB;
    public UI_MiddleSkillPanelButtons RMB;
    public Sprite cancelSprite;

    [Header("Quick access menu")]
    public QuickAccessMenu quickAccessMenu;
    [DisplayWithoutEdit] public bool quickAccessMenuIsOpen;


    Characteristics characteristics;

    void Awake() {
        if (instance == null) 
            instance = this;
    }

    void Start() {
        characteristics = Characteristics.instance;
    }

    void Update() {
        DisplayHPandStamina();

        if (Input.GetKeyDown(KeybindsManager.instance.quickAccessMenu))
            OpenQuickAccessMenu();
        else if (Input.GetKeyUp(KeybindsManager.instance.quickAccessMenu))
            CloseQuickAccessMenu();
    }

    float staminaColorLerp;
    float staminaFillAmount;
    void DisplayHPandStamina () {
        healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, (float)characteristics.health/characteristics.maxHealth, Time.deltaTime * 10);
        healthBar.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = characteristics.health.ToString();
        staminaFillAmount = characteristics.maxStamina == 0 ? 0 : Mathf.Lerp(staminaFillAmount, (float)characteristics.stamina/characteristics.maxStamina, Time.deltaTime * 10);

        StaminaBarPosition();

        if (!Characteristics.instance.canUseStamina) {
            if (staminaColorLerp < 1)
                staminaColorLerp += Time.deltaTime * 10;
        } else {
            if (staminaColorLerp > 0)
                staminaColorLerp -= Time.deltaTime * 10;
        }
        staminaColorLerp = Mathf.Clamp01(staminaColorLerp);
        staminaFillAmount = Mathf.Clamp01(staminaFillAmount);
        
        staminaBar.material.SetFloat("_ColorLerp", staminaColorLerp);
        staminaBar.material.SetFloat("_FillAmount", staminaFillAmount);
    }

    void StaminaBarPosition () {
        Vector3 currentPos = staminaBar.transform.GetComponent<RectTransform>().anchoredPosition;
        Vector2 desPos = Camera.main.WorldToScreenPoint(PlayerControlls.instance.transform.position + PlayerControlls.instance.playerCamera.transform.right * 0.5f + Vector3.up * 1.2f);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), desPos, PeaceCanvas.instance.UICamera, out desPos);
        staminaBar.transform.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(currentPos, desPos, 10 * Time.deltaTime);
    }

    public void HideStamina () {
        staminaBar.transform.gameObject.SetActive(false);
    }
    public void ShowStamina () {
        staminaBar.transform.gameObject.SetActive(true);
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

    public void DisplayCastingBar (float castingNormalizedAnimationTime) {
        StartCoroutine(DisplayCastinBarIenum(castingNormalizedAnimationTime));
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

    public void DisplayProgressBar (bool prewarm, float progress = 0, in bool done = false) {
        if (prewarm){
            castingBar.SetActive(true);
            castingBar.transform.GetChild(0).GetComponent<Image>().fillAmount = 0;
        }
        castingBar.transform.GetChild(0).GetComponent<Image>().fillAmount = progress;
        if (done)
            castingBar.SetActive(false);
    }

    public void PickAreaForSkill (Skill skill) {
        LMB.areaPickerSkill = skill;
        RMB.areaPickerSkill = skill;
    }

    void OpenQuickAccessMenu(){
        if (PeaceCanvas.instance.anyPanelOpen)
            return;

        quickAccessMenu.gameObject.SetActive(true);
        PlayerControlls.instance.cameraControl.stopInput = true;
        quickAccessMenuIsOpen = true;
    }
    public void CloseQuickAccessMenu () {
        quickAccessMenu.gameObject.SetActive(false);
        PlayerControlls.instance.cameraControl.stopInput = false;
        quickAccessMenuIsOpen = false;
    }
}
