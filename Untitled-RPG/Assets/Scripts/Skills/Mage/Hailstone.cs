using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hailstone : Skill
{
    [Header("Custom vars")]
    public float distance;

    public GameObject projectile;

    protected override float actualDistance () {
        return distance + characteristics.magicSkillDistanceIncrease;
    } 


    protected override void CastingAnim() {
        if (playerControlls.isFlying)
            animator.CrossFade("Attacks.Mage.Hailstone_flying", 0.25f);
        else 
            animator.CrossFade("Attacks.Mage.Hailstone", 0.25f);

    }

    protected override void CustomUse() {}

    public void FireProjectile () {
        finishedCast = true;
        actualDamage = Mathf.RoundToInt(baseDamage * (float)characteristics.magicPower/100f);

        FireHailStone();
    }

    void FireHailStone () {
        GameObject go = Instantiate (projectile, pickedPosition, Quaternion.LookRotation(-playerControlls.transform.forward, Vector3.up));
        go.SetActive(true);
    }
}
