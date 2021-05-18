using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum PassiveNPCType {Patrolling, Static};

[System.Serializable]
public struct PatrollingTarget {
    public Transform target;
    public float stopTime;
    public bool talk;
}

public class PassiveNPC : MonoBehaviour
{
    public PassiveNPCType NPCType;
    [Range(0, 7)] public int meshIndex;

    [Space]
    public bool isTalking;
    public bool isSitting;
    public PatrollingTarget[] patrollingPoints;

    [Header("Transforms")]
    public Transform rightLeg;
    public Transform leftLeg;
    public SkinnedMeshRenderer skinnedMesh;
    public Mesh[] meshes;

    float idleAnimID;
    int desiredIdleAnimID;

    int prevDestinationPoint;
    int destinationPoint;
    float stopTime;
    float timeReachedPoint;
    bool once = true;
    bool switchedTalkAnim;

    Animator animator;
    NavMeshAgent navAgent;
    SittingSpot currentSittingSpot;

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

        ChangeIdleAnimation();
        Invoke("ChangeIdleAnimation", Random.Range(5f, 10f));
    }
    
    void Update() {
        if (NPCType == PassiveNPCType.Patrolling) 
            PatrollingAI();
            
        if(NPCType != PassiveNPCType.Static) {
            navAgent.nextPosition = isSitting ? Vector3.Lerp(transform.position, currentSittingSpot.transform.position, Time.deltaTime * 7f) : navAgent.nextPosition;
            CheckWhichFootIsUp();
        }

        if (idleAnimID != desiredIdleAnimID) {
            idleAnimID = Mathf.MoveTowards(idleAnimID, desiredIdleAnimID, Time.deltaTime * 5);
        }

        Talking();            

        animator.SetFloat("idleAnimationID", idleAnimID);
        animator.SetBool("isTalking", isTalking);
    }

    void Talking () {
        if (isTalking) { //Every 2 seconds generate new talk ID;
            float normalizedTime = Mathf.Round(animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 * 100) / 100f;
            if (normalizedTime >= 0.9f && !switchedTalkAnim) {
                CrossFadeNewTalkAnim();
            }
            if (normalizedTime >= 0.3f && normalizedTime <= 0.8f)
                switchedTalkAnim = false;
        }
    }

    void PatrollingAI () {
        if (!navAgent.pathPending && navAgent.remainingDistance < navAgent.stoppingDistance){
            if (!once) {
                ReachedNewPoint();
                once = true;
            }
            if (Time.time - timeReachedPoint < stopTime) {
                transform.rotation = Quaternion.Slerp(transform.rotation, patrollingPoints[prevDestinationPoint].target.rotation, Time.deltaTime * 7f);
                animator.SetBool("isWalking", false);
                isTalking = patrollingPoints[prevDestinationPoint].talk;
                return;
            }   
            GotoNextPoint();
        }
    }

    void GotoNextPoint() {
        if (patrollingPoints.Length == 0)
            return;
        
        once = false;
        isTalking = false;
        if (currentSittingSpot != null)
            Unsit(currentSittingSpot);

        // Set the agent to go to the currently selected destination.
        navAgent.destination = patrollingPoints[destinationPoint].target.position;
        stopTime = patrollingPoints[destinationPoint].stopTime;
        navAgent.autoBraking = stopTime <= 0 ? false : true;

        // Choose the next point in the array as the destination, cycling to the start if necessary.
        prevDestinationPoint = destinationPoint;
        destinationPoint = (destinationPoint + 1) % patrollingPoints.Length;
        animator.SetBool("isWalking", true);
    }

    void ReachedNewPoint () {
        timeReachedPoint = Time.time;

        if (patrollingPoints[prevDestinationPoint].stopTime > 0) {
            if (patrollingPoints[prevDestinationPoint].target.TryGetComponent(out SittingSpot sitSpot)) {
                if (!sitSpot.isTaken) Sit(sitSpot);
            }
        }
    }

    void Sit (SittingSpot spot) {
        isSitting = true;
        animator.SetBool("isSitting", true);
        animator.SetTrigger("Sit");
        spot.isTaken = true;
        currentSittingSpot = spot;
    }
    void Unsit(SittingSpot spot) {
        isSitting = false;
        animator.SetBool("isSitting", false);
        spot.isTaken = false;
        currentSittingSpot = null;
    }

    void CheckWhichFootIsUp() {
        if (NPCType != PassiveNPCType.Static) animator.SetBool("isRightLegUp", leftLeg.position.y < rightLeg.position.y ? true : false);
    }

    void ChangeIdleAnimation() {
        desiredIdleAnimID = Random.Range(0, 2);
    }

    void OnAnimatorMove () {
        if(Time.timeScale != 0 && NPCType != PassiveNPCType.Static)
            navAgent.speed = (animator.deltaPosition / Time.deltaTime).magnitude;
    }

    void OnValidate() {
        meshIndex = Mathf.Clamp(meshIndex, 0, meshes.Length-1);
        skinnedMesh.sharedMesh = meshes[meshIndex];
    }

    void CrossFadeNewTalkAnim () {
        switchedTalkAnim = true;
        switch (Random.Range(1, 9)) {
            case 1:
                animator.CrossFade("Talk.Talk1", 0.1f);
                break;
            case 2:
                animator.CrossFade("Talk.Talk2", 0.1f);
                break;
            case 3:
                animator.CrossFade("Talk.Talk3", 0.1f);
                break;
            case 4:
                animator.CrossFade("Talk.Talk4", 0.1f);
                break;
            case 5:
                animator.CrossFade("Talk.Talk5", 0.1f);
                break;
            case 6:
                animator.CrossFade("Talk.Talk6", 0.1f);
                break;
            case 7:
                animator.CrossFade("Talk.Talk7", 0.1f);
                break;
            case 8:
                animator.CrossFade("Talk.Talk8", 0.1f);
                break;
        }
    }
}
