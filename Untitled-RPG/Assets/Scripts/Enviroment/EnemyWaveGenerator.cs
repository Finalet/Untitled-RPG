using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum WaveDurationType {ByTotalEnemies, ByTimePassed}

[System.Serializable] public struct WaveEnemy {
    public GameObject enemyPrefab;
    public int simultaiousEnemies;
    [MinMaxSlider(0, 1)] public Vector2 spawnRadius;

    public WaveEnemy(GameObject enemyPrefab, int simultaiousEnemies, Vector2 spawnRadius)
    {
        this.enemyPrefab = enemyPrefab;
        this.simultaiousEnemies = simultaiousEnemies;
        this.spawnRadius = spawnRadius;
    }
}

[System.Serializable] public struct SpawnedEnemy {
    public List<GameObject> spawnedGameObjects;
    public GameObject relatedEnemyPrefab;

    public SpawnedEnemy(List<GameObject> spawnedGameObjects, GameObject relatedEnemyPrefab)
    {
        this.spawnedGameObjects = spawnedGameObjects;
        this.relatedEnemyPrefab = relatedEnemyPrefab;
    }
}

[System.Serializable]
public class EnemyWave {
    [DisplayWithoutEdit] public bool isActive;
    [Space] public WaveDurationType durationType;
    [AllowNesting, ShowIf("durationType", WaveDurationType.ByTimePassed)] public float waveDuration;
    [AllowNesting, ShowIf("durationType", WaveDurationType.ByTotalEnemies)] public int totalNumerOfEnemies;
    public WaveEnemy[] enemies;

    [ReadOnly] public List<SpawnedEnemy> spawnedEnemies;

    float timeWaveStarted;
    int totalSpawnedEnemies;
    
    float lastCleanTime;
    float cleanFrequency = 0.5f;

    bool isDone;

    [System.NonSerialized] public EnemyWaveGenerator waveGenerator;

    public float waveProgress {
        get {
            if (isDone) return 1;
            if (!isActive) return 0;

            if (durationType == WaveDurationType.ByTotalEnemies) {
                return (float)totalSpawnedEnemies / (float)totalNumerOfEnemies;
            } else if (durationType == WaveDurationType.ByTimePassed) {
                return (Time.time - timeWaveStarted) / waveDuration;
            } else {
                throw new System.Exception($"Cannot get wave progress. \"{durationType}\" is not supported");
            }
        }
    }

    public bool isBossWave  {
        get {
            for (int i = 0; i < enemies.Length; i++) {
                if (enemies[i].enemyPrefab.TryGetComponent(out Boss b)) {
                    return true;
                }
            }
            return false;
        }
    }

    public void StartWave () {
        SetupSpawnedEnemiesList();

        waveGenerator.StartCoroutine(Wave());
    }

    IEnumerator Wave () {
        isActive = true;
        timeWaveStarted = Time.time;
        while (!shouldWaveEnd()) {
            for (int i = 0; i < enemies.Length; i++) {
                if (enemies[i].simultaiousEnemies > spawnedEnemies[GetSpawnedEnemyIndex(enemies[i])].spawnedGameObjects.Count) {
                    SpawnEnemy(enemies[i]);
                }
            }
            if (Time.time - lastCleanTime > 1 / cleanFrequency) CleanSpawnedEnemies();
            yield return null;
        }
        while (numberOfActiveEnemies() > 0) {
            if (Time.time - lastCleanTime > 1 / cleanFrequency) CleanSpawnedEnemies();
            yield return null;
        }

        isActive = false;
        isDone = true;
    }

    void SetupSpawnedEnemiesList () {
        spawnedEnemies = new List<SpawnedEnemy>();

        for (int i = 0; i < enemies.Length; i++) {
            spawnedEnemies.Add(new SpawnedEnemy(new List<GameObject>(), enemies[i].enemyPrefab));
        }
    }

