using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState {Idle, Approaching, Attacking, Returning};
 
[System.Serializable] public struct Loot {
    public Item lootItem;
    public int lootItemAmount;
    [Range(0,1)] public float dropProbability;
}

public abstract class Enemy : MonoBehaviour
{
    public string enemyName;
    [Header("Stats")]
    public int maxHealth;
    public int currentHealth;
    public int baseDamage;
    public float playerDetectRadius;
    public float attackRange;
    public HitType hitType;
    public bool immuneToInterrupt;
    public bool immuneToKnockDown;
    public bool immuneToKickBack;
    public Vector3 localEnemyBounds;
    [Header("Loot")]
    public Loot[] itemsLoot;
    public int goldLootAmount;

    [Header("AI State")]
    public EnemyState currentState;
    
    [Header("Attack")]
    public bool isCoolingDown;
    public float attackCoolDown;
    protected float coolDownTimer;
    public float agrTime;
    protected float agrTimer;

    [Header("Adjustements from debuffs")]
    public float TargetSkillDamagePercentage;

    public float distanceToPlayer;

    [Header("States")]
    public bool isAttacking;
    public bool isGettingInterrupted;
    public bool isDead;
    public bool isKnockedDown;
    public bool canGetHit = true;
    public bool agr; //Agressive - if true, then targets and attacks the player. if false then resting/idling

    [Space]
    public GameObject healthBar;
    public ParticleSystem hitParticles;
    public AudioClip[] getHitSounds;
    public AudioClip[] stabSounds;
    public AudioClip[] stepsSounds;
    public AudioClip[] attackSounds;

    [Space]
    public List<RecurringEffect> currentRecurringEffects = new List<RecurringEffect>(); 

    protected Animator animator;
    protected NavMeshAgent navAgent;
    protected Transform target;
    protected AudioSource audioSource;
    protected Vector3 initialPos;
    protected SkinnedMeshRenderer skinnedMesh;

    protected float agrDelay; 
    protected float agrDelayTimer;
    protected bool delayingAgr;

    protected virtual void Start() {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;

        SetInitialPosition();

        //Create new instance of material to allow making changes without affecting other instances
        skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
        Material newMat = skinnedMesh.material;
        skinnedMesh.material = newMat;
    }

    protected void SetInitialPosition() {
        initialPos = transform.position;
        //if spawned in the air - drop on the floor
        RaycastHit hit;
        if (Physics.Raycast(initialPos, -Vector3.up, out hit, 4f)) 
            initialPos = hit.point;
    }

    protected virtual void Update() {
        if (isDead || PlayerControlls.instance == null) { //Player instance is null when level is only loading.
            return;
        }
        
        Health();
        ShowHealthBar();
        if (isDead) return; //need second check since he dies after running "Health()"
            
        AttackCoolDown();

        distanceToPlayer = Vector3.Distance(transform.position, PlayerControlls.instance.transform.position);
    
        CheckAgr();
    
        if (agrDelayTimer > 0) {
            agrDelayTimer -= Time.deltaTime;
            delayingAgr = true;
        } else {
            delayingAgr = false;
        }

        AI();
        RunRecurringEffects();
    }

    protected virtual void AI () {
        if (isDead) return;

        if (distanceToPlayer <= playerDetectRadius) {
            Agr();
        }
        
        if (agr) {
            if (distanceToPlayer > attackRange) {
                if (!isAttacking) currentState = EnemyState.Approaching;
            } else {
                currentState = EnemyState.Attacking;
            } 
        } else {
            if (Vector3.Distance(transform.position, initialPos) > navAgent.stoppingDistance) {
                currentState = EnemyState.Returning;     
            } else {
                currentState = EnemyState.Idle;
            }
        }

        if (delayingAgr)
            return;

        if (currentState == EnemyState.Idle) {
            Idle();
        } else if (currentState == EnemyState.Approaching) {
            target = PlayerControlls.instance.transform;
            ApproachTarget();
        } else if (currentState == EnemyState.Attacking) {
            TryAttackTarget();
            FaceTarget();
        } else if (currentState == EnemyState.Returning) {
            ReturnToPosition();
        }
    }

