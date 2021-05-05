using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    public static Combat instanace;

    public List<Enemy> enemiesInBattle = new List<Enemy>();

    [Space]
    public SkillTree[] currentSkillTrees = new SkillTree[2];
    public UI_SkillPanelSlot[] allSkillSlots;

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

    void Awake() {
        instanace = this;
    }

    void Update() {
        PlayerControlls.instance.attackedByEnemies = enemiesInBattle.Count == 0 ? false : true;
    }
    
    public void SetCurrentSkillTrees(SkillTree skillTree , int skillTreeNumber) {
        currentSkillTrees[skillTreeNumber] = skillTree;
        ValidateCurrentSkills();
    }
    void ValidateCurrentSkills () {
        for (int i = 0; i < allSkillSlots.Length; i++){ //for each skill slot
            if (allSkillSlots[i].skillInSlot != null) {
                bool valid = false;
                for (int i1 = 0; i1 < currentSkillTrees.Length; i1++){ //for each skill tree
                    if (allSkillSlots[i].skillInSlot.skillTree == currentSkillTrees[i1]) 
                        valid = true;
                }
                if (!valid) allSkillSlots[i].ClearSlot();
            }
        }
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
