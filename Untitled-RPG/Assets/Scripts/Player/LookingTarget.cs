﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookingTarget : MonoBehaviour
{
    public GameObject target;
    [Tooltip("At what distance can you start to add target")] public int viewDistance;
    public bool targetIsEnemy;

    public float distanceToTarget;

    RaycastHit hit;
    void Update()
    {
        Quaternion rot = Quaternion.Euler(0, transform.rotation.y - 90, 0);
        Vector3 vec = rot * transform.right;

        if (Physics.Raycast(PlayerControlls.instance.transform.position + Vector3.up, vec, out hit, viewDistance)){
            target = hit.transform.gameObject;
            Enemy en = target.GetComponent<Enemy>();
            if (en != null) {
                targetIsEnemy = true;
                CanvasScript.instance.DisplayEnemyInfo(en.name, (float)en.health/en.maxHealth);
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
