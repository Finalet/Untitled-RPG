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
        name1.text = "";
        Skill skill = AssetHolder.instance.getSkill(skillID);
        GetComponent<Image>().sprite = skill.icon;
        string[] words = skill.name.Split(' ');
        if (words.Length == 0) {
            name1.text = skill.name;
            return;
        }
        for (int i = 0; i < words.Length; i++) {
            name1.text += words[i] + "\n";
        }
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
