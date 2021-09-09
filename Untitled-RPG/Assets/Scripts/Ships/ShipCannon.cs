using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCannon : MonoBehaviour
{
    [Space]
    public Transform shotPoint;
    public ParticleSystem shotVFX;
    public AudioClip[] shotSounds;
    public Transform playerCollider;

    ShipAttachements shipAttachements;
    AudioSource audioSource;

    void Awake() {
        shipAttachements = GetComponentInParent<ShipAttachements>();
        audioSource = GetComponent<AudioSource>();
    }


    public void Shoot (float power = 200) {
        if (!shipAttachements || !shipAttachements.cannonballPrefab) return;

        Rigidbody cannonBall = Instantiate(shipAttachements.cannonballPrefab, shotPoint.position, shotPoint.rotation).GetComponent<Rigidbody>();
        cannonBall.AddForce(cannonBall.transform.forward * power, ForceMode.Impulse);
        
        if (shotSounds.Length > 0) {
            audioSource.clip = shotSounds[Random.Range(0, shotSounds.Length)];
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.Play();
        }
        
        shotVFX.Play();
        Destroy(cannonBall.gameObject, 5);
    }

    void OnDestroy() {
        if (playerCollider && playerCollider.gameObject) Destroy(playerCollider.gameObject);
    }
}
