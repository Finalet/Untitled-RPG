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
    
    protected override void AttackTarget () {
        UseAttack(plannedAttack);
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
