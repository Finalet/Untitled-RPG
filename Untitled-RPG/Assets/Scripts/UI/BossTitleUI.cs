using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BossTitleUI : MonoBehaviour
{
    Boss relatedBoss;
    
    public Image healthLine;
    public TextMeshProUGUI healthNumberLabel;
    public TextMeshProUGUI bossNameLabel;

    CanvasGroup canvasGroup;

    public void Init(Boss _relatedBoss) {
        canvasGroup = GetComponent<CanvasGroup>();
        
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
        healthLine.fillAmount = (float)relatedBoss.currentHealth / (float)relatedBoss.maxHealth;
        healthNumberLabel.text = relatedBoss.currentHealth.ToString();
    }
}
