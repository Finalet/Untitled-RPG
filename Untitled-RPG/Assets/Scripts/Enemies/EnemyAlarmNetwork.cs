using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAlarmNetwork : MonoBehaviour
{
    public float alarmRadius = 10f;
    public float alarmDelay = 0.3f;

    [DisplayWithoutEdit()] public bool isTriggered;
    
    void OnEnable() {
        if (GetComponent<Enemy>() == null) {
            throw new MissingComponentException("Missing Enemy component");
        }
    }

    public void TriggerAlarm () {
        StartCoroutine(Trigger());
    }

    IEnumerator Trigger () {
        isTriggered = true;
        Collider[] collidersAround = Physics.OverlapSphere(transform.position, alarmRadius, LayerMask.GetMask("Enemy"));
        // print("Alarm triggered from " + gameObject.name);

        foreach (Collider col in collidersAround){
            if (col != null && col.gameObject != gameObject) {
                EnemyAlarmNetwork eam = col.gameObject.GetComponentInParent<EnemyAlarmNetwork>();
                if (eam != null) {
                    yield return new WaitForSeconds(Random.Range(0.7f * alarmDelay, 1.3f * alarmDelay));
                    if (!eam.isTriggered && !GetComponent<Enemy>().isDead) col.gameObject.GetComponentInParent<EnemyAlarmNetwork>().AlarmEnemy(gameObject.name);
                }
            }
        }
    }

    public void AlarmEnemy (string whoTriggered) {
        // print($"{gameObject.name} is alarmed by {whoTriggered}");
        GetComponent<Enemy>().Agr();
    }
}
