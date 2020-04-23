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
        GameObject en = Instantiate(enemy, transform.position, Quaternion.identity);
        en.GetComponent<Enemy>().spawner = transform;
        listOfAllEnemies.Add(en);
        particles.Play();
    }
}
