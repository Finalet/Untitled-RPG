using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour, ISavable
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
    
    private int _currentSkillSlotsRow;
    public int currentSkillSlotsRow {
        get {
            return _currentSkillSlotsRow;
        }
        set {
            _currentSkillSlotsRow = value;
            for (int i = 0; i < allSkillSlots.Length; i++){ 
                allSkillSlots[i].SwitchRows(_currentSkillSlotsRow);
            }
        }
    }
    [Space]
    [System.NonSerialized] public int numberOfSkillSlotsRows = 2;
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
        SaveManager.instance.saveObjects.Add(this);
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
    public void SwitchSkillRows (int row = -1) {
        if (PeaceCanvas.instance.isDraggingItemOrSkill || PeaceCanvas.instance.isGamePaused) return;
        
        currentSkillSlotsRow = row == -1 ? (currentSkillSlotsRow + 1) % numberOfSkillSlotsRows : row;
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

    public void DisplayDamageNumber(DamageInfo damageInfo, Vector3 position) {
        GameObject ddText = Instantiate(AssetHolder.instance.ddText, position, Quaternion.identity);
        ddText.GetComponent<ddText>().Init(damageInfo);
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
            case 26: 
                skill.GetComponent<ShieldSlam>().Hit(skillID.floatParameter);
                break;
            case 101: 
                if (skillID.floatParameter == 0) 
                    skill.GetComponent<CaptainThrow>().ThrowShield();
                break; 
        }
    }

#endregion

#region  Savable

    public LoadPriority loadPriority {
        get {
            return LoadPriority.First;
        }
    }

    public void Save () {
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

        //Save skill slots
        for (int i = 0; i < allSkillSlots.Length; i++){ 
            allSkillSlots[i].SaveSlot();
        }

        //Save row number for slots 
        ES3.Save<int>("rowIndex", currentSkillSlotsRow, savefilePath);
    }
    public void Load() {
        //Load current skill trees
        SkillTree[] defaultSkilltrees = new SkillTree[2];
        defaultSkilltrees[0] = SkillTree.Knight;
        defaultSkilltrees[1] = SkillTree.Hunter;

        currentSkillTrees = ES3.Load<SkillTree[]>("currentSkillTrees", savefilePath, defaultSkilltrees);
        
        //Load learned skills
        List<int> skillIDs = ES3.Load<List<int>>("learnedSkillsIDs", savefilePath, new List<int>());
        foreach (int ID in skillIDs)
            learnedSkills.Add(AssetHolder.instance.getSkill(ID));
        
        //Load picked skills
        skillIDs = ES3.Load<List<int>>("currentPickedSkillsIDs", savefilePath, new List<int>());
        foreach (int ID in skillIDs)
            currentPickedSkills.Add(AssetHolder.instance.getSkill(ID));

        //Load all skill slots 
        for (int i = 0; i < allSkillSlots.Length; i++){
            allSkillSlots[i].LoadSlot();
        }

        //Load row number for slots 
        SwitchSkillRows(ES3.Load<int>("rowIndex", savefilePath, 0));
    }

#endregion
}
