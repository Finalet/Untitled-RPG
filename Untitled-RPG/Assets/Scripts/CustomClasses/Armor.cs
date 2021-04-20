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
}
