using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations;

public struct PetSpawnPosRot {
    public Vector3 pos;
    public Quaternion rot;

    public PetSpawnPosRot (Vector3 _pos, Quaternion _rot) {
        this.pos = _pos;
        this.rot = _rot;
    }
}

public class PetsManager : MonoBehaviour
{
    public static PetsManager instance;
    
    public LayerMask goundLayers;
    public ParticleSystem petDisapparParticles;
    public GameObject mountKeySuggestionsPrefab; MountKeySuggestionsUI instanciatedKeySuggestions;

    public bool isMountOut {
        get {
            if (PlayerControlls.instance.rider.m_MountStored == null || PlayerControlls.instance.rider.m_MountStored.Value == null) return false;
            
            return !PlayerControlls.instance.rider.m_MountStored.Value.IsPrefab();
        }
    }
    public MountController currentMountController {
        get {
            if (!isMountOut) return null;

            return PlayerControlls.instance.rider.m_MountStored.Value.GetComponent<MountController>();
        }
    }

    void Awake() {
        instance = this;
    }

    void Update() {
        if (Input.GetKeyDown(KeybindsManager.instance.currentKeyBinds["Call mount"])) {
            CallMount();
        }

        DisplayKeySuggestions();
    }

    public void CallMount () {
        PlayerControlls.instance.rider.CallAnimal(true);
    }

    public void UncallMount (){
        if (isMountOut) Destroy(Instantiate(petDisapparParticles, PlayerControlls.instance.rider.m_MountStored.Value.transform.position + Vector3.up, Quaternion.identity).gameObject, 2);
        if (isMountOut) Destroy(PlayerControlls.instance.rider.m_MountStored.Value);
        
        Mount m = EquipmentManager.instance.getMountItem();
        if (m != null) PlayerControlls.instance.rider.SetStoredMount(m.mountPrefab);
        else PlayerControlls.instance.rider.ClearStoredMount();
    }

    void DisplayKeySuggestions() {
        if (PlayerControlls.instance.isMounted) {
            if (!instanciatedKeySuggestions) {
                instanciatedKeySuggestions = Instantiate(mountKeySuggestionsPrefab, PeaceCanvas.instance.transform).GetComponent<MountKeySuggestionsUI>();
                instanciatedKeySuggestions.transform.SetAsFirstSibling();
            }
        } else {
            if (instanciatedKeySuggestions) Destroy(instanciatedKeySuggestions.gameObject);
        }
    }

    public void UpdateKeySuggestions () {
        if (instanciatedKeySuggestions) instanciatedKeySuggestions.UpdateKeySuggestions();
    }

    public PetSpawnPosRot getSpawnPosAndRot () {
        Vector3 spawnPos;
        
        Vector3 backDirection = (PlayerControlls.instance.playerCamera.transform.position - PlayerControlls.instance.transform.position);
        backDirection.y = 0;
        Vector3 raycastPos = PlayerControlls.instance.transform.position + backDirection.normalized * 10 + Vector3.up * 5;
        RaycastHit hit;
        if (Physics.Raycast(raycastPos, Vector3.down, out hit, 20, goundLayers)) {
            spawnPos = hit.point;
        } else {
            spawnPos = raycastPos - Vector3.up*5;
        }

        Vector3 lookPos = PlayerControlls.instance.playerCamera.transform.position - spawnPos;
        lookPos.y = 0;

        return new PetSpawnPosRot(spawnPos, Quaternion.LookRotation(lookPos));
    }
}
