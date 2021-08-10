using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    protected DamageInfo damageInfo;
    
    public float lifeTime = 20;
    public float gravityScale = 0.4f;
    public float gravityDelay = 0.2f;
    public TrailRenderer trail;
    [System.NonSerialized] public bool instantShot = false;
    public AudioClip[] impactSounds;

    protected float timeShot;
    protected bool shot = false;
    protected bool applyGravity;

    protected Rigidbody rb;
    protected AudioSource audioSource;

    protected List<IDamagable> damagablesHit = new List<IDamagable>();
    protected virtual void Start() {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        if (!instantShot) rb.isKinematic = true;
        rb.useGravity = false;
        if (!instantShot) trail.emitting = false;
    }

    protected virtual void Update() {
        if (shot && rb != null && rb.velocity.magnitude > 0)
            transform.rotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
        
        if (shot && Time.time - timeShot >= lifeTime) {
            Destroy(gameObject);
        } else if (shot && Time.time - timeShot >= gravityDelay) {
            applyGravity = true;
        }
    }

    protected virtual void FixedUpdate() {
        if (applyGravity && rb != null)
            rb.AddForce(Physics.gravity * gravityScale);
    }

    public virtual void Shoot (float _strength, Vector3 _shotPoint, DamageInfo _damageInfo) {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        damageInfo = _damageInfo;
        Vector3 direction = _shotPoint - transform.position; 

        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.AddForce(_strength * direction.normalized, ForceMode.Impulse);
        transform.SetParent(null);

        timeShot = Time.time;
        shot = true;
        trail.emitting = true;

        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.07f, 1f, 0.05f, transform.position);
    }

    protected virtual void OnTriggerEnter(Collider other) {
        if (!shot || other.isTrigger || other.CompareTag("Player"))
            return;
        
        Collision(other.transform);

        IDamagable en = other.transform.GetComponentInParent<IDamagable>();
        if (en == null) 
            return;
        
        Hit(en);
    }

    protected virtual void Collision (Transform collisionObj) {
        if (!shot)
            return;
        transform.position -= rb.velocity * Time.fixedDeltaTime;
        transform.SetParent(collisionObj);
        
        Destroy(rb);
        
        GetComponent<CapsuleCollider>().enabled = false;
        trail.emitting = false;
        
        if (impactSounds.Length != 0) {
            audioSource.clip = impactSounds[Random.Range(0, impactSounds.Length-1)];
            audioSource.Play();
        }
    }

    protected virtual void Hit (IDamagable en) {
        if (!damagablesHit.Contains(en)) {
            en.GetHit(damageInfo, false, false, HitType.Interrupt, transform.position);
            damagablesHit.Add(en);

            bl_UCrosshair.Instance.OnHit();
        }
    } 
}
