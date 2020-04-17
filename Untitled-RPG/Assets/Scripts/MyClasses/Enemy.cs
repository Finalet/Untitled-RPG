using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public class Enemy : MonoBehaviour
{
    public string enemyName;
    public int health;

    public bool isWalking;

    float prevHitID;

    Animator animator;
    NavMeshAgent agent;
    Transform target;

    void Start() {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        target = PlayerControlls.instance.gameObject.transform;
        agent.updatePosition = false;
    }

    void Update() {
        if (health <= 0) {
            Die();
        }
        Movement();
    }

    void Movement() {
        animator.SetBool("isWalking", isWalking);
        agent.destination = target.position;
        agent.nextPosition = transform.position;

        if (agent.remainingDistance > agent.stoppingDistance)
            isWalking = true;
        else 
            isWalking = false;
    }

    Vector3 position;
    void OnAnimatorMove ()
    {
        position = animator.rootPosition;
        position.y = agent.nextPosition.y;
        transform.position = position;
    }

    public virtual void GetHit(int damage, float hitID) {
        if (hitID == prevHitID)
            return;

        prevHitID = hitID;
        health -= damage;
        DisplayDamageNumber (damage);
        print ("Got damaged by " + damage + "HP. " + health + " HP left.");
    }

    public virtual void Die() {
        print("Died");
        Destroy(gameObject);
    }

    void DisplayDamageNumber(int damage) {
        TextMeshProUGUI ddText = Instantiate(AssetHolder.instance.ddText);
        ddText.text = damage.ToString();
    }
}
