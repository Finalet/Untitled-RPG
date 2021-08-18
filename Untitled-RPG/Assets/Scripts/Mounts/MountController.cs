using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Controller;
using MalbersAnimations;
using UnityEngine.AI;
using MalbersAnimations.Utilities;


public class MountController : MonoBehaviour
{
    [DisplayWithoutEdit] public float currentStamina;
    [Header("Base stats")]
    [DisplayWithoutEdit] [SerializeField] float BaseMovementSpeed = 10;
    [DisplayWithoutEdit] [SerializeField] float BaseMaxStamina = 100;
    [Header("Equipment stats")]
    [DisplayWithoutEdit] [SerializeField] float movementSpeedFromEquip = 0;
    [DisplayWithoutEdit] [SerializeField] float maxStaminaFromEquip = 0;
    [Header("Total stats")]
    [DisplayWithoutEdit] public float totalMovementSpeed = 0;
    [DisplayWithoutEdit] public float totalMaxStamina = 0;
    [Space]
    public float staminaUseThreashold = 25;
    [Space]
    public float staminaUsePerSecond = 20;
    public float staminaRegeneratePerSecond = 15;

    MAnimal animal;
    ActiveMeshes animalMeshes;
    MaterialChanger animalMaterials;
    MalbersInput animalInputs;
    NavMeshSurface navMeshSurface;

    bool calledEventStaminaZero;
    bool calledEventStaminaAboveThreashold;

    bool hidingStamina;
    float timer;

    float navMeshArea = 50;

    public float speedAnimModifier {
        get {
            return 0.04f * totalMovementSpeed + 0.40f;
        }
    }
    
    public void Init(Mount mountItem) {
        animal = GetComponent<MAnimal>();
        animalMeshes = GetComponentInChildren<ActiveMeshes>();
        animalMaterials = GetComponentInChildren<MaterialChanger>();
        animalInputs = GetComponent<MalbersInput>();

        BaseMovementSpeed = mountItem.movementSpeed;
        BaseMaxStamina = mountItem.maxStamina;

        ResetAllStats();
        AddAllStats();
        CalculateTotalStats();

        currentStamina = totalMaxStamina;

        InitNavMeshSurface();

        UpdateInputs();
        UpdateVisuals();
    }

    public void UpdateInputs () {
        animalInputs.FindInput("Jump").key = KeybindsManager.instance.currentKeyBinds["Jump"];
        animalInputs.FindInput("Sprint").key = KeybindsManager.instance.currentKeyBinds["Run"];
        animalInputs.FindInput("Speed ++").key = KeybindsManager.instance.currentKeyBinds["Increase Mount Speed"];
        animalInputs.FindInput("Speed --").key = KeybindsManager.instance.currentKeyBinds["Decrease Mount Speed"];
        animalInputs.FindInput("Strafe").key = KeybindsManager.instance.currentKeyBinds["Toggle Mount Strafe"];

        if (animalInputs.FindInput("Fly") != null) animalInputs.FindInput("Fly").key = KeybindsManager.instance.currentKeyBinds["Toggle Mount Flight"];
    }

    void Update() {
        if (animal.Sprint) DepleteStamina();
        else RegenerateStamina();
        
        currentStamina = Mathf.Clamp(currentStamina, 0, totalMaxStamina);

        if (currentStamina <= 0) {
            OnStaminaZero();
        } else if (currentStamina >= staminaUseThreashold) {
            OnStaminaAboveThreashold();
        }

        DisplayStamina();
        
        CheckNavMesh();
    }

    void DisplayStamina () {
        if (currentStamina == totalMaxStamina) {
            if (!hidingStamina){
                hidingStamina = true;
                timer = Time.time;
            }
            if (Time.time - timer >= 1f) {
                if (PlayerControlls.instance.isMounted) CanvasScript.instance.HideStamina();
            }
            return;
        }

        if (PlayerControlls.instance.isMounted) CanvasScript.instance.ShowStamina();
        hidingStamina = false;
    }

    void OnStaminaZero () {
        if (calledEventStaminaZero) return;

        calledEventStaminaZero = true;
        calledEventStaminaAboveThreashold = false;

        animal.UseSprint = false;
    }
    void OnStaminaAboveThreashold() {
        if (calledEventStaminaAboveThreashold) return;

        calledEventStaminaZero = false;
        calledEventStaminaAboveThreashold = true;

        animal.UseSprint = true;
    }

