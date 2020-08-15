using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Characteristics : MonoBehaviour
{

    public static Characteristics instance;

    public bool canGetHit;
    [Header("Main")]
    public int maxHP;
    public int maxStamina;
    public int HP;
    public int Stamina;
    [Header("Stats")]
    public int strength;
    public int agility;
    public int intellect;
    [Header("Attacks")]
    public int meleeAttack; public float meleeMultiplier;
    public int rangedAttack; public float rangedMultiplier;
    public int magicPower; public float magicPowerMultiplier;
    public int healingPower; public float healingPowerMultiplier;
    public int defense; public float defenseMultiplier;
    
    [Header("Misc")]

    [Tooltip("X - casting speed percentage, Y - casting speed adjustement, Z - casting speed inverted")]
    public Vector3 castingSpeed;
    float castingSpeedPercentage; float castingSpeedPercentageAdjustement; float castingSpeedPercentageIverted;
    [Tooltip("X - attack speed percentage, Y - attack speed adjustement, Z - attack speed inverted")]
    public Vector3 attackSpeed;
    float attackSpeedPercentage; float attackSpeedPercentageAdjustement; float attackSpeedPercentageInverted;

    public float magicSkillDistanceIncrease;

    int statsRatio = 2;
    [Header("Stats regeneration")]
    public bool canRegenerateHealth; 
    public int HealthPointsPerSecond; 
    public bool canRegenerateStamina; 
    public int baseStaminaPerSecond; 
    int StaminaPerSecond; 

    public GameObject buffIcon;

    public List<Skill> activeBuffs = new List<Skill>();

    void Awake() {
        if (instance == null)
            instance = this;

        StatsCalculations();
    }

    void Start() {
        canGetHit = true;

        HP = maxHP;
        Stamina = maxStamina;

        meleeMultiplier = 1;
        rangedMultiplier = 1;
        magicPowerMultiplier = 1;
        healingPowerMultiplier = 1;
        defenseMultiplier= 1;

    }

    void Update() {
        StatsCalculations();
        regenerateHealth();
        regenerateStamina();
    }

    void StatsCalculations () {
        maxHP = 1000 + strength / statsRatio;
        maxStamina = 1000 + (agility + intellect) / statsRatio;

        HP = Mathf.Clamp(HP, 0, maxHP);
        Stamina = Mathf.Clamp(Stamina, 0, maxStamina);

        meleeAttack = Mathf.RoundToInt( (strength / statsRatio) * meleeMultiplier);
        rangedAttack = Mathf.RoundToInt( (agility / statsRatio) * rangedMultiplier);
        magicPower = Mathf.RoundToInt( (intellect / statsRatio) * magicPowerMultiplier);
        healingPower = Mathf.RoundToInt( (intellect / statsRatio) * healingPowerMultiplier);
        defense = Mathf.RoundToInt( (strength / statsRatio + agility / statsRatio) * defenseMultiplier);

        attackSpeedPercentage = 1 + attackSpeedPercentageAdjustement;
        attackSpeedPercentageInverted = 1/attackSpeedPercentage;

        attackSpeed.x = attackSpeedPercentage;
        attackSpeed.y = attackSpeedPercentageAdjustement;
        attackSpeed.z = attackSpeedPercentageInverted;

        castingSpeedPercentage = 1 + castingSpeedPercentageAdjustement;
        castingSpeedPercentageIverted = 1/castingSpeedPercentage;

        castingSpeed.x = castingSpeedPercentage; 
        castingSpeed.y = castingSpeedPercentageAdjustement; 
        castingSpeed.z = castingSpeedPercentageIverted;
    }

    float hpTimer = 1;
    void regenerateHealth() {
        if (HP == maxHP)
            return;

        if (canRegenerateHealth && HP < maxHP) {
            if (hpTimer <= 0) {
                HP += HealthPointsPerSecond/10;
                hpTimer = 0.1f;
            } else {
                hpTimer -= Time.deltaTime;
            }
        }
    }
    float staminaTimer = 1;
    void regenerateStamina() {
        if (Stamina == maxStamina)
            return;

        if (PlayerControlls.instance.isSprinting) {
            StaminaPerSecond = -40;
        } else {
            StaminaPerSecond = baseStaminaPerSecond;
        }

        if (canRegenerateStamina) {
            if (staminaTimer <= 0) {
                Stamina += StaminaPerSecond/10;
                staminaTimer = 0.1f;
            } else {
                staminaTimer -= Time.deltaTime;
            }
        }
    }
    
    void DisplayDamageNumber(int damage) {
        GameObject ddText = Instantiate(AssetHolder.instance.ddText, transform.position + Vector3.up * 1f, Quaternion.identity);
        ddText.GetComponent<ddText>().damage = damage;
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
        Stamina -= amount;
    }

    public void AddBuff(Skill skill) {
        activeBuffs.Add(skill);
        GameObject icon = Instantiate(buffIcon, Vector3.zero, Quaternion.identity, CanvasScript.instance.buffs.transform);
        icon.GetComponent<BuffIcon>().skill = skill;

        switch (skill.ID) {
            case 5: //Rage 
                float buffIncrease = skill.GetComponent<Rage>().buffIncrease;
                meleeMultiplier += buffIncrease/100;
                attackSpeedPercentageAdjustement += buffIncrease/100;
                icon.GetComponent<BuffIcon>().timer = skill.GetComponent<Rage>().duration;
                break;
            case 12: //Power sphere
                magicPowerMultiplier += skill.GetComponent<PowerSphere>().magicDamageIncrease;
                castingSpeedPercentageAdjustement += skill.GetComponent<PowerSphere>().castSpeedIncrease;
                defenseMultiplier += skill.GetComponent<PowerSphere>().defenseIncrease;
                //No timer since its active while player is indside;
                break;
            case 14: // Levitation
                magicSkillDistanceIncrease += skill.GetComponent<Levitation>().skillDistanceIncrease;
                magicPowerMultiplier += skill.GetComponent<Levitation>().magicPowerPercentageIncrease/100;
                icon.GetComponent<BuffIcon>().timer = skill.GetComponent<Levitation>().flightDuration;
                break;
            default: Debug.LogError("Wrong skill ID for adding buff");
                break;
        }    
    }

    public void RemoveBuff(Skill skill) {
        switch (skill.ID) {
            case 5:
                float buffIncrease = skill.GetComponent<Rage>().buffIncrease;
                meleeMultiplier -= buffIncrease/100;
                attackSpeedPercentageAdjustement -= buffIncrease/100;
                break;
            case 12:
                magicPowerMultiplier -= skill.GetComponent<PowerSphere>().magicDamageIncrease;
                castingSpeedPercentageAdjustement -= skill.GetComponent<PowerSphere>().castSpeedIncrease;
                defenseMultiplier -= skill.GetComponent<PowerSphere>().defenseIncrease;
                break;
            case 14:
                magicSkillDistanceIncrease -= skill.GetComponent<Levitation>().skillDistanceIncrease;
                magicPowerMultiplier -= skill.GetComponent<Levitation>().magicPowerPercentageIncrease/100;
                break;
            default: Debug.LogError("Wrong skill ID for buff removal");
                break;
        }

        activeBuffs.Remove(skill);
    }

#region Get hit overloads 
    bool checkFailed () {
        if (!canGetHit)
            return true;
        else   
            return false;
    }

    void BasicGetHit (int damage) {
        PlayerControlls.instance.InterruptCasting();
        PlayerControlls.instance.animator.CrossFade("GetHit.GetHit", 0.25f, PlayerControlls.instance.animator.GetLayerIndex("GetHit"), 0);
        int actualDamage = Mathf.RoundToInt(damage); 
        HP -= damage;
        DisplayDamageNumber(damage);
        GetComponent<PlayerAudioController>().PlayGetHitSound();
    }

    public void GetHit (int damage) {
        if (checkFailed())
            return;

        BasicGetHit(damage);
    }
    public void GetHit (int damage, float cameraShakeFrequency, float cameraShakeAmplitude) {
        if (checkFailed())
            return;

        BasicGetHit(damage);
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(cameraShakeFrequency, cameraShakeAmplitude, 0.1f, transform.position);
    }

#endregion

    public void GetHealed(int healAmount) {
        HP += healAmount;
        DisplayHealNumber(healAmount);
    }
    public void GetStamina(int staminaAmount) {
        Stamina += staminaAmount;
        DisplayStaminaNumber(staminaAmount);
    }
}
