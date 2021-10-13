using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipAttachementUI : MonoBehaviour
{
    public UI_ShipAttachementSlot cannonsSlot;
    public UI_ShipAttachementSlot sailsSlot;
    public UI_ShipAttachementSlot helmSlot;
    public UI_ShipAttachementSlot flagSlot;

    public void Open() {
        PeaceCanvas.instance.openPanels ++;
        PeaceCanvas.instance.OpenInventory(true, true);
        transform.SetAsLastSibling();
        gameObject.SetActive(true);
    }

    public void Close() {
        PeaceCanvas.instance.openPanels --;
        PeaceCanvas.instance.CloseInventory();
        gameObject.SetActive(false);
    }
}
