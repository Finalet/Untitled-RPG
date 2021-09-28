using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeWeapon : MonoBehaviour
{
    public Enemy enemy;
    public float value; //hit only when its equal to the curve

    bool once;

    void Start() {
        if(!enemy) enemy = GetComponentInParent<Enemy>();
    }

    // void OnTriggerEnter(Collider other) {
    //     if (other.CompareTag("Player") && other.GetType() == typeof(CapsuleCollider) && enemy.checkCanHit(value)) { // Checks if charater got hit, and not its triggers
    //         enemy.Hit();
    //     }
    // }

    void OnTriggerStay (Collider other) {
        if (!enemy.checkCanHit(value)) {
            once = false;
            return;
        } 

        if (!once && other.CompareTag("Player") && other.GetType() == typeof(CapsuleCollider)) { // Checks if charater got hit, and not its triggers
            enemy.Hit();
            once = true;
        }
    }

    // void OnTriggerExit (Collider other) {
    //     if (!once) return;

    //     if (other.CompareTag("Player") && other.GetType() == typeof(CapsuleCollider)) {
    //         once = false;
    //     }
    // }
}
