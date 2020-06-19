using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HailstoneProjectile : MonoBehaviour
{

    public int actualDamage;
    public ParticleSystem debris;

    public Transform debirsPos;
    Vector3 finalDebrisPos;

    List<Enemy> enemiesHit = new List<Enemy>();

    bool isExtra;
    void Start() {
        if (isExtra) {
            return;
        }
        
        GetComponent<Rigidbody>().AddForce(transform.right * 20, ForceMode.Impulse);
        for (int i = 0; i < 10; i++) {
            Vector3 offset = Random.onUnitSphere * Random.Range(1.6f, 2.4f);
            GameObject go = Instantiate(gameObject, transform.position + offset, transform.rotation, transform.parent);
            go.GetComponent<HailstoneProjectile>().isExtra = true;
            go.GetComponent<CapsuleCollider>().enabled = false;
            go.transform.localScale = transform.localScale/5;
            go.transform.GetChild(0).localScale = go.transform.localScale * 3;
            go.GetComponent<Rigidbody>().AddForce(transform.right * Random.Range(18f, 22f), ForceMode.Impulse);
        }

        Destroy(transform.parent.gameObject, 2f);
    }

    void OnTriggerEnter(Collider other) {
        if (other.isTrigger || other.CompareTag("Player"))
            return;
        
        if (other.gameObject.GetComponent<Enemy>() != null) {
            if (!enemiesHit.Contains(other.GetComponent<Enemy>())) {
                other.GetComponent<Enemy>().GetHit(damage());
                enemiesHit.Add(other.GetComponent<Enemy>());
            }
        }
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.2f, 0.1f, damage());
        finalDebrisPos = debirsPos.position;
        Instantiate(debris, finalDebrisPos, debris.transform.rotation, transform.parent).Play();
        
        if (!isExtra) {
            GetComponent<AudioSource>().time = 0.4f;
            GetComponent<AudioSource>().Play();
        }
    }

    int damage () {
        return Mathf.RoundToInt(Random.Range(actualDamage*0.85f, actualDamage*1.15f));
    }
}
