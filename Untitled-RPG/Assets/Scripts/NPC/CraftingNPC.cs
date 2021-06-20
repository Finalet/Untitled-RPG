using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CraftingNPC : NPC
{
    [Header("Crafting")]
    public Item[] craftingItems;
    public Item selectedItem;
    public int craftQuanitity = 1;

    [Space]
    public GameObject craftingWindowPrefab;
    [Header("Sounds")]
    public AudioClip openCraftSound;
    public AudioClip closeCraftSound;
    public AudioClip startCraftSound;

    float craftingTime = 2;
    bool craftCanceled;

    CraftingWindowUI instanciatedCraftingWindow;

    protected override void CustomInterract()
    {
        OpenCraftingWindow();
    }
    protected override void CustomStopInterract()
    {
        CloseCraftingWindow();
    }

    void OpenCraftingWindow() {
        if (instanciatedCraftingWindow != null)
            return;

        craftQuanitity = 1;

        instanciatedCraftingWindow = Instantiate(craftingWindowPrefab, PeaceCanvas.instance.transform).GetComponent<CraftingWindowUI>();
        instanciatedCraftingWindow.ownerNPC = this;
        instanciatedCraftingWindow.Init();

        selectedItem = craftingItems[0];
        instanciatedCraftingWindow.DisplaySelectedItem();

        audioSource.clip = openCraftSound;
        audioSource.Play();
    }

    void CloseCraftingWindow (){
        craftQuanitity = 1;
        Destroy(instanciatedCraftingWindow.gameObject);
        audioSource.clip = closeCraftSound;
        audioSource.Play();
    }

    public void Select(Item item) {
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
        selectedItem = item;
        instanciatedCraftingWindow.DisplaySelectedItem();
    }

    public bool canCraftItem () {
        for (int i = 0; i < selectedItem.craftingRecipe.Length; i++) {
            if (InventoryManager.instance.getItemAmountInInventory(selectedItem.craftingRecipe[i].resource) < selectedItem.craftingRecipe[i].requiredAmount*craftQuanitity)
                return false;       
        }
        return true;
    }

    public void CraftItem () {
        if (canCraftItem()) {
            if (InventoryManager.instance.getNumberOfEmptySlots() == 0) {
                CanvasScript.instance.DisplayWarning("Inventory is full");
                return;
            }
            StartCoroutine(Crafting());
        }
    }

    IEnumerator Crafting () {
        PlayerControlls.instance.PlayGeneralAnimation(2, true);
        CanvasScript.instance.DisplayProgressBar(true);
        audioSource.clip = startCraftSound;
        audioSource.Play();
        float startedTime = Time.time;
        while (Time.time - startedTime < craftingTime){
            if (craftCanceled){
                craftCanceled = false;
                PlayerControlls.instance.ExitGeneralAnimation();
                instanciatedCraftingWindow.cancelButton.interactable = false;
                CanvasScript.instance.DisplayProgressBar(false, 1, true);
                audioSource.Stop();
                yield break;
            }
            CanvasScript.instance.DisplayProgressBar(false, (Time.time - startedTime) / craftingTime, in craftCanceled);
            instanciatedCraftingWindow.cancelButton.interactable = true;
            yield return null;
        }
        instanciatedCraftingWindow.cancelButton.interactable = false;
        PlayerControlls.instance.ExitGeneralAnimation();
        CanvasScript.instance.DisplayProgressBar(false, 1, true);
        audioSource.Stop();
        CompleteCraft();
    }
    
    public void CancelCraft () {
        craftCanceled = true;
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
    }

    void CompleteCraft () {
        for (int i = 0; i < selectedItem.craftingRecipe.Length; i++) {
                InventoryManager.instance.RemoveItemFromInventory(selectedItem.craftingRecipe[i].resource, selectedItem.craftingRecipe[i].requiredAmount*craftQuanitity);      
            }
        InventoryManager.instance.AddItemToInventory(selectedItem, craftQuanitity);
        instanciatedCraftingWindow.DisplaySelectedItem(); 

        CraftCompletedNotification();
    }

    void CraftCompletedNotification () {
        RectTransform craftCompletedNotification = new GameObject().AddComponent<RectTransform>();
        Image img = craftCompletedNotification.gameObject.AddComponent<Image>();

        craftCompletedNotification.transform.SetParent(PeaceCanvas.instance.transform);
        craftCompletedNotification.transform.localScale = Vector3.one;
        craftCompletedNotification.localPosition = Vector3.zero; //otherwise it spawns at Z -83, even when setting anchored position vector 3 below... trust me this is dumb
        craftCompletedNotification.anchoredPosition = new Vector2(0, 400);
        craftCompletedNotification.sizeDelta = new Vector2(100, 100);
        
        img.sprite = selectedItem.itemIcon;
        img.color = new Color(1,1,1,0);
        img.DOFade(1, 0.4f).SetEase(Ease.OutQuad);
        img.DOFade(0, 0.4f).SetEase(Ease.OutQuad).SetDelay(2);
        
        Destroy(img.gameObject, 3);
    }
}
