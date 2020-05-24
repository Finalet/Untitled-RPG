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
        Vector3 spawnPos = new Vector3(transform.position.x + Random.Range(-10f, 10f), transform.position.y, transform.position.z + Random.Range(-10f, 10f) );
        Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360),0);
        GameObject en = Instantiate(enemy, spawnPos, rotation);
        en.GetComponent<Enemy>().spawner = transform;
        listOfAllEnemies.Add(en);
        particles.transform.position = spawnPos;
        particles.Play();
    }
}
