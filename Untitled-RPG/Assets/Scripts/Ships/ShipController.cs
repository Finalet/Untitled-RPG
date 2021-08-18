using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crest;
using Cinemachine;

public class ShipController : MonoBehaviour
{
    [Header("Ships stats")]
    public string shipName;
    public float baseSpeed;
    public float baseTurnPower;
    [DisplayWithoutEdit, SerializeField] int currentSpeedIndex; int maxSpeedIndex = 2;
    float speedRatio {
        get {
            return (float)currentSpeedIndex / (float)maxSpeedIndex;
        }
    }


    [Header("Visuals")]
    public float sailsStretchStrength;
    Vector3 sailsStretchVector = new Vector3(1, 0, 0);

    [Header("Misc")]
    public GameObject deckMesh;
    public DeckCollider deckCollider;

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
    }

    void Update() {
        HandleInput();
        SyncShip();

        CM_cam.Priority = (currentSpeedIndex > 0) ? 50 : 0;
    }

    void HandleInput () {
        if (Input.GetKeyDown(increaseSpeedIndex)) IncreaseSpeed();
        else if (Input.GetKeyDown(decreaseSpeedIndex)) DecreaseSpeed();

        boatProbe._turnBias = currentSpeedIndex == 0 ? 0 : Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0;
    }

    void IncreaseSpeed () {
        currentSpeedIndex ++;
        currentSpeedIndex = Mathf.Clamp(currentSpeedIndex, 0, maxSpeedIndex);
    }

    void DecreaseSpeed () {
        currentSpeedIndex --;
        currentSpeedIndex = Mathf.Clamp(currentSpeedIndex, 0, maxSpeedIndex);
    }

    void SyncShip () {
        boatProbe._engineBias = speedRatio;
        for (int i = 0; i < sails.Length; i++) {
            sails[i].externalAcceleration = Vector3.MoveTowards(sails[i].externalAcceleration, sailsStretchVector * sailsStretchStrength * speedRatio, 30 * Time.deltaTime);
        }
    }

    void InitializeColliderDummy() {
        deckCollider = new GameObject().AddComponent<DeckCollider>();
        deckCollider.Init(this);
    }
}
