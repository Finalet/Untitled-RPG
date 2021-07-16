using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using DG.Tweening;

public class Portal : MonoBehaviour
{
    public VisualEffect vfx;
    public Transform insideVFX;
    public Vector3 teleportPosition;
    [Range(0,1)] public float progress;
    [Range(0.5f, 3f)] public float radius = 1.4f;
    float portalOpenDuration = 10;

    [System.NonSerialized] public Portal returnPortal;
    public bool isReturnPortal;

    public AudioSource audioSource;

    void OnValidate() {
        vfx.SetFloat("Progress", progress);
        vfx.SetFloat("Radius", radius);
        insideVFX.localScale = insideVFXScale() * progress;
    }

    void Start(){
        StartCoroutine(open());

        if (isReturnPortal)
            return;
        
        returnPortal = Instantiate(gameObject, teleportPosition - transform.forward, transform.rotation).GetComponent<Portal>();
        returnPortal.teleportPosition = transform.position + transform.forward;
        returnPortal.isReturnPortal = true;
    }
    IEnumerator open (float timeToOpen = 3) {
        audioSource.Play();

        progress = 0;
        transform.position += Vector3.up * (0.3f+radius);
        BoxCollider col = GetComponent<BoxCollider>();
        col.enabled = false;
        
        insideVFX.localScale = Vector3.zero;

        DOTween.To(()=> progress, x=> progress = x, 1, timeToOpen);
        while (progress < 1) {
            vfx.SetFloat("Progress", progress);
            insideVFX.localScale = insideVFXScale() * progress;
            yield return null;
        }
        col.enabled = true;
        vfx.SetFloat("Progress", 1);
        insideVFX.localScale = insideVFXScale();

        yield return new WaitForSeconds(portalOpenDuration);
        StartCoroutine(close());
    }
    IEnumerator close (float timeToClose = 3) {

        BoxCollider col = GetComponent<BoxCollider>();
        col.enabled = false;
        
        insideVFX.localScale = insideVFXScale();

        DOTween.To(()=> progress, x=> progress = x, 0, timeToClose);
        while (progress > 0) {
            vfx.SetFloat("Progress", progress);
            insideVFX.localScale = insideVFXScale() * progress;
            yield return null;
        }
        vfx.SetFloat("Progress", 0);
        insideVFX.localScale = Vector3.zero;
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other) {
        if (other.isTrigger)
            return;

        Transform teleportee = null;
        if (other.CompareTag("Player"))
            teleportee = PlayerControlls.instance.transform;
        else if (other.GetComponentInParent<Enemy>()) {
            teleportee = other.GetComponentInParent<Enemy>().transform;
        }

        if (teleportee == null || Time.time - TeleportManager.instance.lastTeleportedTime < TeleportManager.instance.teleportDelay) 
            return;
        Teleport(teleportee);
    }

    void Teleport(Transform teleportee) {
        teleportee.position = teleportPosition;
        TeleportManager.instance.lastTeleportedTime = Time.time;
    }

    Vector3 insideVFXScale () {
        return Vector3.one * radius * 2;
    }
}
