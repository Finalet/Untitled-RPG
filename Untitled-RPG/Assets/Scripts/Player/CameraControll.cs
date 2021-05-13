using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Animations.Rigging;
using DG.Tweening;

enum CamSettings {Smooth, Hard};

public class CameraControll : MonoBehaviour
{
    [System.NonSerialized] public float camDistance;
    public bool stopInput;

    public bool isAiming;
    public bool isShortAiming;
    [Space]
    public LayerMask roofDetectionMask;
    [Header("Animation IK")]
    public Transform headAimTarget;
    public MultiAimConstraint headAimIK;
    

    Vector3 shake;

    Camera cam;
    [Space]
    public GameObject crosshair;
    public CinemachineFreeLook CM_cam;
    public CinemachineCameraOffset CM_offset;

    Vector3 leftShoulder;
    Vector3 rightShoulder;
    Vector3 center;
    Vector3 desiredOffset;

    Vector2 topOrbit;
    Vector2 middleOrbit;
    Vector2 bottomOrbit;

    float baseMouseXsensitivity;
    float baseMouseYsensitivity;

    CamSettings currentCamSettings = CamSettings.Smooth;

    void Start()
    {
        leftShoulder = new Vector3(-1, 0, 0);
        rightShoulder = new Vector3(0.5f, 0.3f, 0);
        center = new Vector3(0, 0, 0);

        cam = GetComponent<Camera>();

        desiredOffset = CM_offset.m_Offset;

        topOrbit = new Vector2(CM_cam.m_Orbits[0].m_Height, CM_cam.m_Orbits[0].m_Radius);
        middleOrbit = new Vector2(CM_cam.m_Orbits[1].m_Height, CM_cam.m_Orbits[1].m_Radius);
        bottomOrbit = new Vector2(CM_cam.m_Orbits[2].m_Height, CM_cam.m_Orbits[2].m_Radius);
    }

    void Update()
    {
        camDistance = Vector3.Distance(transform.position, PlayerControlls.instance.transform.position + Vector3.up*1.6f);

        UpdateMouseSettings();

        
        if (stopInput) {
            CM_cam.m_XAxis.m_InputAxisName = "";
            CM_cam.m_YAxis.m_InputAxisName = "";

            CM_cam.m_XAxis.m_InputAxisValue = 0;
            CM_cam.m_YAxis.m_InputAxisValue = 0;
        } else {
            CM_cam.m_XAxis.m_InputAxisName = "Mouse X";
            CM_cam.m_YAxis.m_InputAxisName = "Mouse Y";
        }
        
        CheckCameraUnderRoof();
    }

    void UpdateMouseSettings() {
        baseMouseXsensitivity = SettingsManager.instance.mouseSensitivity * 0.03f;
        baseMouseYsensitivity = SettingsManager.instance.mouseSensitivity * 0.0002f;
        CM_cam.m_YAxis.m_InvertInput = !SettingsManager.instance.invertY;
    }

    float rotationX;
    void FixedUpdate()
    { 
        FOV();

        if (CM_offset.m_Offset != desiredOffset) {
            CM_offset.m_Offset = Vector3.MoveTowards(CM_offset.m_Offset, desiredOffset, 3 * Time.deltaTime);
        }

        // DISABLED CAUSE ANIMATION RIGGING SINKS THE CHARACTER AND RUINS ROLLING ANIMATION
        /*
        if (PlayerControlls.instance.isIdle && headAimTarget.GetComponentInParent<MultiAimConstraint>().weight < 0.7f) {
            headAimIK.weight += Time.deltaTime;
        } else if (!PlayerControlls.instance.isIdle && headAimTarget.GetComponentInParent<MultiAimConstraint>().weight > 0) {
            headAimIK.weight -= Time.deltaTime;
        }

        Vector3 aimPos = transform.position + transform.forward * 13;
        aimPos.y = Mathf.Clamp(aimPos.y, PlayerControlls.instance.transform.position.y, PlayerControlls.instance.transform.position.y + 5);
        headAimTarget.transform.position = aimPos;
        headAimTarget.transform.localPosition = new Vector3(headAimTarget.transform.localPosition.x, headAimTarget.transform.localPosition.y, Mathf.Clamp(headAimTarget.transform.localPosition.z, 1, 10));
        */
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

        CM_cam.m_XAxis.m_MaxSpeed = baseMouseXsensitivity;   //normal sensitivity
        CM_cam.m_YAxis.m_MaxSpeed = baseMouseYsensitivity; //normal sensitivity
        
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

        if (currentCamSettings == CamSettings.Hard)
            SmoothCameraSettings();
    }

