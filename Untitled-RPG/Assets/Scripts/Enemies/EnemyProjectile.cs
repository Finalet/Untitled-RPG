using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public int baseDamage;
    public HitType hitType;
    public ParticleSystem hitParticles;
    public bool shot;

    AudioSource audioSource;

    void Start() {
        Destroy(gameObject, 5);
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other) {
        if (!shot)
            return;
            
        if (other.CompareTag("Player") && other.GetType() == typeof(CapsuleCollider)) { // Checks if charater got hit, and not its triggers
            PlayerControlls.instance.GetComponent<Characteristics>().GetHit(damage(), hitType, 0.2f, 1f);
        }
        if (!other.isTrigger) {
            audioSource.PlayOneShot(audioSource.clip);
            hitParticles.Play();
        }
    }

    int damage () {
        return Mathf.RoundToInt(Random.Range(baseDamage*0.85f, baseDamage*1.15f));
    }
}
