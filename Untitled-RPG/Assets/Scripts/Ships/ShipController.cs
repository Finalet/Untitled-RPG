using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Crest;
using Cinemachine;

[System.Serializable]
public struct RotationContainer {
    public Transform transform;
    public Quaternion initialRotation;
    public Quaternion maxRotation;

    public RotationContainer(Transform transform, Quaternion initialRotation, Quaternion maxRotation)
    {
        this.transform = transform;
        this.initialRotation = initialRotation;
        this.maxRotation = maxRotation;
    }
}

public class ShipController : MonoBehaviour
{
    public static ShipController instance;

    [Header("Ships stats")]
    public string shipName;
    public float baseSpeed;
    public float baseTurnPower;
    [DisplayWithoutEdit, SerializeField, Space] int currentSpeedIndex; int maxSpeedIndex = 2;
    [DisplayWithoutEdit] public bool isControlled;
    float speedRatio {
        get {
            return (float)currentSpeedIndex / (float)maxSpeedIndex;
        }
    }

    [Foldout("Visuals")] public float sailsStretchStrength;
    [Foldout("Visuals")] public float flagStretchStrength;
    [Foldout("Visuals")] public int maxRudderAngle = 40;
    [Foldout("Visuals")] public int maxWheelRotationAngle = 100;
    [Foldout("Visuals")] public int maxMastRotationAngle = 30;
    [Foldout("Visuals"), SerializeField] float mastsLerpValue = 0.5f;
    [Foldout("Visuals"), UnityEngine.Range(0, 360)] public float globalWindDirection;
    [Foldout("Visuals"), SerializeField, ReadOnly] float angleToWind;
    Vector3 windVector;
    float mastRotation;

    [Header("Setup")]
    [Required("Assign main deck mesh for the collision dummy")] public GameObject deckMesh;
    public Transform steeringWheel;
    public Transform rudder;
    public Cloth flag;
    public Transform[] masts;

    [Space]
    [ReadOnly] public DeckCollider deckCollider;
    float turnInput;
    
    [SerializeField] RotationContainer rudderRot;
    RotationContainer wheelRot;
    RotationContainer[] mastsRot;

    BoatProbes boatProbe;
    Cloth[] sails;
    CinemachineFreeLook CM_cam;

    KeyCode increaseSpeedIndex = KeyCode.Alpha2;
    KeyCode decreaseSpeedIndex = KeyCode.Alpha1;

    void Awake() {
        boatProbe = GetComponent<BoatProbes>();
        sails = GetComponentsInChildren<Cloth>();
        CM_cam = GetComponentInChildren<CinemachineFreeLook>();

        boatProbe._enginePower = baseSpeed;

        InitializeColliderDummy();
        SetInitialRotations();
    }

    void Update() {
        HandleInput();
        SyncSpeed();
        RunVisuals();
    
        CM_cam.Priority = isControlled ? 50 : 0;
        
        if (isControlled) PlayerControlls.instance.cameraControl.MatchCameraSettings(ref CM_cam);
    }

    void HandleInput () {
        if (!isControlled) return;

        if (Input.GetKeyDown(increaseSpeedIndex)) IncreaseSpeed();
        else if (Input.GetKeyDown(decreaseSpeedIndex)) DecreaseSpeed();

        turnInput = !isControlled || currentSpeedIndex == 0 ? 0 : Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0;
    }

    void IncreaseSpeed () {
        currentSpeedIndex ++;
        currentSpeedIndex = Mathf.Clamp(currentSpeedIndex, 0, maxSpeedIndex);
    }

    void DecreaseSpeed () {
        currentSpeedIndex --;
        currentSpeedIndex = Mathf.Clamp(currentSpeedIndex, 0, maxSpeedIndex);
    }

    void SyncSpeed () {
        boatProbe._engineBias = speedRatio;
        boatProbe._turnBias = Mathf.MoveTowards(boatProbe._turnBias, turnInput, Time.deltaTime * 2);
    }
    
    void RunVisuals () {
        if (!boatProbe) boatProbe = GetComponent<BoatProbes>();
        if (mastsRot == null) SetInitialRotations();

        for (int i = 0; i < sails.Length; i++) {
            sails[i].externalAcceleration = Vector3.MoveTowards(sails[i].externalAcceleration, transform.right * mastRotation * sailsStretchStrength * speedRatio, 30 * Time.deltaTime);
        }

        flag.externalAcceleration = windVector * flagStretchStrength;
        flag.randomAcceleration = Quaternion.Euler(Vector3.up * 90) * windVector * flagStretchStrength * 0.2f;

        rudder.localRotation = Quaternion.LerpUnclamped(rudderRot.initialRotation, rudderRot.maxRotation, boatProbe._turnBias);
        steeringWheel.localRotation = Quaternion.LerpUnclamped(wheelRot.initialRotation, wheelRot.maxRotation, boatProbe._turnBias);

        windVector = new Vector3(Mathf.Sin(globalWindDirection*Mathf.Deg2Rad), 0, Mathf.Cos(globalWindDirection*Mathf.Deg2Rad));
        angleToWind = Vector3.SignedAngle(transform.forward, windVector, transform.up);
        
        mastRotation = ( (angleToWind > 0 ? angleToWind-180 : angleToWind+180) - (maxMastRotationAngle) ) / (-maxMastRotationAngle*2) * 2 - 1;
        mastRotation = Mathf.Clamp(mastRotation, -1, 1);

        for (int i = 0; i < mastsRot.Length; i++) {
            Quaternion desRotation = Quaternion.LerpUnclamped(mastsRot[i].initialRotation, mastsRot[i].maxRotation, mastRotation);
            masts[i].localRotation = Quaternion.Lerp(masts[i].localRotation, desRotation, Time.deltaTime * mastsLerpValue);
        }
    }
    
    void SetInitialRotations () {
        rudderRot = new RotationContainer(rudder, rudder.localRotation, Quaternion.Euler(-rudder.up * maxRudderAngle) * rudder.localRotation); 
        wheelRot = new RotationContainer(steeringWheel, steeringWheel.localRotation, Quaternion.Euler(steeringWheel.InverseTransformDirection(-steeringWheel.forward) * maxWheelRotationAngle) * steeringWheel.localRotation);

        mastsRot = new RotationContainer[masts.Length];
        for (int i = 0; i < mastsRot.Length; i++) {
            mastsRot[i] = new RotationContainer(masts[i], masts[i].localRotation, Quaternion.Euler(-masts[i].up * maxMastRotationAngle) * masts[i].localRotation);
        }
    }

    void InitializeColliderDummy() {
        deckCollider = new GameObject().AddComponent<DeckCollider>();
        deckCollider.Init(this);
    }

    public void TogglePlayerControl (Transform posRot) {
        isControlled = !isControlled;
        PlayerControlls.instance.disableControlRequests += isControlled ? 1 : -1;
        CanvasScript.instance.ShowSkillpanel = !isControlled;

        if (isControlled) PlayerControlls.instance.OverridePosRot(posRot);
        else PlayerControlls.instance.OverridePosRot(false);
    }
}
