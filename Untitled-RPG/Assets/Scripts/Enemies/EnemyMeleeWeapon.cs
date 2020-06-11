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
        if (other.CompareTag("Player") && other.GetType() == typeof(CharacterController)) { // Checks if charater got hit, and nots its triggers
            enemy.Hit();
        }
    }
}
