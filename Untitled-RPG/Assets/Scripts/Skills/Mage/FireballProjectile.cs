using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    public float distance;
    public DamageInfo damageInfo;

    public bool doNotDestroy;

    public ParticleSystem fire;
    public ParticleSystem explostionSparks;

    public GameObject light1;

    Vector3 begPos;
    bool done;

    bool explosionExpand;
    float explosionRadius;

    List<IDamagable> damagablesHit = new List<IDamagable>();
    void Start() {
        if (doNotDestroy)
            return;

        fire.GetComponent<ParticleSystem>().Play();
        begPos = transform.position;

        explosionRadius = GetComponent<SphereCollider>().radius * 3;
    }

    void Update() {
        if (PeaceCanvas.instance.isGamePaused)
            return;

        if (doNotDestroy)
            return;

        if (Vector3.Distance(transform.position, begPos) >= distance && !done) {
            done = true;
            StartCoroutine(Disappear());
        }

        transform.Rotate(randomRotation() * Time.deltaTime, randomRotation() * Time.deltaTime, randomRotation() * Time.deltaTime);
    
        if(explosionExpand) {
            if (GetComponent<SphereCollider>().radius <= explosionRadius)
                GetComponent<SphereCollider>().radius += Time.deltaTime*10;
            else 
                GetComponent<Collider>().enabled = false;    
        }
    }
    int randomRotation () {
        int rand = Random.Range(150, 250);
        return rand;
    }

    void OnTriggerEnter(Collider other) {
        IDamagable en = other.transform.GetComponentInParent<IDamagable>();
        if (other.isTrigger || other.CompareTag("Player") || doNotDestroy)
            return;
        
        Explode();
        
        if (en == null)
            return;

        if (!damagablesHit.Contains(en)) {
            en.GetHit(damageInfo, "Fireball", true, false, HitType.Kickback, transform.position);
            damagablesHit.Add(en);

            bl_UCrosshair.Instance.OnHit();
        }
    }

    void Explode () {
        if (explosionExpand)
            return;

        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.2f, 2f, 0.1f, transform.position);
        explostionSparks.Play();
        GetComponent<AudioSource>().Play(); 
        fire.Stop();
        light1.SetActive(false);
        GetComponent<Rigidbody>().isKinematic = true;
        explosionExpand = true;
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