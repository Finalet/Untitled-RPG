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
public enum SailDirection {ForwardFacing, SideFacing};
[System.Serializable]
public struct Sail {
    public SailDirection sailDirection;
    public Cloth sailCloth;
    [UnityEngine.Range(0,1)] public float followWindDirection;
}

public class ShipController : MonoBehaviour
{
    public static ShipController instance;

    [Header("Ships stats")]
    public string shipName;
    public float baseSpeed;
    public float baseTurnPower;
    public int maxSpeedIndex = 2;
    [Space, DisplayWithoutEdit, SerializeField] int currentSpeedIndex;
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
    public Sail[] sails;

    [Space]
    [ReadOnly] public DeckCollider deckCollider;
    float turnInput;
    
    RotationContainer rudderRot;
    RotationContainer wheelRot;
    RotationContainer[] mastsRot;

    BoatProbes boatProbe;
    CinemachineFreeLook CM_cam;
    [System.NonSerialized] public ShipAttachements shipAttachements;

    void Awake() {
        boatProbe = GetComponent<BoatProbes>();
        CM_cam = GetComponentInChildren<CinemachineFreeLook>();
        shipAttachements = GetComponent<ShipAttachements>();

        InitializeColliderDummy();
        SetInitialRotations();

        if (ShipController.instance && ShipController.instance != this) Destroy(ShipController.instance.gameObject);
        instance = this;
    }

    void Update() {
        HandleInput();
        SyncSpeed();
        RunVisuals();
    
        CM_cam.Priority = isControlled ? 50 : 0;
    }

    void HandleInput () {
        if (!isControlled) return;

        if (Input.GetKeyDown(KeybindsManager.instance.currentKeyBinds["Increase Vehicle Speed"])) IncreaseSpeed();
        else if (Input.GetKeyDown(KeybindsManager.instance.currentKeyBinds["Decrease Vehicle Speed"])) DecreaseSpeed();

        turnInput = !isControlled || currentSpeedIndex == 0 ? 0 : Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0;

        if (shipAttachements) shipAttachements.HandleInput();
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
        boatProbe._enginePower = baseSpeed;
        boatProbe._turnPower = baseTurnPower;
        boatProbe._engineBias = speedRatio;
        boatProbe._turnBias = Mathf.MoveTowards(boatProbe._turnBias, turnInput, Time.deltaTime * 2);
    }
    
    void RunVisuals () {
        if (!boatProbe) boatProbe = GetComponent<BoatProbes>();
        if (mastsRot == null) SetInitialRotations();

        if (sails.Length > 0) {
            for (int i = 0; i < sails.Length; i++) {
                Vector3 dir = sails[i].sailDirection == SailDirection.ForwardFacing ? transform.forward : transform.right;
                dir = Vector3.Lerp(dir, windVector, sails[i].followWindDirection);
                sails[i].sailCloth.externalAcceleration = Vector3.MoveTowards(sails[i].sailCloth.externalAcceleration,  dir * mastRotation * sailsStretchStrength * speedRatio, 30 * Time.deltaTime);
            }
        }
        
        if (flag) flag.externalAcceleration = windVector * flagStretchStrength;
        if (flag) flag.randomAcceleration = Quaternion.Euler(Vector3.up * 90) * windVector * flagStretchStrength * 0.2f;

        if (rudder) rudder.localRotation = Quaternion.LerpUnclamped(rudderRot.initialRotation, rudderRot.maxRotation, boatProbe._turnBias);
        if (steeringWheel) steeringWheel.localRotation = Quaternion.LerpUnclamped(wheelRot.initialRotation, wheelRot.maxRotation, boatProbe._turnBias);

        windVector = new Vector3(Mathf.Sin(globalWindDirection*Mathf.Deg2Rad), 0, Mathf.Cos(globalWindDirection*Mathf.Deg2Rad));
        angleToWind = Vector3.SignedAngle(transform.forward, windVector, transform.up);

        if (masts.Length > 0) {
            mastRotation = ( (angleToWind > 0 ? angleToWind-180 : angleToWind+180) - (maxMastRotationAngle) ) / (-maxMastRotationAngle*2) * 2 - 1;
            mastRotation = Mathf.Clamp(mastRotation, -1, 1);

            for (int i = 0; i < mastsRot.Length; i++) {
                Quaternion desRotation = Quaternion.LerpUnclamped(mastsRot[i].initialRotation, mastsRot[i].maxRotation, mastRotation);
                masts[i].localRotation = Quaternion.Lerp(masts[i].localRotation, desRotation, Time.deltaTime * mastsLerpValue);
            }
        } else {
            mastRotation = 1;
        }
    }

    void SetInitialRotations () {
        if (rudder) rudderRot = new RotationContainer(rudder, rudder.localRotation, rudder.localRotation * Quaternion.Euler(-Vector3.up * maxRudderAngle)); 
        if (steeringWheel) wheelRot = new RotationContainer(steeringWheel, steeringWheel.localRotation, steeringWheel.localRotation * Quaternion.Euler(-Vector3.forward * maxWheelRotationAngle));

        if (masts.Length > 0) {
            mastsRot = new RotationContainer[masts.Length];
            for (int i = 0; i < mastsRot.Length; i++) {
                mastsRot[i] = new RotationContainer(masts[i], masts[i].localRotation, masts[i].localRotation * Quaternion.Euler(-Vector3.up * maxMastRotationAngle));
            }
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
