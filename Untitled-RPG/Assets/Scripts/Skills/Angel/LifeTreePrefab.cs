using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DG.Tweening;

public class LifeTreePrefab : MonoBehaviour
{
    LifeTree skill;
    bool playerInside;
    Collider treeTrigger;
    AudioSource audioSource;
    [DisplayWithoutEdit] bool isFlying;

    public GameObject treePrefab;
    public GameObject throwPrefab;
    public ParticleSystem vfx;
    public AudioClip loopSound;
    [Header("Grow VFX")]
    public Transform mesh;
    public float upGrowDuration = 1.1f;
    public float sidesGrowDuration = 1.1f;
    public Ease ease = Ease.OutSine;
    public float delayOfSideGrowth = 0.2f;
    public Vector3 initialScale = new Vector3(0.05f, 0f, 0.05f);

    Transform handTransform;
    Vector3 target;
    float gravity = 9.81f;
    float throwAngle = 30f;

    public void Throw (LifeTree _ownerSkill, Transform _handTransform, Vector3 _target) {
        treeTrigger = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();

        skill = _ownerSkill;

        gameObject.SetActive(true);
        treePrefab.SetActive(false);
        throwPrefab.SetActive(true);

        treeTrigger.enabled = false;
        isFlying = true;

        handTransform = _handTransform;
        target = _target;

        StartCoroutine(SimulateProjectile());
    }

    IEnumerator SimulateProjectile()
    {
        // Short delay added before Projectile is thrown
        yield return new WaitForSeconds(0.8f);
        transform.SetParent(null);
       
        // Move projectile to the position of throwing object + add some offset if needed.
        transform.position = handTransform.position + new Vector3(0, 0.0f, 0);
       
        // Calculate distance to target
        float target_Distance = Vector3.Distance(transform.position, target);
 
        // Calculate the velocity needed to throw the object to the target at specified angle.
        float projectile_Velocity = target_Distance / (Mathf.Sin(2 * throwAngle * Mathf.Deg2Rad) / gravity);
 
        // Extract the X  Y componenent of the velocity
        float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(throwAngle * Mathf.Deg2Rad);
        float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(throwAngle * Mathf.Deg2Rad);
 
        // Calculate flight time.
        float flightDuration = target_Distance / Vx;
   
        // Rotate projectile to face the target.
        transform.rotation = Quaternion.LookRotation(target - transform.position);
       
        float elapse_time = 0;
 
        while (elapse_time < flightDuration)
        {
            transform.Translate(0, (Vy - (gravity * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);
           
            elapse_time += Time.deltaTime;
 
            yield return null;
        }
        StartGrowing();
    } 

    void StartGrowing () {
        treeTrigger.enabled = true;
        isFlying = false;
        treePrefab.SetActive(true);

        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        PlayGrowVFX();
        StartCoroutine(Work());

        audioSource.clip = loopSound;
        audioSource.Play();
    }

    public void PlayGrowVFX (bool reverse = false){
        if (!reverse) {
            mesh.localScale = initialScale;
            mesh.DOScaleY(1, upGrowDuration).SetEase(ease);
            mesh.DOScaleX(1, sidesGrowDuration).SetDelay(delayOfSideGrowth).SetEase(ease);
            mesh.DOScaleZ(1, sidesGrowDuration).SetDelay(delayOfSideGrowth).SetEase(ease);
        } else {
            mesh.localScale = Vector3.one;
            mesh.DOScaleY(initialScale.y, upGrowDuration).SetEase(ease).SetDelay(delayOfSideGrowth);
            mesh.DOScaleX(initialScale.x, sidesGrowDuration).SetEase(ease);
            mesh.DOScaleZ(initialScale.z, sidesGrowDuration).SetEase(ease);
        }
    }
    IEnumerator Work () {
        yield return new WaitForSeconds(skill.duration-2);
        
        PlayGrowVFX(true);
        if (playerInside) {
            Characteristics.instance.RemoveBuff(skill.buff);
            Characteristics.instance.RemoveRecurringEffect(skill.effect);
        }
        playerInside = false;
        treeTrigger.enabled = false;
        
        Destroy(gameObject, upGrowDuration + delayOfSideGrowth);
        
        vfx.transform.parent = null;
        vfx.Stop();
        Destroy(vfx.gameObject, 6);
    }

    void OnTriggerEnter(Collider other) {
        if (isFlying) return;

        if (other.CompareTag("Player") && !other.isTrigger) {
            Characteristics.instance.AddBuff(skill.buff);
            Characteristics.instance.AddRecurringEffect(skill.effect);
            playerInside = true;
        }
    }
    void OnTriggerExit(Collider other) {
        if (isFlying) return;
        
        if (other.CompareTag("Player") && !other.isTrigger) {
            Characteristics.instance.RemoveBuff(skill.buff);
            Characteristics.instance.RemoveRecurringEffect(skill.effect);
            playerInside = false;
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(LifeTreePrefab))]
class LifeTreePrefabInspecotr : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        LifeTreePrefab fountain = (LifeTreePrefab)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Test Grow VFX")) 
            fountain.PlayGrowVFX();
        
    }
}

#endif