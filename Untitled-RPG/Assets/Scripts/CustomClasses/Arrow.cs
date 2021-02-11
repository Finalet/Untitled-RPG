using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    string skillName;
    int actualDamage;
    
    public float lifeTime = 20;
    public float gravityScale = 0.4f;
    public float gravityDelay = 0.2f;
    public TrailRenderer trail;

    float timeShot;
    bool shot = false;
    bool applyGravity;

    Rigidbody rb;

    List<Enemy> enemiesHit = new List<Enemy>();
    protected virtual void Start() {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        trail.enabled = false;
    }

    protected virtual void Update() {
        if (shot && rb.velocity.magnitude > 0)
            transform.rotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
        
        if (shot && Time.realtimeSinceStartup - timeShot >= lifeTime) {
            Destroy(gameObject);
        } else if (shot && Time.realtimeSinceStartup - timeShot >= gravityDelay) {
            applyGravity = true;
        }
    }

    protected virtual void FixedUpdate() {
        if (applyGravity)
            rb.AddForce(Physics.gravity * gravityScale);
    }

    public virtual void Shoot (float _strength, Vector3 _shotPoint, int _actualDamage, string _skillName) {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        skillName = _skillName;
        actualDamage = _actualDamage;
        Vector3 direction = _shotPoint - transform.position; 

        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.AddForce(_strength * direction.normalized, ForceMode.Impulse);
        transform.SetParent(null);

        timeShot = Time.realtimeSinceStartup;
        shot = true;
        trail.enabled = true;
    }

    protected virtual void OnTriggerEnter(Collider other) {
        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (other.isTrigger || other.CompareTag("Player"))
            return;
        
        Collision();

        if (en == null) 
            return;
        
        Hit(en);
    }

    protected virtual void Collision () {
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.isKinematic = true;
        GetComponent<CapsuleCollider>().enabled = false;
    }

    protected virtual void Hit (Enemy en) {
        if (!enemiesHit.Contains(en)) {
            en.GetHit(damage(), skillName, false, false, HitType.Normal, transform.position);
            enemiesHit.Add(en);
        }
    }

    protected virtual int damage () {
        return Mathf.RoundToInt(Random.Range(actualDamage*0.85f, actualDamage*1.15f));
    }        
}
