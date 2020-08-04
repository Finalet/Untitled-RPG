using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraControll : MonoBehaviour
{
    public float camDistance = 5;

    Vector3 shake;

    Camera cam;
    CinemachineFreeLook CM_cam;
    CinemachineCameraOffset CM_offset;

    const float leftShoulder = -0.5f;
    const float rightShoulder = 0.5f;
    const float center = 0;
    float desiredOffset;

    void Start()
    {
        cam = GetComponent<Camera>();
        CM_cam = GetComponentInChildren<CinemachineFreeLook>();
        CM_offset = GetComponentInChildren<CinemachineCameraOffset>();

        desiredOffset = CM_offset.m_Offset.x;
    }

    void Update()
    {
        //Shoulder camera switching
        if (Input.GetKeyUp(KeyCode.Mouse2))
            SwitchShouderCam();
        
        //Turn on/off UI
        if (Input.GetKeyUp(KeyCode.F3)) {
            CanvasScript.instance.gameObject.SetActive(!CanvasScript.instance.gameObject.activeInHierarchy);
            PeaceCanvas.instance.gameObject.SetActive(!PeaceCanvas.instance.gameObject.activeInHierarchy);
        }
    }

    float rotationX;
    void LateUpdate()
    { 
        SprintCameraFOV();

        if (CM_offset.m_Offset.x != desiredOffset) {
            CM_offset.m_Offset.x = Mathf.MoveTowards(CM_offset.m_Offset.x, desiredOffset, 4 * Time.deltaTime);
        }
    }

    void SprintCameraFOV () {
        if (PlayerControlls.instance.isSprinting) {
            if (CM_cam.m_Lens.FieldOfView < 80)
                CM_cam.m_Lens.FieldOfView = Mathf.Lerp(CM_cam.m_Lens.FieldOfView, 82, 10 * Time.deltaTime);
            else 
                CM_cam.m_Lens.FieldOfView = 80;
        } else {
            if (CM_cam.m_Lens.FieldOfView > 60)
                CM_cam.m_Lens.FieldOfView = Mathf.Lerp(CM_cam.m_Lens.FieldOfView, 58, 10 * Time.deltaTime);
            else CM_cam.m_Lens.FieldOfView = 60;
        }
    }

    void SwitchShouderCam() {
        switch (CM_offset.m_Offset.x) {
            case center: 
                desiredOffset = rightShoulder;
                break;
            case rightShoulder:
                desiredOffset = leftShoulder;
                break;
            case leftShoulder:
                desiredOffset = center;
                break;
            default: desiredOffset = center;
                break;
        }
    }

    public void CameraMounted() {
        //camDesiredPosition += 3;
        //maxCamDistance += 5;
    }
    public void CameraDismount () {
        //camDesiredPosition -= 3;
        //maxCamDistance -= 5;
    }

    public void CameraShake(float frequency = 0.2f, float amplitude = 2f, float duration = 0.1f, Vector3 position = new Vector3()) {
        if (position == Vector3.zero)
            position = transform.position;

        GetComponent<CinemachineImpulseSource>().m_ImpulseDefinition.m_FrequencyGain = frequency;
        GetComponent<CinemachineImpulseSource>().m_ImpulseDefinition.m_AmplitudeGain = amplitude;
        GetComponent<CinemachineImpulseSource>().m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = duration;
        GetComponent<CinemachineImpulseSource>().GenerateImpulseAt(position, Vector3.one);
    }
}