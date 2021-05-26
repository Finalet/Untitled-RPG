using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Characteristics : MonoBehaviour
{

    public static Characteristics instance;

    public bool canGetHit;
    [Header("Main")]
    public int maxHealth; public int healthFromEquip;
    public int maxStamina; public int staminaFromEquip;
    public int health; 
    public int stamina; 
    [Header("Stats")]
    public int strength; public int strengthFromEquip;
    public int agility; public int agilityFromEquip;
    public int intellect; public int intellectFromEquip;
    [Header("Attacks")]
    public int meleeAttack; public int meleeAttackFromEquip;
    public int rangedAttack; public int rangedAttackFromEquip;
    public int magicPower; public int magicPowerFromEquip;
    public int healingPower; public int healingPowerFromEquip;
    public int defense; public int defenseFromEquip;
    
    [Header("Speeds")]
    [Tooltip("X - Casting speed percentage (i.e. 1.1f), Y - Casting speed inverted (i.e. 0.9f)")]
    public Vector2 castingSpeed; public Vector2 castingSpeedFromEquip; //x = 1.1; y = 0.9;
    [Tooltip("X - Attack speed percentage (i.e. 1.0f), Y - Attack speed inverted (i.e. 0.909f)")]
    public Vector2 attackSpeed; public Vector2 attackSpeedFromEquip; //x = 1.1; y = 0.9;

    [Header("Buffs")]
    public float meleeAttackBuff;
    public float rangedAttackBuff;
    public float magicPowerBuff;
    public float healingPowerBuff;
    public float defenseBuff;
    public Vector2 castingSpeedBuff;
    public Vector2 attackSpeedBuff;
    public float walkSpeedBuff;
    public float skillDistanceIncrease;

    int statsRatio = 2;
    [Header("Stats regeneration")]
    public bool canRegenerateHealth; 
    public int HealthPointsPerSecond; 
    public bool canRegenerateStamina; public bool canUseStamina;
    public int StaminaPerSecond;

    public GameObject buffIcon;

    public List<Buff> activeBuffs = new List<Buff>();

    void Awake() {
        if (instance == null)
            instance = this;

        StatsCalculations();
    }

    void Start() {
        canGetHit = true;
        
        health = maxHealth;
        stamina = maxStamina;
        
        meleeAttackBuff = 1;
        rangedAttackBuff = 1;
        magicPowerBuff = 1;
        healingPowerBuff = 1;
        defenseBuff= 1;

        StatsCalculations();
    }

    void Update() {
        CalculateBuffs();
        StatsCalculations();
        regenerateHealth();
        regenerateStamina();
    }

    public void StatsCalculations () {
        strength = Mathf.RoundToInt(strengthFromEquip);
        agility = Mathf.RoundToInt(agilityFromEquip);
        intellect = Mathf.RoundToInt(intellectFromEquip);

        maxHealth = 10000 + (strength / statsRatio) + healthFromEquip;
        maxStamina = 0 + ((agility + intellect) / statsRatio) + staminaFromEquip;

        health = Mathf.Clamp(health, 0, maxHealth);
        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        meleeAttack = Mathf.RoundToInt( ( (strength / statsRatio) + meleeAttackFromEquip) * meleeAttackBuff);
        rangedAttack = Mathf.RoundToInt( ( (agility / statsRatio) + rangedAttackFromEquip) * rangedAttackBuff);
        magicPower = Mathf.RoundToInt( ( (intellect / statsRatio) + magicPowerFromEquip) * magicPowerBuff);
        healingPower = Mathf.RoundToInt( ( (intellect / statsRatio) + healingPowerFromEquip) * healingPowerBuff);
        defense = Mathf.RoundToInt( ( (strength / statsRatio + agility / statsRatio) + defenseFromEquip) * defenseBuff);

        attackSpeed.x = 1 * attackSpeedFromEquip.x * (1+agility*0.0001f) * attackSpeedBuff.x;
        attackSpeed.y = 1 * attackSpeedFromEquip.y * (1-agility*0.0001f) * attackSpeedBuff.y;

        castingSpeed.x = 1 * castingSpeedFromEquip.x * (1+intellect*0.0001f) * castingSpeedBuff.x;
        castingSpeed.y = 1 * castingSpeedFromEquip.y * (1-intellect*0.0001f) * castingSpeedBuff.y;
    }

    float hpTimer = 1;
    void regenerateHealth() {
        if (health == maxHealth)
            return;

        if (canRegenerateHealth && health < maxHealth) {
            if (hpTimer <= 0) {
                health += PlayerControlls.instance.attackedByEnemies ? HealthPointsPerSecond/10 : HealthPointsPerSecond/2;
                hpTimer = 0.1f;
            } else {
                hpTimer -= Time.deltaTime;
            }
        }
    }
    bool hidingStamina;
    float staminaTimer = 0;
    float timer = 0;
    float afterUseTimer = 0;
    void regenerateStamina() {
        if (maxStamina == 0) {
            canUseStamina = false;
            stamina = 0;
            return;
        }

        if (stamina == maxStamina) {
            if (!hidingStamina){
                hidingStamina = true;
                timer = Time.time;
            }
            if (Time.time - timer >= 1f) {
                CanvasScript.instance.HideStamina();
            }
            canUseStamina = true;
            return;
        }
        if (stamina <= 0) { //blocks use of stamina untill fully restored;
            canUseStamina = false;
        } else if (stamina >= 100) {
            canUseStamina = true;
        }

        if (Time.time - afterUseTimer >= 1.5f) { //adds a break between stamina use and stamina regeneration
            canRegenerateStamina = true;
        }

        CanvasScript.instance.ShowStamina();
        hidingStamina = false;

        if (canRegenerateStamina) {
            if (Time.time - staminaTimer >= 1f/StaminaPerSecond) {
                stamina ++;
                staminaTimer = Time.time;
            }
        }
    }
    
    void DisplayDamageNumber(int damage) {
        GameObject ddText = Instantiate(AssetHolder.instance.ddText, transform.position + Vector3.up * 1f, Quaternion.identity);
        ddText.GetComponent<ddText>().damageInfo = new DamageInfo(damage, false);
        ddText.GetComponent<ddText>().isPlayer = true;
    }
    void DisplayHealNumber(int healAmount) {
        GameObject ddText = Instantiate(AssetHolder.instance.ddText, transform.position + Vector3.up * 2f, Quaternion.identity);
        ddText.GetComponent<ddText>().healAmount = healAmount;
        ddText.GetComponent<ddText>().isPlayer = true;
    }
    void DisplayStaminaNumber(int staminaAmount) {
        GameObject ddText = Instantiate(AssetHolder.instance.ddText, transform.position + Vector3.up * 2f, Quaternion.identity);
        ddText.GetComponent<ddText>().staminaAmount = staminaAmount;
        ddText.GetComponent<ddText>().isPlayer = true;
    }

    public void UseOrRestoreStamina (int amount) {
        canRegenerateStamina = false;
        afterUseTimer = Time.time;
        stamina -= amount;
    }

    public void AddBuff(Buff buff) {
        activeBuffs.Add(buff);
        BuffIcon icon = Instantiate(buffIcon, CanvasScript.instance.buffs.transform).GetComponent<BuffIcon>();
        icon.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
        icon.buff = buff;
        /*
        activeBuffs.Add(skill);
        GameObject icon = Instantiate(buffIcon, Vector3.zero, Quaternion.identity, CanvasScript.instance.buffs.transform);
        icon.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
        icon.GetComponent<BuffIcon>().skill = skill;

        switch (skill.ID) {
            case 5: //Rage 
                float buffIncrease = skill.GetComponent<Rage>().buffIncrease;
                meleeMultiplier += buffIncrease;
                
                attackSpeedBuff *= new Vector2( (1+buffIncrease), (1-buffIncrease) );
                icon.GetComponent<BuffIcon>().timer = skill.GetComponent<Rage>().duration;
                break;
            case 12: //Power sphere
                magicPowerMultiplier += skill.GetComponent<PowerSphere>().magicDamageIncrease;
                castingSpeedBuff += skill.GetComponent<PowerSphere>().castSpeedIncrease;
                defenseMultiplier += skill.GetComponent<PowerSphere>().defenseIncrease;
                //No timer since its active while player is indside;
                break;
            case 14: // Levitation
                magicSkillDistanceIncrease += skill.GetComponent<Levitation>().skillDistanceIncrease;
                magicPowerMultiplier += skill.GetComponent<Levitation>().magicPowerPercentageIncrease/100;
                icon.GetComponent<BuffIcon>().timer = skill.GetComponent<Levitation>().flightDuration;
                break;
            case 20: //Archers Practice
                PlayerControlls.instance.baseWalkSpeed += skill.GetComponent<ArchersPractice>().buffIncrease;
                attackSpeedBuff *= new Vector2( (1+skill.GetComponent<ArchersPractice>().buffIncrease), (1-skill.GetComponent<ArchersPractice>().buffIncrease) );
                icon.GetComponent<BuffIcon>().timer = skill.GetComponent<ArchersPractice>().duration;
                break;
            default: Debug.LogError("Wrong skill ID for adding buff");
                break;
        }    */
    }

    public void RemoveBuff(Buff buff) { 
        if (activeBuffs.Contains(buff))
            activeBuffs.Remove(buff);
        /* switch (skill.ID) {
            case 5:
                float buffIncrease = skill.GetComponent<Rage>().buffIncrease;
                meleeMultiplier -= buffIncrease;
                attackSpeedBuff -= buffIncrease;
                break;
            case 12:
                magicPowerMultiplier -= skill.GetComponent<PowerSphere>().magicDamageIncrease;
                castingSpeedBuff -= skill.GetComponent<PowerSphere>().castSpeedIncrease;
                defenseMultiplier -= skill.GetComponent<PowerSphere>().defenseIncrease;
                break;
            case 14:
                magicSkillDistanceIncrease -= skill.GetComponent<Levitation>().skillDistanceIncrease;
                magicPowerMultiplier -= skill.GetComponent<Levitation>().magicPowerPercentageIncrease/100;
                break;
            case 20: 
                PlayerControlls.instance.baseWalkSpeed -= skill.GetComponent<ArchersPractice>().buffIncrease/100;
                attackSpeedBuff -= skill.GetComponent<ArchersPractice>().buffIncrease/100;
                break;
            default: Debug.LogError("Wrong skill ID for buff removal");
                break;
        }

        activeBuffs.Remove(skill); */
    } 

    void CalculateBuffs() {
        ResetBuffStats();
        for (int i = 0; i < activeBuffs.Count; i++){
            meleeAttackBuff *= 1 + activeBuffs[i].meleeAttackBuff;
            rangedAttackBuff *= 1 + activeBuffs[i].rangedAttackBuff;
            magicPowerBuff *= 1 + activeBuffs[i].magicPowerBuff;
            healingPowerBuff *= 1 + activeBuffs[i].healingPowerBuff;
            defenseBuff *= 1 + activeBuffs[i].defenseBuff;

            castingSpeedBuff.x *= (1-activeBuffs[i].castingSpeedBuff);
            castingSpeedBuff.y *= (1+activeBuffs[i].castingSpeedBuff);

            attackSpeedBuff.x *= (1-activeBuffs[i].attackSpeedBuff);
            attackSpeedBuff.y *= (1+activeBuffs[i].attackSpeedBuff);

            walkSpeedBuff *= (1+activeBuffs[i].walkSpeedBuff);

            skillDistanceIncrease +=activeBuffs[i].skillDistanceBuff;
        }
    }
    void ResetBuffStats() {
        meleeAttackBuff = 1;
        rangedAttackBuff = 1;
        magicPowerBuff = 1;
        healingPowerBuff = 1;
        defenseBuff = 1;

        castingSpeedBuff = Vector2.one;
        attackSpeedBuff = Vector2.one;
        
        walkSpeedBuff = 1;

        skillDistanceIncrease = 0;
    }

#region Get hit overloads 

    public void GetHit (int damage, HitType hitType = HitType.Normal, float cameraShakeFrequency = 0, float cameraShakeAmplitude = 0) {
        if (!canGetHit)
            return;


        if (hitType == HitType.Normal) {
            PlayerControlls.instance.animator.CrossFade("GetHitUpperBody.GetHit", 0.1f, PlayerControlls.instance.animator.GetLayerIndex("GetHitUpperBody"), 0);
        } else if (hitType == HitType.Interrupt) {
            PlayerControlls.instance.animator.CrossFade("GetHit.GetHit", 0.1f, PlayerControlls.instance.animator.GetLayerIndex("GetHit"), 0);
            PlayerControlls.instance.InterruptCasting();
        }

        int actualDamage = Mathf.RoundToInt(damage); 
        health -= damage;
        DisplayDamageNumber(damage);
        GetComponent<PlayerAudioController>().PlayGetHitSound();

        if (cameraShakeFrequency != 0 && cameraShakeAmplitude != 0)
            PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(cameraShakeFrequency, cameraShakeAmplitude, 0.1f, transform.position);
    }

#endregion

    public void GetHealed(int healAmount) {
        health += healAmount;
        DisplayHealNumber(healAmount);
    }
    public void GetStamina(int staminaAmount) {
        stamina += staminaAmount;
        DisplayStaminaNumber(staminaAmount);
    }
}
