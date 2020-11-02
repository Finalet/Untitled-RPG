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
    public HitType hitType;

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
    public bool isAttacking;
    public bool isGettingInterrupted; //using
    public bool isDead; //using
    public bool isKnockedDown; //using
    public bool canGetHit = true; //using
    public bool agr; //Agressive - if true, then targets and attacks the player. if false then resting/idling

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
        if (distanceToPlayer <= playerDetectRadius) {
            Agr();
        }
        
        if (agr) {
            if (distanceToPlayer > attackRange && !isAttacking) {
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
        
        navAgent.isStopped = true;
        coolDownTimer = attackCoolDown;
        AttackTarget();
    }
    protected abstract void AttackTarget();
    protected abstract void FaceTarget(bool instant = false);
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

    public virtual void GetHit (int damage, string skillName, bool stopHit = false, bool cameraShake = false, HitType hitType = HitType.Normal, Vector3 damageTextPos = new Vector3 ()) {
        if (isDead || !canGetHit)
            return;
        
        Agr();

        int actualDamage = calculateActualDamage(damage);

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
    
    protected IEnumerator KnockedDown () {
        navAgent.updatePosition = false;    //for some reason navmesh messes with KickBack animation and goblin does not fly as far. so i have to turn off update pos and then reenable it below

        animator.CrossFade("GetHit.KnockDown", 0.1f);
        isKnockedDown = true;
        yield return new WaitForSeconds(3);
        if (isDead)
            yield break;
        animator.CrossFade("GetHit.GetUp", 0.1f);
        yield return new WaitForSeconds(0.7f);
        isKnockedDown = false;

        navAgent.nextPosition = transform.position;
        navAgent.updatePosition = true;
    }
    protected IEnumerator KickBack () {
        navAgent.updatePosition = false;

        animator.CrossFade("GetHit.KickBack", 0.1f);
        isKnockedDown = true;
        yield return new WaitForSeconds(2);
        if (isDead)
            yield break;
        animator.CrossFade("GetHit.GetUp", 0.1f);
        yield return new WaitForSeconds(2);
        isKnockedDown = false;

        navAgent.nextPosition = transform.position; 
        navAgent.updatePosition = true;
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
            FaceTarget(true);
        }
        agrTimer = agrTime;
    }

    public virtual void Hit () {
        if (!canHit())
            return;

        PlayerControlls.instance.GetComponent<Characteristics>().GetHit(damage(), hitType, 0.2f, 1f);
    }

    public bool canHit () {
        if (animator.GetFloat("CanHit") >= 0.5f)
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

    protected virtual void CollisionProtection () {
        if (isDead || isKnockedDown) {
            GetComponent<Rigidbody>().isKinematic = true;
        } else {
            //if (distanceToPlayer <= 1f)
            //    GetComponent<Rigidbody>().isKinematic = true;
            //else 
                GetComponent<Rigidbody>().isKinematic = false;
        } 
    }
}
