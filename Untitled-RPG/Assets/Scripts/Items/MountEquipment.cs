using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum MountEquipmentType {Saddle, Armor}

[CreateAssetMenu(fileName = "New Mount Equipment", menuName = "Item/Mounts/Mount Equipment")]
public class MountEquipment : Item
{
    [Space]
    public MountEquipmentType equipmentType;
    [Header("Stats")]
    public int movementSpeed;
    public int stamina;

    [Header("Malbert Colors Saddle")]
    public Color Color1Reins;
    public Color Color2Seat;
    public Color Color3UnderSeat;
    public Color Color4Clasps;
    [Space]
    public Color Color5SeatClothBottom;
    public Color Color6SeatClothTop;
    public Color Color7SeatSides;
    public Color Color8FeetHolder;
    [Space]
    public Color Color9Pouches;
    public Color Color10LegStraps;
    public Color Color11HorseStraps;
    [Header("Malbert Colors Armor")]
    public Color Color12ArmorUnder;
    public Color Color13ArmorOutline;
    public Color Color14ArmorColor;
    public Color Color15BodyCloth;
    public Color Color16MetalButtons;

    public override void Use() {}
    public override void Use(UI_InventorySlot initialSlot) {
        if (!(initialSlot is UI_MountEquipmentSlot))
            Equip(initialSlot);
        else
            Unequip(initialSlot);
    }

    protected virtual void Equip (UI_InventorySlot initialSlot) {
        switch (equipmentType) {
            case MountEquipmentType.Saddle: EquipmentManager.instance.mount.saddleSlot.AddItem(this, 1, initialSlot);
                break;
            case MountEquipmentType.Armor: EquipmentManager.instance.mount.armorSlot.AddItem(this, 1, initialSlot);
                break;
        }
    }
    protected virtual void Unequip (UI_InventorySlot initialSlot) {
        InventoryManager.instance.AddItemToInventory(this, 1);
        initialSlot.ClearSlot();
    }

    public override int getItemValue()
    {
        if (overridePrice)
            return itemBasePrice;

        float movementSpeedValue = 2500;
        float staminaValue = 1000;
        float rarityValue = 5000;

        float value = 0;
        value += movementSpeed * movementSpeedValue;
        value += stamina * staminaValue;
        value += (int)itemRarity * rarityValue;
        
        return Mathf.RoundToInt(value);
    }

    protected override void OnValidate() {
        base.OnValidate();
        itemBasePrice = getItemValue();
    }

    [Header("Editor colors setup")]
    public Material sampleMaterial;
    public virtual void TransferColorsFromMaterial () {
        Color1Reins = sampleMaterial.GetColor("_Color1");
        Color2Seat = sampleMaterial.GetColor("_Color2");
        Color3UnderSeat = sampleMaterial.GetColor("_Color3");
        Color4Clasps = sampleMaterial.GetColor("_Color4");
        Color5SeatClothBottom = sampleMaterial.GetColor("_Color5");
        Color6SeatClothTop = sampleMaterial.GetColor("_Color6");
        Color7SeatSides = sampleMaterial.GetColor("_Color7");
        Color8FeetHolder = sampleMaterial.GetColor("_Color8");
        Color9Pouches = sampleMaterial.GetColor("_Color9");
        Color10LegStraps = sampleMaterial.GetColor("_Color10");
        Color11HorseStraps = sampleMaterial.GetColor("_Color11");
        Color12ArmorUnder = sampleMaterial.GetColor("_Color12");
        Color13ArmorOutline = sampleMaterial.GetColor("_Color13");
        Color14ArmorColor = sampleMaterial.GetColor("_Color14");
        Color15BodyCloth = sampleMaterial.GetColor("_Color15");
        Color16MetalButtons = sampleMaterial.GetColor("_Color16");

        sampleMaterial = null;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(MountEquipment))]
class MountEquipmentInspector : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        MountEquipment m = (MountEquipment)target;

        if (m.sampleMaterial) {
            if (GUILayout.Button("Transfer color from material")) {
                m.TransferColorsFromMaterial();
            }
        }
    }
}

#endif