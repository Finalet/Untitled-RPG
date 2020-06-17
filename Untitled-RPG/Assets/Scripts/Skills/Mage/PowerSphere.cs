using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSphere : Skill
{
    [Header("Custom Vars")]
    public float sphereSize = 14f;
    public float distance;
    public GameObject powerSphere;
    public AudioClip castingSound;
    
    public ParticleSystem HandsEffect;
    public Transform[] hands;
    public List<ParticleSystem> instanciatedEffects = new List<ParticleSystem>();

    protected override float actualDistance () {
        return distance + characteristics.magicSkillDistanceIncrease;
    } 

    protected override void InterruptCasting() {
        base.InterruptCasting();
        RemoveParticles();
    }

    protected override void CastingAnim() {
        if (playerControlls.isFlying)
            animator.CrossFade("Attacks.Mage.PowerSphere_flying", 0.25f);
        else 
            animator.CrossFade("Attacks.Mage.PowerSphere", 0.25f);

        AddParticles();

        PlaySound(castingSound, 0, 0.3f, 0.3f);
    }

    protected override void CustomUse(){}

    public void SpawnSphere () {
        RemoveParticles();

        GameObject go = Instantiate(powerSphere, pickedPosition, Quaternion.LookRotation(-playerControlls.transform.forward, Vector3.up));
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
            Destroy(instanciatedEffects[0].gameObject, 1f);
            instanciatedEffects.Remove(instanciatedEffects[0]);
        }
    }
}