    bool wasAiming;
    public void AimCamera () {
        float desiredFOV = 0;
        if (isAiming) {
            desiredFOV = 30;
            CM_cam.m_XAxis.m_MaxSpeed = baseMouseXsensitivity/3f;   //lower sensitivity
            CM_cam.m_YAxis.m_MaxSpeed = baseMouseYsensitivity/3f; //lower sensitivity
            desiredOffset = rightShoulder;
        } else {
            desiredFOV = 50;
            desiredOffset = rightShoulder*0.7f;
        }


        if (CM_cam.m_Lens.FieldOfView > desiredFOV)
            CM_cam.m_Lens.FieldOfView = Mathf.Lerp(CM_cam.m_Lens.FieldOfView, desiredFOV-1, 7 * Time.deltaTime);

        wasAiming = true;

        if (currentCamSettings == CamSettings.Smooth)
            HardCameraSettings();
    }

    void SmoothCameraSettings() {
        CM_cam.m_XAxis.m_DecelTime = 0.2f;
        CM_cam.m_YAxis.m_DecelTime = 0.2f;

        CinemachineTransposer CM_trans = CM_cam.GetRig(1).GetCinemachineComponent<CinemachineTransposer>();
        CM_trans.m_XDamping = 0.5f;
        CM_trans.m_ZDamping = 0.5f;

        currentCamSettings = CamSettings.Smooth;
    }

    void HardCameraSettings () {
        CM_cam.m_XAxis.m_DecelTime = 0;
        CM_cam.m_YAxis.m_DecelTime = 0;

        CinemachineTransposer CM_trans = CM_cam.GetRig(1).GetCinemachineComponent<CinemachineTransposer>();
        CM_trans.m_XDamping = 0;
        CM_trans.m_ZDamping = 0;

        currentCamSettings = CamSettings.Hard;
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

    void CheckCameraUnderRoof () {
        if (isUnderRoof()) {
            ReduceFloatTo(ref CM_cam.m_Orbits[0].m_Height, 0.5f * topOrbit.x, 0.5f*topOrbit.x); //Top orbit height
            ReduceFloatTo(ref CM_cam.m_Orbits[1].m_Height, 0.8f * middleOrbit.x, 0.2f*middleOrbit.x); //Middle orbit height
            ReduceFloatTo(ref CM_cam.m_Orbits[1].m_Radius, 0.5f * middleOrbit.y, 0.5f*middleOrbit.y);  //Middle orbit raduis
        } else {
            IncreaseFloatTo(ref CM_cam.m_Orbits[0].m_Height, topOrbit.x, 0.5f*topOrbit.x); //Top orbit height
            IncreaseFloatTo(ref CM_cam.m_Orbits[1].m_Height, middleOrbit.x, 0.2f*middleOrbit.x); //Middle orbit height
            IncreaseFloatTo(ref CM_cam.m_Orbits[1].m_Radius, middleOrbit.y, 0.5f*middleOrbit.y); //Middle orbit raduis
        }
    }

    void ReduceFloatTo (ref float variableToReduce, float reduceTo, float constantDistance, float duration = 0.2f) {
        if (variableToReduce > reduceTo) {
            variableToReduce -= Time.deltaTime * constantDistance / duration;
        } else {
            variableToReduce = reduceTo;
        }
    }
    void IncreaseFloatTo (ref float variableToIncrease, float increaseTo, float constantDistance, float duration = 0.2f) {
        if (variableToIncrease < increaseTo) {
            variableToIncrease += Time.deltaTime * constantDistance / duration;
        } else {
            variableToIncrease = increaseTo;
        }
    } 

    public bool isUnderRoof () {
        return Physics.Raycast(PlayerControlls.instance.transform.position + Vector3.up*1.6f, Vector3.up, 5, roofDetectionMask);
    }
}