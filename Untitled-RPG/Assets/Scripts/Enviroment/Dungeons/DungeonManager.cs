using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public Transform playerSpawnPoint;
    public DungeonDoor dungeonDoor;

    public bool isPlayerInsideFightingRoom;

    float delayFromStartup = 2;
    float timeStarted;

    EnemyWaveGenerator waveGenerator;

    void Awake() {
        waveGenerator = GetComponent<EnemyWaveGenerator>();
    }

    IEnumerator Start() {
        dungeonDoor.isOpen = true;
        while (!PlayerControlls.instance) {
            yield return null;
        }
        PlayerAudioController.instance.groundType = GroundType.Stone;
        PlayerControlls.instance.InstantOverridePosRot(playerSpawnPoint);
        timeStarted = Time.time;
    }

    public void LaunchFight() {
        if (waveGenerator.isActive || Time.time - timeStarted < delayFromStartup) return;

        StartCoroutine(LaunchFightDelayed());
    }
    IEnumerator LaunchFightDelayed () {
        yield return new WaitForSeconds (2);
        waveGenerator.StartWaves();
        dungeonDoor.isOpen = false;
    }
}
