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
            StartCoroutine(Disappear());
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
            other.GetComponent<Enemy>().GetHit(damage(), true, false, transform.position);
        }
        Explode();
    }

    void Explode () {
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.12f, 0.12f, 0);
        explostionSparks.Play();
        fire.Stop();
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        Destroy(gameObject,0.51f);
    }

    IEnumerator Disappear() {
        fire.Stop();
        GetComponent<Collider>().enabled = false;
        while (transform.localScale.x > 0) {
            transform.localScale -= Vector3.one * Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

}
