using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof (ShipController))]
public class ShipAttachements : MonoBehaviour
{
    ShipController shipController;
    [Header("Cannons")]
    public float cannonsSequenceDelays = 0.15f;
    public float cannonsSequenceVariation = 0.3f;
    public float cannonPower = 300;

    public Transform[] cannonPositions;
    public GameObject cannonDecoration;
    public GameObject cannonPrefab;
    public GameObject cannonballPrefab;

    public List<ShipCannon> allCannons = new List<ShipCannon>();
    public List<ShipCannon> leftCannons = new List<ShipCannon>();
    public List<ShipCannon> rightCannons = new List<ShipCannon>();

    public bool areCannonsInstalled {
        get { return allCannons.Count > 0; }
    }

    void Awake() {
        shipController = GetComponent<ShipController>();
    }

    void Start() {
        AddCannons();
    }

    [Button("Add cannons")]
    public void AddCannons () {
        RemoveCannons();

        Transform cannonsParent = shipController.deckCollider.transform.Find("Cannons");
        cannonsParent = cannonsParent == null ? new GameObject().transform : cannonsParent;
        cannonsParent.name = "Cannons";
        cannonsParent.SetParent(shipController.deckCollider.transform);

        for (int i = 0; i < cannonPositions.Length; i ++) {
            ShipCannon c = Instantiate(cannonPrefab, cannonPositions[i]).GetComponent<ShipCannon>();
            allCannons.Add(c);
            c.playerCollider.SetParent(cannonsParent);
            if (Vector3.SignedAngle(transform.forward, cannonPositions[i].forward, transform.up) > 0) rightCannons.Add(c);
            else leftCannons.Add(c);
        }
        if (cannonDecoration) cannonDecoration.SetActive(true);
    }
    [Button("Remove cannons")]
    public void RemoveCannons () {
        for (int i = 0; i < cannonPositions.Length; i ++) {
            if (cannonPositions[i].childCount > 0) {
                for (int i1=cannonPositions[i].childCount-1; i1>=0; i1--) {
                        #if UNITY_EDITOR
                        DestroyImmediate(cannonPositions[i].GetChild(i1).gameObject);
                        #else
                        Destroy(cannonPositions[i].GetChild(i1).gameObject);
                        #endif
                }
            }
        }
        allCannons.Clear();
        leftCannons.Clear();
        rightCannons.Clear();
        if (cannonDecoration) cannonDecoration.SetActive(false);
    }

    public void ShootAll () {
        StartCoroutine(shootAll());
    }

    IEnumerator shootAll () {
        List<ShipCannon> cannonsToUse = Vector3.SignedAngle(transform.forward, PlayerControlls.instance.playerCamera.transform.forward, Vector3.up) > 0 ? rightCannons : leftCannons;
        int[] randomOrderIndecies = new int[cannonsToUse.Count];
        
        for (int i = 0; i < randomOrderIndecies.Length; i++) {
            randomOrderIndecies[i] = i;
        }
        
        int tempInt;
        for (int i = 0; i < randomOrderIndecies.Length; i++) {
            int rnd = Random.Range(0, randomOrderIndecies.Length);
            tempInt = randomOrderIndecies[rnd];
            randomOrderIndecies[rnd] = randomOrderIndecies[i];
            randomOrderIndecies[i] = tempInt;
        }
        
        for (int i = 0; i < cannonsToUse.Count; i++) {
            cannonsToUse[randomOrderIndecies[i]].Shoot(cannonPower);
            yield return new WaitForSeconds (Random.Range(cannonsSequenceDelays * (1-cannonsSequenceVariation), cannonsSequenceDelays * (1+cannonsSequenceVariation)));
        }
    }
}
