using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{

    public ItemType itemType;
    public int itemID;

    public bool isSkillPanel;

    GameObject dragDisplayObject;
    GameObject go;

    void Start() {
        dragDisplayObject = AssetHolder.instance.dragDisplayObject;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (itemType == ItemType.empty)
            return;

        go = Instantiate(dragDisplayObject, Input.mousePosition, Quaternion.identity, PeaceCanvas.instance.transform);
        go.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;
        go.GetComponent<Image>().sprite = GetComponent<Image>().sprite;
        go.GetComponent<DragDisplayObject>().itemType = itemType;
        go.GetComponent<DragDisplayObject>().itemID = itemID;
        PeaceCanvas.instance.itemBeingDragged = go;
        if (isSkillPanel) {
            GetComponent<SlotHolderHandler>().RemoveItemFromSlot();
        }
    }

    public void OnDrag (PointerEventData eventData) {
        if (go == null)
            return;

        go.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (go == null)
            return;

        PeaceCanvas.instance.itemBeingDragged = null;
        Destroy(go);
    }

}
