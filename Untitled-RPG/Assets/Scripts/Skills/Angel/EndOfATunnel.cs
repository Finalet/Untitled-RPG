using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndOfATunnel : Skill
{
    [Header("Custom Vars")]
    public ParticleSystem VFX;
    public Buff buff;

    protected override void CustomUse() {
        buff.icon = icon;
        buff.associatedSkill = this;

        StartCoroutine(Using());
    }

    IEnumerator Using () {
        animator.CrossFade("Attacks.Angel.End of a tunnel Start", 0.25f);
        PlaySound(audioSource.clip, 0, characteristics.attackSpeed.x);
        Combat.instanace.blockSkills = true;
        yield return new WaitForSeconds(0.33f * PlayerControlls.instance.GetComponent<Characteristics>().attackSpeed.y);
        characteristics.GetHealed(characteristics.maxHealth - characteristics.health, skillName);
        VFX.Play();
        Combat.instanace.blockSkills = false;
        playerControlls.isAttacking = false;
        characteristics.AddBuff(buff);
    }

    public override void OnBuffRemove()
    {
        VFX.Stop();
    }

    public override string getDescription() {
        return $"Restores full health and makes you invincible for {buff.duration} seconds.";
    }
}
