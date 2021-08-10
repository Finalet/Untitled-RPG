using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DancingSwordProjectile : MonoBehaviour
{
    protected DamageInfo damageInfo1;
    protected DamageInfo damageInfo2;
    
    public float lifeTime = 20;
    public float gravityScale = 0.4f;
    public float gravityDelay = 0.2f;
    [System.NonSerialized] public bool instantShot = false;

    public Transform mesh;
    public ParticleSystem shootParticles;

    protected float timeShot;
    protected bool shot = false;
    protected bool applyGravity;

    protected Rigidbody rb;

    protected List<IDamagable> damagablesHit = new List<IDamagable>();

    float randomRotation;

    [Space]
    public AudioClip[] hitSounds;
    protected virtual void Start() {
        rb = GetComponent<Rigidbody>();
        if (!instantShot) rb.isKinematic = true;
        rb.useGravity = false;
        randomRotation = Random.value < 0.5f ? -1*Random.Range(200, 1000) : Random.Range(200, 1000);
    }

    protected virtual void Update() {
        if (shot && rb.velocity.magnitude > 0) {
            transform.rotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
            mesh.Rotate(Vector3.up, randomRotation * Time.deltaTime);
        } else if (!shot) {
            mesh.Rotate(Vector3.up, randomRotation*0.1f * Time.deltaTime);
        }
        
        if (shot && Time.time - timeShot >= lifeTime) {
            Destroy(gameObject);
        } else if (shot && Time.time - timeShot >= gravityDelay) {
            applyGravity = true;
        }
    }

    protected virtual void FixedUpdate() {
        if (applyGravity)
            rb.AddForce(Physics.gravity * gravityScale);
    }

    public virtual void Shoot (float _strength, Vector3 _shotPoint, DamageInfo _damageInfo1, DamageInfo _damageInfo2) {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        damageInfo1 = _damageInfo1;
        damageInfo2 = _damageInfo2;
        Vector3 direction = _shotPoint - transform.position; 

        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.AddForce(_strength * direction.normalized, ForceMode.Impulse);
        transform.SetParent(null);
        shootParticles.Play();

        timeShot = Time.time;
        shot = true;

        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.07f, 1f, 0.05f);
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
        
        rb.isKinematic = true;
        
        GetComponent<Collider>().enabled = false;
        GetComponent<AudioSource>().clip = hitSounds[Random.Range(0, hitSounds.Length)];
        GetComponent<AudioSource>().Play();

        shootParticles.Stop();
    }

    protected virtual void Hit (IDamagable en) {
        if (!damagablesHit.Contains(en)) {
            en.GetHit(damageInfo1, false, false, HitType.Interrupt, transform.position);
            en.GetHit(damageInfo2, false, false, HitType.Normal, transform.position);
            damagablesHit.Add(en);

            bl_UCrosshair.Instance.OnHit();
        }
    }    
}