    void DepleteStamina() {
        currentStamina -= staminaUsePerSecond * Time.deltaTime;
    }
    void RegenerateStamina () {
        currentStamina += staminaRegeneratePerSecond * Time.deltaTime;
    }

#region Visuals
    public void UpdateVisuals() {
        Item mountEquipmentItem;
        if (EquipmentManager.instance.isSlotEquiped(MountEquipmentType.Saddle, out mountEquipmentItem)) {
            EquipVisual((MountEquipment)mountEquipmentItem);
        } else {
            UnequipVisual("Saddle");
        }
        if (EquipmentManager.instance.isSlotEquiped(MountEquipmentType.Armor, out mountEquipmentItem)) {
            EquipVisual((MountEquipment)mountEquipmentItem);
        } else {
            UnequipVisual("Armor");
        }
    }

    public void EquipVisual (MountEquipment item) {
        animalMeshes.ChangeMesh(item.equipmentType.ToString(), 0); //0 contains the mesh

        Material newMat = new Material(animalMaterials.getItemMaterial(item.equipmentType.ToString()));
        newMat.name = $"Runtime {item.equipmentType.ToString()} Material";
        if (item.equipmentType == MountEquipmentType.Saddle){
            newMat.SetColor("_Color1", item.Color1Reins);
            newMat.SetColor("_Color2", item.Color2Seat);
            newMat.SetColor("_Color3", item.Color3UnderSeat);
            newMat.SetColor("_Color4", item.Color4Clasps);
            newMat.SetColor("_Color5", item.Color5SeatClothBottom);
            newMat.SetColor("_Color6", item.Color6SeatClothTop);
            newMat.SetColor("_Color7", item.Color7SeatSides);
            newMat.SetColor("_Color8", item.Color8FeetHolder);
            newMat.SetColor("_Color9", item.Color9Pouches);
            newMat.SetColor("_Color10", item.Color10LegStraps);
            newMat.SetColor("_Color11", item.Color11HorseStraps);
        } else if (item.equipmentType == MountEquipmentType.Armor) {
            newMat.SetColor("_Color12", item.Color12ArmorUnder);
            newMat.SetColor("_Color13", item.Color13ArmorOutline);
            newMat.SetColor("_Color14", item.Color14ArmorColor);
            newMat.SetColor("_Color15", item.Color15BodyCloth);
            newMat.SetColor("_Color16", item.Color16MetalButtons);
        }

        animalMaterials.SetItemMaterial(item.equipmentType.ToString(), newMat);
    }
    public void UnequipVisual(string item) {
        animalMeshes.ChangeMesh(item, 1); //1 is no mesh. I know, Malbert anim is weird

        if (item == "Saddle") { //If its a saddle, reset the reins color to defaul;
            animalMeshes.GetActiveMesh("Reins").GetCurrentActiveMesh().GetComponent<Renderer>().sharedMaterial.SetColor("_Color1", new Color(150f/255f, 105f/255f, 88f/255f));
        }
    }

#endregion

#region Stats

    void FixedUpdate() {
        ResetAllStats();
        AddAllStats();
        CalculateTotalStats();
    }
    void ResetAllStats () {
        movementSpeedFromEquip = 0;
        maxStaminaFromEquip = 0;
    }
    void AddAllStats () {
        if (EquipmentManager.instance.mount.saddleSlot.itemInSlot != null) AddStats((MountEquipment)EquipmentManager.instance.mount.saddleSlot.itemInSlot);
        if (EquipmentManager.instance.mount.armorSlot.itemInSlot != null) AddStats((MountEquipment)EquipmentManager.instance.mount.armorSlot.itemInSlot);
    }
    void AddStats (MountEquipment item) {
        movementSpeedFromEquip += item.movementSpeed;
        maxStaminaFromEquip += item.stamina;
    }
    void CalculateTotalStats (){
        totalMovementSpeed = BaseMovementSpeed + movementSpeedFromEquip;
        totalMaxStamina = BaseMaxStamina + maxStaminaFromEquip;
    }

#endregion

#region NavMesh Stuff

    void CheckNavMesh () {
        if (PlayerControlls.instance.isMounted) return;

        if (Vector3.Distance(transform.position, navMeshSurface.transform.position) > navMeshArea * 0.3f) {
            RebuildNavMesh();
        }
    }

    void InitNavMeshSurface () {
        navMeshSurface = new GameObject().AddComponent<NavMeshSurface>();
        navMeshSurface.gameObject.name = name + " NavMeshSurface";
        navMeshSurface.transform.position = transform.position;
        navMeshSurface.collectObjects = CollectObjects.Volume;
        navMeshSurface.size = Vector3.one * navMeshArea;
        navMeshSurface.center = Vector3.zero;
        navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        navMeshSurface.layerMask = LayerMask.GetMask("Default", "StaticLevel", "Terrain");
        navMeshSurface.BuildNavMesh();
    }
    void RebuildNavMesh() {
        navMeshSurface.RemoveData();
        navMeshSurface.transform.position = transform.position;
        navMeshSurface.BuildNavMesh();
    }

    void OnDestroy() {
        if (navMeshSurface.gameObject) Destroy(navMeshSurface.gameObject);
    }

#endregion
}
