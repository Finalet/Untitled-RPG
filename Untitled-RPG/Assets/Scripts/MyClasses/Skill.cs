﻿using System.Collections;
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
    [Tooltip("Damage after applying player's characteristics")] protected int actualDamage;
    public float coolDown;
    public float coolDownTimer;
    public bool isCoolingDown;

    [Header("Timings")]
    [Tooltip("Time needed to prepare and attack")] public float castingTime;
    [Tooltip("Total attack time, excluding casting (enemy can get hit during attackTime*(1-attackTimeOffset)")] public float totalAttackTime;
    public bool finishedCast;


    protected PlayerControlls playerControlls;
    protected Animator animator;
    protected Characteristics characteristics;
    protected AudioSource audioSource;

    public enum SkillTree {Knight, Hunter, Mage, Agnel, Stealth, Shield, Summoner };
    public enum SkillType {Damaging, Healing, Buff };

    protected virtual void Start() {
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
        if (!canBeUsed() || isCoolingDown || playerControlls.isRolling || playerControlls.isGettingHit)
            return;

        if (castingTime != 0)
            StartCoroutine(UseCoroutine());
        else 
            LocalUse();
    }

    protected virtual IEnumerator UseCoroutine (){
        if (playerControlls.isCastingSkill || !playerControlls.isIdle)
            yield break;

        CastingAnim();
        playerControlls.isCastingSkill = true;
        finishedCast = false;
        while (!finishedCast) {
            if (playerControlls.castInterrupted) { 
                InterruptCasting();
                playerControlls.castInterrupted = false;
                yield break;
            }
            yield return null;
        }
        playerControlls.isCastingSkill = false;
        LocalUse();
    }

    protected virtual void LocalUse () {
        playerControlls.InterruptCasting();
        coolDownTimer = coolDown;
        playerControlls.isAttacking = true;
        playerControlls.GetComponent<Characteristics>().UseOrRestoreStamina(staminaRequired);
        CustomUse();
    }

    protected virtual void CastingAnim () {}
    protected virtual void InterruptCasting () {}

    protected abstract void CustomUse(); // Custom code that is overriden in each skill seperately.

    protected virtual void Update() {
        if (coolDownTimer >= 0) {
            coolDownTimer -= Time.deltaTime;
            isCoolingDown = true;
        } else {
            isCoolingDown = false;
        }
    }

    public virtual bool canBeUsed () {
        if (playerControlls.isMounted)
            return false;

        if (skillTree == SkillTree.Knight) {
            if (playerControlls.isWeaponOut)
                return true;
            else 
                return false;
        } else if (skillTree == SkillTree.Mage) {
            return true;
        } else {
            return false;
        }
    }

}
