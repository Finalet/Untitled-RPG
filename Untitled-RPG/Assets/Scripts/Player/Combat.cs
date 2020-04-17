using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    public int baseAttackDamage;

    PlayerControlls playerControlls;
    Animator animator;
    WeaponsController weapons;

    [SerializeField]float baseComboTimer;
    [SerializeField]float comboTimer;

    float animatorLayerWeight;

    int comboCount;
    float hitID;
    float prevHitID;

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
        if (Input.GetButton("Fire1") && !playerControlls.isRolling) {
            animator.SetBool("KeepAttacking", true);
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
            other.GetComponent<Enemy>().GetHit(damage(), hitID);
            if (prevHitID != hitID) {
                GetComponent<Characteristics>().UseOrRestoreStamina(-10);
                prevHitID = hitID;
            }
        }
    }
    void generateHitID () {
        Invoke("CantHit", 0.1f);
        float x = Random.Range(-150.00f, 150.00f);
        hitID = x;
        canHit = true;
    }

    void CantHit () {
        canHit = false;
    }
    int damage () {
        return Mathf.RoundToInt(Random.Range(baseAttackDamage*0.7f, baseAttackDamage*1.3f));
    }
}
