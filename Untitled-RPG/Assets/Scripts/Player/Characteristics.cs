using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable] public struct BuffAndIcon {public Buff buff1; [System.NonSerialized] public BuffIcon icon1;}

public class Characteristics : MonoBehaviour
{

    public static Characteristics instance;

    public string playerName;
    public bool isDead;
    [Space]
    public bool immuneToDamage; int immuneToDamageInt;
    public bool immuneToInterrupt; int immuneToInterruptInt;
    
    [Header("Main")]
    public int maxHealth; public int healthFromEquip; int healthFromBuff;
    public int maxStamina; public int staminaFromEquip; int staminaFromBuff;
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
    //X - Casting speed percentage (i.e. 1.1f), Y - Casting speed inverted (i.e. 0.9f)
    public Vector2 castingSpeed; public Vector2 castingSpeedFromEquip; //x = 1.1; y = 0.9;
    //X - Attack speed percentage (i.e. 1.0f), Y - Attack speed inverted (i.e. 0.909f)
    public Vector2 attackSpeed; public Vector2 attackSpeedFromEquip; //x = 1.1; y = 0.9;
    [Header("Misc")]
    public float critChance; public float critChanceFromEquip; public float critChanceFromBuff; float baseCritChance = 0.05f;
    public float critStrength; float baseCritStrength = 2; public float critStrengthFromEquip; public float critStrengthFromBuff;
    public float blockChance; public float blockChanceFromEquip; public float blockChanceFromBuff;
    public float walkSpeed; public float walkSpeedBuff; public float walkSpeedFromEquipment;

    [Header("Buffs")]
    public float meleeAttackBuff;
    public float rangedAttackBuff;
    public float magicPowerBuff;
    public float healingPowerBuff;
    public float defenseBuff;
    public float strengthBuff;
    public float agilityBuff;
    public float intellectBuff;
    public Vector2 castingSpeedBuff;
    public Vector2 attackSpeedBuff;
    public float skillDistanceIncrease;

    int statsRatio = 6;
    [Header("Stats regeneration")]
    public bool canRegenerateHealth; 
    public int HealthPointsPerSecond; 
    public bool canRegenerateStamina; public bool canUseStamina;
    public int StaminaPerSecond;

    public GameObject buffIcon;

    public List<BuffAndIcon> activeBuffs = new List<BuffAndIcon>();
    public List <RecurringEffect> recurringEffects = new List<RecurringEffect>();

    void Awake() {
        if (instance == null)
            instance = this;

        StatsCalculations();
    }

    void Start() {
        health = maxHealth;
        stamina = maxStamina;
        
        ResetBuffStats();
        StatsCalculations();
    }

    void Update() {
        CalculateBuffs();
        StatsCalculations();
        regenerateHealth();
        regenerateStamina();
        RunRecurringEffects();
    }

    public void StatsCalculations () {
        strength = Mathf.RoundToInt(strengthFromEquip * strengthBuff);
        agility = Mathf.RoundToInt(agilityFromEquip * agilityBuff);
        intellect = Mathf.RoundToInt(intellectFromEquip * intellectBuff);

        maxHealth = 5000 + (strength / statsRatio) + healthFromEquip + healthFromBuff;
        maxStamina = 0 + ((agility + intellect) / statsRatio) + staminaFromEquip + staminaFromBuff;

        health = Mathf.Clamp(health, 0, maxHealth);
        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        meleeAttack = Mathf.RoundToInt( ( (strength / statsRatio) + meleeAttackFromEquip) * meleeAttackBuff);
        rangedAttack = Mathf.RoundToInt( ( (agility / statsRatio) + rangedAttackFromEquip) * rangedAttackBuff);
        magicPower = Mathf.RoundToInt( ( (intellect / statsRatio) + magicPowerFromEquip) * magicPowerBuff);
        healingPower = Mathf.RoundToInt( ( (intellect / statsRatio) + healingPowerFromEquip) * healingPowerBuff);
        defense = Mathf.RoundToInt( ( (strength / statsRatio + agility / statsRatio) + defenseFromEquip) * defenseBuff);

        attackSpeed.x = 1 * attackSpeedFromEquip.x * (1+agility*0.00005f) * attackSpeedBuff.x;
        attackSpeed.y = 1 * attackSpeedFromEquip.y * (1-agility*0.00005f) * attackSpeedBuff.y;

        castingSpeed.x = 1 * castingSpeedFromEquip.x * (1+intellect*0.00005f) * castingSpeedBuff.x;
        castingSpeed.y = 1 * castingSpeedFromEquip.y * (1-intellect*0.00005f) * castingSpeedBuff.y;

        critChance = baseCritChance + critChanceFromEquip + critChanceFromBuff;
        critStrength = baseCritStrength + critStrengthFromEquip + critStrengthFromBuff;

        blockChance = blockChanceFromEquip + blockChanceFromBuff;
        walkSpeed = 1 * walkSpeedBuff * walkSpeedFromEquipment;
    }

