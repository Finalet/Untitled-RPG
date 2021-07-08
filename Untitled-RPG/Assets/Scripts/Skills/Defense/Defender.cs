using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defender : Skill
{
    [Header("Custom Vars")]
    public Buff buff;
    
    protected override void CustomUse() {
        StartCoroutine(Using());

        buff.icon = icon;
        buff.associatedSkill = this;
    }

    IEnumerator Using () {
        animator.CrossFade("Attacks.Defense.Defender / Invincibility", 0.25f);
        PlaySound(audioSource.clip, 0, characteristics.attackSpeed.x);
        Combat.instanace.blockSkills = true;
        yield return new WaitForSeconds(0.5f * PlayerControlls.instance.GetComponent<Characteristics>().attackSpeed.y);
        transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        yield return new WaitForSeconds(0.7f * PlayerControlls.instance.GetComponent<Characteristics>().attackSpeed.y);
        Combat.instanace.blockSkills = false;
        playerControlls.isAttacking = false;
        characteristics.AddBuff(buff);
    }

    public override string getDescription() {
        return $"Increase your defense and block chance by {buff.defenseBuff*100}% for {Mathf.RoundToInt(buff.duration/60)} minutes.\n\nBlocking requires a shield.";
    }
}
