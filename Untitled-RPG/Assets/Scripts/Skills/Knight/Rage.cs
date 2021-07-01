using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rage : Skill
{
    [Header("Custom Vars")]
    public Buff buff;
    
    protected override void CustomUse() {
        StartCoroutine(Using());

        buff.icon = icon;
        buff.associatedSkill = this;
    }

    IEnumerator Using () {
        animator.CrossFade("Attacks.Knight.Rage", 0.25f);
        PlaySound(audioSource.clip, 0, characteristics.attackSpeed.x);
        Combat.instanace.blockSkills = true;
        yield return new WaitForSeconds(0.33f * PlayerControlls.instance.GetComponent<Characteristics>().attackSpeed.y);
        transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        Combat.instanace.blockSkills = false;
        playerControlls.isAttacking = false;
        characteristics.AddBuff(buff);
    }

    public override string getDescription() {
        return $"Become enraged for {Mathf.RoundToInt(buff.duration/60)} minutes. Rage increases melee attack and attack speed by {buff.meleeAttackBuff*100}%.";
    }
}
