using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FireballProjectile : MonoBehaviour
{
    public float distance;
    public int actualDamage;

    public bool doNotDestroy;

    Vector3 begPos;
    bool done;
    void Start() {
        if (doNotDestroy)
            return;

        begPos = transform.position;
        GetComponent<VisualEffect>().enabled = true;
    }

    void Update() {
        if (doNotDestroy)
            return;

        if (Vector3.Distance(transform.position, begPos) >= distance && !done) {
            done = true;
            Explode();
        }

        transform.rotation = Quaternion.LookRotation(GetComponent<Rigidbody>().velocity);
    }

    int damage () {
        return Mathf.RoundToInt(Random.Range(actualDamage*0.85f, actualDamage*1.15f));
    }    

    void OnTriggerEnter(Collider other) {
        if (other.isTrigger || other.CompareTag("Player") || doNotDestroy)
            return;
        
        if (other.gameObject.GetComponent<Enemy>() != null) {
            other.GetComponent<Enemy>().GetHit(damage(), false);
        }
        Explode();
    }

    void Explode () {
        //Play explostion
        Destroy(gameObject);
    }
}
