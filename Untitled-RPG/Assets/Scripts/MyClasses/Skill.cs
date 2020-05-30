using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Skill : MonoBehaviour
{
    public int ID;
    [Space]
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
    [Tooltip("Total attack time, excluding casting (enemy can get hit during attackTime*(1-attackTimeOffset)")] public float totalAttackTime;
    
    Collider hitCollider;

    [System.NonSerialized] public PlayerControlls playerControlls;
    [System.NonSerialized] public Animator animator;
    [System.NonSerialized] public Characteristics characteristics;
    [System.NonSerialized] public AudioSource audioSource;

    public enum SkillTree {Knight, Hunter, Mage, Agnel, Stealth, Shield, Summoner };
    public enum SkillType {Damaging, Healing, Buff };

    protected virtual void Start() {
        hitCollider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        playerControlls = PlayerControlls.instance.GetComponent<PlayerControlls>();
        animator = PlayerControlls.instance.GetComponent<Animator>();
        characteristics = PlayerControlls.instance.GetComponent<Characteristics>();
    }

    public virtual void Use() { //Virtual, because sometimes need to be overriden, for instance in the Target skill.
        if (playerControlls.GetComponent<Characteristics>().Stamina < staminaRequired) {
            CanvasScript.instance.DisplayWarning("Not enough stamina!");
            return;
        }
        if (playerControlls.isMounted || isCoolingDown || !playerControlls.isWeaponOut || playerControlls.isRolling || playerControlls.isUsingSkill || playerControlls.isGettingHit)
            return;

        playerControlls.isUsingSkill = true;
        coolDownTimer = coolDown;
        playerControlls.isAttacking = true;
        playerControlls.GetComponent<Characteristics>().UseOrRestoreStamina(staminaRequired);
        if (skillType == SkillType.Damaging) Invoke("usingSkill", totalAttackTime);
        CustomUse();
    }

    protected abstract void CustomUse(); // Custom code that is overriden in each skill seperately.

    void usingSkill () { //Indicates that the skill is not being used anymore.
        playerControlls.isUsingSkill = false;
    }

    protected virtual void Update() {
        if (coolDownTimer >= 0) {
            coolDownTimer -= Time.deltaTime;
            isCoolingDown = true;
        } else {
            isCoolingDown = false;
        }
    }

}
