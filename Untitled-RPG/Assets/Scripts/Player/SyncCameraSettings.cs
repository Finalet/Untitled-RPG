using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using NaughtyAttributes;

public class SyncCameraSettings : MonoBehaviour
{
    public float sensitivityMultiplier = 1;
    public bool overrideInvertY = false;
    [ShowIf("overrideInvertY")] public bool overrideInvertYValue;

    CinemachineFreeLook CM_freelook;

    CinemachineVirtualCamera CM_virtualCamera;
    CinemachinePOV CM_POV;

    void Awake() {
        CM_freelook = GetComponent<CinemachineFreeLook>();
        if (CM_freelook) return;

        CM_virtualCamera = GetComponent<CinemachineVirtualCamera>();
        if (CM_virtualCamera) CM_POV = CM_virtualCamera.GetCinemachineComponent<CinemachinePOV>();
    }

    void Update() {
        if (CM_freelook && CinemachineCore.Instance.IsLive(CM_freelook)) {
            PlayerControlls.instance.cameraControl.MatchCameraSettings(ref CM_freelook, sensitivityMultiplier, overrideInvertY, overrideInvertYValue);
            return;
        }
        
        if (CM_POV && CinemachineCore.Instance.IsLive(CM_virtualCamera))  {
            PlayerControlls.instance.cameraControl.MatchCameraSettings(ref CM_POV, sensitivityMultiplier, overrideInvertY, overrideInvertYValue);
            return;
        }
    }
}
