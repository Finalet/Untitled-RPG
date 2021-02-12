using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirewallWall : MonoBehaviour
{
    public float duration;
    public DamageInfo damageInfo;

    public Light light1;
    public Light light2;

    List<Enemy> enemiesHit = new List<Enemy>();
    void Start() {
        Destroy(gameObject, duration + 0.5f);
        
        var main = transform.GetChild(0).GetComponent<ParticleSystem>().main;
        main.startLifetime = duration;
        main = transform.GetChild(0).GetChild(1).GetComponent<ParticleSystem>().main;
        main.startLifetime = duration;

        StartCoroutine(Lights());
    }

    void Update() {
        if (GetComponent<BoxCollider>().size.x <= 10.5f) {
            GetComponent<BoxCollider>().center += Vector3.right * Time.deltaTime*9f;
            GetComponent<BoxCollider>().size += Vector3.right * Time.deltaTime*18f;
        }
    }

    void OnTriggerStay(Collider other) {
        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (other.isTrigger || other.CompareTag("Player") || en == null)
            return;
        
        if (!enemiesHit.Contains(en)) {
            en.GetHit(damageInfo, "Firewall", false, false, HitType.Interrupt);
            StartCoroutine(List(en));
        }
    }

    IEnumerator List (Enemy enemy) {
        enemiesHit.Add(enemy);
        yield return new WaitForSeconds(0.7f);
        enemiesHit.Remove(enemy);
    }
    IEnumerator Lights(){
        light1.intensity =0;
        light2.intensity =0;
        yield return new WaitForSeconds(0.3f);
        while(light1.intensity < 20) {
            light1.intensity = Mathf.MoveTowards(light1.intensity, 20, Time.deltaTime * 40);
            light2.intensity = light1.intensity;
            yield return null;
        }
        yield return new WaitForSeconds(duration - 0.3f - 0.5f);
        while(light1.intensity > 0) {
            light1.intensity = Mathf.MoveTowards(light1.intensity, 0, Time.deltaTime * 40);
            light2.intensity = light1.intensity;
            yield return null;
        }
    }
}
