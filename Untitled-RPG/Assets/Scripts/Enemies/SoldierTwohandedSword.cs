using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierTwohandedSword : NavAgentEnemy
{
    [Space]
    public ParticleSystem teleportStartVFX;
    public ParticleSystem teleportEndVFX;
    public ParticleSystem hitGroundVFX;
    public SkinnedMeshRenderer meshRenderer;
    [Space]
    public Transform sword;
    public Transform swordHandSpot;
    public Transform swordBackSpot;

    bool shouldntReturn;

    ParticleSystem trails;

    protected override void Start()
    {
        base.Start();
        agrDelay = 1.7f;
        
        trails = sword.GetComponentInChildren<ParticleSystem>();
    }

    protected override void ReturnToPosition()
    {
        base.ReturnToPosition();
        StartCoroutine(DelayReturnWhileSheathing());
        navAgent.isStopped = shouldntReturn;
    }
    IEnumerator DelayReturnWhileSheathing() {
        shouldntReturn = true;
        while(!animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Main")).IsName("Returning")) 
            yield return null;
        shouldntReturn = false;
    }

    protected override void AttackTarget()
    {
        UseAttack(plannedAttack);
        if (plannedAttack.attackName == "Teleport") {
            StartCoroutine(Teleport());
        }
    }

    IEnumerator Teleport(Vector3 destination = new Vector3()) {
        teleportStartVFX.Play();
        meshRenderer.enabled = false;
        yield return new WaitForSeconds(0.5f);

        if (destination == Vector3.zero) destination = PlayerControlls.instance.transform.position + PlayerControlls.instance.rb.velocity * 0.5f;

        Vector3 lookRot = PlayerControlls.instance.transform.position - destination;
        lookRot.y = 0;
        transform.position = destination;
        transform.rotation = Quaternion.LookRotation(lookRot.normalized, Vector3.up);
        teleportEndVFX.Play();
        if (destination == Vector3.zero) UseAttack(attacks[0]);
        yield return new WaitForSeconds(0.2f);
        meshRenderer.enabled = true;

        distanceToPlayer = Vector3.Distance(transform.position, PlayerControlls.instance.transform.position);
        
        plannedAttack = ClosestAttack();
    }

    public void Unshethe () {
        sword.SetParent(swordHandSpot);
        sword.transform.localPosition = Vector3.zero;
        sword.transform.localRotation = Quaternion.identity;
    }
    public void Sheathe () {
        sword.SetParent(swordBackSpot);
        sword.transform.localPosition = Vector3.zero;
        sword.transform.localRotation = Quaternion.identity;
    }

    public void EnableTrails() {
        trails.Play();
    }
    public void DisableTrails() {
        trails.Stop();
    }
    public void GroundHitVFX() {
        hitGroundVFX.Play();
    }

    protected override void OnStateChange()
    {
        base.OnStateChange();
        if (currentState == EnemyState.Returning) animator.SetTrigger("Return");
    }

    protected override void ApplyEnemyControllerSettings () {
        enemyController.useRootMotion = true;
        enemyController.useRootMotionRotation = false;
        enemyController.speed = enemyController.useRootMotion ? baseControllerSpeed * 50 : baseControllerSpeed;
    }

    protected override void OnHitAbuseDetected()
    {
        base.OnHitAbuseDetected();
        Vector3 destination = PlayerControlls.instance.transform.position + PlayerControlls.instance.transform.forward * (10 + Random.value * 10) + Vector3.up * 1 + PlayerControlls.instance.transform.right * (Random.value-0.5f) * 10;
        RaycastHit hit;
        if (Physics.Raycast(destination, -Vector3.up, out hit, 20, LayerMask.GetMask("Terrain", "Static Level", "Default"))) {
            destination.y = hit.point.y;
        }
        int numberOfTries = 0;
        while(Physics.Raycast(destination, Vector3.up, out hit, 50)) {
            destination = PlayerControlls.instance.transform.position + PlayerControlls.instance.transform.forward * (10 + Random.value * 10) + Vector3.up * 1 + PlayerControlls.instance.transform.right * (Random.value-0.5f) * 10;
            RaycastHit hit1;
            if (Physics.Raycast(destination, -Vector3.up, out hit1, 20, LayerMask.GetMask("Terrain", "Static Level", "Default"))) {
                destination.y = hit1.point.y;
            }
            numberOfTries ++;
            if (numberOfTries > 100) break;
        }
        StartCoroutine(Teleport(destination));
    }

    protected override void KickBack(float kickBackStrength = 50)
    {
        base.KickBack(kickBackStrength/3);
    }
}