    protected virtual void ApproachTarget() {
        if (!PlayerControlls.instance.GetComponent<Combat>().enemiesInBattle.Contains(this))
            PlayerControlls.instance.GetComponent<Combat>().enemiesInBattle.Add(this);
    }
    protected virtual void TryAttackTarget() {
        if (!PlayerControlls.instance.GetComponent<Combat>().enemiesInBattle.Contains(this))
            PlayerControlls.instance.GetComponent<Combat>().enemiesInBattle.Add(this);

        if (isCoolingDown || isDead || isKnockedDown)
            return;
        
        navAgent.isStopped = true;
        coolDownTimer = attackCoolDown;
        AttackTarget();
    }
    protected abstract void AttackTarget();
    protected abstract void FaceTarget(bool instant = false);
    protected virtual void ReturnToPosition() {
        if (PlayerControlls.instance.GetComponent<Combat>().enemiesInBattle.Contains(this))
            PlayerControlls.instance.GetComponent<Combat>().enemiesInBattle.Remove(this);
    }
    protected virtual void Idle() {
        if (PlayerControlls.instance.GetComponent<Combat>().enemiesInBattle.Contains(this))
            PlayerControlls.instance.GetComponent<Combat>().enemiesInBattle.Remove(this);
    }
    
    protected virtual void Health () {
        if (isDead)
            return;

        if (currentHealth <= 0) {
            isDead = true;
            animator.CrossFade("GetHit.Die", 0.25f);
            StartCoroutine(die());
            DropLoot();
            if (navAgent != null) navAgent.enabled = false;
            foreach (Collider col in GetComponentsInChildren<Collider>()) col.enabled = false;
        }

        //Add health regeneration
    }

    protected virtual void DropLoot () {
        for (int i = 0; i < itemsLoot.Length; i++) {
            if (Random.value < itemsLoot[i].dropProbability) {
                AssetHolder.instance.DropItem(itemsLoot[i].lootItem, itemsLoot[i].lootItemAmount, transform.position);
            }
        }
        if (goldLootAmount > 0) {
            AssetHolder.instance.DropGold(goldLootAmount, transform.position);
        }
    }

    IEnumerator die (){
        yield return new WaitForSeconds (8);
        float x = 0;
        while (x <=1) {
            skinnedMesh.material.SetFloat("_DisolveProgress", x);
            x += Time.fixedDeltaTime/2;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        Destroy(gameObject);
    }

    public virtual void GetHit (DamageInfo damageInfo, string skillName, bool stopHit = false, bool cameraShake = false, HitType hitType = HitType.Normal, Vector3 damageTextPos = new Vector3 ()) {
        if (isDead || !canGetHit)
            return;
        
        Agr();

        int actualDamage = calculateActualDamage(damageInfo.damage);

        if (immuneToKickBack && hitType == HitType.Kickback)
            hitType = HitType.Normal;
        if (immuneToKnockDown && hitType == HitType.Knockdown)
            hitType = HitType.Normal;
        if (immuneToInterrupt && hitType == HitType.Interrupt)
            hitType = HitType.Normal;

        if (hitType == HitType.Normal)
            animator.CrossFade("GetHitUpperBody.GetHit", 0.1f, animator.GetLayerIndex("GetHitUpperBody"), 0);
        else if (hitType == HitType.Interrupt)
            animator.CrossFade("GetHit.GetHit", 0.1f, animator.GetLayerIndex("GetHit"), 0);
        else if (hitType == HitType.Kickback)
            StartCoroutine(KickBack());
        else if (hitType == HitType.Knockdown)
            StartCoroutine(KnockedDown());

        currentHealth -= actualDamage;
        PlayHitParticles();
        PlayGetHitSounds();
        PlayStabSounds();
        
        if (stopHit || damageInfo.isCrit) StartCoroutine(HitStop(damageInfo.isCrit));
        if (cameraShake) PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.2f, 1*(1+actualDamage/3000), 0.1f, transform.position);
        DisplayDamageNumber(new DamageInfo(actualDamage, damageInfo.isCrit), damageTextPos);

        string criticalDEBUGtext = damageInfo.isCrit ? "CRITICAL " : "";
        PeaceCanvas.instance.DebugChat($"[{System.DateTime.Now.Hour}:{System.DateTime.Now.Minute}:{System.DateTime.Now.Second}] {enemyName} was hit <color=red>{criticalDEBUGtext}{actualDamage}</color> points by <color=#80FFFF>{skillName}</color>.");
    }

