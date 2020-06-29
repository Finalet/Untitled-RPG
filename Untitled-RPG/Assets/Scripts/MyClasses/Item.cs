using UnityEngine;

public abstract class Item : ScriptableObject
{
    public int ID;
    public ItemType itemType;
    public string itemName;
    public string itemDesctription;
    public Sprite itemIcon;
    public GameObject itemPrefab;

    public abstract void Use ();
}
