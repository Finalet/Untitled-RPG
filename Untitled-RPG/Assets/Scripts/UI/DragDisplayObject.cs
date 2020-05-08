using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType {empty, skill};

public class DragDisplayObject : MonoBehaviour
{
    public ItemType itemType;
    public int itemID;
}
