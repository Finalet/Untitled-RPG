using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class EnemyWaveUI : MonoBehaviour
{
    EnemyWaveGenerator waveGenerator;

    public TextMeshProUGUI titleLabel;
    public Image fillBar;

    CanvasGroup canvasGroup;

    bool ending;

    bool _showUI;
    bool showUI {
        set {
            _showUI = value;
            canvasGroup.DOFade(showUI ? 1 : 0, 0.5f);
        }
        get {
            return _showUI;
        }
    }

    public void Init(EnemyWaveGenerator _waveGenerator) {
        canvasGroup = GetComponent<CanvasGroup>();

        waveGenerator = _waveGenerator;
        fillBar.fillAmount = 0;
        UpdateUI();
    }

    public void End () {
        ending = true;
        showUI = false;
        Destroy(gameObject, 1);
    }

    void UpdateUI () { 
        if (ending) return;
        
        if (!waveGenerator) {
            showUI = false;
            return;
        }
        
        if (waveGenerator.isCurrentWaveBoss) {
            showUI = false;
            return;
        }
        
        showUI = true;
        titleLabel.text = $"Wave {waveGenerator.currentWaveIndex + 1} out of {waveGenerator.waves.Count}";
        fillBar.fillAmount = Mathf.Lerp(fillBar.fillAmount, waveGenerator.currentWaveProgress, Time.deltaTime);
    }

    void Update() {
        UpdateUI();
    }
}
