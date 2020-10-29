using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState {Idle, Approaching, Attacking, Returning};

public abstract class Enemy : MonoBehaviour
{
    public string enemyName;
    [Header("Stats")]
    public int maxHealth;
    public int currentHealth;
    public int baseDamage;
    public float playerDetectRadius;
    public float attackRange;

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

    protected float distanceToPlayer;

    //FOR TESTING ONLY
    [Header("States")]
    public bool isWalking;
    public bool isAttacking;
    public bool isGettingHit; //using
    public bool isDead; //using
    public bool isKnockedDown; //using
    public bool isReturning;
    public bool canGetHit = true; //using
    public bool agr; //Agressive - if true, then targets and attacks the player. if false then resting/idling
    bool hitOnce;

    [Space]
    public GameObject healthBar;
    public ParticleSystem hitParticles;
    public AudioClip[] getHitSounds;
    public AudioClip[] stabSounds;

    protected Animator animator;
    protected NavMeshAgent navAgent;
    protected Transform target;
    protected AudioSource audioSource;
    protected Vector3 initialPos;

    protected virtual void Start() {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;
        initialPos = transform.position;
    }

    protected virtual void Update() {
        if (isDead || PlayerControlls.instance == null) { //Player instance is null when level is only loading.
            return;
        }
        
        Health();
        ShowHealthBar();
        AttackCoolDown();

        distanceToPlayer = Vector3.Distance(transform.position, PlayerControlls.instance.transform.position);
    
        CheckAgr();

        AI();

        CollisionProtection(); //Creates collision between enemies, but makes kinematic when interacting with player to stop him from applying force.
    }

    protected virtual void AI () {
        if (agr) {
            if (distanceToPlayer > attackRange) {
                currentState = EnemyState.Approaching;
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

    protected abstract void ApproachTarget();
    protected virtual void TryAttackTarget() {
        if (isCoolingDown || isDead || isKnockedDown)
            return;
        
        coolDownTimer = attackCoolDown;
        AttackTarget();
    }
    protected abstract void AttackTarget();
    protected abstract void FaceTarget();
    protected abstract void ReturnToPosition();
    protected abstract void Idle();
    
    protected virtual void Health () {
        if (isDead)
            return;

        if (currentHealth <= 0) {
            isDead = true;
            animator.CrossFade("GetHit.Die", 0.25f);
            GetComponent<Collider>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            Destroy(gameObject, 10f);
        }

        //Add health regeneration
    }

    public virtual void GetHit (int damage, string skillName, bool stopHit = false, bool cameraShake = false, Vector3 damageTextPos = new Vector3 ()) {
        if (isDead || !canGetHit)
            return;
        
        int actualDamage = calculateActualDamage(damage);

        if (!isKnockedDown)
            animator.CrossFade("GetHit.GetHit", 0.25f, animator.GetLayerIndex("GetHit"), 0);

        currentHealth -= actualDamage;
        PlayHitParticles();
        PlayGetHitSounds();
        PlayStabSounds();
        
        if (stopHit) StartCoroutine(HitStop());
        if (cameraShake) PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.2f, 1*(1+actualDamage/3000), 0.1f, transform.position);
        DisplayDamageNumber(actualDamage, damageTextPos);

        PeaceCanvas.instance.DebugChat($"[{System.DateTime.Now.Hour}:{System.DateTime.Now.Minute}:{System.DateTime.Now.Second}] {enemyName} was hit <color=red>{actualDamage}</color> points by <color=#80FFFF>{skillName}</color>.");
    }

    protected void DisplayDamageNumber(int damage, Vector3 position = new Vector3()) {
        if (position == Vector3.zero)   //If position is not specified then place on standard spot
            position = transform.position + Vector3.up * 1.5f;

        GameObject ddText = Instantiate(AssetHolder.instance.ddText, position, Quaternion.identity);
        ddText.GetComponent<ddText>().damage = damage;
    }

    protected int calculateActualDamage (int damage) {
        return Mathf.RoundToInt( damage * (1 + TargetSkillDamagePercentage/100) );
    }

    protected IEnumerator HitStop () {
        float timer = Time.realtimeSinceStartup;
        Time.timeScale = 0.5f;
        while(Time.realtimeSinceStartup - timer < 0.07f) {
            yield return null;
        }
        Time.timeScale = 1;
    }
    
    public virtual void GetKnockedDown() {
        if (!isDead)
            StartCoroutine(KnockedDown());
    }
    IEnumerator KnockedDown () {
        animator.Play("GetHit.KnockDown");
        isKnockedDown = true;
        float timer = 3;
        while (timer>0) {
            timer -= Time.fixedDeltaTime;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
            if (isDead)
                yield break;
        }
        animator.Play("GetHit.GetUp");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(animator.GetLayerIndex("GetHit")).Length);
        isKnockedDown = false;
    }

    protected virtual void ShowHealthBar () {
        if (isDead) {
            healthBar.SetActive(false);
            return;
        }

        if (distanceToPlayer <= PlayerControlls.instance.playerCamera.GetComponent<LookingTarget>().viewDistance / 1.5f) {
            healthBar.transform.GetChild(0).localScale = new Vector3((float)currentHealth/maxHealth, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
            healthBar.transform.LookAt (PlayerControlls.instance.playerCamera.transform);
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
        if (distanceToPlayer <= playerDetectRadius || isGettingHit)
            agrTimer = agrTime;
        
        if (agrTimer > 0) {
            agr = true;
            agrTimer -= Time.deltaTime;
        } else {
            agr = false;
        }
    }

    public virtual void Hit () {
        if (hitOnce || !canHit())
            return;

        PlayerControlls.instance.GetComponent<Characteristics>().GetHit(damage(), 0.2f, 1f);
        hitOnce = true;
    }

    public bool canHit () {
        if (animator.GetFloat("CanHit") >= 0.5f && !isGettingHit)
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
            hitOnce = false;
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

    protected virtual void CollisionProtection () {
        if (isDead || isKnockedDown) {
            GetComponent<Rigidbody>().isKinematic = true;
        } else {
            if (distanceToPlayer <= 1.5f)
                GetComponent<Rigidbody>().isKinematic = true;
            else 
                GetComponent<Rigidbody>().isKinematic = false; 
        }
    }
}
