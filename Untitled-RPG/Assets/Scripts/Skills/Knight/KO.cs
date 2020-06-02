using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KO : Skill
{
    List<GameObject> enemiesInTrigger = new List<GameObject>();

    [Header("Custom vars")]
    public AudioClip[] sounds;

    protected override void Update() {
        base.Update();
        ClearTrigger();
    }

    protected override void CustomUse() {
        actualDamage = Mathf.RoundToInt( baseDamage * (float)characteristics.meleeAttack/100f);
        animator.CrossFade("Attacks.Knight.KO", 0.25f);

        Invoke("PlaySound", 0.15f * characteristics.attackSpeed.z);
        Invoke("PlaySound", 0.6f * characteristics.attackSpeed.z);
    }

    float x = 0;
    void PlaySound(){
        if (x == 0) {
            audioSource.clip = sounds[0];
            audioSource.Play();
            x = 1;
        } else {
            audioSource.clip = sounds[1];
            audioSource.Play();
            x = 0;
        }
    }

    int damage () {
        return Mathf.RoundToInt(Random.Range(actualDamage*0.85f, actualDamage*1.15f));
    }    

    void OnTriggerEnter(Collider other) {
        if (other.GetComponent<Enemy>() != null && !other.isTrigger) {
            enemiesInTrigger.Add(other.gameObject);
        }
    }
    void OnTriggerExit(Collider other) {
        if (other.GetComponent<Enemy>() != null && !other.isTrigger) {
            enemiesInTrigger.Remove(other.gameObject);
        }
    }

    public void Hit (float knockDown) {
        for (int i = 0; i < enemiesInTrigger.Count; i++) {
            if (knockDown == 1) {
                enemiesInTrigger[i].GetComponent<Enemy>().GetKnockedDown();
            }
            enemiesInTrigger[i].GetComponent<Enemy>().GetHit(damage(), true, true);
        }
    }

    void ClearTrigger () {
        for (int i = 0; i < enemiesInTrigger.Count; i++) {
            if (enemiesInTrigger[i].gameObject == null) {
                enemiesInTrigger.RemoveAt(i);
            }
        }
    }
}
