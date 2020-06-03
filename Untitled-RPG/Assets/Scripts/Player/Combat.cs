using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    public int baseAttackDamage;
    public int actualAttackDamage;

    PlayerControlls playerControlls;
    Animator animator;
    BoxCollider basicAttackCollider;
    Vector3 baseColliderSize;
    WeaponsController weaponsController;

    [SerializeField]float baseComboTimer;
    [SerializeField]float comboTimer;

    public List<GameObject> enemiesInCombatTrigger = new List<GameObject>();

    void Start() {
        weaponsController = GetComponent<WeaponsController>();
        playerControlls = GetComponent<PlayerControlls>();    
        animator = GetComponent<Animator>();
        basicAttackCollider = GetComponent<BoxCollider>();
        baseColliderSize = basicAttackCollider.size;
    }

    void Update() {
        Timer();
        ClearTrigger();
        AttackSpeed();

        if (!playerControlls.isJumping && weaponsController.isWeaponOut) {
            Attacks();
        }
    }
    
    void ClearTrigger () {
        for (int i = 0; i < enemiesInCombatTrigger.Count; i++) {
            if (enemiesInCombatTrigger[i].gameObject == null) {
                enemiesInCombatTrigger.RemoveAt(i);
            }
        }
    }

    void Attacks() {
        if (Input.GetButton("Fire1") && !playerControlls.isRolling && !PeaceCanvas.instance.anyPanelOpen && !playerControlls.isGettingHit && !playerControlls.isMounted) {
            Attack();
        } 

        actualAttackDamage = Mathf.RoundToInt( baseAttackDamage * (float)GetComponent<Characteristics>().meleeAttack/100f);
    }

    void Attack () {
        animator.SetBool("KeepAttacking", true);
        playerControlls.isAttacking = true;
        comboTimer = baseComboTimer;

        PlayerControlls.instance.InterruptCasting();
    }

    void Timer () {
        if (comboTimer >= 0) {
            comboTimer -= Time.deltaTime;
        } else {
            animator.SetBool("KeepAttacking", false);
        } 
    }

    void OnTriggerEnter(Collider other) {
        if (other.GetComponent<Enemy>() != null && !other.isTrigger) {
            enemiesInCombatTrigger.Add(other.gameObject);
        }
    }
    void OnTriggerExit(Collider other) {
        if (other.GetComponent<Enemy>() != null && !other.isTrigger) {
            enemiesInCombatTrigger.Remove(other.gameObject);
        }
    }

    void Hit () {
        for (int i = 0; i < enemiesInCombatTrigger.Count; i++) {
            enemiesInCombatTrigger[i].GetComponent<Enemy>().GetHit(damage(), true, true);
        }
    }

    int damage () {
        return Mathf.RoundToInt(Random.Range(actualAttackDamage*0.85f, actualAttackDamage*1.15f));
    }

    public void IncreaseCollider() {
        basicAttackCollider.size += Vector3.right;
        Invoke("ResetCollider", 0.9f);
    }
    void ResetCollider() {
        basicAttackCollider.size = baseColliderSize;
    }

    void AttackSpeed () {
        animator.SetFloat("AttackSpeed", GetComponent<Characteristics>().attackSpeed.x);
        animator.SetFloat("CastingSpeed", GetComponent<Characteristics>().castingSpeed.x);
    }



#region Voids for skills

    public void SkillHit(AnimationEvent skillID) {
        if (skillID.intParameter == 0) 
            AssetHolder.instance.Skills[skillID.intParameter].GetComponent<Dash>().Hit(skillID.floatParameter);
        else if (skillID.intParameter == 1) 
            AssetHolder.instance.Skills[skillID.intParameter].GetComponent<StrongAttack>().Hit();
        else if (skillID.intParameter == 2)
            AssetHolder.instance.Skills[skillID.intParameter].GetComponent<KO>().Hit(skillID.floatParameter);
        else if (skillID.intParameter == 5)
            AssetHolder.instance.Skills[skillID.intParameter].GetComponent<StoneHit>().ApplyDamage();
        else if (skillID.intParameter == 7)
            AssetHolder.instance.Skills[skillID.intParameter].GetComponent<Fireball>().FireProjectile();
    }

#endregion
}
