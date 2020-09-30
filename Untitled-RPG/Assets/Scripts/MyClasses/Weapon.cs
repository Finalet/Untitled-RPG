using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Item/Weapon")]
public class Weapon : Equipment
{
    public WeaponType weaponType;
    [Header("Stats")]
    public int MeleeAttack;
    public int RangedAttack;
    public int MagicPower;
    public int HealingPower;
    public int defense;
}