    float hpTimer = 1;
    void regenerateHealth() {
        if (health == maxHealth || isDead)
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
        if (isDead)
            return;

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
                if (!PlayerControlls.instance.isMounted) CanvasScript.instance.HideStamina();
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

        if (!PlayerControlls.instance.isMounted) CanvasScript.instance.ShowStamina();
        hidingStamina = false;

        if (canRegenerateStamina) {
            if (Time.time - staminaTimer >= 1f/StaminaPerSecond) {
                stamina ++;
                staminaTimer = Time.time;
            }
        }
    }
    
    void DisplayDamageNumber(string text, DamageTextColor dtc = DamageTextColor.Regular) {
        GameObject ddText = Instantiate(AssetHolder.instance.ddText, transform.position + Vector3.up * 1f, Quaternion.identity);
        ddText.GetComponent<ddText>().Init(text, dtc);
    }
    void DisplayDamageNumber(int damage) {
        GameObject ddText = Instantiate(AssetHolder.instance.ddText, transform.position + Vector3.up * 1f, Quaternion.identity);
        ddText.GetComponent<ddText>().Init(new DamageInfo(damage, DamageType.Enemy, false, ""));
    }
    void DisplayHealNumber(int healAmount) {
        GameObject ddText = Instantiate(AssetHolder.instance.ddText, transform.position + Vector3.up * 2f, Quaternion.identity);
        ddText.GetComponent<ddText>().Init(healAmount, DamageTextColor.Green);
    }
    void DisplayStaminaNumber(int staminaAmount) {
        GameObject ddText = Instantiate(AssetHolder.instance.ddText, transform.position + Vector3.up * 2f, Quaternion.identity);
        ddText.GetComponent<ddText>().Init(staminaAmount, DamageTextColor.Cyan);
    }

    public void UseOrRestoreStamina (int amount) {
        canRegenerateStamina = false;
        afterUseTimer = Time.time;
        stamina -= amount;
    }

    public void AddBuff(Buff buff) {
        for (int i = 0; i < activeBuffs.Count; i++)
            if (activeBuffs[i].buff1 == buff) return;

        BuffAndIcon bi;
        bi.buff1 = buff;
        bi.icon1 = Instantiate(buffIcon, CanvasScript.instance.buffs.transform).GetComponent<BuffIcon>();
        bi.icon1.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
        bi.icon1.buff = buff;

        activeBuffs.Add(bi);
    }

    public void RemoveBuff(Buff buff) { 
        for (int i = 0; i < activeBuffs.Count; i++) {
            if (activeBuffs[i].buff1 == buff) {
                if (buff.associatedSkill != null) buff.associatedSkill.OnBuffRemove();
                Destroy(activeBuffs[i].icon1.gameObject);
                activeBuffs.Remove(activeBuffs[i]);
                break;
            }
        }
    } 

    void CalculateBuffs() {
        ResetBuffStats();
        for (int i = 0; i < activeBuffs.Count; i++){
            healthFromBuff += activeBuffs[i].buff1.healthBuff;
            staminaFromBuff += activeBuffs[i].buff1.staminaBuff;
            
            strengthBuff *= 1 + activeBuffs[i].buff1.strengthBuff;
            agilityBuff *= 1 + activeBuffs[i].buff1.agilityBuff;
            intellectBuff *= 1 + activeBuffs[i].buff1.intellectBuff;

            meleeAttackBuff *= 1 + activeBuffs[i].buff1.meleeAttackBuff;
            rangedAttackBuff *= 1 + activeBuffs[i].buff1.rangedAttackBuff;
            magicPowerBuff *= 1 + activeBuffs[i].buff1.magicPowerBuff;
            healingPowerBuff *= 1 + activeBuffs[i].buff1.healingPowerBuff;
            defenseBuff *= 1 + activeBuffs[i].buff1.defenseBuff;

            castingSpeedBuff.x *= (1-activeBuffs[i].buff1.castingSpeedBuff);
            castingSpeedBuff.y *= (1+activeBuffs[i].buff1.castingSpeedBuff);

            attackSpeedBuff.x *= (1-activeBuffs[i].buff1.attackSpeedBuff);
            attackSpeedBuff.y *= (1+activeBuffs[i].buff1.attackSpeedBuff);


            skillDistanceIncrease += activeBuffs[i].buff1.skillDistanceBuff;

            immuneToDamageInt += activeBuffs[i].buff1.immuneToDamage ? 1 : 0;
            immuneToInterruptInt += activeBuffs[i].buff1.immuneToInterrupt ? 1 : 0;

            critChanceFromBuff += activeBuffs[i].buff1.critChanceBuff;
            critStrengthFromBuff += activeBuffs[i].buff1.critStrengthBuff;
            blockChanceFromBuff += activeBuffs[i].buff1.blockChanceBuff;
            walkSpeedBuff *= (1+activeBuffs[i].buff1.walkSpeedBuff);
        }

        immuneToDamage = immuneToDamageInt > 0 ? true : false;
        immuneToInterrupt = immuneToInterruptInt > 0 ? true : false;
    }
    void ResetBuffStats() {
        healthFromBuff = 0;
        staminaFromBuff = 0;
        
        strengthBuff = 1;
        agilityBuff = 1;
        intellectBuff = 1;
        
        meleeAttackBuff = 1;
        rangedAttackBuff = 1;
        magicPowerBuff = 1;
        healingPowerBuff = 1;
        defenseBuff = 1;

        castingSpeedBuff = Vector2.one;
        attackSpeedBuff = Vector2.one;
        

        skillDistanceIncrease = 0;

        immuneToDamageInt = 0;
        immuneToInterruptInt = 0;

        critChanceFromBuff = 0;
        critStrengthFromBuff = 0;
        blockChanceFromBuff = 0;
        walkSpeedBuff = 1;
    }

#region Getting Hit

