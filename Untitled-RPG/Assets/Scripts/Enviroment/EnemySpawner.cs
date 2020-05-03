using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy;
    public int numberOfEnemies;
    float delay = 2;
    
    public List<GameObject> listOfAllEnemies;
    public ParticleSystem particles;

    void Start() {
        listOfAllEnemies = new List<GameObject>();
    }

    void Update() {
        if (listOfAllEnemies.Count < numberOfEnemies && delay <= 0) {
            Spawn();
            delay = 2;
        } else {
            delay -= Time.deltaTime;
        }
    } 

    void Spawn () {
        Vector3 spawnPos = new Vector3(transform.position.x + Random.Range(-5f, 5f), transform.position.y, transform.position.z + Random.Range(-5f, 5f) );
        GameObject en = Instantiate(enemy, spawnPos, Quaternion.identity);
        en.GetComponent<Enemy>().spawner = transform;
        listOfAllEnemies.Add(en);
        particles.transform.position = spawnPos;
        particles.Play();
    }
}
