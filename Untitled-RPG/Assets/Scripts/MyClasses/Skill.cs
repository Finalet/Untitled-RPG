using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    public string skillName;
    public string description;
    [Tooltip("From what tree the skill is")] public SkillTree skillTree; 
    public SkillType skillType; 
    public int staminaRequired;
    [Tooltip("Base damage without player's characteristics")] public int baseDamage;
    public float coolDown;
    public float coolDownTimer;
    public bool isCoolingDown;

    [Header("Timings")]
    [Tooltip("Time needed to prepare and attack")] public float castingTime;
    [Tooltip("Offset in %. For instance, an attack animation takes some time to start actaully hitting - include that time into this offset")] [Range(0, 1)] public float startAttackTime;
    [Tooltip("Offset in %. For instance, an attack animation takes some time to start actaully hitting - include that time into this offset")] [Range(0, 1)] public float stopAttackTime = 1;
    [Tooltip("Total attack time, excluding casting (enemy can get hit during attackTime*(1-attackTimeOffset)")] public float totalAttackTime;
    
    Collider hitCollider;

    [System.NonSerialized] public PlayerControlls playerControlls;
    [System.NonSerialized] public Animator animator;

    public bool canHit;

    public enum SkillTree {Knight, Hunter, Mage, Agnel, Stealth, Shield, Summoner };
    public enum SkillType {Damaging, Healing };

    void Start() {
        hitCollider = GetComponent<Collider>();
        playerControlls = PlayerControlls.instance.GetComponent<PlayerControlls>();
        animator = PlayerControlls.instance.GetComponent<Animator>();
    }

    public virtual void Use() {
        print("Used skill " + skillName + ". Cool down " + coolDown + " seconds");
        coolDownTimer = coolDown;
        playerControlls.isAttacking = true;
        playerControlls.GetComponent<Combat>().currentSkillDamage = baseDamage; //Adjust later to characteristics
    }

    public virtual void Update() {
        if (coolDownTimer >= 0) {
            coolDownTimer -= Time.deltaTime;
            isCoolingDown = true;
        } else {
            isCoolingDown = false;
        }
    }


}
