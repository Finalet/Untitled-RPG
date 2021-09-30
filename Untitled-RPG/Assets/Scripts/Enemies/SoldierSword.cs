using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierSword : NavAgentEnemy
{
    [Space]
    public Transform sword;
    public Transform swordHandSpot;
    public Transform swordHipSpot;

    bool shouldntReturn;


    protected override void Start()
    {
        base.Start();
        agrDelay = 1.7f;
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

    protected override void Update()
    {
        base.Update();

        if (isRagdoll || isKnockedDown || isDead || PlayerControlls.instance == null) { //Player instance is null when level is only loading.
            if (navAgent.enabled) navAgent.isStopped = true;
            return;
        }

        enemyController.speed = currentState == EnemyState.Returning ? 1.5f : 4.8f;
    }


    protected override void AttackTarget()
    {
        UseAttack(plannedAttack);
    }

    public void Unshethe () {
        sword.SetParent(swordHandSpot);
        sword.transform.localPosition = Vector3.zero;
        sword.transform.localRotation = Quaternion.identity;
    }
    public void Sheathe () {
        sword.SetParent(swordHipSpot);
        sword.transform.localPosition = Vector3.zero;
        sword.transform.localRotation = Quaternion.identity;
    }

    protected override void OnStateChange()
    {
        base.OnStateChange();
        if (currentState == EnemyState.Returning) animator.SetTrigger("Return");

        if (currentState == EnemyState.Approaching && previousState == EnemyState.Idle) isDefending = true;
    }
}
