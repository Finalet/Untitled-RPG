using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] RandomLoot rewards;
    [SerializeField] ChestQuality chestQuality;

    [Header("Setup")]
    [SerializeField] Transform playerSpawnPoint;
    [SerializeField] DungeonDoor dungeonEntranceDoor;
    [SerializeField] DungeonDoor[] dungeonExitDoors;
    [SerializeField] Chest rewardChest;

    float delayFromStartup = 2;
    float timeStarted;

    EnemyWaveGenerator waveGenerator;

    void Awake() {
        waveGenerator = GetComponent<EnemyWaveGenerator>();
    }

    IEnumerator Start() {
        ToggleAllDoors(true);
        rewardChest.isLocked = true;
        while (!PlayerControlls.instance) {
            yield return null;
        }
        PlayerAudioController.instance.groundType = GroundType.Stone;
        PlayerControlls.instance.InstantOverridePosRot(playerSpawnPoint);
        timeStarted = Time.time;
    }

    public void LaunchFight() {
        if (waveGenerator.isDone || waveGenerator.isActive || Time.time - timeStarted < delayFromStartup) return;

        StartCoroutine(LaunchFightDelayed());
    }
    IEnumerator LaunchFightDelayed () {
        yield return new WaitForSeconds (2);
        waveGenerator.StartWaves();
        ToggleAllDoors(false);
        waveGenerator.OnAllWavesDone += EndFight;
    }

    void EndFight () {
        ToggleAllDoors(true);
        AddRewards();
        rewardChest.isLocked = false;
    }
    
    void AddRewards () {
        rewardChest.ResetContainer();
        rewardChest.defaultItemsInside.AddRange(rewards.GetLoot());
    }

    void ToggleAllDoors (bool open) {
        dungeonEntranceDoor.isOpen = open;
        foreach (DungeonDoor door in dungeonExitDoors) door.isOpen = open;
    }

    void OnValidate() {
        rewards.OnValidate();
        if (rewardChest) {
            rewardChest.chestQuality = chestQuality;
            rewardChest.UpdateMesh();
        }
    }
}
