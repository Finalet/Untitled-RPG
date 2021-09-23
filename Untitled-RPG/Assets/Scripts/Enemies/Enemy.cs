using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState {Idle, Approaching, Attacking, Returning, Celebrating, Stunned};
 
[RequireComponent(typeof (FieldOfView))] [SelectionBase]
public abstract class Enemy : MonoBehaviour, IDamagable, ILootable
{
    public string enemyName;
    [Header("Stats")]
    public int maxHealth;
    public int currentHealth;
    public int baseDamage;
    public float attackRange;
    public HitType hitType;
    [Space]
    public bool immuneToInterrupt;
    public bool immuneToKnockDown;
    public bool immuneToKickBack;
    [Space]
    public Vector3 localEnemyBounds;
    [Header("Loot")]
    public RandomLoot loot;
    public int goldLootAmount;

    [Header("AI")]
    [DisplayWithoutEdit] public bool agr; //Agressive - if true, then targets and attacks the player. if false then resting/idling
    [DisplayWithoutEdit] public EnemyState currentState;
    [DisplayWithoutEdit] [SerializeField] protected float distanceToPlayer;
    
    [Header("Attack")]
    public bool isCoolingDown;
    public float attackCoolDown;
    protected float coolDownTimer;
    public float agrTime;
    protected float agrTimer;

    [Header("Adjustements from debuffs")]
    public float TargetSkillDamagePercentage;


    [Header("States")]
    public bool isAttacking;
    public bool isGettingInterrupted;
    public bool isDead;
    public bool isKnockedDown;
    public bool isRagdoll;
    public bool isStunned;
    public bool canGetHit = true;

    [Space]
    public GameObject healthBar;
    public ParticleSystem hitParticles;
    public AudioClip[] getHitSounds;
    public AudioClip[] stabSounds;
    public AudioClip[] attackSounds;

    protected Animator animator;
    protected NavMeshAgent navAgent;
    protected Transform target;
    protected AudioSource audioSource;
    protected Vector3 initialPos;
    protected SkinnedMeshRenderer skinnedMesh;
    protected FieldOfView fieldOfView;
    protected RagdollController ragdollController;
    protected EnemyController enemyController;
    protected FootstepManager footstepManager;
    protected float baseControllerSpeed;

    protected float agrDelay; 
    protected float agrDelayTimer;
    protected bool delayingAgr;

    protected float celebrationDuration = 5;
    protected float startedCelebratingTime;

    protected virtual void Start() {
        enemyController = GetComponent<EnemyController>();
        ragdollController = GetComponent<RagdollController>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        fieldOfView = GetComponent<FieldOfView>();
        fieldOfView.InitForEnemy();
        footstepManager = GetComponentInChildren<FootstepManager>();

        currentHealth = maxHealth;
        baseControllerSpeed = enemyController.speed;

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
        if (Physics.Raycast(initialPos + Vector3.up, -Vector3.up, out hit, 10f)) 
            initialPos = hit.point + Vector3.up * 0.05f;
    }

    protected virtual void Update() {
        if (isDead || PlayerControlls.instance == null) { //Player instance is null when level is only loading.
            return;
        }
        
        if(ragdollController != null) isRagdoll = ragdollController.ragdolled;
        else isRagdoll = false;

        Health();
        ShowHealthBar();
        if (isDead) return; //need second check since he dies after running "Health()"
            
        AttackCoolDown();

        distanceToPlayer = fieldOfView.distanceToTarget;
    
        CheckAgr();
    
        if (agrDelayTimer > 0) {
            agrDelayTimer -= Time.deltaTime;
            delayingAgr = true;
        } else {
            delayingAgr = false;
        }

        AI();
        RunRecurringEffects();
        RunKnockDowns();

        ApplyEnemyControllerSettings();
    }

    protected virtual void ApplyEnemyControllerSettings () {
        enemyController.useRootMotion = isAttacking || isGettingInterrupted;       
        enemyController.useRootMotionRotation = isAttacking || isGettingInterrupted;       
        enemyController.speed = enemyController.useRootMotion ? baseControllerSpeed * 50 : baseControllerSpeed;
    }

