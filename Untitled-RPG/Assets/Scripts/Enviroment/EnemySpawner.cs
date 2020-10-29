using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy;
    public int numberOfEnemies;
    public float spawnFrequency = 2;
    float timer;
    
    public List<GameObject> listOfAllEnemies;
    public ParticleSystem particles;

    void Start() {
        listOfAllEnemies = new List<GameObject>();
    }

    void Update() {
        CleanList();

        if (listOfAllEnemies.Count < numberOfEnemies && timer <= 0) {
            Spawn();
            timer = 1/spawnFrequency;
        } else {
            timer -= Time.deltaTime;
        }
    } 

    void Spawn () {
        Vector3 spawnPos = new Vector3(transform.position.x + Random.Range(-5f, 5f), transform.position.y, transform.position.z + Random.Range(-5f, 5f) );
        Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360),0);
        GameObject en = Instantiate(enemy, spawnPos, rotation);
        en.transform.SetParent(transform);
        listOfAllEnemies.Add(en);
        particles.transform.position = spawnPos;
        particles.Play();
    }

    void CleanList () {
        for (int i = 0; i < listOfAllEnemies.Count; i++) {
            if (listOfAllEnemies[i] == null)
                listOfAllEnemies.Remove(listOfAllEnemies[i]);
        }
    }
}
