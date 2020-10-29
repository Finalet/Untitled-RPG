using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : Skill
{
    List<GameObject> enemiesHit = new List<GameObject>();

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
        audioSource.PlayDelayed(0.1f * characteristics.attackSpeed.z);
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.GetComponent<Enemy>() != null && !other.isTrigger) {
            if (!enemiesHit.Contains(other.gameObject)) {
                other.GetComponent<Enemy>().GetHit(damage(), skillName, true, true);
                enemiesHit.Add(other.gameObject);
            }
        }
    }

    public void Hit (float stopHit) {
        if (stopHit == 0) {
            GetComponent<BoxCollider>().enabled = true;
            playerControlls.independentFromInputFwd += dashDistance;
        } else {
            playerControlls.independentFromInputFwd -= dashDistance;
            GetComponent<BoxCollider>().enabled = false;
            enemiesHit.Clear();
        }
    }

    void ClearTrigger () {
        for (int i = 0; i < enemiesHit.Count; i++) {
            if (enemiesHit[i].gameObject == null) {
                enemiesHit.RemoveAt(i);
            }
        }
    }
}
