using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Characteristics : MonoBehaviour
{
    [Header("Main")]
    public int maxHP;
    public int maxStamina;
    public int HP;
    public int Stamina;
    [Header("Stats")]
    public int strength;
    public int agility;
    public int intelligence;
    [Header("Attacks")]
    public int meleeAttack;
    public int rangedAttack;
    public int magicPower;
    public int healingPower;
    public int defense;
    [Header("Misc")]
    public int castingTime;

    int statsRatio = 2;
    [Header("Stats regeneration")]
    public int HealthPointsPerSecond; 
    public bool canRegenerateHealth; 

    void Awake() {
        StatsCalculations();
    }

    void Start() {
        HP = maxHP;
        Stamina = maxStamina;
    }

    void Update() {
        StatsCalculations();
        regenerateHealth();
    }

    void StatsCalculations () {
        maxHP = 1000 + strength / statsRatio;
        maxStamina = 300 + agility / statsRatio;

        meleeAttack = 0 + strength / statsRatio;
        rangedAttack = 0 + agility / statsRatio;
        magicPower = 0 + intelligence / statsRatio;
        healingPower = 0 + intelligence / statsRatio;
        defense = 0 + strength / statsRatio + agility / statsRatio;
    }

    float hpTimer = 1;
    void regenerateHealth() {
        if (canRegenerateHealth && HP < maxHP) {
            if (hpTimer <= 0) {
                HP += HealthPointsPerSecond/10;
                hpTimer = 0.1f;
            } else {
                hpTimer -= Time.deltaTime;
            }
        }
    }
}
