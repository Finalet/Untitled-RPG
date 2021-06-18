using UnityEngine.UI;
using UnityEngine;

public class UI_QuickAccessSlot : UI_InventorySlot
{
    [Header("Quick access slot")]
    public UI_QuickAccessSlot pairedSlot;
    public bool isParentSlot;
    [Space]
    public bool isSelected;
    public Image frame;
    
    public Sprite notSelectedFrame;
    public Sprite selectedFrame;

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

    void OnDisable() {
        if (isParentSlot)
            return;

        if (isSelected)
            UseItem();
        
        isSelected = false;
    }

    public override void AddItem(Item item, int amount, UI_InventorySlot initialSlot)
    {
        base.AddItem(item, amount, initialSlot);
        SyncItem();
    }
    public override void ClearSlot()
    {
        base.ClearSlot();
        SyncItem();
    }
    public override void UseItem()
    {
        base.UseItem();
        SyncItem();
    }

    void SyncItem () {
        pairedSlot.itemInSlot = itemInSlot;
        pairedSlot.itemAmount = itemAmount;
    }

    protected override void Awake() {
        if (!isParentSlot)
            return;

        savefilePath = "saves/quickAccessSlots.txt";
        if (slotID == -1) slotID = System.Convert.ToInt16(name.Substring(name.IndexOf('(') + 1, 2)); //only works for slots after 10, first 10 needs to be assigned manually
    }

    protected override void Start()
    {
        if (isParentSlot) base.Start();
    }
}
