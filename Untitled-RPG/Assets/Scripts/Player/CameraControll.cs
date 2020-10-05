using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Animations.Rigging;

public class CameraControll : MonoBehaviour
{
    public float camDistance = 5; //set manually for skills distance calculations NEED TO FIX
    
    public bool isAiming;
    public bool isShortAiming;
    public GameObject crosshair;
    [Header("Animation IK")]
    public Transform headAimTarget;
    

    Vector3 shake;

    Camera cam;
    [Space]
    public CinemachineFreeLook CM_cam;
    public CinemachineCameraOffset CM_offset;

    const float leftShoulder = -1f;
    const float rightShoulder = 1f;
    const float center = 0;
    float desiredOffset;

    float mouseXsensitivity;
    float mouseYsensitivity;

    void Start()
    {
        cam = GetComponent<Camera>();

        desiredOffset = CM_offset.m_Offset.x;

        mouseXsensitivity = CM_cam.m_XAxis.m_MaxSpeed;
        mouseYsensitivity = CM_cam.m_YAxis.m_MaxSpeed;
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
        FOV();

        if (CM_offset.m_Offset.x != desiredOffset) {
            CM_offset.m_Offset.x = Mathf.MoveTowards(CM_offset.m_Offset.x, desiredOffset, 4 * Time.deltaTime);
        }
    }

    void FOV () {
        if (!isAiming && !isShortAiming)
            SprintingFOV();
        else
            AimCamera();

        if (isAiming || isShortAiming)
            crosshair.SetActive(true);
        else
            crosshair.SetActive(false);
    }

    void SprintingFOV () {
        desiredOffset = center;

        if (CM_cam.m_Lens.FieldOfView < 60 && wasAiming) { //After aiming
            CM_cam.m_Lens.FieldOfView = Mathf.Lerp(CM_cam.m_Lens.FieldOfView, 62, 7 * Time.deltaTime);
            return;
        }

        wasAiming = false;
        
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

    bool wasAiming;
    public void AimCamera () {
        float desiredFOV = 30;
        if (isShortAiming)
            desiredFOV = 50;

        if (CM_cam.m_Lens.FieldOfView > desiredFOV)
            CM_cam.m_Lens.FieldOfView = Mathf.Lerp(CM_cam.m_Lens.FieldOfView, desiredFOV-1, 7 * Time.deltaTime);

        desiredOffset = rightShoulder;
        wasAiming = true;
    }

    public void CameraShake(float frequency = 0.2f, float amplitude = 2f, float duration = 0.1f, Vector3 position = new Vector3()) {
        if (position == Vector3.zero)
            position = transform.position;

        CinemachineImpulseSource impulseSource = GetComponent<CinemachineImpulseSource>();
        
        impulseSource.m_ImpulseDefinition.m_FrequencyGain = frequency;
        impulseSource.m_ImpulseDefinition.m_AmplitudeGain = amplitude;
        impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = duration;
        impulseSource.GenerateImpulseAt(position, Vector3.one);
    }
}