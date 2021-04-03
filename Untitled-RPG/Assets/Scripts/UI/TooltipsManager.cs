using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    Vector2 iconSize;

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
            if (raycastResults[i].gameObject.GetComponent<UI_InventorySlot>() != null) {
                if (raycastResults[i].gameObject.GetComponent<UI_InventorySlot>().itemInSlot != null) {
                    lastScreenPos = screenPos;

                    focusItem = raycastResults[i].gameObject.GetComponent<UI_InventorySlot>().itemInSlot;
                    screenPos = raycastResults[i].gameObject.GetComponent<RectTransform>().position;
                    iconSize = raycastResults[i].gameObject.GetComponent<RectTransform>().sizeDelta;
                    if (lastScreenPos != screenPos)
                        startTime = Time.realtimeSinceStartup;
                    return;
                }
            }
        }
        focusItem = null;
        screenPos = Vector2.zero;
        iconSize = Vector2.zero;
    }

    void GenerateToolTip () {
        if (focusItem != null) {
            if (Time.realtimeSinceStartup - startTime > toolTipDelay || currentToolTip != null) {
                if (currentToolTip == null) {
                    currentToolTip = Instantiate(toolTipPrefab, PeaceCanvas.instance.transform).gameObject.GetComponent<Tooltip>();
                }
                currentToolTip.gameObject.GetComponent<RectTransform>().position = screenPos;
                currentToolTip.gameObject.GetComponent<RectTransform>().anchoredPosition -= new Vector2(iconSize.x/2 + 10, -iconSize.y/2);
                currentToolTip.focusItem = focusItem;
                currentToolTip.Init();
            }
        } else if (currentToolTip != null) {
            Destroy(currentToolTip.gameObject);
        }
    }
}
