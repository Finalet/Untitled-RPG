using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyWaveUI : MonoBehaviour
{
    EnemyWaveGenerator waveGenerator;

    public TextMeshProUGUI titleLabel;
    public Image fillBar;

    CanvasGroup canvasGroup;

    bool _showUI;
    bool showUI {
        set {
            _showUI = value;
            canvasGroup.alpha = showUI ? 1 : 0;
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

    void UpdateUI () { 
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
