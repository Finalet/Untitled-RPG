using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering.Universal;

public class TooltipsManager : MonoBehaviour
{
    public GameObject toolTipPrefab;

    [Space]
    public float toolTipDelay = 0.4f;
    float startTime;

    Tooltip currentToolTip;

    Item focusItem;
    Vector3 screenPos;
    Vector3 lastScreenPos;
    Vector2 anchoredShift;
    Vector2 pivot;

    void Update() {
        if (!PeaceCanvas.instance.anyPanelOpen)
            return;

        CheckMouseOver();
        GenerateToolTip();
    }

    void CheckMouseOver() {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        for (int i = 0; i < raycastResults.Count; i++) {
            //UI_Inventory slots
            if (raycastResults[i].gameObject.GetComponent<UI_InventorySlot>() != null) {
                if (raycastResults[i].gameObject.GetComponent<UI_InventorySlot>().itemInSlot != null) {
                    lastScreenPos = screenPos;

                    focusItem = raycastResults[i].gameObject.GetComponent<UI_InventorySlot>().itemInSlot;
                    screenPos = raycastResults[i].gameObject.GetComponent<RectTransform>().position;
                    anchoredShift = new Vector2(-raycastResults[i].gameObject.GetComponent<RectTransform>().sizeDelta.x/2 - 10, +raycastResults[i].gameObject.GetComponent<RectTransform>().sizeDelta.y/2) ;
                    pivot = Vector2.one;
                    if (lastScreenPos != screenPos)
                        startTime = Time.realtimeSinceStartup;
                    return;
                }
            }

            //StoreSlots
            if (raycastResults[i].gameObject.GetComponent<StoreItemUI>() != null) {
                if (raycastResults[i].gameObject.GetComponent<StoreItemUI>().item != null) {
                    lastScreenPos = screenPos;

                    focusItem = raycastResults[i].gameObject.GetComponent<StoreItemUI>().item;
                    anchoredShift = new Vector2(raycastResults[i].gameObject.GetComponent<RectTransform>().sizeDelta.x/2, raycastResults[i].gameObject.GetComponent<RectTransform>().sizeDelta.y/2) ;
                    screenPos = raycastResults[i].gameObject.GetComponent<RectTransform>().position;
                    pivot = new Vector2(0, 1);
                    if (lastScreenPos != screenPos)
                        startTime = Time.realtimeSinceStartup;
                    return;
                }
            }
            
            //CraftingSlots
            if (raycastResults[i].gameObject.GetComponent<CraftingItemUI>() != null) {
                if (raycastResults[i].gameObject.GetComponent<CraftingItemUI>().item != null) {
                    lastScreenPos = screenPos;

                    focusItem = raycastResults[i].gameObject.GetComponent<CraftingItemUI>().item;
                    anchoredShift = new Vector2(-raycastResults[i].gameObject.GetComponent<RectTransform>().sizeDelta.x/2, raycastResults[i].gameObject.GetComponent<RectTransform>().sizeDelta.y/2) ;
                    screenPos = raycastResults[i].gameObject.GetComponent<RectTransform>().position;
                    pivot = new Vector2(1, 1);
                    if (lastScreenPos != screenPos)
                        startTime = Time.realtimeSinceStartup;
                    return;
                }
            }
        }
        focusItem = null;
        screenPos = Vector2.zero;
        anchoredShift = Vector2.zero;
        pivot = Vector2.zero;
    }

    void GenerateToolTip () {
        if (focusItem != null) {
            if (Time.realtimeSinceStartup - startTime > toolTipDelay || currentToolTip != null) {
                if (currentToolTip == null) {
                    currentToolTip = Instantiate(toolTipPrefab, PeaceCanvas.instance.transform).gameObject.GetComponent<Tooltip>();
                }
                currentToolTip.gameObject.GetComponent<RectTransform>().position = screenPos;
                currentToolTip.gameObject.GetComponent<RectTransform>().anchoredPosition += anchoredShift;
                currentToolTip.gameObject.GetComponent<RectTransform>().pivot = pivot;
                currentToolTip.focusItem = focusItem;
                currentToolTip.Init();

                if (!currentToolTip.GetComponent<RectTransform>().IsFullyVisibleFrom(PlayerControlls.instance.playerCamera.GetUniversalAdditionalCameraData().cameraStack[0])) {
                    pivot.x = Mathf.Abs(pivot.x-1);

                    currentToolTip.gameObject.GetComponent<RectTransform>().anchoredPosition -= anchoredShift + new Vector2(anchoredShift.x, -anchoredShift.y);
                    currentToolTip.gameObject.GetComponent<RectTransform>().pivot = pivot;
                }
            }
        } else if (currentToolTip != null) {
            Destroy(currentToolTip.gameObject);
        }
    }
}
