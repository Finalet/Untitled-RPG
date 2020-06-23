using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningProjectile : MonoBehaviour
{
    public int actualDamage;
    public AudioClip[] sounds;
    public int shots;

    void Start() {
        Invoke("EnableCollider", 0.1f);
        Destroy(gameObject, 1f);
    }

    int damage () {
        return Mathf.RoundToInt(Random.Range(actualDamage*0.85f, actualDamage*1.15f));
    }  

    void OnTriggerEnter(Collider other) {
        if (other.isTrigger || other.CompareTag("Player"))
            return;
        
        if (other.gameObject.GetComponent<Enemy>() != null) {
            other.GetComponent<Enemy>().GetHit(damage(), false, true, transform.position, "Lightning");
        }
    }

    void EnableCollider () {
        GetComponent<SphereCollider>().enabled = true;
    }
}
