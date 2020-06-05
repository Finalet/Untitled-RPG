using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Levitation : Skill
{
    [Header("Custom vars")] 
    [Tooltip("Increase distance of mage skill by meters")] public float skillDistanceIncrease = 10;
    public float magicPowerPercentageIncrease = 20;
    [Tooltip("Duration in seconds")] public float flightDuration = 120;

    public Transform[] feetsAndHands;
    public ParticleSystem bodypartsVFX;

    List<ParticleSystem> instanciatedParticles = new List<ParticleSystem>();

    protected override void Start() {
        base.Start(); 
        totalAttackTime = flightDuration;
    }

    protected override void CustomUse() {
        PlayerControlls.instance.TakeOff();
        characteristics.AddBuff(this);
        StartCoroutine(flightTimer());

        for (int i = 0; i < feetsAndHands.Length; i ++) {
            ParticleSystem ps = Instantiate(bodypartsVFX, feetsAndHands[i]);
            ps.gameObject.SetActive(true);
            instanciatedParticles.Add(ps);
        }
    }

    IEnumerator flightTimer () {
        yield return new WaitForSeconds(flightDuration);
        PlayerControlls.instance.LandFromFlying();

        int x = instanciatedParticles.Count;
        for (int i = 0; i < x; i ++) {
            instanciatedParticles[0].Stop();
            Destroy(instanciatedParticles[0].gameObject, 1f);
            instanciatedParticles.Remove(instanciatedParticles[0]);
        }
    }
}
