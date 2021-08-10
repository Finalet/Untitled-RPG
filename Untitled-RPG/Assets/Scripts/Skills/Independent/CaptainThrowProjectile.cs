using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptainThrowProjectile : MonoBehaviour
{
    public bool isThrown;
    public Transform returnTransform;
    public DamageInfo damageInfo;
    public CaptainThrow skill;
    public AudioClip hitSound;
    public AudioClip catchSound;
    public Transform mesh;

    bool isReturning;
    float strength;
    Vector3 shootPoint;
    Vector3 right;
    Vector3 lookDir;
    AudioSource audioSource;

    bool playedReturnSound;

    List<IDamagable> damagablesHit = new List<IDamagable>();
        
    public void Throw (Vector3 _shootPoint, float _strength) {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        transform.SetParent(null);
        damagablesHit.Clear();
        right = PlayerControlls.instance.transform.right;
        lookDir = _shootPoint - transform.position;
        shootPoint = _shootPoint;
        strength = _strength;
        isThrown = true;
        playedReturnSound = false;
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

            if (!playedReturnSound) {
                playedReturnSound = true;
                audioSource.PlayOneShot(catchSound);
            }
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
        audioSource.PlayOneShot(hitSound);

        skill.PlayHitFlash(transform.position);

        isReturning = true;
        IDamagable en = other.GetComponentInParent<IDamagable>();
        if (en != null && !damagablesHit.Contains(en)) {
            en.GetHit(damageInfo, true, true, HitType.Knockdown, transform.position);
            damagablesHit.Add(en);

            bl_UCrosshair.Instance.OnHit();
        }

    }
}