    protected virtual void AI () {
        if (isDead) return;

        if (fieldOfView.isTargetVisible) {
            Agr();
        }
        
        if (agr) {
            if (distanceToPlayer > attackRange) {
                if(!isAttacking) currentState = EnemyState.Approaching;
            } else {
                currentState = EnemyState.Attacking;
            }

            if (Characteristics.instance.isDead && currentState != EnemyState.Celebrating){
                currentState = EnemyState.Celebrating;
                startedCelebratingTime = Time.time;
                agrTimer = -10;
            }

        } else if (currentState != EnemyState.Celebrating || ( (Time.time - startedCelebratingTime > celebrationDuration) || distanceToPlayer > 20) ) {
            currentState = (navAgent.isActiveAndEnabled ? navAgent.remainingDistance : Vector3.Distance(navAgent.nextPosition, initialPos))  > enemyController.stoppingDistance ? EnemyState.Returning : EnemyState.Idle;
        }

        if (isStunned) currentState = EnemyState.Stunned;

        

        if (delayingAgr)
            return;

        if (currentState == EnemyState.Idle) {
            Idle();
        } else if (currentState == EnemyState.Approaching) {
            target = PlayerControlls.instance.transform;
            ApproachTarget();
        } else if (currentState == EnemyState.Attacking) {
            TryAttackTarget();
            TryFaceTarget();
        } else if (currentState == EnemyState.Returning) {
            ReturnToPosition();
        } else if (currentState == EnemyState.Celebrating) {
            Celebrate();
        } else if (currentState == EnemyState.Stunned) {
            Stun();
        }
    }

    protected virtual void ApproachTarget() {
        if (!PlayerControlls.instance.GetComponent<Combat>().enemiesInBattle.Contains(this))
            PlayerControlls.instance.GetComponent<Combat>().enemiesInBattle.Add(this);
    }
    protected virtual void TryAttackTarget() {
        if (!PlayerControlls.instance.GetComponent<Combat>().enemiesInBattle.Contains(this))
            PlayerControlls.instance.GetComponent<Combat>().enemiesInBattle.Add(this);

        if (isCoolingDown || isDead || isKnockedDown || !fieldOfView.isTargetVisible || isRagdoll)
            return;
        
        if (navAgent.enabled) navAgent.isStopped = true;
        coolDownTimer = attackCoolDown;
        AttackTarget();
    }
    protected abstract void AttackTarget();
    protected virtual void TryFaceTarget(bool instant = false) {
        if (isKnockedDown || isRagdoll) 
            return;
        
        FaceTarget(instant);
    }
    protected abstract void FaceTarget (bool instant = false);
    
    protected virtual void ReturnToPosition() {
        if (PlayerControlls.instance.GetComponent<Combat>().enemiesInBattle.Contains(this))
            PlayerControlls.instance.GetComponent<Combat>().enemiesInBattle.Remove(this);

        if(navAgent.enabled) {
            navAgent.destination = initialPos;
            navAgent.isStopped = false;
        }
        RegenerateMaxHealth();
    }
    protected virtual void Idle() {
        if (PlayerControlls.instance.GetComponent<Combat>().enemiesInBattle.Contains(this))
            PlayerControlls.instance.GetComponent<Combat>().enemiesInBattle.Remove(this);

        RegenerateMaxHealth();
    }
    protected virtual void Celebrate() {}
    protected virtual void Stun() {}

    protected virtual void Health () {
        if (isDead)
            return;

        if (currentHealth <= 0) {
            Die();
        }
    }

    protected virtual void RegenerateMaxHealth () {
        if (currentHealth < maxHealth)
            currentHealth = Mathf.RoundToInt(Mathf.MoveTowards(currentHealth, maxHealth, Time.deltaTime * 10f));
    }

