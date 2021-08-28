using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SyncCameraSettings : MonoBehaviour
{
    CinemachineFreeLook CM_freelook;

    void Awake() {
        CM_freelook = GetComponent<CinemachineFreeLook>();
    }

    void Update() {
        if (CM_freelook && CinemachineCore.Instance.IsLive(CM_freelook))  PlayerControlls.instance.cameraControl.MatchCameraSettings(ref CM_freelook);
    }
}
