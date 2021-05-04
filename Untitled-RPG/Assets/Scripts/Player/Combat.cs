﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    public static Combat instanace;

    public List<Enemy> enemiesInBattle = new List<Enemy>();
    
    [Space]
    public bool blockSkills;

    AimingSkill aimingSkill;
    public AimingSkill AimingSkill {
        get {
            return aimingSkill;
        }
        set {
            aimingSkill = value;
            PlayerControlls.instance.isAimingSkill = aimingSkill == null ? false : true;
        }
    }

    Animator animator;

    void Awake() {
        instanace = this;
    }

    void Start() {  
        animator = GetComponent<Animator>();
    }

    void Update() {
        PlayerControlls.instance.attackedByEnemies = enemiesInBattle.Count == 0 ? false : true;
    }



#region Voids for skills

    public void SkillHit(AnimationEvent skillID) {
        Skill skill = AssetHolder.instance.getSkill(skillID.intParameter);
        switch (skillID.intParameter) {
            case 0: skill.GetComponent<Slash>().Hit();
                break;
            case 1: skill.GetComponent<Dash>().Hit(skillID.floatParameter);
                break;
            case 2: skill.GetComponent<StrongAttack>().Hit();
                break;
            case 3: skill.GetComponent<KO>().Hit(skillID.floatParameter);
                break;
            case 6: skill.GetComponent<StoneHit>().ApplyDamage();
                break;
            case 9: skill.GetComponent<Fireball>().FireProjectile();
                break;
            case 11: skill.GetComponent<Hailstone>().FireProjectile();
                break;
            case 12: 
                if (skillID.floatParameter == 1)
                    skill.GetComponent<PowerSphere>().SpawnSphere();
                else if (skillID.floatParameter == 0)
                    skill.GetComponent<PowerSphere>().ShootSphere();
                break;
            case 15: skill.GetComponent<Armageddon>().StartHell();
                break;
            case 16: skill.GetComponent<SimpleBowShot>().GrabBowstring();
                break;
            case 17: 
                if (skillID.floatParameter == 0) 
                    skill.GetComponent<StrongArrow>().GrabBowstring();
                else if (skillID.floatParameter == 1)
                    skill.GetComponent<StrongArrow>().Shoot();
                break;
            case 18: 
                if (skillID.floatParameter == 0) 
                    skill.GetComponent<PoisonArrow>().GrabBowstring();
                else if (skillID.floatParameter == 1)
                    skill.GetComponent<PoisonArrow>().Shoot();
                break;
            case 19: 
                if (skillID.floatParameter == 0) 
                    skill.GetComponent<ExplosiveArrow>().GrabBowstring();
                else if (skillID.floatParameter == 1)
                    skill.GetComponent<ExplosiveArrow>().Shoot();
                break;
            case 21: 
                if (skillID.floatParameter == 0) 
                    skill.GetComponent<HuntersCage>().GrabBowstring();
                else if (skillID.floatParameter == 1)
                    skill.GetComponent<HuntersCage>().Shoot();
                break;
            case 22: 
                if (skillID.floatParameter == 0) 
                    skill.GetComponent<ArrowSet>().GrabBowstring();
                else if (skillID.floatParameter == 1)
                    skill.GetComponent<ArrowSet>().Shoot();
                else if (skillID.floatParameter == 2)
                    skill.GetComponent<ArrowSet>().Shoot(true);
                break;
            case 23: 
                if (skillID.floatParameter == 0) 
                    skill.GetComponent<RainOfArrows>().GrabBowstring();
                else if (skillID.floatParameter == 1)
                    skill.GetComponent<RainOfArrows>().Shoot();
                else if (skillID.floatParameter == 2)
                    skill.GetComponent<RainOfArrows>().Shoot(true);
                break;
        }
    }

#endregion
}
