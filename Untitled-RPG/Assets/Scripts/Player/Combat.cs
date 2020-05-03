using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    public int baseAttackDamage;
    public int actualAttackDamage;

    PlayerControlls playerControlls;
    Animator animator;
    WeaponsController weapons;
    BoxCollider basicAttackCollider;
    Vector3 baseColliderSize;
    WeaponsController weaponsController;

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
        basicAttackCollider = GetComponent<BoxCollider>();
        baseColliderSize = basicAttackCollider.size;
    }

    void Update() {
        Timer();

        if (!playerControlls.isJumping && weapons.isWeaponOut) {
            Attacks();
        }
    }

    bool canHit;
    void Attacks() {
        if (Input.GetButton("Fire1") && !playerControlls.isRolling && !PeaceCanvas.instance.anyPanelOpen) {
            animator.SetBool("KeepAttacking", true);
            playerControlls.isAttacking = true;
            comboTimer = baseComboTimer;
        }

        AttackSpeed();
        actualAttackDamage = Mathf.RoundToInt( baseAttackDamage * (float)GetComponent<Characteristics>().meleeAttack/100f);
    }
    public void moveFwdAttack () {
        StartCoroutine(moveCoroutine());
    }
    IEnumerator moveCoroutine() {
        float timer = 0.1f;
        while (timer > 0) {
            playerControlls.GetComponent<CharacterController>().Move(transform.forward * Time.fixedDeltaTime * 10);
            timer -= Time.fixedDeltaTime;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
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

    void Hit () {
        Invoke("CantHit", 0.1f);
        float x = Random.Range(-150.00f, 150.00f);
        hitID = x;
        canHit = true;
    }

    void CantHit () {
        canHit = false;
        basicAttackCollider.size = baseColliderSize;
    }
    int damage () {
        return Mathf.RoundToInt(Random.Range(actualAttackDamage*0.85f, actualAttackDamage*1.15f));
    }

    public void IncreaseCollider() {
        basicAttackCollider.size += Vector3.right;
    }

    void AttackSpeed () {
        animator.SetFloat("AttackSpeed", GetComponent<Characteristics>().attackSpeedPercentage);
    }
}
