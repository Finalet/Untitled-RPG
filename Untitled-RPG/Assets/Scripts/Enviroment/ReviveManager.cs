using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;
using UnityEngine.UI;

public class ReviveManager : MonoBehaviour
{
    public static ReviveManager instance;

    public GameObject reviveWindow;
    GameObject instanciatedWindow;

    public List<ReviveStatue> reviveStatues = new List<ReviveStatue>();
    void Awake() {
        instance = this;
    }

    public ReviveStatue getClosestStatue(Vector3 position) {
        ReviveStatue closestStatue = null;
        float closestDistance = Mathf.Infinity;
        foreach (ReviveStatue statue in reviveStatues) {
            if (Vector3.Distance(position, statue.transform.position) < closestDistance){
                closestDistance = Vector3.Distance(position, statue.transform.position);
                closestStatue = statue;
            }
        }
        return closestStatue;
    }

    public void ShowReviveWindow() {
        instanciatedWindow = Instantiate(reviveWindow, PeaceCanvas.instance.transform);
        instanciatedWindow.transform.SetAsLastSibling();

        Volume v = instanciatedWindow.GetComponentInChildren<Volume>();
        DOTween.To(()=> v.weight, x=> v.weight = x, 1, 1);

        Image img = instanciatedWindow.GetComponent<Image>();
        img.fillAmount = 0;
        img.DOFillAmount(1, 1).SetDelay(1);

        CanvasGroup cg = instanciatedWindow.GetComponent<CanvasGroup>();
        cg.alpha = 0;
        cg.DOFade(1, 1).SetDelay(1);
        
        instanciatedWindow.GetComponentInChildren<Button>().onClick.AddListener(delegate{Revive();});
    }

    void Revive() {
        Destroy(instanciatedWindow);
        ReviveStatue closestStatue = getClosestStatue(transform.position);

        PlayerControlls.instance.transform.position = closestStatue.transform.position + closestStatue.transform.forward * 3 + closestStatue.transform.up * 0.5f;
        PlayerControlls.instance.transform.rotation = closestStatue.transform.rotation;

        Characteristics.instance.Revive();
    }
}
