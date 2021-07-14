using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HailstoneProjectile : MonoBehaviour
{

    public DamageInfo damageInfo;
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

        GetComponent<AudioSource>().PlayDelayed(0.8f);
        Destroy(transform.parent.gameObject, 2f);
    }

    void OnTriggerEnter(Collider other) {
        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (other.isTrigger || other.CompareTag("Player") || en == null)
            return;
        
        if (!enemiesHit.Contains(en)) {
            en.GetHit(damageInfo, "Hailstone");
            enemiesHit.Add(en);
        }
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.2f, 2f* (1+damageInfo.damage/2000), 0.1f, transform.position);

        finalDebrisPos = debirsPos.position;
        Instantiate(debris, finalDebrisPos, debris.transform.rotation, transform.parent).Play();
    }
}
