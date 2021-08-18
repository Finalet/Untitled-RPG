using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TooltipsManager : MonoBehaviour
{
    public static TooltipsManager instance;

    public GameObject toolTipPrefab;
    public GameObject skillToolTipPrefab;
    [Space]
    public float toolTipDelay = 0.2f;

    Tooltip currentToolTip;
    Tooltip currentCompareTooltip;
    SkillTooltip currentSkillToolTip;

    GameObject currentTooltipOwner;
    bool cancelTooltip;

    float timeStarted;

    void Awake() {
        instance = this;
    }
    void Update() {
        if (!PeaceCanvas.instance.anyPanelOpen) {
            cancelTooltip = true;
            currentTooltipOwner = null;
            if(currentToolTip != null) {
                Destroy(currentToolTip.gameObject); 
            }
            if (currentSkillToolTip != null) {
                Destroy(currentSkillToolTip.gameObject); 
            }
        }
    }

    public void RequestTooltip (Item focusItem, GameObject requestFrom) {
        StartCoroutine(GenerateTooltip(focusItem, requestFrom.GetComponent<RectTransform>()));
    }
    public void RequestTooltip (Skill focusSkill, GameObject requestFrom) {
        StartCoroutine(GenerateTooltip(focusSkill, requestFrom.GetComponent<RectTransform>()));
    }
    IEnumerator GenerateTooltip (Item focusItem, RectTransform slotTransform) {
        timeStarted = Time.time;
        currentTooltipOwner = slotTransform.gameObject;
        cancelTooltip = false;

        if (currentCompareTooltip != null)
            Destroy(currentCompareTooltip.gameObject);
        
        bool instant = currentToolTip;
        while (Time.time - timeStarted < toolTipDelay && !instant) {
            if (cancelTooltip) {
                cancelTooltip = false;
                currentTooltipOwner = null;
                yield break;
            }
            yield return null;
        }
        if (currentTooltipOwner != slotTransform.gameObject) //protection layer. without it sometimes tooltip shows previous slot.
            yield break;
        
        Item compareItem = null;
        bool shouldCompare = shouldCompareItem(focusItem, out compareItem, slotTransform);

        Vector2 pivot = Vector2.one;
        Vector2 anchoredShift = new Vector2(-slotTransform.sizeDelta.x/2 - 10, +slotTransform.sizeDelta.y/2);
        if (currentToolTip == null) {
            currentToolTip = Instantiate(toolTipPrefab, PeaceCanvas.instance.transform).gameObject.GetComponent<Tooltip>();
        }
        RectTransform rt = currentToolTip.gameObject.GetComponent<RectTransform>();
        rt.position = slotTransform.position;
        rt.anchoredPosition += anchoredShift;
        rt.pivot = pivot;
        currentToolTip.focusItem = focusItem;
        currentToolTip.Init(compareItem);

        bool isVisible = true;
        if (!currentToolTip.GetComponent<RectTransform>().IsFullyVisibleFrom(PlayerControlls.instance.playerCamera.GetUniversalAdditionalCameraData().cameraStack[0])) {
            pivot.x = Mathf.Abs(pivot.x-1);

            rt.anchoredPosition -= anchoredShift + new Vector2(anchoredShift.x, -anchoredShift.y);
            rt.pivot = pivot;

            isVisible = false;
        }

        //Generate second tooltip fro comparing with equiped item
        if (!shouldCompare)
            yield break;
        
        yield return new WaitForSeconds(0.5f);
        if (currentToolTip == null || currentTooltipOwner != slotTransform.gameObject) //check if there is still a tooltip || protection layer. without it sometimes tooltip shows previous slot.
            yield break;
        if (currentCompareTooltip == null) {
            currentCompareTooltip = Instantiate(toolTipPrefab, currentToolTip.transform).gameObject.GetComponent<Tooltip>();
        }
        rt = currentCompareTooltip.gameObject.GetComponent<RectTransform>();
        rt.position = slotTransform.position;
        rt.anchoredPosition += anchoredShift;
        rt.pivot = new Vector2(pivot.x * 2, pivot.y);
        currentCompareTooltip.focusItem = compareItem;
        currentCompareTooltip.Init(null);
        currentCompareTooltip.leftBottomLabel.text = "Equiped";

        if (!isVisible) {
            pivot.x = -Mathf.Abs(pivot.x-1);

            rt.anchoredPosition -= anchoredShift + new Vector2(anchoredShift.x, -anchoredShift.y);
            rt.pivot = pivot;
        }

        if (!currentCompareTooltip.GetComponent<RectTransform>().IsFullyVisibleFrom(PlayerControlls.instance.playerCamera.GetUniversalAdditionalCameraData().cameraStack[0])) {
            pivot.x = 2 * Mathf.Abs(pivot.x-1);

            rt.anchoredPosition -= anchoredShift + new Vector2(anchoredShift.x, -anchoredShift.y);
            rt.pivot = pivot;
        }
    }
    IEnumerator GenerateTooltip (Skill focusSkill, RectTransform slotTransform) {
        timeStarted = Time.time;
        currentTooltipOwner = slotTransform.gameObject;
        cancelTooltip = false;
        
        bool instant = currentSkillToolTip;
        while (Time.time - timeStarted < toolTipDelay && !instant) {
            if (cancelTooltip) {
                cancelTooltip = false;
                currentTooltipOwner = null;
                yield break;
            }
            yield return null;
        }

        if (currentTooltipOwner != slotTransform.gameObject) //protection layer. without it sometimes tooltip shows previous skill.
            yield break;

        Vector2 pivot = Vector2.one;
        Vector2 anchoredShift = new Vector2(-slotTransform.sizeDelta.x/2 - 10, +slotTransform.sizeDelta.y/2);
        if (currentSkillToolTip == null) {
            currentSkillToolTip = Instantiate(skillToolTipPrefab, PeaceCanvas.instance.transform).gameObject.GetComponent<SkillTooltip>();
        }
        currentSkillToolTip.gameObject.GetComponent<RectTransform>().position = slotTransform.position;
        currentSkillToolTip.gameObject.GetComponent<RectTransform>().anchoredPosition += anchoredShift;
        currentSkillToolTip.gameObject.GetComponent<RectTransform>().pivot = pivot;
        currentSkillToolTip.focusSkill = focusSkill;
        currentSkillToolTip.Init();

        if (!currentSkillToolTip.GetComponent<RectTransform>().IsFullyVisibleFrom(PlayerControlls.instance.playerCamera.GetUniversalAdditionalCameraData().cameraStack[0])) {
            pivot.x = Mathf.Abs(pivot.x-1);

            currentSkillToolTip.gameObject.GetComponent<RectTransform>().anchoredPosition -= anchoredShift + new Vector2(anchoredShift.x, -anchoredShift.y);
            currentSkillToolTip.gameObject.GetComponent<RectTransform>().pivot = pivot;
        }
    }
    
    public void CancelTooltipRequest (GameObject requestFrom) {
        if (requestFrom == currentTooltipOwner) StartCoroutine(CancelTooltip(requestFrom));
    }
    IEnumerator CancelTooltip (GameObject requestFrom) {
        yield return null; //Wait one frame before checking if you are still an owner;
        if (requestFrom == currentTooltipOwner) {
            cancelTooltip = true;
            currentTooltipOwner = null;
            if(currentToolTip != null) {
                Destroy(currentToolTip.gameObject); 
            }
            if (currentSkillToolTip != null) {
                Destroy(currentSkillToolTip.gameObject); 
            }
        }
    }

    bool shouldCompareItem (Item item, out Item comparedItem, RectTransform slot) {
        comparedItem = null;
        if (!(item is Equipment || item is Mount || item is MountEquipment) || (slot.GetComponent<UI_EquipmentSlot>() != null || slot.GetComponent<UI_MountSlot>() != null || slot.GetComponent<UI_MountEquipmentSlot>() != null)) {
            return false;
        }
        
        if (item is Weapon) {
            Weapon wp = (Weapon)item;
            if (EquipmentManager.instance.isSlotEquiped(wp.weaponHand, out comparedItem)) {
                return true;
            }
        } else if (item is Armor) {
            Armor ar = (Armor)item;
            if (EquipmentManager.instance.isSlotEquiped(ar.armorType, out comparedItem)) {
                return true;
            }
        } else if (item is Mount) {
            if (EquipmentManager.instance.isMountEquiped(out comparedItem)) {
                return true;
            }
        } else if (item is MountEquipment) {
            MountEquipment me = (MountEquipment)item;
            if (EquipmentManager.instance.isSlotEquiped(me.equipmentType, out comparedItem)) {
                return true;
            }
        }
        
        return false;
    }
}
