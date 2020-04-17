using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy;
    public int numberOfEnemies;
    float delay = 2;
    
    List<GameObject> listOfAllEnemies;
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


        foreach(GameObject go in listOfAllEnemies) {
            if(go == null)
                listOfAllEnemies.Remove(go);
        }
    } 

    void Spawn () {
        GameObject en = Instantiate(enemy, transform.position, Quaternion.identity);
        en.GetComponent<Enemy>().idlePosition = transform;
        listOfAllEnemies.Add(en);
        particles.Play();
    }
}
