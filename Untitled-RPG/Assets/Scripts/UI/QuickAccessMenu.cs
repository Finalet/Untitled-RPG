using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickAccessMenu : MonoBehaviour
{

    public UI_QuickAccessSlot topSlot;
    public UI_QuickAccessSlot bottomSlot;
    public UI_QuickAccessSlot leftSlot;
    public UI_QuickAccessSlot rightSlot;
    
    Vector2 mouseDelta;

    float angle;
    float magnitudeThreashold = 5;
    void Update()
    {
        if (PeaceCanvas.instance.anyPanelOpen) {
            rightSlot.isSelected = false;
            topSlot.isSelected = false;
            leftSlot.isSelected = false;
            bottomSlot.isSelected = false;
            CanvasScript.instance.CloseQuickAccessMenu();
        }

        mouseDelta.x = Input.GetAxis("Mouse X");
        mouseDelta.y = Input.GetAxis("Mouse Y");

        if (mouseDelta.sqrMagnitude < magnitudeThreashold)
            return; // don't do tiny movements.
        
        angle = Mathf.Atan2 (mouseDelta.y, mouseDelta.x) * Mathf.Rad2Deg;
        if (angle<0) angle += 360;

        rightSlot.isSelected = angle < 15 || angle > 345 ? true : false;
        topSlot.isSelected = angle < 105 && angle > 75 ? true : false;
        leftSlot.isSelected = angle < 195 && angle > 165 ? true : false;
        bottomSlot.isSelected = angle < 285 && angle > 255 ? true : false;
    }
}