    public void GetHit (DamageInfo enemyDamageInfo, HitType hitType = HitType.Normal, float cameraShakeFrequency = 0, float cameraShakeAmplitude = 0) {
        if (isDead)
            return;
        
        bool blocked = Random.value < blockChance && WeaponsController.instance.leftHandStatus == SingleHandStatus.Shield;

        if (blocked) {
            PlayerControlls.instance.animator.CrossFade("LeftHandArm.Block", 0.1f);
            WeaponsController.instance.InstantUnsheathe();
        } else if (hitType == HitType.Normal && !immuneToDamage) {
            PlayerControlls.instance.animator.CrossFade("GetHitUpperBody.GetHit", 0.1f);
        } else if (hitType == HitType.Interrupt) {
            if (!immuneToInterrupt) {
                PlayerControlls.instance.animator.CrossFade("GetHit.GetHit", 0.1f);
                PlayerControlls.instance.InterruptCasting();
            } else if (!immuneToDamage) {
                PlayerControlls.instance.animator.CrossFade("GetHitUpperBody.GetHit", 0.1f);
            }
        }

        int actualDamage = immuneToDamage || blocked ? 0 : Mathf.RoundToInt( enemyDamageInfo.damage * defenseCoeff() ); 
        health -= actualDamage;
        if (blocked) {
            DisplayDamageNumber("Blocked", DamageTextColor.White);
            PlayerAudioController.instance.PlayGetHitSound(GetHitType.Block);
        } else if (immuneToDamage) {
            DisplayDamageNumber("Invincible", DamageTextColor.LightBlue);
            PlayerAudioController.instance.PlayGetHitSound(GetHitType.Invincibility);
        } else {
            DisplayDamageNumber(actualDamage);
            PlayerAudioController.instance.PlayGetHitSound(GetHitType.Hit);
        }

        PrintHitInChat(actualDamage, enemyDamageInfo.sourceName, blocked, immuneToDamage);

        if (cameraShakeFrequency != 0 && cameraShakeAmplitude != 0)
            PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(cameraShakeFrequency, cameraShakeAmplitude, 0.1f, transform.position);

        if (health <= 0)
            Die();
    }
    public void RunRecurringEffects () {
        for (int i = recurringEffects.Count-1; i >= 0; i--) {
            recurringEffects[i].frequencyTimer -= Time.deltaTime;
            recurringEffects[i].durationTimer -= Time.deltaTime;
            if (recurringEffects[i].frequencyTimer <= 0) {
                
                if (recurringEffects[i].recurringEffectType == RecurringEffectType.Damaging)
                    GetHit(CalculateDamage.enemyDamageInfo(recurringEffects[i].baseEffectPercentage, recurringEffects[i].name));
                else if (recurringEffects[i].recurringEffectType == RecurringEffectType.Healing)
                    GetHealed(CalculateDamage.damageInfo(recurringEffects[i].damageType, recurringEffects[i].baseEffectPercentage, recurringEffects[i].name));
                
                recurringEffects[i].frequencyTimer = 1/recurringEffects[i].frequencyPerSecond;
            }
            if (recurringEffects[i].durationTimer <= 0) {
                RemoveRecurringEffect(recurringEffects[i]);
            }
        }
    }
    
