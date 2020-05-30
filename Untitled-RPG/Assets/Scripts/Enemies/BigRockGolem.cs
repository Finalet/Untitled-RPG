using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigRockGolem : Enemy
{

    [Header("Custom vars")]
    public bool playerWithinReach;

    public float attackCoolDown = 2;
    public bool isCoolingDown;
    float cooldownTimer;

    public ParticleSystem trailsParticles;

    [Header("Audio")]
    public AudioSource attackAudioSource;
    float attackBasePitch;
    public AudioClip[] attackSounds; 
    public AudioClip[] footstepsSounds; 
    public bool footstepSoundsOn = true;
    bool leftFootDown;
    bool rightFootDown;
    public Transform leftFoot;
    public Transform rightFoot;

    protected override void Start() {
        base.Start();
        attackBasePitch = attackAudioSource.pitch;
    }

    protected override void Update() {
        if (PlayerControlls.instance == null) // Player instnace is null when loading level;
            return;

        base.Update();

        AI();
        CheckFootsteps();

        if (isAttacking)
            trailsParticles.Play();
        else 
            trailsParticles.Stop();
    }

    void AI () {
        if (distanceToPlayer <= attackRadius && !isCoolingDown && !isDead && !isKnockedDown) {
            Attack();
        }
        CoolDown();
    }

    void Attack() {
        isAttacking = true;
        animator.CrossFade("Main.Attack", 0.25f);
        PlayAttackSounds();
        cooldownTimer = attackCoolDown;
    }

    int times = 0;
    void PlayAttackSounds() {
        if (attackSounds.Length == 0)
            return;

        if (times == 0) {
            int x = Random.Range(0, attackSounds.Length);
            attackAudioSource.clip = attackSounds[x];
            attackAudioSource.pitch = attackBasePitch + Random.Range(-0.1f, 0.1f);
            attackAudioSource.Play();
            times ++;
            Invoke("PlayAttackSounds", 1);
        } else {
            int x = Random.Range(0, attackSounds.Length);
            attackAudioSource.clip = attackSounds[x];
            attackAudioSource.pitch = attackBasePitch + Random.Range(-0.1f, 0.1f);
            attackAudioSource.Play();
            times = 0;
        }
    }

    void CoolDown () {
        if (cooldownTimer >= 0) {
            cooldownTimer -= Time.deltaTime;
            isCoolingDown = true;
        } else {
            isCoolingDown = false;
            isAttacking = false;
        }
    }

    void OnTriggerEnter(Collider other) {

        if (other.CompareTag("Player")) {
            if (other != PlayerControlls.instance.GetComponent<CharacterController>())
                return;

            playerWithinReach = true;
        }    
    }
    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            if (other != PlayerControlls.instance.GetComponent<CharacterController>())
                return;
                
            playerWithinReach = false;
        }    
    }

    void Hit () {
        if (!canHit || !playerWithinReach)
            return;

        PlayerControlls.instance.GetComponent<Characteristics>().GetHit(damage());
    }

    int damage () {
        return Mathf.RoundToInt(Random.Range(baseDamage*0.85f, baseDamage*1.15f));
    }


    float leftDisToGround;
    float rightDisToGround;
    float threashold = 0.07f;
    void CheckFootsteps () {
        RaycastHit hitLeft;
        if (Physics.Raycast(leftFoot.position -leftFoot.up * 0.1f, leftFoot.up, out hitLeft, 5f)) { //For some reason in game the left foot transform is up side down. Thats why the signs are inverted here
           leftDisToGround = leftFoot.position.y - hitLeft.point.y;
        } else {
            leftDisToGround = 1000;
        }

        if (leftDisToGround <= threashold && !leftFootDown) {
            leftFootDown = true;
            PlayFootStepSound("left");
        } else if (leftDisToGround >= threashold) {
            leftFootDown = false;
        }


        RaycastHit hitRight;
        if (Physics.Raycast(rightFoot.position + rightFoot.up * 0.1f, -rightFoot.up, out hitRight, 5f)) {
            rightDisToGround = rightFoot.position.y - hitRight.point.y;
        } else {
            rightDisToGround = 1000;
        }

        if (rightDisToGround <= threashold && !rightFootDown) {
            rightFootDown = true;
            PlayFootStepSound("right");
        } else if (rightDisToGround >= threashold) {
            rightFootDown = false;
        }
    }

    void PlayFootStepSound(string foot) {
        if (!footstepSoundsOn)
            return;

        int footstepIndex = Random.Range(0, footstepsSounds.Length);

        if (foot == "left") {
            leftFoot.GetComponent<AudioSource>().clip = footstepsSounds[footstepIndex];
            leftFoot.GetComponent<AudioSource>().Play();
        } else if (foot == "right") {
            rightFoot.GetComponent<AudioSource>().clip = footstepsSounds[footstepIndex];
            rightFoot.GetComponent<AudioSource>().Play();
        } else {
            print("Wrong foot string"); //Error
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, playerDetectRadius);
    }
}
