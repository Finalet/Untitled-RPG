using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Skill : MonoBehaviour
{
    public string skillName;
    public string description;
    public Sprite icon;
    [Tooltip("From what tree the skill is")] public SkillTree skillTree; 
    public SkillType skillType; 
    public int staminaRequired;
    [Tooltip("Base damage without player's characteristics")] public int baseDamage;
    [Tooltip("Damage after applying player's characteristics")] public int actualDamage;
    public float coolDown;
    public float coolDownTimer;
    public bool isCoolingDown;

    [Header("Timings")]
    [Tooltip("Time needed to prepare and attack")] public float castingTime;
    [Tooltip("Offset in %. For instance, an attack animation takes some time to start actaully hitting - include that time into this offset")] public float startAttackTime;
    [Tooltip("Offset in %. For instance, an attack animation takes some time to start actaully hitting - include that time into this offset")] public float stopAttackTime = 1;
    [Tooltip("Total attack time, excluding casting (enemy can get hit during attackTime*(1-attackTimeOffset)")] public float totalAttackTime;
    
    Collider hitCollider;

    [System.NonSerialized] public PlayerControlls playerControlls;
    [System.NonSerialized] public Animator animator;
    [System.NonSerialized] public Characteristics characteristics;

    public bool canHit;

    public enum SkillTree {Knight, Hunter, Mage, Agnel, Stealth, Shield, Summoner };
    public enum SkillType {Damaging, Healing, Buff };

    public virtual void Start() {
        hitCollider = GetComponent<Collider>();
        playerControlls = PlayerControlls.instance.GetComponent<PlayerControlls>();
        animator = PlayerControlls.instance.GetComponent<Animator>();
        characteristics = PlayerControlls.instance.GetComponent<Characteristics>();
    }

    public virtual void Use() {
        if (playerControlls.GetComponent<Characteristics>().Stamina < staminaRequired) {
            CanvasScript.instance.DisplayWarning("Not enough stamina!");
            return;
        }
        if (playerControlls.isMounted || isCoolingDown || !playerControlls.isWeaponOut || playerControlls.isRolling || playerControlls.isUsingSkill)
            return;

        playerControlls.isUsingSkill = true;
        coolDownTimer = coolDown;
        playerControlls.isAttacking = true;
        playerControlls.GetComponent<Characteristics>().UseOrRestoreStamina(staminaRequired);
        if (skillType == SkillType.Damaging) Invoke("usingSkill", totalAttackTime);

        gameObject.SendMessage("CustomUse", null, SendMessageOptions.DontRequireReceiver);
    }

    void usingSkill () {
        playerControlls.isUsingSkill = false;
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
