using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class SkillOnPanel : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{

    public int skillID;
    public TextMeshProUGUI name1;

    GameObject go;

    void Start() {
        GetComponent<Image>().sprite = AssetHolder.instance.getSkill(skillID).icon;
        name1.text = AssetHolder.instance.getSkill(skillID).skillName;
    }

    public void OnBeginDrag(PointerEventData eventData) {
       PeaceCanvas.instance.StartDraggingSkill(GetComponent<RectTransform>().sizeDelta, AssetHolder.instance.getSkill(skillID), null);
    }

    public void OnDrag (PointerEventData pointerData) {
        PeaceCanvas.instance.DragItem(pointerData.delta.x, pointerData.delta.y);
    }

    public void OnEndDrag(PointerEventData eventData) {
        PeaceCanvas.instance.EndDrag();
    }

}
