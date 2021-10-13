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
    public GameObject cannonballPrefab;
    GameObject cannonPrefab;

    [Header("Setup")]
    public List<ShipCannon> allCannons = new List<ShipCannon>();
    public List<ShipCannon> leftCannons = new List<ShipCannon>();
    public List<ShipCannon> rightCannons = new List<ShipCannon>();
    [Space]
    [SerializeField] GameObject ShipAttachementsUI;
    ShipAttachementUI instanciatedUI;
    UI_ShipAttachementSlot cannonsSlot => instanciatedUI.cannonsSlot;
    UI_ShipAttachementSlot sailsSlot => instanciatedUI.sailsSlot;
    UI_ShipAttachementSlot helmSlot => instanciatedUI.helmSlot;
    UI_ShipAttachementSlot flagSlot => instanciatedUI.flagSlot;

    public bool areCannonsInstalled {
        get { return allCannons.Count > 0; }
    }
    
    public bool attachementUIOpen {
        get { return instanciatedUI.gameObject.activeInHierarchy; }
    }


    void Awake() {
        shipController = GetComponent<ShipController>();
    }

    void Start() {
        InitUI();
    }

    void AddCannonsVisuals () {
        RemoveCannonsVisuals();

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
    void RemoveCannonsVisuals () {
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

    void InitUI (){
        if (!instanciatedUI) instanciatedUI = Instantiate(ShipAttachementsUI, PeaceCanvas.instance.transform).GetComponent<ShipAttachementUI>();
        instanciatedUI.gameObject.SetActive(false);
    }

    public void OpenUI (){
        instanciatedUI.Open();
    }
    public void CloseUI () {
        instanciatedUI.Close();
    }

    public void AddAttachement(ShipAttachement attachement, UI_InventorySlot initialSlot) {
        switch (attachement.attachementType) {
            case ShipAttachementType.Cannons: 
                AddCannons(attachement, initialSlot);
                break;
            case ShipAttachementType.Sails: 
                sailsSlot.AddItem(attachement as Item, 1, initialSlot);
                break;
            case ShipAttachementType.Helm: 
                helmSlot.AddItem(attachement as Item, 1, initialSlot);
                break;
            case ShipAttachementType.Flag: 
                flagSlot.AddItem(attachement as Item, 1, initialSlot);
                break;
        }
    }

    public void RemoveAttachement (ShipAttachementType attachementType) {
        switch (attachementType) {
            case ShipAttachementType.Cannons: 
                sailsSlot.ClearSlot();
                RemoveCannonsVisuals();
                break;
            case ShipAttachementType.Sails: 
                sailsSlot.ClearSlot();
                break;
            case ShipAttachementType.Helm: 
                sailsSlot.ClearSlot();
                break;
            case ShipAttachementType.Flag: 
                sailsSlot.ClearSlot();
                break;
        }
    }

    void AddCannons (ShipAttachement attachement, UI_InventorySlot initialSlot) {
        cannonsSlot.AddItem(attachement as Item, 1, initialSlot);
        cannonPrefab = attachement.prefab;
        AddCannonsVisuals();
    }

    public bool isSlotEquiped (ShipAttachementType attachementType, out Item equipedItem) {
        equipedItem = null;
        if (attachementType == ShipAttachementType.Cannons) {
            equipedItem = cannonsSlot.itemInSlot;
            return cannonsSlot.itemInSlot;
        } else if (attachementType == ShipAttachementType.Sails) {
            equipedItem = sailsSlot.itemInSlot;
            return sailsSlot.itemInSlot;
        } else if (attachementType == ShipAttachementType.Helm) {
            equipedItem = helmSlot.itemInSlot;
            return helmSlot.itemInSlot;
        } else if (attachementType == ShipAttachementType.Flag) {
            equipedItem = flagSlot.itemInSlot;
            return flagSlot.itemInSlot;
        }
        throw new System.Exception("Cannont check if ship slot is equipped. Wrong attachement type \"{attachementType}\".");
    }
}
