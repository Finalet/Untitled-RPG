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
       PeaceCanvas.instance.StartDraggingItem(GetComponent<RectTransform>().sizeDelta, GetComponent<Image>().sprite, AssetHolder.instance.Skills[skillID].GetComponent<Skill>());
    }

    public void OnDrag (PointerEventData eventData) {
        PeaceCanvas.instance.DragItem(GetComponent<RectTransform>().sizeDelta);
    }

    public void OnEndDrag(PointerEventData eventData) {
        PeaceCanvas.instance.EndDrag();
    }

}
