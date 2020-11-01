using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeWeapon : MonoBehaviour
{
    Enemy enemy;

    void Start() {
        enemy = GetComponentInParent<Enemy>();
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && other.GetType() == typeof(CharacterController) && enemy.canHit()) { // Checks if charater got hit, and not its triggers
            enemy.Hit();
        }
    }
}
