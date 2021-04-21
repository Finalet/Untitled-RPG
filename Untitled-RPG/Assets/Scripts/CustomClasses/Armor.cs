using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleDrakeStudios.ModularCharacters;

[CreateAssetMenu(fileName = "New Armor", menuName = "Item/Armor")]
public class Armor : Equipment
{
    [Space]
    public ArmorType armorType;
    public ModularArmor modularArmor;

    protected override void Equip (UI_InventorySlot initialSlot) {
        switch (armorType) {
            case ArmorType.Helmet: {
                EquipmentManager.instance.helmet.AddItem(this, 1, initialSlot);
                break;
            }
            case ArmorType.Chest: {
                EquipmentManager.instance.chest.AddItem(this, 1, initialSlot);
                break;
            }
            case ArmorType.Gloves: {
                EquipmentManager.instance.gloves.AddItem(this, 1, initialSlot);
                break;
            }
            case ArmorType.Pants: {
                EquipmentManager.instance.pants.AddItem(this, 1, initialSlot);
                break;
            }
            case ArmorType.Boots: {
                EquipmentManager.instance.boots.AddItem(this, 1, initialSlot);
                break;
            }
            case ArmorType.Back: {
                EquipmentManager.instance.back.AddItem(this, 1, initialSlot);
                break;
            }
        }
    }
}