    public void AddRecurringEffect (RecurringEffect effect) {
        RecurringEffect newEffect = new RecurringEffect(effect.name, effect.damageType, effect.recurringEffectType, effect.baseEffectPercentage, effect.frequencyPerSecond, effect.duration,
                    effect.vfx, effect.frequencyTimer, effect.durationTimer);
        newEffect.durationTimer = newEffect.duration;
        newEffect.frequencyTimer = 0;
        if (newEffect.vfx != null) {
            newEffect.vfx = Instantiate(newEffect.vfx, transform.position + Vector3.up, Quaternion.identity, transform).GetComponent<ParticleSystem>();
            newEffect.vfx.Play();
        }
        recurringEffects.Add(newEffect);
    }
    public void RemoveRecurringEffect (RecurringEffect effect) {
        RecurringEffect effectToRemove = null;
        if (!recurringEffects.Contains(effect)) {
            for (int i = 0; i < recurringEffects.Count; i++) { //Lets try finding this effect
                if (recurringEffects[i].name == effect.name) {
                    effectToRemove = recurringEffects[i];
                    break;
                }
            }
        } else {
            effectToRemove = effect;
        }
        if (effectToRemove == null) return;

        if (effectToRemove.vfx) {
            Destroy(effectToRemove.vfx.gameObject, 3f);
            effectToRemove.vfx.Stop();
        }
        recurringEffects.Remove(effectToRemove);
    }

    void Die() {
        if (isDead)
            return;

        PlayerControlls.instance.animator.CrossFade("GetHit.Death", 0.25f);
        PlayerControlls.instance.cameraControl.isAiming = false;
        PlayerControlls.instance.cameraControl.isShortAiming = false;
        TeleportManager.instance.ShowReviveWindow();
        isDead = true;
    }
    public void Revive () {
        if (!isDead)
            return;

        PlayerControlls.instance.animator.Play("GetHit.Empty");
        health = maxHealth;
        StartCoroutine(ReviveCourutine()); //Calling this cause without a delay some enemies think player is already alive but he is not
    }
    IEnumerator ReviveCourutine () {
        yield return new WaitForSeconds(0.2f);
        isDead = false;
    }

    void PrintHitInChat (int damage, string enemyName, bool blocked = false, bool immuneToDamage = false) {
        string color = "#" + ColorUtility.ToHtmlStringRGB(UI_General.highlightTextColor);
        if (blocked) 
            PeaceCanvas.instance.DebugChat($"{getCurrentTime()} <color={color}>{Characteristics.instance.playerName}</color> <color=#def9ff>blocked</color> damage from <color=#80FFFF>{enemyName}</color>.");
        else if (immuneToDamage)
            PeaceCanvas.instance.DebugChat($"{getCurrentTime()} <color={color}>{Characteristics.instance.playerName}</color> was <color=#def9ff>invincible</color> from damage by <color=#80FFFF>{enemyName}</color>.");
        else
            PeaceCanvas.instance.DebugChat($"{getCurrentTime()} <color={color}>{Characteristics.instance.playerName}</color> was hit with <color=red>{damage}</color> damage by <color=#80FFFF>{enemyName}</color>.");
    }

    string getCurrentTime () {
        return $"[{System.DateTime.Now.Hour}:{System.DateTime.Now.Minute}:{System.DateTime.Now.Second}]";
    }

    float defenseCoeff () { //https://www.desmos.com/calculator/pjaj8kestx
        float g = 0.9999f;
        return Mathf.Pow(g, defense + Mathf.Log(1, g));
    }

    public void GetHealed(DamageInfo healInfo) {
        health += healInfo.damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        DisplayHealNumber(healInfo.damage);
        string criticalDEBUGtext = healInfo.isCrit ? " CRITICAL" : "";
        PeaceCanvas.instance.DebugChat($"[{System.DateTime.Now.Hour}:{System.DateTime.Now.Minute}:{System.DateTime.Now.Second}] <color={"#"+ColorUtility.ToHtmlStringRGB(UI_General.highlightTextColor)}>{Characteristics.instance.playerName}</color> healed<color=green>{criticalDEBUGtext} {healInfo.damage}</color> points with <color=#80FFFF>{healInfo.sourceName}</color>.");
    }
    public void GetStamina(DamageInfo staminaInfo) {
        stamina += staminaInfo.damage;
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
        DisplayStaminaNumber(staminaInfo.damage);
        string criticalDEBUGtext = staminaInfo.isCrit ? " CRITICAL" : "";
        PeaceCanvas.instance.DebugChat($"[{System.DateTime.Now.Hour}:{System.DateTime.Now.Minute}:{System.DateTime.Now.Second}] <color={"#"+ColorUtility.ToHtmlStringRGB(UI_General.highlightTextColor)}>{Characteristics.instance.playerName}</color> regenerated<color=green>{criticalDEBUGtext} {staminaInfo.damage}</color> stamina from <color=#80FFFF>{staminaInfo.sourceName}</color>.");
    }
}

#endregion