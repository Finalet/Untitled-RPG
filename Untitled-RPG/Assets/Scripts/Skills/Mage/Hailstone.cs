using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hailstone : Skill
{
    [Header("Custom vars")]
    public float distance;

    public GameObject projectile;
    
    public ParticleSystem HandsEffect;
    public Transform[] hands;

    public List<ParticleSystem> instanciatedEffects = new List<ParticleSystem>();

    public AudioClip castingSound;
    public AudioClip fireSound;

    protected override float actualDistance () {
        return distance + characteristics.magicSkillDistanceIncrease;
    } 


    protected override void CastingAnim() {
        if (playerControlls.isFlying)
            animator.CrossFade("Attacks.Mage.Hailstone_flying", 0.25f);
        else 
            animator.CrossFade("Attacks.Mage.Hailstone", 0.25f);

        AddParticles();


        PlaySound(castingSound, 0, 0.3f, 0.3f);
    }

    protected override void CustomUse() {}

    public void FireProjectile () {
        RemoveParticles();

        finishedCast = true;

        GameObject go = Instantiate (projectile, pickedPosition, Quaternion.LookRotation(-playerControlls.transform.forward, Vector3.up));
        go.transform.GetChild(0).GetComponent<HailstoneProjectile>().actualDamage = actualDamage();
        go.SetActive(true);

        PlaySound(fireSound);
    }

    void AddParticles() {
        for (int i = 0; i < hands.Length; i ++) {
            ParticleSystem ps = Instantiate(HandsEffect, hands[i]);
            instanciatedEffects.Add(ps);
            ps.gameObject.SetActive(true);
        }
    }
    void RemoveParticles() {
        int x = instanciatedEffects.Count;
        for (int i = 0; i < x; i++) {
            instanciatedEffects[0].Stop();
            Destroy(instanciatedEffects[0].gameObject, 1f);
            instanciatedEffects.Remove(instanciatedEffects[0]);
        }
    }
}
