using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ReviveStatue : NPC
{
    [Header("Custom vars")]
    public GameObject reviveStatuePrefab;

    ReviveStatueWindowUI instanciatedWindow;

    protected override void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    protected override void RotateNameLable()
    {
        // do nothing
    }

    protected override void CustomInterract () {
        Invoke("OpenReviveWindow", 1.5f);
    }
    void OpenReviveWindow() {
        if (instanciatedWindow != null)
            return;

        instanciatedWindow = Instantiate(reviveStatuePrefab, PeaceCanvas.instance.transform).GetComponent<ReviveStatueWindowUI>();
        instanciatedWindow.GetComponent<CanvasGroup>().alpha = 0;
        instanciatedWindow.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        instanciatedWindow.ownerNPC = this;
        instanciatedWindow.Init();
    }
    protected override void CustomStopInterract () {
        Destroy(instanciatedWindow.gameObject);
    }
}