    protected virtual void Die () {
        isDead = true;
        currentHealth = 0;
        gameObject.SetLayer(LayerMask.NameToLayer("Dead Enemy"), true);
        animator.CrossFade("GetHit.Die", 0.25f);
        animator.SetBool("isDead", true);
        StartCoroutine(die());
        DropLoot();
        if (navAgent != null && navAgent.enabled) navAgent.enabled = false;
        if (ragdollController != null) {
            ragdollController.EnableRagdoll();
            ragdollController.blockStopRagdoll = true;  
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

    protected int calculateActualDamage (int damage) {
        return Mathf.RoundToInt( damage * (1 + TargetSkillDamagePercentage/100) );
    }

    protected IEnumerator HitStop (bool isCrit) {
        float timeStarted = Time.realtimeSinceStartup;
        float time = isCrit ? 0.3f : 0.12f;
        Time.timeScale = 0.2f;
        Time.fixedDeltaTime = 0.006f;
        while(Time.realtimeSinceStartup - timeStarted < time) {
            yield return null;
        }
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f;
    }
    
    protected void KnockedDown () {
        isKnockedDown = true;
        gettingUp = false;
        timeKnockedDown = Time.time;
        knockDownDuration *= 0.8f + Random.value * 0.4f;
        if (ragdollController != null) ragdollController.EnableRagdoll(-transform.forward * 5);
        else animator.CrossFade("GetHit.KnockDown", 0.1f);
    }
    protected virtual void KickBack (float kickBackStrength = 50) {
        //rotate to face the player so the enemy would fly away from the player
        StartCoroutine(InstantFaceTarget());
        timeKnockedDown = Time.time;
        isKnockedDown = true;
        gettingUp = false;
        if (ragdollController != null) ragdollController.EnableRagdoll(-transform.forward * kickBackStrength + Vector3.up*2);
        else animator.CrossFade("GetHit.KickBack", 0.1f);
        knockDownDuration *= 0.8f + Random.value * 0.4f;
    }
    protected virtual void GetUpFromKnockDown () {
        gettingUp = true;
        if (ragdollController != null) ragdollController.StopRagdoll();
        else animator.CrossFade("GetHit.GetUp", 0.1f);
    }

    float timeKnockedDown;
    float knockDownDuration = 2;
    bool gettingUp;
    protected virtual void RunKnockDowns () {
        if (!isKnockedDown)
            return;
        
        if (Time.time - timeKnockedDown > knockDownDuration && !gettingUp) {
            GetUpFromKnockDown();
        } else if (Time.time - timeKnockedDown > knockDownDuration + 0.7f) {
            gettingUp = false;
            isKnockedDown = false;
        }
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
        if (!healthBar)
            return;
            
        if (isDead) {
            healthBar.SetActive(false);
            return;
        }

        if (distanceToPlayer <= 25) {
            healthBar.transform.GetChild(0).localScale = new Vector3((float)currentHealth/maxHealth, healthBar.transform.GetChild(0).transform.localScale.y, healthBar.transform.GetChild(0).transform.localScale.z);
            healthBar.transform.LookAt (healthBar.transform.position + PlayerControlls.instance.playerCamera.transform.rotation * Vector3.back, PlayerControlls.instance.playerCamera.transform.rotation * Vector3.up);
            if (animator.GetBoneTransform(HumanBodyBones.Head) != null) healthBar.transform.position = animator.GetBoneTransform(HumanBodyBones.Head).position + Vector3.up * 0.5f;
            healthBar.SetActive(true);
        } else {
            healthBar.SetActive(false);
        }
    }

    protected virtual void CheckAgr () {
        if (agrTimer > 0) {
            agr = true;
            agrTimer -= Time.deltaTime;
        } else {
            agr = false;
            if (TryGetComponent(out EnemyAlarmNetwork eam)) {
                eam.isTriggered = false;
            }
        }
    }

    public virtual void Agr() {
        if (Characteristics.instance.isDead)
            return;

        if (!agr) {
            target = PlayerControlls.instance.transform;
            agr = true;
            agrDelayTimer = agrDelay;
            delayingAgr = true;
            FaceTarget(true);
            if (TryGetComponent(out EnemyAlarmNetwork eam)) eam.TriggerAlarm();
        }
        agrTimer = agrTime;
    }

    public virtual void Hit () {
        DamageInfo enemyDamageInfo = CalculateDamage.enemyDamageInfo(baseDamage, enemyName);
        PlayerControlls.instance.GetComponent<Characteristics>().GetHit(enemyDamageInfo, hitType, 0.2f, 1.5f);
    }

    public bool checkCanHit (float value) {
        if (animator.GetFloat("CanHit") == value)
            return true;
        else 
            return false;
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
    public void PlaySound(AudioClip clip, float timeOffest = 0, float pitch = 1, float delay = 0, float volume = 1) {
        audioSource.clip = clip;
        audioSource.time = timeOffest;
        audioSource.pitch = pitch;
        audioSource.volume = volume;
        if (delay == 0)
            audioSource.PlayOneShot(audioSource.clip);
        else
            audioSource.PlayDelayed(delay);
    }

    public virtual void FootStep () {
        if (!footstepManager)
            return;

        footstepManager.PlayFootstepSound();
    }
    public void SetGroundType (GroundType groundType) {
        if (!footstepManager) footstepManager = GetComponentInChildren<FootstepManager>();

        if (!footstepManager)
            return;

        footstepManager.currentGroundType = groundType;
    }

    public virtual void PlayAttackSound (AnimationEvent animationEvent) {
        float pitch = animationEvent.floatParameter == 0 ? 1 : animationEvent.floatParameter;
        audioSource.pitch = pitch + Random.Range(-0.1f, 0.1f);
        audioSource.PlayOneShot(attackSounds[animationEvent.intParameter]);
    }

    public Vector3 globalEnemyBounds (){
        return Vector3.Scale(localEnemyBounds, transform.localScale);
    }

    float interruptBlockDuration = 10;
    float accumulatedInterrupsWindow = 7;
    int maxInterruptions = 5;
    float firstInterruptTime;
    int accumulatedInterruptions;
    Coroutine interruptCor;
    protected void CheckInterruptLimit () {
        if (Time.time - firstInterruptTime > accumulatedInterrupsWindow) {
            accumulatedInterruptions = 0;
            firstInterruptTime = Time.time;
        }

        accumulatedInterruptions++;
        if (accumulatedInterruptions >= maxInterruptions) {
            if (interruptCor != null) StopCoroutine(interruptCor);
            interruptCor = StartCoroutine(BlockInterrupts());

            accumulatedInterruptions = 0;
            firstInterruptTime = Time.time;
        }
    }
    IEnumerator BlockInterrupts() {
        immuneToInterrupt = true;
        yield return new WaitForSeconds(interruptBlockDuration);
        immuneToInterrupt = false;
    }

    public virtual void OnWeakSpotHit() {}

    void OnValidate() {
        loot.OnValidate();
    }

#region IDamagable

    private List<RecurringEffect> _recurringEffects = new List<RecurringEffect>();
    public List<RecurringEffect> recurringEffects {
        get {
            return _recurringEffects;
        }
    }

    public virtual void GetHit (DamageInfo damageInfo, bool stopHit = false, bool cameraShake = false, HitType hitType = HitType.Normal, Vector3 damageTextPos = new Vector3 (), float kickBackStrength = 50) {
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

        if (hitType == HitType.Normal) {
            if (!isKnockedDown) animator.CrossFade("GetHitUpperBody.GetHit", 0.1f, animator.GetLayerIndex("GetHitUpperBody"), 0);
        } else if (hitType == HitType.Interrupt) {
            if (!isKnockedDown) animator.CrossFade("GetHit.GetHit", 0.1f, animator.GetLayerIndex("GetHit"), 0);
            animator.CrossFade("Attacks.Empty", 0.1f);
            CheckInterruptLimit();
        } else if (hitType == HitType.Kickback) {
            KickBack(kickBackStrength);
        } else if (hitType == HitType.Knockdown) {
            KnockedDown();
        }

        currentHealth -= actualDamage;
        PlayHitParticles();
        PlayGetHitSounds();
        PlayStabSounds();
        
        if (stopHit || damageInfo.isCrit) StartCoroutine(HitStop(damageInfo.isCrit));
        if (cameraShake) PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.2f, 1*(1+actualDamage/3000), 0.1f, transform.position);
        
        damageTextPos = damageTextPos == Vector3.zero ? transform.position + Vector3.up * 1.5f : damageTextPos;
        Combat.instanace.DisplayDamageNumber(new DamageInfo(actualDamage, damageInfo.damageType, damageInfo.isCrit, damageInfo.sourceName), damageTextPos);

        string criticalDEBUGtext = damageInfo.isCrit ? " CRITICAL" : "";
        PeaceCanvas.instance.DebugChat($"[{System.DateTime.Now.Hour}:{System.DateTime.Now.Minute}:{System.DateTime.Now.Second}] <color=blue>{enemyName}</color> was hit with<color=red>{criticalDEBUGtext} {actualDamage} {damageInfo.damageType}</color> damage by <color=#80FFFF>{damageInfo.sourceName}</color>.");
    }

    public virtual void RunRecurringEffects () {
        for (int i = recurringEffects.Count-1; i >= 0; i--) {
            recurringEffects[i].frequencyTimer -= Time.deltaTime;
            recurringEffects[i].durationTimer -= Time.deltaTime;
            if (recurringEffects[i].frequencyTimer <= 0) {
                GetHit(CalculateDamage.damageInfo(recurringEffects[i].damageType, recurringEffects[i].baseEffectPercentage, recurringEffects[i].name, 0));
                recurringEffects[i].frequencyTimer = 1/recurringEffects[i].frequencyPerSecond;
            }
            if (recurringEffects[i].durationTimer <= 0) {
                if (recurringEffects[i].vfx) {
                    Destroy(recurringEffects[i].vfx.gameObject, 3f);
                    recurringEffects[i].vfx.Stop();
                }
                recurringEffects.RemoveAt(i);
            }
        }
    }
    
    public virtual void AddRecurringEffect (RecurringEffect effect) {
        RecurringEffect newEffect = new RecurringEffect(effect.name, effect.damageType, effect.recurringEffectType, effect.baseEffectPercentage, effect.frequencyPerSecond, effect.duration,
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
        recurringEffects.Add(newEffect);
    }

#endregion

#region ILootable 

    private bool _canDropLoot = true;
    public bool canDropLoot {
        get {
            return _canDropLoot;
        }
        set {
            _canDropLoot = value;
        }
    }

    public virtual void DropLoot () {
        if (!canDropLoot) return;

        ItemAmountPair[] allLoot = loot.GetLoot();
        foreach (ItemAmountPair ia in allLoot) {
            LootManager.instance.DropItem(ia.item1, ia.amount1, transform.position);
        }
        
        if (goldLootAmount > 0) {
           LootManager.instance.DropGold(goldLootAmount, transform.position);
        }
    }

#endregion
}
