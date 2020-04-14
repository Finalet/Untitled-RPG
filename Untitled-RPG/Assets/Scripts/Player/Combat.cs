using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    public int currentSkillDamage;

    PlayerControlls playerControlls;
    Animator animator;
    WeaponsController weapons;

    [SerializeField]float baseComboTimer;
    [SerializeField]float comboTimer;

    float animatorLayerWeight;

    int comboCount;
    float hitID;

    void Start() {
        weapons = GetComponent<WeaponsController>();
        playerControlls = GetComponent<PlayerControlls>();    
        animator = GetComponent<Animator>();
    }

    void Update() {
        Timer();

        if (!playerControlls.isJumping && weapons.isWeaponOut) {
            Attacks();
        }
    }

    bool canHit;
    void Attacks() {
        if (Input.GetButton("Fire1")) {
            animator.SetBool("KeepAttacking", true);
            currentSkillDamage = 10;
            comboTimer = baseComboTimer;
        }
    }

    void Timer () {
        if (comboTimer >= 0) {
            comboTimer -= Time.deltaTime;
        } else {
            animator.SetBool("KeepAttacking", false);
        } 
    }

    void OnTriggerStay(Collider other) {
        if (other.gameObject.GetComponent<Enemy>() != null && canHit) {
            other.GetComponent<Enemy>().GetHit(PlayerControlls.instance.GetComponent<Combat>().currentSkillDamage, hitID);
        }
    }
    void generateHitID () {
        canHit = true;
        Invoke("CantHit", 0.1f);
        float x = Random.Range(-150.00f, 150.00f);
        print(x);
        hitID = x;
    }
    void CantHit () {
        canHit = false;
    }
}
