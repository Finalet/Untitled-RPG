using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillOnPanel : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{

    public int skillID;

    GameObject dragDisplayObject;
    GameObject go;

    void Start() {
        dragDisplayObject = AssetHolder.instance.dragDisplayObject;
        GetComponent<Image>().sprite = AssetHolder.instance.Skills[skillID].GetComponent<Skill>().icon;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        go = Instantiate(dragDisplayObject, Input.mousePosition, Quaternion.identity, PeaceCanvas.instance.transform);
        go.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;
        go.GetComponent<Image>().sprite = GetComponent<Image>().sprite;
        go.GetComponent<DragDisplayObject>().itemType = ItemType.skill;
        go.GetComponent<DragDisplayObject>().itemID = skillID;
        PeaceCanvas.instance.itemBeingDragged = go;
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