    void SpawnEnemy (WaveEnemy enemyToSpawn) {
       Vector3 spawnPos = getSpawnPos(enemyToSpawn); 
       

        for (int i = 0; i < spawnedEnemies.Count; i++) {
            if (spawnedEnemies[i].relatedEnemyPrefab == enemyToSpawn.enemyPrefab) {
                spawnedEnemies[i].spawnedGameObjects.Add(Object.Instantiate(enemyToSpawn.enemyPrefab, spawnPos, getSpawnRot(spawnPos), waveGenerator.transform));
                totalSpawnedEnemies ++;        
                return;
            }
        }

        spawnedEnemies.Add(new SpawnedEnemy(new List<GameObject>(), enemyToSpawn.enemyPrefab));    
        spawnedEnemies[spawnedEnemies.Count-1].spawnedGameObjects.Add(Object.Instantiate(enemyToSpawn.enemyPrefab, spawnPos, getSpawnRot(spawnPos), waveGenerator.transform));
        totalSpawnedEnemies ++; 
    }

    void CleanSpawnedEnemies () {
        for (int i = 0; i < spawnedEnemies.Count; i++) {
            for (int i1 = spawnedEnemies[i].spawnedGameObjects.Count - 1; i1 >= 0; i1--) {
                if (spawnedEnemies[i].spawnedGameObjects[i1].gameObject == null) spawnedEnemies[i].spawnedGameObjects.RemoveAt(i1);
            }
        }
        lastCleanTime = Time.time;
    }

    Vector3 getSpawnPos (WaveEnemy enemyToSpawn) {
        Vector3 spawnPos = waveGenerator.transform.position + waveGenerator.SpawnVolumeCenter;

        if (enemyToSpawn.spawnRadius.x > 0 && enemyToSpawn.spawnRadius.y >= 1) { // Example: 0.2 - 1
            if (Random.value < 0.5f) { //Left-right
                spawnPos.x += (Random.value <= 0.5f ? 1 : -1) * (Random.Range(enemyToSpawn.spawnRadius.x, enemyToSpawn.spawnRadius.y) * waveGenerator.SpawnVolumeSize.x / 2);
                spawnPos.z += (Random.value <= 0.5f ? 1 : -1) * Random.value * waveGenerator.SpawnVolumeSize.z / 2;
            } else { //Top-bottom
                spawnPos.x += (Random.value <= 0.5f ? 1 : -1) * Random.value * waveGenerator.SpawnVolumeSize.x / 2; 
                spawnPos.z += (Random.value <= 0.5f ? 1 : -1) * (Random.Range(enemyToSpawn.spawnRadius.x, enemyToSpawn.spawnRadius.y) * waveGenerator.SpawnVolumeSize.z / 2);
            }
        } else if (enemyToSpawn.spawnRadius.x <= 0 && enemyToSpawn.spawnRadius.y < 1) { // 0 - 0.2
            spawnPos.x += (Random.value <= 0.5f ? 1 : -1) * (Random.Range(enemyToSpawn.spawnRadius.x, enemyToSpawn.spawnRadius.y) * waveGenerator.SpawnVolumeSize.x / 2);
            spawnPos.z += (Random.value <= 0.5f ? 1 : -1) * (Random.Range(enemyToSpawn.spawnRadius.x, enemyToSpawn.spawnRadius.y) * waveGenerator.SpawnVolumeSize.z / 2);
        } else if (enemyToSpawn.spawnRadius.x > 0 && enemyToSpawn.spawnRadius.y < 1) { // Example 0.2 - 0.8
            if (Random.value < 0.5f) { //Left-right
                spawnPos.x += (Random.value <= 0.5f ? 1 : -1) * (Random.Range(enemyToSpawn.spawnRadius.x, enemyToSpawn.spawnRadius.y) * waveGenerator.SpawnVolumeSize.x / 2);
                spawnPos.z += (Random.value <= 0.5f ? 1 : -1) * Random.Range(0, enemyToSpawn.spawnRadius.y) * waveGenerator.SpawnVolumeSize.z / 2;
            } else { //Top-bottom
                spawnPos.x += (Random.value <= 0.5f ? 1 : -1) * Random.Range(0, enemyToSpawn.spawnRadius.y) * waveGenerator.SpawnVolumeSize.x / 2;
                spawnPos.z += (Random.value <= 0.5f ? 1 : -1) * (Random.Range(enemyToSpawn.spawnRadius.x, enemyToSpawn.spawnRadius.y) * waveGenerator.SpawnVolumeSize.z / 2);
            }
        }
        return spawnPos;
    }

