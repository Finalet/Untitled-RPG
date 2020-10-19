using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash : Skill
{
    [Header("Custom vars")]
    public AudioClip[] sounds;

    List<GameObject> enemiesInCombatTrigger = new List<GameObject>();

    int hits;
    float lastHitTime;
    float timing;

    Vector3 baseColliderSize;

    protected override void Start() {
        base.Start();
        baseColliderSize = GetComponent<BoxCollider>().size;
    }

    public override void Use() {
        if (playerControlls.GetComponent<Characteristics>().Stamina < staminaRequired) {
            CanvasScript.instance.DisplayWarning("Not enough stamina!");
            return;
        }
        if (!skillActive() || isCoolingDown || playerControlls.isRolling || playerControlls.isGettingHit || playerControlls.isCastingSkill)
            return;

        StartCoroutine(StartUse());
    }

    protected override void CustomUse() {
        if (hits == 1 || hits == 2)
            timing = 0.46f * characteristics.attackSpeed.z;
        else 
            timing = 0.7f * characteristics.attackSpeed.z;

        if (Time.time - lastHitTime > timing)
            Attack();
    }

    void Attack() {
        hits++;

        if (hits == 1)
            animator.CrossFade("Attacks.Knight.Slash_1", 0.2f);
        else if (hits == 2)
            animator.CrossFade("Attacks.Knight.Slash_2", 0.2f);
        else if (hits == 3)
            animator.CrossFade("Attacks.Knight.Slash_3", 0.2f);

        lastHitTime = Time.time;

        if (hits == 1) {
            PlaySound(sounds[0], 0, 1, 0.15f * characteristics.attackSpeed.z);
            GetComponent<BoxCollider>().size = baseColliderSize;
        } else if (hits == 2) {
            PlaySound(sounds[1], 0, 1, 0.2f * characteristics.attackSpeed.z);
            GetComponent<BoxCollider>().size = baseColliderSize;
        } else if (hits == 3) {
            PlaySound(sounds[2], 0, 1, 0.3f * characteristics.attackSpeed.z);
            Invoke("PlayLastSound", 0.45f * characteristics.attackSpeed.z); //Invoke because otherwise the sound does not play

            GetComponent<BoxCollider>().size += Vector3.right;
        }      
    }

    void PlayLastSound () {
        PlaySound(sounds[3]);
    }

    protected override void Update() {
        base.Update();
        ClearTrigger();

        if (hits >= 3 || Time.time - lastHitTime > 1.5f) {
            hits = 0;
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.GetComponent<Enemy>() != null && !other.isTrigger) {
            enemiesInCombatTrigger.Add(other.gameObject);
        }
    }
    void OnTriggerExit(Collider other) {
        if (other.GetComponent<Enemy>() != null && !other.isTrigger) {
            enemiesInCombatTrigger.Remove(other.gameObject);
        }
    }

    public void Hit () {
        for (int i = 0; i < enemiesInCombatTrigger.Count; i++) {
            enemiesInCombatTrigger[i].GetComponent<Enemy>().GetHit(damage(), true, true, skillName);
        }
    }

    void ClearTrigger () {
        for (int i = 0; i < enemiesInCombatTrigger.Count; i++) {
            if (enemiesInCombatTrigger[i].gameObject == null) {
                enemiesInCombatTrigger.RemoveAt(i);
            }
        }
    }
}
