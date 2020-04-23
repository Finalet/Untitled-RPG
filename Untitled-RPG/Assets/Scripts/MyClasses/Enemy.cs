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

    public float playerDetectRadius;
    public Transform spawner;

    bool isWalking;
    public bool isDead;

    float prevHitID;

    Animator animator;
    NavMeshAgent agent;
    Transform target;

    void Start() {
        health = maxHealth;
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        target = PlayerControlls.instance.gameObject.transform;
        agent.updatePosition = false;
    }

    void Update() {
        if (isDead)
            return;

        if (health <= 0) {
            Die();
        }
        Movement();
    }

    void Gravity () {
        if (!GetComponent<Rigidbody>().useGravity)
            return;

        RaycastHit hit;
        if (!Physics.Raycast(transform.position + Vector3.up * 0.1f, -Vector3.up, out hit, 0.15f))
            position.y = transform.position.y - 10 * Time.deltaTime;
    }

    void Movement() {

        animator.SetBool("isWalking", isWalking);
        agent.destination = target.position;
        agent.nextPosition = transform.position;

        UpdateTarget();

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
            target = spawner;
        }
    }

    Vector3 position;
    void OnAnimatorMove ()
    {
        position = animator.rootPosition;
        Gravity();
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

    public virtual void GetHit(int damage, float hitID) {
        if (hitID == prevHitID || isDead)
            return;

        prevHitID = hitID;
        animator.Play("GetHit.GetHit", animator.GetLayerIndex("GetHIt"), 0);
        health -= damage;
        DisplayDamageNumber (damage);
    }

    public virtual void Die() {
        isDead = true;
        animator.CrossFade("GetHit.Die", 0.25f);
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        spawner.GetComponent<EnemySpawner>().listOfAllEnemies.Remove(gameObject);
        Destroy(gameObject, 10f);
    }

    void DisplayDamageNumber(int damage) {
        TextMeshProUGUI ddText = Instantiate(AssetHolder.instance.ddText);
        ddText.text = damage.ToString();
    }
}
