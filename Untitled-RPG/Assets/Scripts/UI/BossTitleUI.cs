using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BossTitleUI : MonoBehaviour
{
    NavAgentBoss relatedBoss;
    
    public Image healthLine;
    public TextMeshProUGUI healthNumberLabel;
    public TextMeshProUGUI bossNameLabel;

    Material healthLineMaterial;
    CanvasGroup canvasGroup;

    float prevFill;
    float desFill;
    public void Init(NavAgentBoss _relatedBoss) {
        canvasGroup = GetComponent<CanvasGroup>();
        healthLineMaterial = healthLine.material;
        relatedBoss = _relatedBoss;
        bossNameLabel.text = relatedBoss.enemyName;

        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, 2);
    }

    public void Hide() {
        canvasGroup.DOFade(0, 2);
        Destroy(gameObject, 2.2f);
    }

    void Update() {
        prevFill = healthLineMaterial.GetFloat("_FillAmount");
        desFill = (float)relatedBoss.currentHealth / (float)relatedBoss.maxHealth;
        healthLineMaterial.SetFloat("_FillAmount", Mathf.Lerp(prevFill, desFill, Time.deltaTime * 10));
        healthNumberLabel.text = relatedBoss.currentHealth.ToString();
    }
}
