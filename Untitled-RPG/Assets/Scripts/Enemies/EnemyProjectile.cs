using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public DamageInfo enemyDamageInfo;
    public HitType hitType;
    public ParticleSystem hitParticles;
    public bool shot;

    public string enemyName;
    AudioSource audioSource;

    bool playedSound;

    void Start() {
        Destroy(gameObject, 5);
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other) {
        if (!shot)
            return;
            
        if (other.CompareTag("Player") && other.GetType() == typeof(CapsuleCollider)) { // Checks if charater got hit, and not its triggers
            PlayerControlls.instance.GetComponent<Characteristics>().GetHit(enemyDamageInfo, hitType, 0.2f, 1f);
        }
        if (!other.isTrigger && !playedSound) {
            audioSource.PlayOneShot(audioSource.clip);
            hitParticles.Play();
            playedSound = true;
        }
    }
}
