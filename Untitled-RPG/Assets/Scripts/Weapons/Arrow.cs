using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    protected string skillName;
    protected DamageInfo damageInfo;
    
    public float lifeTime = 20;
    public float gravityScale = 0.4f;
    public float gravityDelay = 0.2f;
    public TrailRenderer trail;
    [System.NonSerialized] public bool instantShot = false;

    protected float timeShot;
    protected bool shot = false;
    protected bool applyGravity;

    protected Rigidbody rb;

    protected List<Enemy> enemiesHit = new List<Enemy>();
    protected virtual void Start() {
        rb = GetComponent<Rigidbody>();
        if (!instantShot) rb.isKinematic = true;
        rb.useGravity = false;
        if (!instantShot) trail.emitting = false;
    }

    protected virtual void Update() {
        if (shot && rb.velocity.magnitude > 0)
            transform.rotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
        
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

    public virtual void Shoot (float _strength, Vector3 _shotPoint, DamageInfo _damageInfo, string _skillName) {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        skillName = _skillName;
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

        Enemy en = other.transform.GetComponentInParent<Enemy>();
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
        
        GetComponent<CapsuleCollider>().enabled = false;
        trail.emitting = false;
        GetComponent<AudioSource>().Play();
    }

    protected virtual void Hit (Enemy en) {
        if (!enemiesHit.Contains(en)) {
            en.GetHit(damageInfo, skillName, false, false, HitType.Interrupt, transform.position);
            enemiesHit.Add(en);
        }
    }    
}
