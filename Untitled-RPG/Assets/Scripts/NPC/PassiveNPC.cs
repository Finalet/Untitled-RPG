using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
    [Range(0, 3)] public int materialIndex;
    public bool randomizeWalkSpeed = true;

    [Space]
    public bool isTalking;
    public bool isSitting;
    [Space]
    public PatrollingTarget[] patrollingPoints;

    [Header("Transforms")]
    public Transform rightLeg;
    public Transform leftLeg;
    public SkinnedMeshRenderer skinnedMesh;
    public Mesh[] meshes;
    public Material[] materials;
    public GameObject allPathPoints;

    float idleAnimID;
    int desiredIdleAnimID;

    [System.NonSerialized] public int prevDestinationPoint;
    [System.NonSerialized] public int destinationPoint;
    [System.NonSerialized] public float maxPatrollingRadius;
    float stopTime;
    float timeReachedPoint;
    bool reachedDestination = true;
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
        if (NPCType == PassiveNPCType.Static) {
            if (isSitting) {
                Sit(patrollingPoints[0].target.GetComponent<SittingSpot>());
            }
        }

        ChangeIdleAnimation();
        Invoke("ChangeIdleAnimation", Random.Range(5f, 10f));
        if(randomizeWalkSpeed) animator.SetFloat("walkSpeedMultiplier", 0.85f + Random.value * 0.3f);
        animator.SetFloat("walkCycleOffset", Random.value);
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
            if (!reachedDestination) {
                ReachedNewPoint();
                reachedDestination = true;
            }
        }
        if (reachedDestination) {
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
        
        reachedDestination = false;
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
        if(Time.timeScale != 0 && NPCType != PassiveNPCType.Static && Time.deltaTime > 0)
            navAgent.speed = (animator.deltaPosition / Time.deltaTime).magnitude;
    }

    void OnValidate() {
        meshIndex = Mathf.Clamp(meshIndex, 0, meshes.Length-1);
        materialIndex = Mathf.Clamp(materialIndex, 0, materials.Length-1);
        skinnedMesh.sharedMesh = meshes[meshIndex];
        
        if (materials.Length > 0) skinnedMesh.material = materials[materialIndex];

        if (NPCType == PassiveNPCType.Static) {
            if (!TryGetComponent(out NavMeshObstacle obstacle)) {
                obstacle = gameObject.AddComponent<NavMeshObstacle>();
                obstacle.shape = NavMeshObstacleShape.Capsule;
                obstacle.center = Vector3.up;
                obstacle.height = 2;
                obstacle.carving = true;
            }
            if (TryGetComponent(out NavMeshAgent agent)) {
                UnityEditor.EditorApplication.delayCall+=()=>
                {
                    DestroyImmediate(agent);
                };
            }
        } else if (NPCType == PassiveNPCType.Patrolling){
            if (!TryGetComponent(out NavMeshAgent agent)) {
                agent = gameObject.AddComponent<NavMeshAgent>();
                agent.angularSpeed = 500;
                agent.avoidancePriority = 50 + Random.Range(-20, 20);
                agent.radius = 0.4f;
                agent.height = 2;
                agent.stoppingDistance = 0.5f;
            }
            if (TryGetComponent(out NavMeshObstacle obstacle)) {
                UnityEditor.EditorApplication.delayCall+=()=>
                {
                    DestroyImmediate(obstacle);
                };
            }
        }
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

    public void GenerateRandomPath (int _maxPathLength = -1, int _minPathLength = -1, float maxAreaRadius = -1) {
        List<Transform> paths = new List<Transform>();
        for (int i = 0; i < allPathPoints.transform.childCount; i++)
        {
            float dis = maxAreaRadius == -1 ? -10 : Vector3.Distance (transform.position, allPathPoints.transform.GetChild(i).position);
            if (dis < maxAreaRadius)
                paths.Add(allPathPoints.transform.GetChild(i));
        }
        
        if (paths.Count < 2) {
            Debug.LogError($"Found only {paths.Count} possible path points. Try increasing Area Radius");
            return;
        }

        int maxPathLength = _maxPathLength == -1 ? paths.Count : Mathf.Min(paths.Count, _maxPathLength);
        int minPathLength = _minPathLength == -1 ? 2 : _minPathLength;

        patrollingPoints = new PatrollingTarget[Random.Range(minPathLength, maxPathLength)];

        List<int> pickedPoints = new List<int>();
        for (int i = 0; i < patrollingPoints.Length; i++){
            patrollingPoints[i].target = paths[DevTools.getUniqueIndex(pickedPoints, paths.Count)];
        }

        System.GC.Collect();
        EditorUtility.SetDirty(this);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PassiveNPC))]
