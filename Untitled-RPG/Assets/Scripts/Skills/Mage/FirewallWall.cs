using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirewallWall : MonoBehaviour
{
    public float duration;
    public int actualDamage;

    List<Enemy> enemiesHit = new List<Enemy>();
    void Start() {
        Destroy(gameObject, duration + 0.5f);
        
        var main = transform.GetChild(0).GetComponent<ParticleSystem>().main;
        main.startLifetime = duration;
        main = transform.GetChild(0).GetChild(1).GetComponent<ParticleSystem>().main;
        main.startLifetime = duration;
    }

    void Update() {
        if (GetComponent<BoxCollider>().size.x <= 10.5f) {
            GetComponent<BoxCollider>().center += Vector3.right * Time.deltaTime*9f;
            GetComponent<BoxCollider>().size += Vector3.right * Time.deltaTime*18f;
        }
    }

    void OnTriggerStay(Collider other) {
        if (other.isTrigger || other.CompareTag("Player"))
            return;
        
        if (other.gameObject.GetComponent<Enemy>() != null) {
            if (!enemiesHit.Contains(other.GetComponent<Enemy>())) {
                other.GetComponent<Enemy>().GetHit(damage());
                StartCoroutine(List(other.GetComponent<Enemy>()));
            }
        }
    }

    IEnumerator List (Enemy enemy) {
        enemiesHit.Add(enemy);
        yield return new WaitForSeconds(1);
        enemiesHit.Remove(enemy);
    }

    int damage () {
        return Mathf.RoundToInt(Random.Range(actualDamage*0.85f, actualDamage*1.15f));
    }   
}
