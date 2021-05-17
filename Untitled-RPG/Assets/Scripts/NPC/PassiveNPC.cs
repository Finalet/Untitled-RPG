using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum PassiveNPCType {Patrolling, Static};

[System.Serializable]
public struct PatrollingTarget {
    public Transform target;
    public float stopTime;
}

public class PassiveNPC : MonoBehaviour
{
    public PassiveNPCType NPCType;
    public PatrollingTarget[] patrollingPoints;
    public float patrollingDelay;

    [Header("Transforms")]
    public Transform rightLeg;
    public Transform leftLeg;

    int prevDestinationPoint;
    int destinationPoint;
    float stopTime;
    float timeReachedPoint;
    bool once;

    Animator animator;
    NavMeshAgent navAgent;

    void Awake() {
        animator = GetComponent<Animator>(); 
        
        if (NPCType != PassiveNPCType.Static) {
            navAgent = GetComponent<NavMeshAgent>();
        }
    }

    void Start() {
        if (NPCType == PassiveNPCType.Patrolling) {
            GotoNextPoint();
        }
    }
    
    void Update() {
        if (NPCType == PassiveNPCType.Patrolling)
            PatrollingAI();
    }

    void PatrollingAI () {
        if (!navAgent.pathPending && navAgent.remainingDistance < navAgent.stoppingDistance)
            GotoNextPoint();
    }

    void GotoNextPoint() {
        if (!once) {
            timeReachedPoint = Time.time;
            once = true;
        }
        if (Time.time - timeReachedPoint < stopTime) {
            transform.rotation = Quaternion.Slerp(transform.rotation, patrollingPoints[prevDestinationPoint].target.rotation, Time.deltaTime * 7f);
            animator.SetBool("isWalking", false);
            return;
        }

        if (patrollingPoints.Length == 0)
            return;

        once = false;

        // Set the agent to go to the currently selected destination.
        navAgent.destination = patrollingPoints[destinationPoint].target.position;
        stopTime = patrollingPoints[destinationPoint].stopTime;
        navAgent.autoBraking = stopTime <= 0 ? false : true;

        // Choose the next point in the array as the destination, cycling to the start if necessary.
        prevDestinationPoint = destinationPoint;
        destinationPoint = (destinationPoint + 1) % patrollingPoints.Length;
        animator.SetBool("isWalking", true);
    }

    void CheckWhichFootIsUp() {
        if (NPCType != PassiveNPCType.Static) animator.SetBool("isRightLegUp", leftLeg.position.y < rightLeg.position.y ? true : false);
    }

    void OnAnimatorMove () {
        if(Time.timeScale != 0) navAgent.speed = (animator.deltaPosition / Time.deltaTime).magnitude;
    }
}
