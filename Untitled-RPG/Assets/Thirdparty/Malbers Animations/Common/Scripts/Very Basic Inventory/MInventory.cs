using UnityEngine;
using MalbersAnimations.Events;
using System.Collections.Generic;

namespace MalbersAnimations
{
    public class MInventory : MonoBehaviour,IInventory
    {
        public List<GameObject> Inventory;
        public GameObjectEvent OnEquipItem; 

        public virtual void EquipItem(int Slot)
        {
            if (Slot < Inventory.Count)
            {
                OnEquipItem.Invoke(Inventory[Slot]);
            }
        }
    }
}
