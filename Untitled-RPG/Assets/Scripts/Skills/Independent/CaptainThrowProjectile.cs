using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptainThrowProjectile : MonoBehaviour
{
    public bool isThrown;
    public Transform returnTransform;
    public DamageInfo damageInfo;
    public Skill skill;
    public AudioClip hitSound;
    public Transform mesh;

    bool isReturning;
    float strength;
    Vector3 shootPoint;
    Vector3 right;
    Vector3 lookDir;
    AudioSource audioSource;

    List<Enemy> enemiesHit = new List<Enemy>();
        
    public void Throw (Vector3 _shootPoint, float _strength) {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        transform.SetParent(null);
        enemiesHit.Clear();
        right = PlayerControlls.instance.transform.right;
        lookDir = _shootPoint - transform.position;
        shootPoint = _shootPoint;
        strength = _strength;
        isThrown = true;
        audioSource.clip = hitSound;
    }

    void Update() {
        if (!isThrown)
            return;

        mesh.transform.RotateAround(mesh.transform.position, mesh.transform.forward, 500 * Time.deltaTime);

        if (isThrown && !isReturning) {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Vector3.Cross(lookDir, right), Vector3.up), Time.deltaTime * 5);
            transform.position = Vector3.MoveTowards(transform.position, shootPoint, Time.deltaTime * strength);
        } else if (isReturning) {
            if (Vector3.Distance(transform.position, returnTransform.position) <= 5f)
                transform.rotation = Quaternion.Lerp(transform.rotation, returnTransform.rotation, Time.deltaTime * 7);
            transform.position = Vector3.MoveTowards(transform.position, returnTransform.position, Time.deltaTime * strength);
        }

        if (Vector3.Distance(transform.position, shootPoint) < 0.1f)
            isReturning = true;

        if (isReturning && Vector3.Distance(transform.position, returnTransform.position) <= 0.1f) {
            transform.SetParent(returnTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(Vector3.zero);
            transform.localScale = Vector3.one;
            mesh.transform.localPosition = Vector3.zero;
            mesh.transform.localRotation = Quaternion.Euler(Vector3.zero);
            mesh.transform.localScale = Vector3.one;
            isThrown = false;
            isReturning = false;
        }
    }

    void OnTriggerEnter(Collider other) {
        if (!isThrown || other.isTrigger || other.CompareTag("Player"))
            return;

        audioSource.time = 0.2f;
        audioSource.Play();

        isReturning = true;
        Enemy en = other.GetComponentInParent<Enemy>();
        if (en != null && !enemiesHit.Contains(en)) {
            en.GetHit(damageInfo, skill.skillName, true, true, HitType.Knockdown, transform.position);
            enemiesHit.Add(en);
        }

    }
}
