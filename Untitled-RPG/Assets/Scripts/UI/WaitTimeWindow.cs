using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Funly.SkyStudio;
using DG.Tweening;
using Cinemachine;

public class WaitTimeWindow : MonoBehaviour
{
    public Slider currentTimeSlider;
    public Slider selectedTimeSlider;

    public TextMeshProUGUI currentTimeLabel;
    public TextMeshProUGUI selectedTimeLabel;

    public Button waitButton;
    public Button closeButton;
    public TextMeshProUGUI waitUntilLabel;

    public CinemachineVirtualCamera CM_cam;

    bool isWaiting;

    void Update() {
        MatchCurrentTime();
        waitButton.interactable = Mathf.Abs(selectedTimeSlider.value - TimeOfDayController.instance.timeOfDay) > 0.03f;
        closeButton.interactable = !isWaiting;

        if (Input.GetKeyDown(KeyCode.Escape))
            CloseWindow();
    }

    public void OpenWindow() {
        MatchCurrentTime();
        selectedTimeSlider.value = (currentTimeSlider.value + 0.1f) % 1;
        SetSelectedTime();
        gameObject.SetActive(true);
    }

    public void CloseWindow() {
        if (!isWaiting) {
            gameObject.SetActive(false);
        }
    }

    void OnEnable() {
        PeaceCanvas.instance.forceAnyPanelOpen = true;
    }
    void OnDisable() {
        PeaceCanvas.instance.forceAnyPanelOpen = false;
    }

    void MatchCurrentTime () {
        currentTimeSlider.value = TimeOfDayController.instance.timeOfDay;
        currentTimeLabel.text = TimeOfDayController.instance.TimeStringFromPercent(currentTimeSlider.value);
    }

    void SetSelectedTime() {
        selectedTimeLabel.text = TimeOfDayController.instance.TimeStringFromPercent(selectedTimeSlider.value);
        waitUntilLabel.text = waitUntilButtonString();
    }

    public void OnSelectedTimeChange () {
        SetSelectedTime();
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
    }
    
    public void WaitButton () {
        if (!isWaiting) {
            StartCoroutine(WaitUntilTime());
        } else {
            StopWaiting();
        }
    }
    void StopWaiting () {
        isWaiting = false;
    }
    float startStopTime = 4;
    float stopThreashold = 0.023f;
    IEnumerator WaitUntilTime () {
        isWaiting = true;
        waitUntilLabel.text = "Stop waiting";
        float pastSpeed = TimeOfDayController.instance.automaticIncrementSpeed;
        DOTween.To(()=> Time.timeScale, x=> Time.timeScale = x, 5, startStopTime); 
        DOTween.To(()=> TimeOfDayController.instance.automaticIncrementSpeed, x=> TimeOfDayController.instance.automaticIncrementSpeed = x, 0.015f, startStopTime); 
        selectedTimeSlider.interactable = false;
        CM_cam.enabled = true;
        CM_cam.transform.position = PlayerControlls.instance.playerCamera.transform.position + Vector3.up * 2;
        CM_cam.transform.rotation = Quaternion.Euler(CM_cam.transform.eulerAngles.x, PlayerControlls.instance.playerCamera.transform.eulerAngles.y, CM_cam.transform.eulerAngles.z);
        while ( Mathf.Abs(selectedTimeSlider.value - TimeOfDayController.instance.timeOfDay) > stopThreashold) {
            if (!isWaiting)
                break;
            yield return null;
        }
        CM_cam.enabled = false;
        isWaiting = false;
        selectedTimeSlider.interactable = true;
        DOTween.To(()=> Time.timeScale, x=> Time.timeScale = x, 1, startStopTime); 
        DOTween.To(()=> TimeOfDayController.instance.automaticIncrementSpeed, x=> TimeOfDayController.instance.automaticIncrementSpeed = x, pastSpeed, startStopTime); 
        waitUntilLabel.text = waitUntilButtonString();
    }

    string waitUntilButtonString() {
        return $"Wait untill: {TimeOfDayController.instance.TimeStringFromPercent(selectedTimeSlider.value)}";
    }
}
