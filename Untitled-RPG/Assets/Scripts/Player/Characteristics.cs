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

        strength = Mathf.RoundToInt(strengthFromEquip);
        agility = Mathf.RoundToInt(agilityFromEquip);
        intellect = Mathf.RoundToInt(intellectFromEquip);

        maxHP = 10000 + strength / statsRatio;
        maxStamina = 0 + (agility + intellect) / statsRatio;

        HP = Mathf.Clamp(HP, 0, maxHP);
        Stamina = Mathf.Clamp(Stamina, 0, maxStamina);

        meleeAttack = Mathf.RoundToInt( ( (strength / statsRatio) + meleeAttackFromEquip) * meleeMultiplier);
        rangedAttack = Mathf.RoundToInt( ( (agility / statsRatio) + rangedAttackFromEquip) * rangedMultiplier);
        magicPower = Mathf.RoundToInt( ( (intellect / statsRatio) + magicPowerFromEquip) * magicPowerMultiplier);
        healingPower = Mathf.RoundToInt( ( (intellect / statsRatio) + healingPowerFromEquip) * healingPowerMultiplier);
        defense = Mathf.RoundToInt( ( (strength / statsRatio + agility / statsRatio) + defenseFromEquip) * defenseMultiplier);

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
    bool hidingStamina;
    float staminaTimer = 0;
    float timer = 0;
    float afterUseTimer = 0;
    void regenerateStamina() {
        if (Stamina == maxStamina) {
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
        if (Stamina <= 0) { //blocks use of stamina untill fully restored;
            canUseStamina = false;
        } else if (Stamina >= 0.3f*maxStamina) {
            canUseStamina = true;
        }

        if (Time.time - afterUseTimer >= 1.5f) { //adds a break between stamina use and stamina regeneration
            canRegenerateStamina = true;
        }

        CanvasScript.instance.ShowStamina();
        hidingStamina = false;

        if (canRegenerateStamina) {
            if (Time.time - staminaTimer >= 1f/StaminaPerSecond) {
                Stamina ++;
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
        Stamina -= amount;
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
        HP -= damage;
        DisplayDamageNumber(damage);
        GetComponent<PlayerAudioController>().PlayGetHitSound();

        if (cameraShakeFrequency != 0 && cameraShakeAmplitude != 0)
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
