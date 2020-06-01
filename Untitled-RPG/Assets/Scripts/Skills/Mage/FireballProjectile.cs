using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FireballProjectile : MonoBehaviour
{
    public float distance;
    public int actualDamage;

    public bool doNotDestroy;

    public ParticleSystem fire;
    public ParticleSystem explostionSparks;
    public ParticleSystem emptySparks;

    Vector3 begPos;
    bool done;

    void Start() {
        if (doNotDestroy)
            return;

        GetComponent<MeshRenderer>().enabled = true;
        fire.GetComponent<ParticleSystem>().Play();
        begPos = transform.position;
    }

    void Update() {
        if (doNotDestroy)
            return;

        if (Vector3.Distance(transform.position, begPos) >= distance && !done) {
            done = true;
            Explode(false);
        }

        transform.Rotate(randomRotation() * Time.deltaTime, randomRotation() * Time.deltaTime, randomRotation() * Time.deltaTime);
    }

    int damage () {
        return Mathf.RoundToInt(Random.Range(actualDamage*0.85f, actualDamage*1.15f));
    }    

    int randomRotation () {
        int rand = Random.Range(150, 250);
        return rand;
    }

    void OnTriggerEnter(Collider other) {
        if (other.isTrigger || other.CompareTag("Player") || doNotDestroy)
            return;
        
        if (other.gameObject.GetComponent<Enemy>() != null) {
            other.GetComponent<Enemy>().GetHit(damage(), false, false);
        }
        Explode(true);
    }

    void Explode (bool hit) {
        if (hit)
            explostionSparks.Play();
        else 
            emptySparks.Play();
        fire.Stop();
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.12f, 0.12f, 0);
        Destroy(gameObject,0.51f);
    }

}
