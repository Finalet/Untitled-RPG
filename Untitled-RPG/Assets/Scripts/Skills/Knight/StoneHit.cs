using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneHit : Skill
{
    float hitID;
    bool yes;

    Vector3 baseCenter;
    Vector3 newCenter;
    public override void Start() {
        base.Start();
        
        baseCenter = GetComponent<BoxCollider>().center;
        newCenter = baseCenter + Vector3.forward * 4.5f;
    }

    public void CustomUse() {
        actualDamage = Mathf.RoundToInt( baseDamage * (float)characteristics.meleeAttack/100f);
        StartCoroutine(Using());
    }

    bool knockDown;
    IEnumerator Using () {
        GenerateHitID();
        animator.CrossFade("Attacks.DoubleSwords.StoneHit", 0.25f);
        while(!yes) {
            yield return null;
        }
        transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        canHit = true;
        knockDown = true;
        while (GetComponent<BoxCollider>().center != newCenter) {
            GetComponent<BoxCollider>().center = Vector3.MoveTowards(GetComponent<BoxCollider>().center, newCenter, 15 * Time.deltaTime);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        canHit = false;
        yes = false;
        knockDown = false;
        GetComponent<BoxCollider>().center = baseCenter;
    }
    
    public void ApplyDamage() {
        yes = true;
    }
    
    int damage () {
        return Mathf.RoundToInt(Random.Range(actualDamage*0.85f, actualDamage*1.15f));
    }    

    void OnTriggerStay(Collider other) {
        if (other.GetComponent<Enemy>() != null && !other.isTrigger && canHit) {
            if (knockDown) {
                other.GetComponent<Enemy>().GetKnockedDown();
            }
            other.GetComponent<Enemy>().GetHit(damage(), hitID);
        }
    }

    void GenerateHitID () {
        hitID = Random.Range(-100.00f, 100.00f);
    }
}
