using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeWeapon : MonoBehaviour
{
    Enemy enemy;
    public float value; //hit only when its equal to the curve

    void Start() {
        enemy = GetComponentInParent<Enemy>();
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && other.GetType() == typeof(CharacterController) && enemy.checkCanHit(value)) { // Checks if charater got hit, and not its triggers
            enemy.Hit();
        }
    }
}
