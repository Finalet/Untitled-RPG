using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CampEnemyInfo {
    public GameObject enemyPrefab;
    public Vector3 position;
    public Quaternion rotation;

    public CampEnemyInfo (GameObject _enemyPrefab, Vector3 _position, Quaternion _rotation){
        enemyPrefab = _enemyPrefab;
        position = _position;
        rotation = _rotation;
    }
}

public class GoblinCamp : MonoBehaviour
{
    public List<CampEnemyInfo> campEnemies = new List<CampEnemyInfo>();
    public List<GameObject> spawnedEnemies = new List<GameObject>();
    public Transform enemies;

    float checkTimer = 20;
    float respawnDelay = 60;
    float lastCheck;
    bool waitingForRespawn;

    void Start() {
        checkEnemies(0);
    }

    void Update() {
        if (waitingForRespawn)
            return;
            
        if (Time.time - lastCheck > checkTimer) {
            lastCheck = Time.time;
            checkEnemies(respawnDelay);
        }
    }

    void checkEnemies (float _respawnDelay) {
        for (int i = 0; i < spawnedEnemies.Count; i++){
            if (spawnedEnemies[i] != null){
                return;
            }
        }
        waitingForRespawn = true;
        Invoke("RespawnAllEnemies", _respawnDelay);
    }

    void RespawnAllEnemies (){
        spawnedEnemies.Clear();
        foreach (CampEnemyInfo cei in campEnemies) {
            spawnedEnemies.Add(Instantiate(cei.enemyPrefab, cei.position, cei.rotation, enemies));
        }
        waitingForRespawn = false;
    }
}
