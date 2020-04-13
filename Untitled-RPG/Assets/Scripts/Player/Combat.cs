using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    PlayerControlls playerControlls;
    Animator animator;
    Weapons weapons;

    public float baseComboTimer;
    public float comboTimer;

    float animatorLayerWeight;

    void Start() {
        weapons = GetComponent<Weapons>();
        playerControlls = GetComponent<PlayerControlls>();    
        animator = GetComponent<Animator>();
    }

    void Update() {
        Timer();

        if (!playerControlls.isJumping && weapons.isWeaponOut) {
            Attacks();
        }

        animator.SetBool("isAttacking", playerControlls.isAttacking);
    }

    public Animation anim;
    void Attacks() {

        if (Input.GetButton("Fire1")) {
            playerControlls.isAttacking = true;
            comboTimer = baseComboTimer;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            animator.CrossFade("Attacks.DoubleSword.Skill1", 0.25f);
            playerControlls.isAttacking = true;
        }
    }

    void Timer () {
        if (comboTimer >= 0) {
            comboTimer -= Time.deltaTime;
        } else {
            playerControlls.isAttacking = false;
        }
    }
}
