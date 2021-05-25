using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class SkillOnPanel : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Space]
    public Skill skill;
    public bool isPicked;

    [Space]
    public Image frame;
    public Sprite regularFrame;
    public Sprite pickedFrame;

    void Start() {
        UpdateDisplay();
    }

    void Update() {
        if (skill != null) isPicked = Combat.instanace.currentPickedSkills.Contains(skill);
        UpdateDisplay();
    }

    public void UpdateDisplay () {
        if (skill == null) 
            return;
        
        frame.sprite = isPicked ? pickedFrame : regularFrame;
        GetComponent<Image>().sprite = skill.icon;
        if (Combat.instanace != null) GetComponent<Image>().color = Combat.instanace.isPickedSkillTree(skill.skillTree) ? Color.white : Color.gray;
    }

    void OnValidate() {
        UpdateDisplay();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (!isPicked)
            return;

        if(eventData.button == PointerEventData.InputButton.Right)
            return;

        PeaceCanvas.instance.StartDraggingSkill(GetComponent<RectTransform>().sizeDelta, skill, null);
    }

    public void OnDrag (PointerEventData pointerData) {
        if (!isPicked)
            return;
        
        if(pointerData.button == PointerEventData.InputButton.Right)
            return;

        PeaceCanvas.instance.DragItem(pointerData.position);
    }

    public void OnEndDrag(PointerEventData eventData) {
        if(eventData.button == PointerEventData.InputButton.Right)
            return;
            
        PeaceCanvas.instance.EndDrag();
    }

    public virtual void OnPointerClick (PointerEventData pointerData) {
        if (pointerData.button == PointerEventData.InputButton.Left && skill != null){
            if (!isPicked) {
                PickSkill();   
            } else {
                UnPickSkill();
            }
            UpdateDisplay();
            PeaceCanvas.instance.SkillsPanel.GetComponent<SkillPanelUI>().UpdatePickedSkill();
        }      
    }

    void PickSkill () {
        if (Combat.instanace.availableSkillPoints <= 0)
            return;
        if (!Combat.instanace.isPickedSkillTree(skill.skillTree))
            return;

        if (!Combat.instanace.currentPickedSkills.Contains(skill))
            Combat.instanace.currentPickedSkills.Add(skill);
    }
    void UnPickSkill () {
        if (Combat.instanace.currentPickedSkills.Contains(skill))
            Combat.instanace.currentPickedSkills.Remove(skill);
    }

}
