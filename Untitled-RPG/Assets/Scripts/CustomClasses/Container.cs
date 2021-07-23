using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Container : MonoBehaviour
{
    public List<ItemAmountPair> defaultItemsInside = new List<ItemAmountPair>();
    public bool isLocked;
    public bool isOpen;
    [Header("Conainter UI")]
    public int numberOfSlots = 8;
    public int nColumns = 4;
    public GameObject UIPrefab;
    public GameObject slotPrefab;
    
    GridLayoutGroup slotsGrid;
    RectTransform uiRect;
    Button closeButton;
    bool initiazlied;
    bool opening;

    public void TryOpenContainer (float openAnimDuration = 0) {
        if (isOpen || opening)
            return;
        if (isLocked) {
            CanvasScript.instance.DisplayWarning("Locked.");
            return;
        }
        StartCoroutine(OpenContainer(openAnimDuration));
    }

    IEnumerator OpenContainer (float openAnimDuration = 0) {
        opening = true;
        OpenAnimation();
        yield return new WaitForSeconds(openAnimDuration);
        
        InitContainer();

        uiRect.SetParent(PeaceCanvas.instance.transform);
        uiRect.SetAsLastSibling();
        uiRect.gameObject.SetActive(true);
        
        isOpen = true;
        PeaceCanvas.instance.currentInterractingContainer = this;
        PeaceCanvas.instance.openPanels++;
        opening = false;
    }
    
    void InitContainer () {
        if (initiazlied)
            return;

        uiRect = Instantiate(UIPrefab, PeaceCanvas.instance.transform).GetComponent<RectTransform>();
        slotsGrid = uiRect.GetComponent<GridLayoutGroup>();
        closeButton = uiRect.GetComponentInChildren<Button>();

        numberOfSlots = Mathf.Max(numberOfSlots, defaultItemsInside.Count);
        closeButton.onClick.AddListener(delegate{CloseContainer();});
        uiRect.sizeDelta = new Vector2(gridWidth(), gridHeight());

        for (int i = 0; i < numberOfSlots; i++) {
            UI_ContainerSlot slot = Instantiate(slotPrefab, slotsGrid.transform).GetComponent<UI_ContainerSlot>();
            if (i < defaultItemsInside.Count)
                slot.AddItem(defaultItemsInside[i].item1, defaultItemsInside[i].amount1, null);
        }

        initiazlied = true;
    }

    public void CloseContainer () {
        CloseAnimation();
        PeaceCanvas.instance.currentInterractingContainer = null;
        PeaceCanvas.instance.openPanels--;
        isOpen = false;
        uiRect.SetParent(transform);
        uiRect.gameObject.SetActive(false);
    }

    protected virtual void OpenAnimation() {}
    protected virtual void CloseAnimation() {}

    int gridWidth() {
        return Mathf.RoundToInt(slotsGrid.padding.left + slotsGrid.padding.right + nColumns * slotsGrid.cellSize.x + (nColumns-1) * slotsGrid.spacing.x);
    }
    int gridHeight() {
        int nRows = Mathf.CeilToInt((float)numberOfSlots / (float)nColumns);
        return Mathf.RoundToInt(slotsGrid.padding.top + slotsGrid.padding.bottom + nRows * slotsGrid.cellSize.y + (nRows - 1) * slotsGrid.spacing.y + 20 + closeButton.GetComponent<RectTransform>().sizeDelta.y);
    }

#region Public methods

    /// <summary>
    /// Add default items to the container. If container is already initialized (opened) then you need to Reset() it to see the updated default items.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="amount"></param>
    public void AddItem(Item item, int amount) {
        ItemAmountPair p;
        p.item1 = item;
        p.amount1 = amount;

        defaultItemsInside.Add(p);
    }
    /// <summary>
    /// Reset this container. Rewrites all items inside to default ones.
    /// </summary>
    public void ResetContainer () {
        initiazlied = false;
    }

#endregion
}
