using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoblinArcher : NavAgentEnemy
{
    [Header("Custom vars")]
    public bool huntPlayer;
    public GameObject arrowPrefab;
    public float shootStrength;
    public Transform bowTransform;
    public Transform rightHandTransform;
    
    EnemyArrow newArrow;
    LayerMask ignoreEnemy;
    Vector3 shootPoint;
    bool grabBowstring;

    protected override void Start()
    {
        base.Start();
        agrDelay = 1.7f;

        plannedAttack = attacks[0];
    }


    protected override void CalculateCurrentState()
    {
        base.CalculateCurrentState();
        if (currentState == EnemyState.Approaching && !huntPlayer) currentState = EnemyState.Idle;
    }

    protected override void ApproachTarget () {
        base.ApproachTarget();

        navAgent.isStopped = isGettingInterrupted ? true : false;
        navAgent.destination = target.position;
    }

    protected override void AttackTarget () {
        UseAttack(plannedAttack);
        grabBowstring = true;
        StartCoroutine(GrabBowstringIE());
        StartCoroutine(SpawnArrowIE());
        //play attack sound
    }
    public void Shoot() {
        if (newArrow == null)
            newArrow = Instantiate(arrowPrefab, bowTransform).GetComponent<EnemyArrow>();

        ignoreEnemy =~ LayerMask.GetMask("Enemy");

        shootPoint = PlayerControlls.instance.transform.position + Vector3.up * 1.5F + PlayerControlls.instance.rb.velocity * 0.5f * distanceToPlayer/30;

        newArrow.Shoot(shootStrength, shootPoint, CalculateDamage.enemyDamageInfo(finalDamage, enemyName));
        newArrow.hitType = hitType;
        bowTransform.GetComponent<Bow>().ReleaseString();
        grabBowstring = false;

        newArrow = null;

        audioSource.PlayOneShot(attackSounds[Random.Range(0, attackSounds.Length-1)]);
    }

    IEnumerator GrabBowstringIE () {
        while (grabBowstring) {
            bowTransform.GetComponent<Bow>().bowstring.position = rightHandTransform.transform.position;
            yield return null;
        }
    }
    IEnumerator SpawnArrowIE () {
        newArrow = Instantiate(arrowPrefab, bowTransform).GetComponent<EnemyArrow>();
        while (newArrow != null) {
            newArrow.transform.position = rightHandTransform.position + 0.03f * newArrow.transform.forward;
            newArrow.transform.LookAt(bowTransform.transform);
            yield return null;
        }
    }
}