public class PassiveNPCEditor : Editor
{
    float minPathLength;
    float maxPathLength;
    float areaRadius;

    PassiveNPC npc;
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        npc = (PassiveNPC)target;
        if (npc.allPathPoints != null) {
            GUILayout.Space(10);
            
            EditorGUILayout.MinMaxSlider($"Path Length Range: {minPathLength}-{maxPathLength}", ref minPathLength, ref maxPathLength, 2, npc.allPathPoints.transform.childCount);

            minPathLength = Mathf.RoundToInt(minPathLength);
            maxPathLength = Mathf.RoundToInt(maxPathLength);

            areaRadius = EditorGUILayout.Slider("Area Radius", areaRadius, -1, 100);

            if (areaRadius <= 1)
                areaRadius = -1;
            if (areaRadius > 0){
                Handles.color = Color.white;
                Handles.DrawWireArc (npc.transform.position, Vector3.up, Vector3.forward, 360, 10);
            }
            if(GUILayout.Button("Generate random path")) {
                npc.GenerateRandomPath( Mathf.RoundToInt(maxPathLength), Mathf.RoundToInt(minPathLength), areaRadius);
                SceneView.RepaintAll();
            }

            npc.maxPatrollingRadius = areaRadius;
        }
    }

    void Reset() {
        npc = (PassiveNPC)target;
        if (npc.allPathPoints == null)
            return;
        minPathLength = 2;
        maxPathLength = npc.allPathPoints.transform.childCount;
    }

    void OnSceneGUI() {
        npc = (PassiveNPC)target;
        if (npc.patrollingPoints.Length == 0)
            return;
        
        Handles.color = Color.blue;
        if (npc.patrollingPoints[npc.prevDestinationPoint].target != null)
            Handles.DrawLine(npc.transform.position, npc.patrollingPoints[npc.prevDestinationPoint].target.position);
        for (int i = 0; i < npc.patrollingPoints.Length; i++) {
            int i2 = (i + 1) % npc.patrollingPoints.Length;
            if (npc.patrollingPoints[i].target == null || npc.patrollingPoints[i2].target == null)
                continue;
            float a = 1 - (float)i/npc.patrollingPoints.Length;
            Handles.color = new Color(1, 0, 0, a);
            Handles.DrawLine(npc.patrollingPoints[i].target.position, npc.patrollingPoints[i2].target.position);
            if (npc.patrollingPoints.Length >= 2) {
                Quaternion arrowDir = Quaternion.LookRotation(npc.patrollingPoints[i2].target.position - npc.patrollingPoints[i].target.position);
                Handles.ArrowHandleCap(0, npc.patrollingPoints[i].target.position, arrowDir, 5  , EventType.Repaint);
            }
            Handles.SphereHandleCap(0, npc.patrollingPoints[i].target.position, Quaternion.identity, 0.2f, EventType.Repaint);
        }

        if (npc.allPathPoints == null)
            return;
        Handles.color = Color.white;
        Handles.DrawWireArc(npc.transform.position, Vector3.up, Vector3.forward, 360, npc.maxPatrollingRadius);
    }
}
#endif