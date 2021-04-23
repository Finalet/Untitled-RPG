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
    public int meleeAttack; public int meleeAttackFromEquip; public float meleeMultiplier;
    public int rangedAttack; public int rangedAttackFromEquip; public float rangedMultiplier;
    public int magicPower; public int magicPowerFromEquip; public float magicPowerMultiplier;
    public int healingPower; public int healingPowerFromEquip; public float healingPowerMultiplier;
    public int defense; public int defenseFromEquip; public float defenseMultiplier;
    
    [Header("Misc")]

    [Tooltip("X - casting speed percentage (i.e. 1.1f), Y - casting speed adjustement (i.e. 0.1f), Z - casting speed inverted (i.e. 0.9f)")]
    public Vector3 castingSpeed;
    float castingSpeedPercentage; float castingSpeedPercentageAdjustement; float castingSpeedPercentageIverted;
    [Tooltip("X - attack speed percentage (i.e. 1.0f), Y - attack speed adjustement (i.e. 0.1f), Z - attack speed inverted (i.e. 0.909f)")]
    public Vector3 attackSpeed;
    float attackSpeedPercentage; float attackSpeedPercentageAdjustement; float attackSpeedPercentageInverted;

    public float magicSkillDistanceIncrease;

    int statsRatio = 2;
    [Header("Stats regeneration")]
    public bool canRegenerateHealth; 
    public int HealthPointsPerSecond; 
    public bool canRegenerateStamina; public bool canUseStamina;
    public int StaminaPerSecond;

    public GameObject buffIcon;

    public List<Skill> activeBuffs = new List<Skill>();

    void Awake() {
        if (instance == null)
            instance = this;

        StatsCalculations();
    }

    void Start() {
        canGetHit = true;
        
        health = maxHealth;
        stamina = maxStamina;
        
        meleeMultiplier = 1;
        rangedMultiplier = 1;
        magicPowerMultiplier = 1;
        healingPowerMultiplier = 1;
        defenseMultiplier= 1;

        StatsCalculations();
    }

    void Update() {
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

        meleeAttack = Mathf.RoundToInt( ( (strength / statsRatio) + meleeAttackFromEquip) * meleeMultiplier);
        rangedAttack = Mathf.RoundToInt( ( (agility / statsRatio) + rangedAttackFromEquip) * rangedMultiplier);
        magicPower = Mathf.RoundToInt( ( (intellect / statsRatio) + magicPowerFromEquip) * magicPowerMultiplier);
        healingPower = Mathf.RoundToInt( ( (intellect / statsRatio) + healingPowerFromEquip) * healingPowerMultiplier);
        defense = Mathf.RoundToInt( ( (strength / statsRatio + agility / statsRatio) + defenseFromEquip) * defenseMultiplier);

        attackSpeedPercentage = 1 + attackSpeedPercentageAdjustement + agility*0.00025f;
        attackSpeedPercentageInverted = 1/attackSpeedPercentage;

        attackSpeed.x = attackSpeedPercentage;
        attackSpeed.y = attackSpeedPercentageAdjustement;
        attackSpeed.z = attackSpeedPercentageInverted;

        castingSpeedPercentage = 1 + castingSpeedPercentageAdjustement + intellect*0.00025f;
        castingSpeedPercentageIverted = 1/castingSpeedPercentage;

        castingSpeed.x = castingSpeedPercentage; 
        castingSpeed.y = castingSpeedPercentageAdjustement; 
        castingSpeed.z = castingSpeedPercentageIverted;
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
        } else if (stamina >= 0.3f*maxStamina) {
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

    public void AddBuff(Skill skill) {
        activeBuffs.Add(skill);
        GameObject icon = Instantiate(buffIcon, Vector3.zero, Quaternion.identity, CanvasScript.instance.buffs.transform);
        icon.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
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
            case 20: //Archers Practice
                PlayerControlls.instance.baseWalkSpeed += skill.GetComponent<ArchersPractice>().buffIncrease/100;
                attackSpeedPercentageAdjustement += skill.GetComponent<ArchersPractice>().buffIncrease/100;
                icon.GetComponent<BuffIcon>().timer = skill.GetComponent<ArchersPractice>().duration;
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
            case 20: 
                PlayerControlls.instance.baseWalkSpeed -= skill.GetComponent<ArchersPractice>().buffIncrease/100;
                attackSpeedPercentageAdjustement -= skill.GetComponent<ArchersPractice>().buffIncrease/100;
                break;
            default: Debug.LogError("Wrong skill ID for buff removal");
                break;
        }

        activeBuffs.Remove(skill);
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
