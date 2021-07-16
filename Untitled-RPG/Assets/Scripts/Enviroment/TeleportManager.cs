using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager instance;

    public GameObject reviveWindow;
    GameObject instanciatedReviveWindow;

    public GameObject teleportMenu;
    GameObject instanciatedTeleportMenu;

    public GameObject portal;
    GameObject instanciatedPortal;
    public Item teleportationResource;

    public List<ReviveStatue> reviveStatues = new List<ReviveStatue>();

    [System.NonSerialized] public float lastTeleportedTime;
    [System.NonSerialized] public float teleportDelay = 2;

    void Awake() {
        instance = this;
    }
    void Start() {
        reviveStatues.Sort((x, y) => x.teleportationLocationName.CompareTo(y.teleportationLocationName));
    }

    public void ShowReviveWindow() {
        instanciatedReviveWindow = Instantiate(reviveWindow, PeaceCanvas.instance.transform);
        instanciatedReviveWindow.transform.SetAsLastSibling();

        Volume v = instanciatedReviveWindow.GetComponentInChildren<Volume>();
        DOTween.To(()=> v.weight, x=> v.weight = x, 1, 1);

        Image img = instanciatedReviveWindow.GetComponent<Image>();
        img.fillAmount = 0;
        img.DOFillAmount(1, 1).SetDelay(1);

        CanvasGroup cg = instanciatedReviveWindow.GetComponent<CanvasGroup>();
        cg.alpha = 0;
        cg.DOFade(1, 1).SetDelay(1);
        
        instanciatedReviveWindow.GetComponentInChildren<Button>().onClick.AddListener(delegate{});
    }

    void Revive () {
        Destroy(instanciatedReviveWindow);
        PlayerControlls.instance.transform.position = adjustedStatuePos(getClosestStatue(transform.position));
        Characteristics.instance.Revive();
    }

    Vector3 adjustedStatuePos (ReviveStatue statue) {
        return statue.transform.position + statue.transform.forward * 3 + Vector3.up * 0.5f;
    }

    public void TeleportMenu () {
        if (instanciatedTeleportMenu)
            CloseTeleportMenu();
        else
            OpenTeleportMenu();
    }

    void OpenTeleportMenu () {
        if (instanciatedTeleportMenu)
            return;
        
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.ReadBook);

        PeaceCanvas.instance.forceAnyPanelOpen = true;
        PeaceCanvas.instance.CloseInventory();

        instanciatedTeleportMenu = Instantiate(teleportMenu, PeaceCanvas.instance.transform);
        instanciatedTeleportMenu.transform.SetAsLastSibling();

        Transform grid = instanciatedTeleportMenu.transform.GetChild(instanciatedTeleportMenu.transform.childCount-1);
        GameObject template = grid.GetChild(0).gameObject;
        foreach (ReviveStatue statue in reviveStatues) {
            Button b = Instantiate(template, grid).GetComponent<Button>();
            b.onClick.AddListener(delegate{OpenPortal(adjustedStatuePos(statue));});
            b.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = statue.teleportationLocationName;
        }
        Button closeButton = instanciatedTeleportMenu.transform.GetChild(instanciatedTeleportMenu.transform.childCount-2).GetComponent<Button>();
        closeButton.onClick.AddListener(delegate{CloseTeleportMenu();});
        
        TextMeshProUGUI resourceAmountLabel = instanciatedTeleportMenu.transform.GetChild(instanciatedTeleportMenu.transform.childCount-3).GetComponent<TextMeshProUGUI>();
        resourceAmountLabel.text = InventoryManager.instance.getItemAmountInInventory(teleportationResource).ToString();
        resourceAmountLabel.rectTransform.sizeDelta = new Vector2(resourceAmountLabel.preferredWidth + 5, resourceAmountLabel.preferredHeight);

        Destroy(template.gameObject);
    }

    void OpenPortal (Vector3 teleportPosition) {
        if (InventoryManager.instance.getItemAmountInInventory(teleportationResource) < 2) {
            CanvasScript.instance.DisplayWarning($"{teleportationResource.itemName} is required");
            return;
        }
        StartCoroutine(openPortal(teleportPosition));        
    }
    IEnumerator openPortal (Vector3 teleportPosition) {
        PlayerControlls.instance.PlayGeneralAnimation(2, true, 2);
        CloseTeleportMenu();
        CanvasScript.instance.DisplayProgressBar(2);
        yield return new WaitForSeconds(2);
        if (instanciatedPortal){
            Destroy(instanciatedPortal.GetComponent<Portal>().returnPortal.gameObject);
            Destroy(instanciatedPortal);
        }
        
        InventoryManager.instance.RemoveItemFromInventory(teleportationResource, 1);

        instanciatedPortal = Instantiate(portal, PlayerControlls.instance.transform.position + PlayerControlls.instance.transform.forward * 2, PlayerControlls.instance.transform.rotation);
        instanciatedPortal.GetComponent<Portal>().teleportPosition = teleportPosition;
    }

    void CloseTeleportMenu(){
        if (!instanciatedTeleportMenu)
            return;
        Destroy(instanciatedTeleportMenu);
        PeaceCanvas.instance.forceAnyPanelOpen = false;
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.CloseBook);
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
}
