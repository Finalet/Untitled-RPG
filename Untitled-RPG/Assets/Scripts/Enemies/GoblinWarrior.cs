using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoblinWarrior : NavAgentEnemy
{
    [Space]
    public AudioClip[] hitSounds;

    protected override void Start()
    {
        base.Start();
        agrDelay = 1.7f;
    }

    protected override void Update()
    {   
        base.Update();

        if (isRagdoll || isKnockedDown || isDead || PlayerControlls.instance == null) { //Player instance is null when level is only loading.
            navAgent.isStopped = true;
        }
    }


    protected override void ApproachTarget () {
        base.ApproachTarget();

        navAgent.isStopped = isGettingInterrupted ? true : false;
        navAgent.destination = target.position;

        plannedAttack = ClosestAttack();
    }

    protected override void AttackTarget () {
        UseAttack(plannedAttack);
        plannedAttack = ClosestAttack();
    }

    protected override void Idle () {
        base.Idle();
        navAgent.isStopped = true;
    }

    public override void Hit()
    {
        base.Hit();
        audioSource.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Length-1)]);
    }
}
