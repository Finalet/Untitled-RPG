using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hailstone : Skill
{
    [Header("Custom vars")]
    public float distance;

    public GameObject projectile;
    
    public ParticleSystem HandsEffect;
    Transform[] hands;

    public List<ParticleSystem> instanciatedEffects = new List<ParticleSystem>();

    public AudioClip castingSound;

    protected override void Start() {
        base.Start();
        hands = new Transform[2];
        hands[0] = PlayerControlls.instance.leftHandWeaponSlot;
        hands[1] = PlayerControlls.instance.rightHandWeaponSlot;
    }

    protected override float actualDistance () {
        return distance + characteristics.skillDistanceIncrease;
    } 

    protected override void InterruptCasting() {
        base.InterruptCasting();
        RemoveParticles();
    }

    protected override void CastingAnim() {
        if (playerControlls.isFlying)
            animator.CrossFade("Attacks.Mage.Hailstone_flying", 0.25f);
        else 
            animator.CrossFade("Attacks.Mage.Hailstone", 0.25f);

        AddParticles();


        PlaySound(castingSound, 0.1f, characteristics.castingSpeed.x);
    }

    protected override void CustomUse() {}

    public void FireProjectile () {
        RemoveParticles();

        finishedCast = true;

        GameObject go = Instantiate (projectile, pickedPosition, Quaternion.LookRotation(-playerControlls.transform.forward, Vector3.up));
        go.transform.GetChild(0).GetComponent<HailstoneProjectile>().damageInfo = CalculateDamage.damageInfo(damageType, baseDamagePercentage);
        go.SetActive(true);
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
            Destroy(instanciatedEffects[0].gameObject, 2f);
            instanciatedEffects.Remove(instanciatedEffects[0]);
        }
    }

    public override string getDescription()
    {
        DamageInfo dmg = CalculateDamage.damageInfo(damageType, baseDamagePercentage, 0, 0);
        return $"Crash a huge iceberg onto enemies in a specified area dealing {dmg.damage} {dmg.damageType} damage.";
    }
}
