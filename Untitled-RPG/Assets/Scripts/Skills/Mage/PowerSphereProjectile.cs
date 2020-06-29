using UnityEngine.VFX;
using UnityEngine.Rendering;
using UnityEngine;

public class PowerSphereProjectile : MonoBehaviour
{
    public Vector3 position;
    public bool shoot;
    public PowerSphere powerSphereSkill;
    public bool playerInside;

    public Material mat;
    public VisualEffect VFX;
    public Volume PP;
    
    bool done;
    bool destroying;
    bool invoked;

    bool playedAudio;
    void Start() {
        mat.SetFloat("DisolveValue", 0);
        GetComponent<SphereCollider>().enabled = false;
    }

    void Update() {
        if (!shoot) {
            Grow();
            return;
        }

        if (Vector3.Distance(transform.position, position) >= 0.1f) {
            transform.position = Vector3.MoveTowards(transform.position, position, Time.deltaTime * 7);
        } else if (!done) {
            Grow();
        }

        if (destroying)
            Destruction();
    }

    void Grow () {
        if (!shoot) {
            if (transform.localScale.x < 0.015f) {
                transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * 0.1f, Time.deltaTime * 0.05f);
            }
        } else {
            if (powerSphereSkill.sphereSize/10 - transform.localScale.x >= 0.01f) {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * powerSphereSkill.sphereSize/10, 0.05f);
                GetComponent<SphereCollider>().enabled = true;
                if (!playedAudio) {
                    GetComponent<AudioSource>().Play();
                    playedAudio = true;
                }
            } else {
                done = true;
                Invoke("Destruction", powerSphereSkill.duration);
            }
        }
    }

    void Destruction () {
        destroying = true;
        mat.SetFloat("DisolveValue", mat.GetFloat("DisolveValue") + Time.deltaTime/4);
        PP.weight -= Time.deltaTime/4;
        VFX.Stop();
        if (!invoked) {
            if (playerInside) {
                Characteristics.instance.RemoveBuff(powerSphereSkill); //On trigger exit is not called when the collider is diactivated. Thus I stop checking triggers and just manually check if the player was outside or inside when destruction started
                playerInside = false;
            }
            invoked = true;
            Invoke("MoreDestruction", 4);
        }
    }

    void MoreDestruction () {
        mat.SetFloat("DisolveValue", 0);
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other) {
        if (destroying)
            return;

        if (other.CompareTag("Player") && !other.isTrigger) {
            Characteristics.instance.AddBuff(powerSphereSkill);
            playerInside = true;
        }
    }
    void OnTriggerExit(Collider other) {
        if (destroying)
            return;

        if (other.CompareTag("Player") && !other.isTrigger) {
            Characteristics.instance.RemoveBuff(powerSphereSkill);
            playerInside = false;
        }
    }
}
