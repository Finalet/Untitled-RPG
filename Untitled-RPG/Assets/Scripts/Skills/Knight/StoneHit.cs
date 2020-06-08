using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneHit : Skill
{
    bool yes;

    Vector3 baseCenter;
    Vector3 newCenter;

    List<GameObject> enemiesHit = new List<GameObject>();

    public AudioClip first;
    public AudioClip last;

    public GameObject SFX;

    protected override void Start() {
        base.Start();
        
        GetComponent<BoxCollider>().enabled = false;
        
        baseCenter = GetComponent<BoxCollider>().center;
        newCenter = baseCenter + Vector3.forward * 4.5f;
    }

    protected override void Update() {
        base.Update();
        ClearTrigger();
    }

    protected override void CustomUse() {
        actualDamage = Mathf.RoundToInt( baseDamage * (float)characteristics.meleeAttack/100f);
        StartCoroutine(Using());
    }

    IEnumerator Using () {
        animator.CrossFade("Attacks.Knight.StoneHit", 0.25f);
        while(!yes) {
            yield return null;
        }
        transform.GetChild(0).GetComponent<ParticleSystem>().Play();

        GameObject soundFX = Instantiate(SFX.gameObject, transform.position, Quaternion.identity);
        soundFX.GetComponent<AudioSource>().clip = first;
        soundFX.GetComponent<AudioSource>().Play();

        playerControlls.playerCamera.GetComponent<CameraControll>().CameraShake(0.4f, 0.1f);

        GetComponent<BoxCollider>().enabled = true;
        while (GetComponent<BoxCollider>().center != newCenter) {
            GetComponent<BoxCollider>().center = Vector3.MoveTowards(GetComponent<BoxCollider>().center, newCenter, 15 * Time.deltaTime);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        yes = false;
        GetComponent<BoxCollider>().center = baseCenter;
        GetComponent<BoxCollider>().enabled = false;
        enemiesHit.Clear();
        yield return new WaitForSeconds(1.1f);

        soundFX.GetComponent<AudioSource>().clip = last;
        soundFX.GetComponent<AudioSource>().Play();
        while(soundFX.GetComponent<AudioSource>().isPlaying){
            yield return null;
        }
        Destroy(soundFX);
    }
    
    public void ApplyDamage() {
        yes = true;
    }
    
    int damage () {
        return Mathf.RoundToInt(Random.Range(actualDamage*0.85f, actualDamage*1.15f));
    }    

    void OnTriggerEnter(Collider other) {
        if (!yes)
            return;

        if (other.GetComponent<Enemy>() != null && !other.isTrigger) {
            if (!enemiesHit.Contains(other.gameObject)) {
                other.GetComponent<Enemy>().GetKnockedDown();
                other.GetComponent<Enemy>().GetHit(damage(), true, true);
                enemiesHit.Add(other.gameObject);
            }
        }
    }

    void ClearTrigger () {
        for (int i = 0; i < enemiesHit.Count; i++) {
            if (enemiesHit[i].gameObject == null) {
                enemiesHit.RemoveAt(i);
            }
        }
    }
}
