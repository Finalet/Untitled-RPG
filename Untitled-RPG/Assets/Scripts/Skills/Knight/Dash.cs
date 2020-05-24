using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : Skill
{
    List<GameObject> enemiesHit = new List<GameObject>();

    [Header("Custom Vars")]
    public float dashDistance;

    protected override  void Start() {
        base.Start();

        GetComponent<BoxCollider>().enabled = false;
    }
    protected override void Update() {
        base.Update();
        ClearTrigger();
    }

    protected override void CustomUse() {
        actualDamage = Mathf.RoundToInt(baseDamage * (float)characteristics.meleeAttack/100f);
        animator.CrossFade("Attacks.DoubleSwords.Dash", 0.25f);
        audioSource.PlayDelayed(0.1f * characteristics.attackSpeedPercentageInverted);
    }

    int damage () {
        return Mathf.RoundToInt(Random.Range(actualDamage*0.85f, actualDamage*1.15f));
    }    

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.GetComponent<Enemy>() != null && !other.isTrigger) {
            if (!enemiesHit.Contains(other.gameObject)) {
                other.GetComponent<Enemy>().GetHit(damage());
                enemiesHit.Add(other.gameObject);
            }
        }
    }

    public void Hit (float stopHit) {
        if (stopHit == 0) {
            GetComponent<BoxCollider>().enabled = true;
            playerControlls.independentFromInputFwd += dashDistance;
        } else {
            playerControlls.independentFromInputFwd -= dashDistance;
            GetComponent<BoxCollider>().enabled = false;
            enemiesHit.Clear();
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
