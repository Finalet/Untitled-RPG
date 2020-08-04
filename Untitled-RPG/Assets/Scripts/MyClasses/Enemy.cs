using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public abstract class Enemy : MonoBehaviour
{
    public string enemyName;
    [Header("Stats")]
    public int maxHealth;
    public int health;
    public int baseDamage;
    public float movementSpeed = 1;
    public float attackSpeed = 1;

    public float playerDetectRadius;
    public float attackRadius;

    float baseAngularSpeed;

    [Space]
    public float distanceToPlayer;

    public Vector3 initialPos;
    public Transform spawner;
 
    [Header("States")]
    public bool isWalking;
    public bool isAttacking;
    public bool isGettingHit;
    public bool isDead;
    public bool isKnockedDown;
    public bool isReturningToBase;
    public bool canGetHit = true;
    public bool canHit;

    public Animator animator;
    public NavMeshAgent agent;
    public Transform target;

    [Space]
    public GameObject healthBar;
    public ParticleSystem hitParticles;


    [Header("Audio Clips")]
    public AudioSource stabsAudioSource;
    float stabsBasePitch;
    public AudioSource getHitAurdioSource;
    float getHitBasePitch;
    public AudioClip[] stabsClips;
    public AudioClip[] getHitVoicesClips;

    [Header("Adjustements from debuffs")]
    public float TargetSkillDamagePercentage;

    protected virtual void Start() {
        initialPos = transform.position;

        canGetHit = true;
        health = maxHealth;
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        stabsBasePitch = stabsAudioSource.pitch;
        getHitBasePitch = getHitAurdioSource.pitch;

        agent.updatePosition = false;
        baseAngularSpeed = agent.angularSpeed;
    }

    protected virtual void Update() {
        if (isDead || PlayerControlls.instance == null) { //Player instance is null when level is only loading.
            return;
        }

        if (health <= 0) {
            Die();
        }

        if (!isKnockedDown)
            Movement();

        if (isAttacking){
            agent.angularSpeed = 0;
        } else {
            agent.angularSpeed = baseAngularSpeed;
        }

        //Creates collision between enemies, but makes kinematic when interacting with player to stop him from applying force.
        if (isDead || isKnockedDown) {
            GetComponent<Rigidbody>().isKinematic = true;
        } else {
            if (distanceToPlayer <= 1.5f)
                GetComponent<Rigidbody>().isKinematic = true;
            else 
                GetComponent<Rigidbody>().isKinematic = false; 
        }

        animator.SetFloat("movementSpeed", movementSpeed);
        animator.SetFloat("attackSpeed", attackSpeed);
        
        if (animator.GetFloat("CanHit") >= 0.9f)
            canHit = true;
        else 
            canHit = false;

        ShowHealthBar();
    }

    void Movement() { 
        SetTarget();
        WalkToTarget();
    }

    float timer = 0;
    void SetTarget() {

        distanceToPlayer = Vector3.Distance(transform.position, PlayerControlls.instance.transform.position);

        if (distanceToPlayer <= playerDetectRadius || isGettingHit) {
            target = PlayerControlls.instance.transform;
            timer = 10;
        } else {
            if (timer <= 0){
                target = null;
            } else {
                timer -= Time.deltaTime;
            }
        }

        if (target == null) {
            agent.destination = initialPos;
            isReturningToBase = true;
        } else {
            isReturningToBase = false;
            agent.destination = target.position;
        }
    }

    void WalkToTarget() {
        animator.SetBool("isWalking", isWalking);

        agent.nextPosition = transform.position;

        if (agent.remainingDistance > agent.stoppingDistance && !isAttacking) {
            isWalking = true;
        } else {
            isWalking = false;
            FaceTarget();
        }
    }

    void OnAnimatorMove ()
    {
        transform.position = animator.rootPosition;
    } 

    void FaceTarget () {
        if (target == null || isAttacking)
            return;

        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    protected IEnumerator HitStop () {
        float timer = Time.realtimeSinceStartup;
        Time.timeScale = 0.25f;
        while(Time.realtimeSinceStartup - timer < 0.1f) {
            yield return null;
        }
        Time.timeScale = 1;
    }

    public virtual void Die() {
        isDead = true;
        animator.CrossFade("GetHit.Die", 0.25f);
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        if (spawner != null) spawner.GetComponent<EnemySpawner>().listOfAllEnemies.Remove(gameObject);
        Destroy(gameObject, 10f);
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

        if (Vector3.Distance(transform.position, PlayerControlls.instance.transform.position) <= PlayerControlls.instance.playerCamera.GetComponent<LookingTarget>().viewDistance / 1.5f) {
            healthBar.transform.GetChild(0).localScale = new Vector3((float)health/maxHealth, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
            healthBar.transform.LookAt (PlayerControlls.instance.playerCamera.transform);
            healthBar.SetActive(true);
        } else {
            healthBar.SetActive(false);
        }
    }

    protected virtual void PlayGetHitSounds() {
        if (getHitVoicesClips.Length == 0)
            return;

        int playID = Random.Range(0, getHitVoicesClips.Length);
        getHitAurdioSource.clip = getHitVoicesClips[playID];
        getHitAurdioSource.pitch = getHitBasePitch + Random.Range(-0.1f, 0.1f);
        getHitAurdioSource.Play();
    }
    protected virtual void PlayStabSounds() {
        if (stabsClips.Length == 0)
            return;

        int playID = Random.Range(0, stabsClips.Length);
        stabsAudioSource.clip = stabsClips[playID];
        stabsAudioSource.pitch = stabsBasePitch + Random.Range(-0.1f, 0.1f);
        stabsAudioSource.Play();
    }

    protected virtual void HitParticles () {
        if (hitParticles == null)
            return;

        hitParticles.Play();
    }

    public abstract void Hit();

#region Get hit overloads

    protected int actualDamage;

    protected int calculateActualDamage (int damage) {
        return Mathf.RoundToInt( damage * (1 + TargetSkillDamagePercentage/100) );
    }
    protected bool checkFailed () {
        if (isDead || !canGetHit)
            return true;
        else 
            return false;
    }

    protected virtual void BasicGetHit (int damage, string skillName) {
        if (!isKnockedDown)
            animator.CrossFade("GetHit.GetHit", 0.25f, animator.GetLayerIndex("GetHit"), 0);

        actualDamage = calculateActualDamage(damage);

        health -= actualDamage;
        HitParticles(); 
        PlayGetHitSounds(); 
        PlayStabSounds();

        PeaceCanvas.instance.DebugChat($"[{System.DateTime.Now.Hour}:{System.DateTime.Now.Minute}:{System.DateTime.Now.Second}] {enemyName} was hit <color=red>{actualDamage}</color> points by <color=#80FFFF>{skillName}</color>.");
    }

    public virtual void GetHit(int damage, string skillName) {
        if (checkFailed())
            return;      

        BasicGetHit(damage, skillName); 
        DisplayDamageNumber (actualDamage);
    }

    public virtual void GetHit(int damage, Vector3 damageTextPos, string skillName) {
        if (checkFailed())
            return;

        BasicGetHit(damage, skillName);
        DisplayDamageNumber(actualDamage, damageTextPos);
    }

    public virtual void GetHit(int damage, bool stopHit, bool cameraShake, string skillName) {
        if (checkFailed())
            return;

        BasicGetHit(damage, skillName);
        DisplayDamageNumber (actualDamage);
        if (cameraShake) PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.2f, 1*(1+actualDamage/2000), 0.1f, transform.position);
        if (stopHit) StartCoroutine(HitStop());
    }

    public virtual void GetHit(int damage, bool stopHit, bool cameraShake, Vector3 damageTextPos, string skillName) {
        if (checkFailed())
            return;
            
        BasicGetHit(damage, skillName);
        DisplayDamageNumber(actualDamage, damageTextPos);
        if (cameraShake) PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.2f, 1*(1+actualDamage/2000), 0.1f, transform.position);
        if (stopHit) StartCoroutine(HitStop());
    }
#endregion

#region Display damage number overloads
    protected void DisplayDamageNumber(int damage) {
        GameObject ddText = Instantiate(AssetHolder.instance.ddText, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        ddText.GetComponent<ddText>().damage = damage;
    }
    protected void DisplayDamageNumber(int damage, Vector3 position) {
        GameObject ddText = Instantiate(AssetHolder.instance.ddText, position, Quaternion.identity);
        ddText.GetComponent<ddText>().damage = damage;
    }
#endregion

}
