using System.Collections;
using UnityEngine;

public abstract class Item : ScriptableObject
{
    public int ID;
    public string itemName;
    public string itemDesctription;
    public int itemBasePrice;
    public Sprite itemIcon;
    public GameObject itemPrefab;

    public abstract void Use ();
    public abstract void Use (UI_InventorySlot initialSlot);
    public abstract IEnumerator UseEnum ();
}
