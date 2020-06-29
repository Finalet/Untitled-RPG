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
        GetComponent<Image>().sprite = AssetHolder.instance.Skills[skillID].GetComponent<Skill>().icon;
        name1.text = AssetHolder.instance.Skills[skillID].GetComponent<Skill>().skillName;
    }

    public void OnBeginDrag(PointerEventData eventData) {
       PeaceCanvas.instance.StartDraggingSkill(GetComponent<RectTransform>().sizeDelta, AssetHolder.instance.Skills[skillID].GetComponent<Skill>());
    }

    public void OnDrag (PointerEventData pointerData) {
        PeaceCanvas.instance.DragItem(pointerData.delta.x, pointerData.delta.y);
    }

    public void OnEndDrag(PointerEventData eventData) {
        PeaceCanvas.instance.EndDrag();
    }

}
