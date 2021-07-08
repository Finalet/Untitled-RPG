using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invincibility : Skill
{
    [Header("Custom Vars")]
    public Buff buff;
    
    public ParticleSystem ps;

    protected override void CustomUse() {
        StartCoroutine(Using());

        buff.icon = icon;
        buff.associatedSkill = this;
    }

    IEnumerator Using () {
        animator.CrossFade("Attacks.Defense.Defender / Invincibility", 0.25f);
        PlaySound(audioSource.clip, 0, characteristics.attackSpeed.x);
        Combat.instanace.blockSkills = true;
        var main = ps.main;
        main.duration = buff.duration;
        main.startLifetime = buff.duration;
        yield return new WaitForSeconds(0.5f * PlayerControlls.instance.GetComponent<Characteristics>().attackSpeed.y);
        ps.Play();
        ps.transform.SetParent(null);
        yield return new WaitForSeconds(0.7f * PlayerControlls.instance.GetComponent<Characteristics>().attackSpeed.y);
        Combat.instanace.blockSkills = false;
        playerControlls.isAttacking = false;
        characteristics.AddBuff(buff);
    }

    public override string getDescription() {
        return $"Become completely invincible for {Mathf.RoundToInt(buff.duration)} seconds.";
    }

    void FixedUpdate() {
        if (ps.transform.parent != transform) {
            ps.transform.position = transform.position + Vector3.up;
            ps.transform.rotation = Quaternion.identity;
        }
    }

    public override void OnBuffRemove()
    {
        ps.Stop();
        ps.transform.SetParent(transform);
    }
}
