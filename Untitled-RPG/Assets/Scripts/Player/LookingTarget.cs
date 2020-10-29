using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookingTarget : MonoBehaviour
{
    public GameObject target;
    [Tooltip("At what distance can you start to add target")] public int viewDistance;
    public bool targetIsEnemy;

    public float distanceToTarget;

    RaycastHit hit;
    Ray ray;
    void Update()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hit, viewDistance)){
            target = hit.transform.gameObject;
            Enemy en = target.GetComponent<Enemy>();
            if (en != null) {
                targetIsEnemy = true;
                CanvasScript.instance.DisplayEnemyInfo(en.enemyName, (float)en.currentHealth/en.maxHealth, en.currentHealth);
                distanceToTarget = Vector3.Distance(en.gameObject.transform.position, gameObject.transform.position);
            } else {
                target = null;
                CanvasScript.instance.StopDisplayEnemyInfo();
                distanceToTarget = 0;
                targetIsEnemy = false;
            }
        } else {
            target = null;
            CanvasScript.instance.StopDisplayEnemyInfo();
            distanceToTarget = 0;
            targetIsEnemy = false;
        }
    }
}