    Quaternion getSpawnRot (Vector3 spawnPos) {
        Vector3 target = PlayerControlls.instance ? PlayerControlls.instance.transform.position : waveGenerator.transform.position + waveGenerator.SpawnVolumeCenter;

        Vector3 dir = (target - spawnPos);
        dir.y = 0;
        dir = dir.normalized;

        return Quaternion.LookRotation(dir, Vector3.up);
    }

    int GetSpawnedEnemyIndex (WaveEnemy waveEnemy) {
        for (int i = 0; i < spawnedEnemies.Count; i++) {
            if (spawnedEnemies[i].relatedEnemyPrefab == waveEnemy.enemyPrefab) return i;
        }
        return -1;
    }

    bool shouldWaveEnd () {
        if (!isActive) return false;
        
        switch (durationType)
        {
            case WaveDurationType.ByTotalEnemies: return totalSpawnedEnemies >= totalNumerOfEnemies;
            case WaveDurationType.ByTimePassed: return Time.time - timeWaveStarted >= waveDuration;
            default: throw new System.Exception($"Duration type \"{durationType}\" is not supported");
        }
    }

    int numberOfActiveEnemies () {
        int x = 0;
        for (int i = 0; i < spawnedEnemies.Count; i++) {
            for (int i1 = spawnedEnemies[i].spawnedGameObjects.Count - 1; i1 >= 0; i1--) {
                if (spawnedEnemies[i].spawnedGameObjects[i1].gameObject != null) x++;
            }
        }
        return x;
    }
}

public class EnemyWaveGenerator : MonoBehaviour
{
    public float delayBetweenWaves = 5;
    
    [Space]
    public Vector3 SpawnVolumeSize;
    public Vector3 SpawnVolumeCenter;

    [Space]
    public List<EnemyWave> waves = new List<EnemyWave>();

    [Space, ReadOnly] public int currentWaveIndex;

    [Header("UI")]
    public GameObject waveUIPrefab;

    public float currentWaveProgress {
        get {
            return waves[currentWaveIndex].waveProgress;
        }
    }
    public bool isCurrentWaveBoss {
        get { return waves[currentWaveIndex].isBossWave; }
    }
    public bool isActive {
        get {
            for (int i = 0; i < waves.Count; i++) {
                if (waves[i].isActive) return true;
            }
            return false;
        }
    }

    void Awake() {
        foreach (EnemyWave wave in waves) wave.waveGenerator = this;
    }
    
    public void StartWaves() {
        StartCoroutine(LaunchWaves());
    }

    IEnumerator LaunchWaves () {
        Instantiate(waveUIPrefab, CanvasScript.instance.transform).GetComponent<EnemyWaveUI>().Init(this);

        for (int i = 0; i < waves.Count; i++) {
            
            currentWaveIndex = i;
            waves[currentWaveIndex].StartWave();
            while (waves[currentWaveIndex].isActive) {
                yield return null;
            }
            yield return new WaitForSeconds(delayBetweenWaves);

        }
    }

    #if UNITY_EDITOR
    void OnDrawGizmos() {
        Handles.DrawWireCube(transform.position + SpawnVolumeCenter, SpawnVolumeSize);
    }
    #endif
}
