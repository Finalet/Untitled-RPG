using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrongAttack : Skill
{
    public List<GameObject> enemiesInTrigger = new List<GameObject>();

    protected override void Update() {
        base.Update();
        ClearTrigger();
    }

    protected override void CustomUse() {
        actualDamage = Mathf.RoundToInt( baseDamage * (float)characteristics.meleeAttack/100f);
        animator.CrossFade("Attacks.DoubleSwords.StrongAttack", 0.25f);
        audioSource.PlayDelayed(0.35f * characteristics.attackSpeedPercentageInverted);
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

    public void Hit () {
        for (int i = 0; i < enemiesInTrigger.Count; i++) {
            enemiesInTrigger[i].GetComponent<Enemy>().GetHit(damage(), true);
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
