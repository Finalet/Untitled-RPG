using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSphere : Skill
{
    [Header("Custom Vars")]
    public Buff buff;
    public float sphereSize = 14f;
    public float distance;
    public float duration = 20;
    public GameObject powerSphere;
    public AudioClip castingSound;
    
    public ParticleSystem HandsEffect;
    public List<ParticleSystem> instanciatedEffects = new List<ParticleSystem>();

    Transform[] hands;
    GameObject instanciatedSphere;

    protected override void Start() {
        base.Start();
        hands = new Transform[2];
        hands[0] = PlayerControlls.instance.leftHandWeaponSlot;
        hands[1] = PlayerControlls.instance.rightHandWeaponSlot;

        buff.icon = icon;
        buff.associatedSkill = this;
    }

    protected override float actualDistance () {
        return distance + characteristics.skillDistanceIncrease;
    } 

    protected override void InterruptCasting() {
        base.InterruptCasting();
        RemoveParticles();
        if (instanciatedSphere != null)
            Destroy(instanciatedSphere);
    }

    protected override void CastingAnim() {
        if (playerControlls.isFlying)
            animator.CrossFade("Attacks.Mage.PowerSphere_flying", 0.25f);
        else 
            animator.CrossFade("Attacks.Mage.PowerSphere", 0.25f);

        AddParticles();

        PlaySound(castingSound, 0.1f, characteristics.castingSpeed.x);
    }

    protected override void CustomUse(){}

    public void SpawnSphere () {
        Vector3 averageHandsPos = (hands[0].position + hands[1].position) / 2; 

        instanciatedSphere = Instantiate(powerSphere, averageHandsPos, powerSphere.transform.rotation);
        instanciatedSphere.transform.localScale = Vector3.zero;
        instanciatedSphere.GetComponent<PowerSphereProjectile>().powerSphereSkill = this;
        instanciatedSphere.GetComponent<PowerSphereProjectile>().position = pickedPosition + Vector3.up;
        instanciatedSphere.SetActive(true);
    }

    public void ShootSphere () {
        finishedCast = true;

        RemoveParticles();
        if (instanciatedSphere != null) instanciatedSphere.GetComponent<PowerSphereProjectile>().shoot = true; //Sometimes gives an error when canceling skill since it still tries to shoot it
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

    public override string getDescription()
    {
        return $"Cover the battlefield with a powerful dome that increases your casting speed, magic power, and defense by {buff.castingSpeedBuff*100}% while you are inside.";
    }
}
