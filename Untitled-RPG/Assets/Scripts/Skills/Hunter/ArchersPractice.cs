using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArchersPractice : Skill
{
    [Header("Custom Vars")]
    public ParticleSystem VFX;
    public Buff buff;
    
    protected override void CustomUse() {
        StartCoroutine(Using());

        buff.icon = icon;
        buff.associatedSkill = this;
    }

    IEnumerator Using () {
        animator.CrossFade("Attacks.Hunter.Archers Practice start", 0.25f);
        yield return new WaitForSeconds(0.33f * (PlayerControlls.instance.GetComponent<Characteristics>().attackSpeed.y));
        audioSource.Play();
        VFX.Play();

        playerControlls.isAttacking = false;
        characteristics.AddBuff(buff);
    }

    public override string getDescription()
    {
        return $"Recall your archer's knowledge to increase ranged attack and attack speed by {buff.rangedAttackBuff*100}% for {Mathf.RoundToInt(buff.duration/60)} minutes.";
    }
}
