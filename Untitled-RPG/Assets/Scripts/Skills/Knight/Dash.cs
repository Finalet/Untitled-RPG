using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : Skill
{
    List<Enemy> enemiesHit = new List<Enemy>();

    [Header("Custom Vars")]
    public float dashDistance;
    public ParticleSystem dashVFX;

    protected override  void Start() {
        base.Start();

        GetComponent<BoxCollider>().enabled = false;
        var sh = dashVFX.shape; //Dots
        sh.skinnedMeshRenderer = playerControlls.skinnedMesh;

        sh = dashVFX.transform.GetChild(0).GetComponent<ParticleSystem>().shape; //trails
        sh.skinnedMeshRenderer = playerControlls.skinnedMesh;
    }
    protected override void Update() {
        base.Update();
        ClearTrigger();
    }

    protected override void CustomUse() {
        dashVFX.Play();
        animator.CrossFade("Attacks.Knight.Dash", 0.25f);
        audioSource.PlayDelayed(0.1f * characteristics.attackSpeed.y);
    }

    void OnTriggerEnter(Collider other) {
        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (en == null || other.isTrigger)
            return;

        if (!enemiesHit.Contains(en)) {
            en.GetHit(CalculateDamage.damageInfo(damageType, baseDamagePercentage), skillName, true, true);
            enemiesHit.Add(en);
        }
    }

    public void Hit (float stopHit) {
        if (stopHit == 0) {
            GetComponent<BoxCollider>().enabled = true;
            playerControlls.characterController.speedMultiplier = 10;
        } else {
            GetComponent<BoxCollider>().enabled = false;
            playerControlls.characterController.speedMultiplier = 1;
            enemiesHit.Clear();
        }
    }

    void ClearTrigger () {
        for (int i = 0; i < enemiesHit.Count; i++) {
            if (enemiesHit[i] == null) {
                enemiesHit.RemoveAt(i);
            }
        }
    }

    public override string getDescription() {
        DamageInfo dmg = CalculateDamage.damageInfo(damageType, baseDamagePercentage, 0, 0);
        return $"Rushes forward through enemies, dealing {dmg.damage} {dmg.damageType} damage.";
    }
}
