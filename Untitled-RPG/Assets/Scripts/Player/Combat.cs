using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    public static Combat instanace;

    public List<Enemy> enemiesInBattle = new List<Enemy>();

    [Space]
    public int maxSkillPoints = 10;
    public int availableSkillPoints;
    public SkillTree[] currentSkillTrees = new SkillTree[2];
    public List<Skill> learnedSkills = new List<Skill>();
    public List<Skill> currentPickedSkills = new List<Skill>(); 
    public List<Skill> currentSkillsFromEquipment = new List<Skill>();
    [Space]
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

    string savefilePath = "saves/combat.txt";

    void Awake() {
        instanace = this;
    }
    void Start() {
        Load();
        PeaceCanvas.saveGame += Save;
    }

    void Save () {
        //Save skill tress
        ES3.Save<SkillTree[]>("currentSkillTrees", currentSkillTrees, savefilePath);
        
        //Save learned skills
        List<int> skillIDs = new List<int>();
        foreach (Skill sk in learnedSkills)
            skillIDs.Add(sk.ID);
        ES3.Save<List<int>>("learnedSkillsIDs", skillIDs, savefilePath);

        //Save picked skills     
        skillIDs.Clear();
        foreach (Skill sk in currentPickedSkills) 
            skillIDs.Add(sk.ID);
        ES3.Save<List<int>>("currentPickedSkillsIDs", skillIDs, savefilePath);
    }
    void Load() {
        //Load current skill trees
        currentSkillTrees = ES3.Load<SkillTree[]>("currentSkillTrees", savefilePath, new SkillTree[2]);
        
        //Load learned skills
        List<int> skillIDs = ES3.Load<List<int>>("learnedSkillsIDs", savefilePath, new List<int>());
        foreach (int ID in skillIDs)
            learnedSkills.Add(AssetHolder.instance.getSkill(ID));
        
        //Load picked skills
        skillIDs = ES3.Load<List<int>>("currentPickedSkillsIDs", savefilePath, new List<int>());
        foreach (int ID in skillIDs)
            currentPickedSkills.Add(AssetHolder.instance.getSkill(ID));

        ValidateSkillSlots();
    }

    void Update() {
        PlayerControlls.instance.attackedByEnemies = enemiesInBattle.Count == 0 ? false : true;

        availableSkillPoints = maxSkillPoints - currentPickedSkills.Count;
    }
    
    public void SetCurrentSkillTrees(SkillTree skillTree , int skillTreeNumber) {
        currentSkillTrees[skillTreeNumber] = skillTree;
        ValidateCurrentSkills();
    }
    public void ValidateCurrentSkills () {
        for (int i = currentPickedSkills.Count-1; i >= 0; i--) { //for each skill
            bool valid = false;
            for (int i1 = 0; i1 < currentSkillTrees.Length; i1++){ //for each skill tree
                if (currentPickedSkills[i].skillTree == currentSkillTrees[i1])
                    valid = true;
            }
            if (!valid) currentPickedSkills.Remove(currentPickedSkills[i]);
        }
        ValidateSkillSlots();
    }
    public void ValidateSkillSlots () {
        for (int i = 0; i < allSkillSlots.Length; i++){ //validate each skill slot
            allSkillSlots[i].ValidateSkillSlot();
        }
    }

    public void LearnSkill (Skill skillToLearn) {
        if (learnedSkills.Contains(skillToLearn)) {
            CanvasScript.instance.DisplayWarning($"You already know {skillToLearn.skillName}");
        } else {
            learnedSkills.Add(skillToLearn);
        }
    }
    public void ForgetSkill(Skill skillToForget) {
        if (learnedSkills.Contains(skillToForget))
            learnedSkills.Remove(skillToForget);
    }

    public bool isPickedSkillTree (SkillTree skillTree) {
        bool p = false;
        for (int i = 0; i < currentSkillTrees.Length; i++){
            if (currentSkillTrees[i] == skillTree)
                p = true;
        }
        return p;
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
