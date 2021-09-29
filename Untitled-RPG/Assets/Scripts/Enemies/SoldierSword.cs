using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierSword : NavAgentEnemy
{

    protected override void Start()
    {
        base.Start();
        agrDelay = 1.7f;
    }

    protected override void Update()
    {
        base.Update();

        if (isRagdoll || isKnockedDown || isDead || PlayerControlls.instance == null) { //Player instance is null when level is only loading.
            if (navAgent.enabled) navAgent.isStopped = true;
            return;
        }
    }


    protected override void AttackTarget()
    {
        UseAttack(plannedAttack);
    }
}
