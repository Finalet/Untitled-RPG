using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookingTarget : MonoBehaviour
{
    public GameObject target;

    RaycastHit hit;
    void Update()
    {
        Quaternion rot = Quaternion.Euler(0, transform.rotation.y - 90, 0);
        Vector3 vec = rot * transform.right;

        Debug.DrawRay(PlayerControlls.instance.transform.position + Vector3.up, vec * 20, Color.red);
        if (Physics.Raycast(PlayerControlls.instance.transform.position + Vector3.up, vec, out hit, 20)){
            target = hit.transform.gameObject;
            Enemy en = target.GetComponent<Enemy>();
            if (en != null) {
                CanvasScript.instance.DisplayEnemyInfo(en.name, (float)en.health/en.maxHealth);
            } else {
                CanvasScript.instance.StopDisplayEnemyInfo();
            }
        } else {
            target = null;
            CanvasScript.instance.StopDisplayEnemyInfo();
        }
    }
}
