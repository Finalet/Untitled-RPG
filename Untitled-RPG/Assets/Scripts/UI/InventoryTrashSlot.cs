using UnityEngine.EventSystems;
using UnityEngine;

public class InventoryTrashSlot : MonoBehaviour, IDropHandler 
{
    public virtual void OnDrop (PointerEventData pointerData) {
        if (PeaceCanvas.instance.itemBeingDragged != null) { //Dropping Item
            PeaceCanvas.instance.dragSuccess = true;
            UIAudioManager.instance.PlayUISound(UIAudioManager.instance.DropItem);
        }
    }
}
