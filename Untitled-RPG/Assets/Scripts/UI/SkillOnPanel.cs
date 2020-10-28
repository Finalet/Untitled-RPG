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
        if(eventData.button == PointerEventData.InputButton.Right)
            return;

        PeaceCanvas.instance.StartDraggingSkill(GetComponent<RectTransform>().sizeDelta, AssetHolder.instance.getSkill(skillID), null);
    }

    public void OnDrag (PointerEventData pointerData) {
        if(pointerData.button == PointerEventData.InputButton.Right)
            return;

        PeaceCanvas.instance.DragItem(pointerData.position);
    }

    public void OnEndDrag(PointerEventData eventData) {
        if(eventData.button == PointerEventData.InputButton.Right)
            return;
            
        PeaceCanvas.instance.EndDrag();
    }

}
