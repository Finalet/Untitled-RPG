using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public class Enemy : MonoBehaviour
{
    public string enemyName;
    public int maxHealth;
    public int health;
    public int baseDamage;

    public float playerDetectRadius;
    public float attackRadius;

    public float distanceToPlayer;

    public bool staticEnemy;
    public Transform spawner;
 
    [Header("States")]
    public bool isWalking;
    public bool isGettingHit;
    public bool isDead;
    public bool isKnockedDown;
    public bool canGetHit = true;

    public Animator animator;
    public NavMeshAgent agent;
    public Transform target;

    [System.NonSerialized] public bool canHit;

    [Header("Adjustements from debuffs")]
    public float TargetSkillDamagePercentage;

    public virtual void Start() {
        canGetHit = true;
        health = maxHealth;
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        if (!staticEnemy)
            agent.updatePosition = false;
    }

    public virtual void Update() {
        if (isDead)
            return;

        if (health <= 0) {
            Die();
        }

        if (!staticEnemy && !isKnockedDown)
            Movement();


        distanceToPlayer = Vector3.Distance(transform.position, PlayerControlls.instance.transform.position);
        //Creates collision between enemies, but makes kinematic when interacting with player to stop him from applying force.
        if (isDead || isKnockedDown) {
            GetComponent<Rigidbody>().isKinematic = true;
        } else {
            if (distanceToPlayer <= 1.5f)
                GetComponent<Rigidbody>().isKinematic = true;
            else 
                GetComponent<Rigidbody>().isKinematic = false; 
        }
    }

    void Movement() {
        UpdateTarget();

        animator.SetBool("isWalking", isWalking);
        agent.destination = target.position;
        agent.nextPosition = transform.position;


        if (agent.remainingDistance > agent.stoppingDistance) {
            isWalking = true;
        } else {
            isWalking = false;
            FaceTarget();
        }
    }

    void UpdateTarget () {
        if (Vector3.Distance(transform.position, PlayerControlls.instance.transform.position) <= playerDetectRadius) {
            target = PlayerControlls.instance.transform;
        } else {
            if (spawner != null)
                target = spawner;
        }
    }

    Vector3 position;
    void OnAnimatorMove ()
    {
        position = animator.rootPosition;
        transform.position = position;
    } 

    void FaceTarget () {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(transform.position, playerDetectRadius);   
    }

    public virtual void GetHit(int damage) {
        if (isDead || !canGetHit)
            return;
            
        if (!staticEnemy && !isKnockedDown) animator.Play("GetHit.GetHit", animator.GetLayerIndex("GetHit"), 0);
        int actualDamage = Mathf.RoundToInt( damage * (1 + TargetSkillDamagePercentage/100) ); 
        health -= actualDamage;
        DisplayDamageNumber (actualDamage);
        gameObject.SendMessage("CustomGetHit", actualDamage, SendMessageOptions.DontRequireReceiver);
    }

    public virtual void Die() {
        isDead = true;
        animator.CrossFade("GetHit.Die", 0.25f);
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        spawner.GetComponent<EnemySpawner>().listOfAllEnemies.Remove(gameObject);
        Destroy(gameObject, 10f);
    }

    public virtual void GetKnockedDown() {
        if (!isDead && !staticEnemy)
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
        canGetHit = false;
        animator.Play("GetHit.GetUp");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(animator.GetLayerIndex("GetHit")).Length);
        isKnockedDown = false;
        canGetHit = true;
    }


    void DisplayDamageNumber(int damage) {
        GameObject ddText = Instantiate(AssetHolder.instance.ddText, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        ddText.GetComponent<ddText>().damage = damage;
    }
}
