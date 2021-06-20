using DG.Tweening;
using UnityEngine;

public class QuickAccessMenu : MonoBehaviour
{

    public UI_QuickAccessSlot topSlot;
    public UI_QuickAccessSlot leftSlot;
    public UI_QuickAccessSlot rightSlot;
    public UI_QuickAccessSlot cancelSlot;
    
    Vector2 mouseDelta;

    float angle;
    float magnitudeThreashold = 5;
    
    void OnEnable() {
        rightSlot.GetComponent<RectTransform>().DOAnchorPosX(0, 0.2f).SetEase(Ease.OutCubic);
        leftSlot.GetComponent<RectTransform>().DOAnchorPosX(0, 0.2f).SetEase(Ease.OutCubic);
        topSlot.GetComponent<RectTransform>().DOAnchorPosY(0, 0.2f).SetEase(Ease.OutCubic);
        cancelSlot.GetComponent<RectTransform>().DOAnchorPosY(0, 0.2f).SetEase(Ease.OutCubic);

        rightSlot.GetComponent<RectTransform>().DOScale(Vector3.one, 0.2f);
        leftSlot.GetComponent<RectTransform>().DOScale(Vector3.one, 0.2f);
        topSlot.GetComponent<RectTransform>().DOScale(Vector3.one, 0.2f);
        cancelSlot.GetComponent<RectTransform>().DOScale(Vector3.one, 0.2f);
    }
    
    public void Close() {
        rightSlot.GetComponent<RectTransform>().DOAnchorPosX(-100, 0.2f).SetEase(Ease.OutCubic);
        leftSlot.GetComponent<RectTransform>().DOAnchorPosX(100, 0.2f).SetEase(Ease.OutCubic);
        topSlot.GetComponent<RectTransform>().DOAnchorPosY(-100, 0.2f).SetEase(Ease.OutCubic);
        cancelSlot.GetComponent<RectTransform>().DOAnchorPosY(100, 0.2f).SetEase(Ease.OutCubic);

        rightSlot.GetComponent<RectTransform>().DOScale(Vector3.zero, 0.2f);
        leftSlot.GetComponent<RectTransform>().DOScale(Vector3.zero, 0.2f);
        topSlot.GetComponent<RectTransform>().DOScale(Vector3.zero, 0.2f);
        cancelSlot.GetComponent<RectTransform>().DOScale(Vector3.zero, 0.2f);

        if (topSlot.isSelected)
            topSlot.UseItem();
        else if (cancelSlot.isSelected)
            cancelSlot.UseItem();
        else if (rightSlot.isSelected)
            rightSlot.UseItem();
        else if (leftSlot.isSelected)
            leftSlot.UseItem();

        topSlot.isSelected = false;
        cancelSlot.isSelected = false;
        leftSlot.isSelected = false;
        rightSlot.isSelected = false;

        Invoke("SetInactive", 0.3f);
    }

    void SetInactive () {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (PeaceCanvas.instance.anyPanelOpen) {
            rightSlot.isSelected = false;
            topSlot.isSelected = false;
            leftSlot.isSelected = false;
            cancelSlot.isSelected = false;
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
        cancelSlot.isSelected = angle < 285 && angle > 255 ? true : false;
    }
}
