using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Animations.Rigging;

public class CameraControll : MonoBehaviour
{
    public float camDistance = 5;
    
    [Header("Animation IK")]
    public Transform headAimTarget;
    

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

    void FixedUpdate() {
        if (PlayerControlls.instance.isIdle && headAimTarget.GetComponentInParent<MultiAimConstraint>().weight < 0.7f) {
            headAimTarget.GetComponentInParent<MultiAimConstraint>().weight += Time.deltaTime;
        } else if (!PlayerControlls.instance.isIdle && headAimTarget.GetComponentInParent<MultiAimConstraint>().weight > 0) {
            headAimTarget.GetComponentInParent<MultiAimConstraint>().weight -= Time.deltaTime;
        }

        Vector3 aimPos = transform.position + transform.forward * 13;
        aimPos.y = Mathf.Clamp(aimPos.y, PlayerControlls.instance.transform.position.y, PlayerControlls.instance.transform.position.y + 5);
        headAimTarget.transform.position = aimPos;
        headAimTarget.transform.localPosition = new Vector3(headAimTarget.transform.localPosition.x, headAimTarget.transform.localPosition.y, Mathf.Clamp(headAimTarget.transform.localPosition.z, 1, 10));
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
                CM_cam.m_Lens.FieldOfView = Mathf.Lerp(CM_cam.m_Lens.FieldOfView, 82, 7 * Time.deltaTime);
            else 
                CM_cam.m_Lens.FieldOfView = 80;
        } else {
            if (CM_cam.m_Lens.FieldOfView > 60)
                CM_cam.m_Lens.FieldOfView = Mathf.Lerp(CM_cam.m_Lens.FieldOfView, 58, 7 * Time.deltaTime);
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

    public void CameraShake(float frequency = 0.2f, float amplitude = 2f, float duration = 0.1f, Vector3 position = new Vector3()) {
        if (position == Vector3.zero)
            position = transform.position;

        GetComponent<CinemachineImpulseSource>().m_ImpulseDefinition.m_FrequencyGain = frequency;
        GetComponent<CinemachineImpulseSource>().m_ImpulseDefinition.m_AmplitudeGain = amplitude;
        GetComponent<CinemachineImpulseSource>().m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = duration;
        GetComponent<CinemachineImpulseSource>().GenerateImpulseAt(position, Vector3.one);
    }
}