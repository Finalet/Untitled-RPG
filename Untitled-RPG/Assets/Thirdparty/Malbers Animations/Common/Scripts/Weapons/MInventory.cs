using UnityEngine;
using MalbersAnimations.Events;
using System.Collections.Generic;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Weapons/MInventory Basic")]

    public class MInventory : MonoBehaviour 
    {
        public List<GameObject> Inventory;
        public GameObjectEvent OnEquipItem;

        public virtual void EquipItem(int Slot)
        {
            if (Slot < Inventory.Count)  OnEquipItem.Invoke(Inventory[Slot]);
        }

        public virtual void AddItem(GameObject item) => Inventory.Add(item);

    }
}
