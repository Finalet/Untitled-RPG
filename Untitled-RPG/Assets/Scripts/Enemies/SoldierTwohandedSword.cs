using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierTwohandedSword : NavAgentEnemy
{
    [Space]
    public ParticleSystem teleportStartVFX;
    public ParticleSystem teleportEndVFX;
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

    IEnumerator Teleport() {
        teleportStartVFX.Play();
        meshRenderer.enabled = false;
        yield return new WaitForSeconds(0.5f);
        Vector3 teleportPos = PlayerControlls.instance.transform.position + PlayerControlls.instance.transform.forward * 1.5f;
        Vector3 lookRot = PlayerControlls.instance.transform.position - teleportPos;
        lookRot.y = 0;
        transform.position = teleportPos;
        transform.rotation = Quaternion.LookRotation(lookRot.normalized, Vector3.up);
        teleportEndVFX.Play();
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
}
