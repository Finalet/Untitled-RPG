using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : Skill
{

    [Header("Skill Specific Vars")]
    public float skillDistance;
    public GameObject targetIcon;

    public override void Use() {
        if (isCoolingDown) {
            print("'" + skillName + "'" +" is still cooling down. " + Mathf.RoundToInt(coolDownTimer) + " seconds left.");
            return;
        }
        if (playerControlls.GetComponent<Characteristics>().Stamina < staminaRequired) {
            CanvasScript.instance.DisplayWarning("Not enough stamina!");
            return;
        }
        if (!playerControlls.isWeaponOut || playerControlls.isRolling || playerControlls.isUsingSkill)
            return;

        if(!AssetHolder.instance.PlayersCamera.GetComponent<LookingTarget>().targetIsEnemy) {
            CanvasScript.instance.DisplayWarning("No enemy target!");
            return;
        }    

        base.Use();
        StartCoroutine(Using());
    }

    public float timer;
    IEnumerator Using () {
        yield return new WaitForSeconds (startAttackTime);
        timer = 0;
        GameObject newTargetIcon = Instantiate(targetIcon, Vector3.zero, Quaternion.identity, AssetHolder.instance.PlayersCamera.GetComponent<LookingTarget>().target.transform);
        newTargetIcon.SetActive(true);
        newTargetIcon.transform.localPosition = Vector3.up * 2.3f;
        PlayerControlls.instance.GetComponent<PlayerControlls>().isUsingSkill = false;
        PlayerControlls.instance.GetComponent<PlayerControlls>().isAttacking = false;
        while (totalAttackTime - startAttackTime + timer >= 0) {
            timer -= Time.fixedDeltaTime;
            newTargetIcon.transform.LookAt(AssetHolder.instance.PlayersCamera.transform);
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        Destroy(newTargetIcon);
    }
}
