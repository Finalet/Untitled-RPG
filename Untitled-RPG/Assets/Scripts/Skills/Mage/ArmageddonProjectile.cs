using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ArmageddonProjectile : MonoBehaviour
{
    public float speed;
    public DamageInfo damageInfo;
    public ParticleSystem hitParticles;
    public ParticleSystem smokeParticles;
    public AudioClip[] explosionSounds;
    public SphereCollider explosionCollider;

    List<IDamagable> damagablesHit = new List<IDamagable>();
    bool exploded;


    void Start() {
        GetComponent<Rigidbody>().AddForce(Vector3.down * speed, ForceMode.Impulse);
        Destroy(gameObject, 3.5f);
    }

    void OnTriggerEnter(Collider other) {
        if (other.GetComponent<ArmageddonProjectile>() != null || other.isTrigger)
            return;

        if (!exploded) {
            hitParticles.Play();
            GetComponent<AudioSource>().clip = explosionSounds[Random.Range(0, explosionSounds.Length)];
            GetComponent<AudioSource>().Play();
            DOTween.To(()=> explosionCollider.radius, x=> explosionCollider.radius = x, 9f, 0.2f).SetEase(Ease.OutSine);
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            smokeParticles.Stop();
            exploded = true;
        }

        IDamagable en = other.transform.GetComponentInParent<IDamagable>();
        if (other.isTrigger || other.CompareTag("Player") || en == null)
            return;
        
        if (!damagablesHit.Contains(en)) {
            en.GetHit(damageInfo, "Armageddon", false, false, HitType.Knockdown);
            damagablesHit.Add(en);
        }

        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.2f, 2*(1+damageInfo.damage/2000), 0.2f, transform.position);
    }
}
