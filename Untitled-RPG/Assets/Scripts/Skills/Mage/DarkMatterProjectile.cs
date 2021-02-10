using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkMatterProjectile : MonoBehaviour
{
    public float distance;
    public int actualDamage;

    public bool doNotDestroy;

    public ParticleSystem fire;
    public ParticleSystem explostionSparks;

    public GameObject light1;

    Vector3 begPos;
    bool done;

    List<Enemy> enemiesHit = new List<Enemy>();
    void Start() {
        if (doNotDestroy)
            return;

        fire.GetComponent<ParticleSystem>().Play();
        begPos = transform.position;

        GetComponent<AudioSource>().time = 0.1f; 
        GetComponent<AudioSource>().pitch = 1.7f; 
        GetComponent<AudioSource>().Play(); 
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
        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (other.isTrigger || other.CompareTag("Player") || doNotDestroy)
            return;
        
        Explode();

        if (en == null) 
            return;
        
        if (!enemiesHit.Contains(en)) {
            en.GetHit(damage(), "Dark Matter", false, false, HitType.Normal, transform.position);
            enemiesHit.Add(en);
        }
    }

    void Explode () {
        explostionSparks.Play();
        fire.Stop();
        light1.SetActive(false);
        GetComponent<Rigidbody>().isKinematic = true;
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