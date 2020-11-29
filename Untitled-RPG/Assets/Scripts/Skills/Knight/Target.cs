using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : Skill
{

    [Header("Skill Specific Vars")]
    public float skillDistance;
    public float duration;
    public GameObject targetPrefab;
    public GameObject SkillTarget;
    public float damageIncrease = 20;

    public override void Use() {
        if(!playerControlls.playerCamera.GetComponent<LookingTarget>().targetIsEnemy) {
            CanvasScript.instance.DisplayWarning("No enemy target!");
            return;
        }   

        base.Use();
    }

    protected override void CustomUse() { 
        StartCoroutine(Using());
    }

    public float timer;
    IEnumerator Using () {
        audioSource.time = 0.21f;
        audioSource.Play();

        SkillTarget = playerControlls.playerCamera.GetComponent<LookingTarget>().target.GetComponentInParent<Enemy>().gameObject;
        
        timer = 0;
        
        GameObject newTargetPrefab = Instantiate(targetPrefab, Vector3.zero, Quaternion.identity, SkillTarget.transform);
        SkillTarget.GetComponent<Enemy>().TargetSkillDamagePercentage = damageIncrease;

        newTargetPrefab.SetActive(true);
        newTargetPrefab.transform.localPosition = Vector3.up * 2.3f;

        PlayerControlls.instance.GetComponent<PlayerControlls>().isAttacking = false;
        float y =0;
        while (duration + timer >= 0) {
            timer -= Time.fixedDeltaTime;
            y += Time.fixedDeltaTime * 4;
            newTargetPrefab.transform.localPosition = Vector3.up * (2.3f + Mathf.Sin(y)/10);
            newTargetPrefab.transform.Rotate(Vector3.up, 1f);
            if (SkillTarget.GetComponentInParent<Enemy>().isDead)
                timer = -100;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        Destroy(newTargetPrefab);
        SkillTarget.GetComponentInParent<Enemy>().TargetSkillDamagePercentage = 0;
    }
}
