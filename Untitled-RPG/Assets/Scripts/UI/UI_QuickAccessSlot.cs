using UnityEngine.UI;
using UnityEngine;

public class UI_QuickAccessSlot : UI_InventorySlot
{
    [Header("Quick access slot")]
    public UI_QuickAccessSlot pairedSlot;
    public bool isParentSlot;
    [Space]
    public bool isCancel;
    [Space]
    public bool isSelected;
    public Image frame;
    
    public Sprite notSelectedFrame;
    public Sprite selectedFrame;

    protected override string savefilePath() {
        return "saves/quickAccessSlots.txt";
    }

    protected override void Update() {
        base.Update();

        if (!isParentSlot) {
            frame.sprite = isSelected ? selectedFrame : notSelectedFrame;
            frame.rectTransform.offsetMin = isSelected ? Vector2.one * -12 : Vector2.zero;
            frame.rectTransform.offsetMax = isSelected ? Vector2.one * 12 : Vector2.zero;
        }
    }

    void OnEnable() {
        if (!isParentSlot) isSelected = false;
    }

    public override void AddItem(Item item, int amount, UI_InventorySlot initialSlot)
    {
        if (isCancel)
            return;

        base.AddItem(item, amount, initialSlot);
        SyncItem();
    }
    public override void ClearSlot()
    {
        if (isCancel)
            return;

        base.ClearSlot();
        SyncItem();
    }
    public override void UseItem()
    {
        if (isCancel)
            return;

        base.UseItem();
        SyncItem();
    }

    void SyncItem () {
        pairedSlot.itemInSlot = itemInSlot;
        pairedSlot.itemAmount = itemAmount;
    }

    void Awake() {
        if (!isParentSlot)
            return;

        SetSlotID();
    }

    protected override void Start()
    {
        if (isParentSlot) base.Start();
    }
}
