using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Crest;

public class Ship : MonoBehaviour
{
    public bool playerControlling;

    [Header("Collider Alignment")]
    public Transform shipModel;
    public Transform shipCollider;
    public Collider crestCollider;
    public BoatProbes crestShip;

    CinemachineFreeLook CM_Cam;
    ShipWheelTrigger wheelTrigger;


    void Start() {
        wheelTrigger = crestShip.GetComponentInChildren<ShipWheelTrigger>();
        CM_Cam = GetComponentInChildren<CinemachineFreeLook>();
        
        InitializeColliders (); //Disable all collisions between the ocean physics collider and player collider
    }

    void Update() {
        if (wheelTrigger.playerOnTrigger && Input.GetKeyDown(KeyCode.F))  {
            if (!playerControlling)
                TakeControl();
            else 
                GiveUpControl();
        }
    }

    void TakeControl() {
        playerControlling = true;
        CM_Cam.Priority = 2;
        PlayerControlls.instance.enabled = false;
        StartCoroutine(LerpPlayerToWheel());
        crestShip._playerControlled = true;
        crestShip.GetComponent<ShipAnchor>().enabled = false;
    }

    void GiveUpControl() {
        playerControlling = false;
        CM_Cam.Priority = 0;
        PlayerControlls.instance.enabled = true;
        crestShip._playerControlled = false;
    }

    IEnumerator LerpPlayerToWheel() {
        PlayerControlls.instance.GetComponent<CharacterController>().enabled = false; //It overrides any changes to transform.position that I make manually
        while (Vector3.Distance(PlayerControlls.instance.transform.position, wheelTrigger.transform.position) >= 0.1f) {
            PlayerControlls.instance.transform.position = Vector3.MoveTowards(PlayerControlls.instance.transform.position, wheelTrigger.transform.position, 10 * Time.deltaTime);
            yield return null;
        }
        PlayerControlls.instance.GetComponent<CharacterController>().enabled = true;
    }

    void InitializeColliders() {
        Collider[] allColliders = shipCollider.GetComponentsInChildren<Collider>();
        for (int i = 0; i < allColliders.Length; i++) {
            Physics.IgnoreCollision(crestCollider, allColliders[i], true);
            if (allColliders[i].tag != "Ship") //Don't forget to set the tag for player detection (should have been done manually)
                allColliders[i].tag = "Ship";
        }
    }

    void LateUpdate() {
        //Set the collider to the rigidbody transform
        shipCollider.transform.position = shipModel.position;
        shipCollider.transform.rotation = shipModel.rotation;
        shipCollider.transform.localScale = shipModel.localScale;
    }
}