    protected void DisplayDamageNumber(DamageInfo damageInfo, Vector3 position = new Vector3()) {
        if (position == Vector3.zero)   //If position is not specified then place on standard spot
            position = transform.position + Vector3.up * 1.5f;

        GameObject ddText = Instantiate(AssetHolder.instance.ddText, position, Quaternion.identity);
        ddText.GetComponent<ddText>().damageInfo = damageInfo;
    }

    protected int calculateActualDamage (int damage) {
        return Mathf.RoundToInt( damage * (1 + TargetSkillDamagePercentage/100) );
    }

    protected IEnumerator HitStop (bool isCrit) {
        float timeStarted = Time.realtimeSinceStartup;
        float time = isCrit ? 0.4f : 0.12f;
        Time.timeScale = 0.3f;
        Time.fixedDeltaTime = 0.006f;
        while(Time.realtimeSinceStartup - timeStarted < time) {
            yield return null;
        }
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f;
    }
    
    protected IEnumerator KnockedDown () {
        animator.CrossFade("GetHit.KnockDown", 0.1f);
        isKnockedDown = true;
        yield return new WaitForSeconds(Random.Range(2.5f, 3.5f));
        if (isDead)
            yield break;
        animator.CrossFade("GetHit.GetUp", 0.1f);
        yield return new WaitForSeconds(0.7f);
        isKnockedDown = false;
    }
    protected virtual IEnumerator KickBack () {
        //rotate to face the player so the enemy would fly away from the player
        StartCoroutine(InstantFaceTarget());

        animator.CrossFade("GetHit.KickBack", 0.1f);
        isKnockedDown = true;
        yield return new WaitForSeconds(Random.Range(1.8f, 2.2f));
        if (isDead)
            yield break;
        animator.CrossFade("GetHit.GetUp", 0.1f);
        yield return new WaitForSeconds(2);
        isKnockedDown = false;
    }
    protected IEnumerator InstantFaceTarget () {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        while (Quaternion.Angle(transform.rotation, lookRotation) > 1) {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 30f);
            yield return null;
        }
    }

    protected virtual void ShowHealthBar () {
        if (isDead) {
            healthBar.SetActive(false);
            return;
        }

        if (distanceToPlayer <= 25) {
            healthBar.transform.GetChild(0).localScale = new Vector3((float)currentHealth/maxHealth, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
            healthBar.transform.LookAt (healthBar.transform.position + PlayerControlls.instance.playerCamera.transform.rotation * Vector3.back, PlayerControlls.instance.playerCamera.transform.rotation * Vector3.up);
            healthBar.SetActive(true);
        } else {
            healthBar.SetActive(false);
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, playerDetectRadius);
    }

    protected virtual void CheckAgr () {
        if (agrTimer > 0) {
            agr = true;
            agrTimer -= Time.deltaTime;
        } else {
            agr = false;
        }
    }

    protected virtual void Agr() {
        if (!agr) {
            target = PlayerControlls.instance.transform;
            agr = true;
            agrDelayTimer = agrDelay;
            delayingAgr = true;
            FaceTarget(true);
        }
        agrTimer = agrTime;
    }

    public virtual void Hit () {
        PlayerControlls.instance.GetComponent<Characteristics>().GetHit(damage(), hitType, 0.2f, 1.5f);
    }

    public bool checkCanHit (float value) {
        if (animator.GetFloat("CanHit") == value)
            return true;
        else 
            return false;
    }

    protected virtual int damage () {
        return Mathf.RoundToInt(Random.Range(baseDamage*0.85f, baseDamage*1.15f));
    }

    protected virtual void AttackCoolDown() {
        if (coolDownTimer > 0) {
            coolDownTimer -= Time.deltaTime;
            isCoolingDown = true;
        } else {
            isCoolingDown = false;
        }
    }

    protected virtual void PlayHitParticles () {
        if (hitParticles == null)
            return;
        hitParticles.transform.localEulerAngles = new Vector3(hitParticles.transform.localEulerAngles.x + Random.Range(-30, 30), hitParticles.transform.localEulerAngles.y, hitParticles.transform.localEulerAngles.z);
        hitParticles.Play();
    }
    protected virtual void PlayGetHitSounds () {
        if (getHitSounds.Length == 0)
            return;

        int playID = Random.Range(0, getHitSounds.Length);
        audioSource.pitch = 1 + Random.Range(-0.1f, 0.1f);
        audioSource.PlayOneShot(getHitSounds[playID]);
    }
    protected virtual void PlayStabSounds() {
        if (stabSounds.Length == 0)
            return;

        int playID = Random.Range(0, stabSounds.Length);
        audioSource.pitch = 1 + Random.Range(-0.1f, 0.1f);
        audioSource.PlayOneShot(stabSounds[playID]);
    }

    public virtual void FootStep () {
        if (stepsSounds.Length == 0)
            return;

        int playID = Random.Range(0, stepsSounds.Length);
        audioSource.pitch = 1 + Random.Range(-0.1f, 0.1f);
        audioSource.PlayOneShot(stepsSounds[playID]);
    }

    public virtual void PlayAttackSound (AnimationEvent animationEvent) {
        float pitch = animationEvent.floatParameter == 0 ? 1 : animationEvent.floatParameter;
        audioSource.pitch = pitch + Random.Range(-0.1f, 0.1f);
        audioSource.PlayOneShot(attackSounds[animationEvent.intParameter]);
    }

    protected virtual void RunRecurringEffects () {
        for (int i = currentRecurringEffects.Count-1; i >= 0; i--) {
            currentRecurringEffects[i].frequencyTimer -= Time.deltaTime;
            currentRecurringEffects[i].durationTimer -= Time.deltaTime;
            if (currentRecurringEffects[i].frequencyTimer <= 0) {
                GetHit(CalculateDamage.damageInfo(currentRecurringEffects[i].skillTree, currentRecurringEffects[i].baseEffectPercentage, 0), currentRecurringEffects[i].name);
                currentRecurringEffects[i].frequencyTimer = 1/currentRecurringEffects[i].frequencyPerSecond;
            }
            if (currentRecurringEffects[i].durationTimer <= 0) {
                Destroy(currentRecurringEffects[i].vfx.gameObject, 3f);
                currentRecurringEffects[i].vfx.Stop();
                currentRecurringEffects.RemoveAt(i);
            }
        }
    }
    
    public virtual void AddRecurringEffect (RecurringEffect effect) {
        RecurringEffect newEffect = new RecurringEffect(effect.name, effect.skillTree, effect.baseEffectPercentage, effect.frequencyPerSecond, effect.duration,
                    effect.vfx, effect.frequencyTimer, effect.durationTimer);
        newEffect.durationTimer = newEffect.duration;
        newEffect.frequencyTimer = 0;
        if (newEffect.vfx != null) {
            newEffect.vfx = Instantiate(newEffect.vfx, transform.position + Vector3.up * globalEnemyBounds().y, Quaternion.identity, transform).GetComponent<ParticleSystem>();
            var shape = newEffect.vfx.shape;
            shape.radius = (globalEnemyBounds().x + globalEnemyBounds().z)*0.5f;
            var shape2 = newEffect.vfx.transform.GetChild(0).GetComponent<ParticleSystem>().shape;
            shape2.radius = shape.radius*0.8f;
            newEffect.vfx.Play();
        }
        currentRecurringEffects.Add(newEffect);
    }

    public Vector3 globalEnemyBounds (){
        return Vector3.Scale(localEnemyBounds, transform.localScale);
    }
}
