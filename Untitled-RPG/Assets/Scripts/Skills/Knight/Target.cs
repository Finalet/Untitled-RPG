using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : Skill
{

    [Header("Skill Specific Vars")]
    public float skillDistance;
    public GameObject targetPrefab;
    public GameObject SkillTarget;
    public float damageIncrease = 20;

    public override void Use() {
        if(!AssetHolder.instance.PlayersCamera.GetComponent<LookingTarget>().targetIsEnemy) {
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
        SkillTarget = AssetHolder.instance.PlayersCamera.GetComponent<LookingTarget>().target;
        
        timer = 0;
        
        GameObject newTargetPrefab = Instantiate(targetPrefab, Vector3.zero, Quaternion.identity, SkillTarget.transform);
        SkillTarget.GetComponent<Enemy>().TargetSkillDamagePercentage = damageIncrease;

        newTargetPrefab.SetActive(true);
        newTargetPrefab.transform.localPosition = Vector3.up * 2.3f;

        PlayerControlls.instance.GetComponent<PlayerControlls>().isUsingSkill = false;
        PlayerControlls.instance.GetComponent<PlayerControlls>().isAttacking = false;
        float y =0;
        while (totalAttackTime + timer >= 0) {
            timer -= Time.fixedDeltaTime;
            y += Time.fixedDeltaTime * 4;
            newTargetPrefab.transform.localPosition = Vector3.up * (2.3f + Mathf.Sin(y)/10);
            newTargetPrefab.transform.Rotate(Vector3.up, 1f);
            if (SkillTarget.GetComponent<Enemy>().isDead)
                timer = -100;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        Destroy(newTargetPrefab);
        SkillTarget.GetComponent<Enemy>().TargetSkillDamagePercentage = 0;
    }
}
