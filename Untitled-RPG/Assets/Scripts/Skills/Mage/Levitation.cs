using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Levitation : Skill
{
    [Header("Custom vars")]
    [Tooltip("Duration in seconds")] public float flightDuration = 120;
    public Buff buff;

    Transform[] feetsAndHands;
    public ParticleSystem bodypartsVFX;
    public ParticleSystem fullbodyVFX;

    List<ParticleSystem> instanciatedParticles = new List<ParticleSystem>();

    public override void Use () {
        if (playerControlls.isAttacking)
            return;
        base.Use();
    }
    protected override void Start() {
        base.Start(); 
        var sh = fullbodyVFX.shape;
        sh.skinnedMeshRenderer = playerControlls.skinnedMesh;

        feetsAndHands = new Transform[4];
        feetsAndHands[0] = PlayerControlls.instance.leftFootRoot;
        feetsAndHands[1] = PlayerControlls.instance.rightFootRoot;
        feetsAndHands[2] = PlayerControlls.instance.leftHandWeaponSlot;
        feetsAndHands[3] = PlayerControlls.instance.rightHandWeaponSlot;
    }

    protected override void CustomUse() {
        PlayerControlls.instance.TakeOff();
        GetComponent<AudioSource>().PlayDelayed(0.4f);
        characteristics.AddBuff(buff);
        StartCoroutine(flightTimer());

        for (int i = 0; i < feetsAndHands.Length; i ++) {
            ParticleSystem ps = Instantiate(bodypartsVFX, feetsAndHands[i]);
            ps.gameObject.SetActive(true);
            instanciatedParticles.Add(ps);
        }


        fullbodyVFX.Play();
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

        fullbodyVFX.Stop();
    }

    protected override void LocalUse () {
        playerControlls.InterruptCasting();
        coolDownTimer = coolDown;
        //playerControlls.GetComponent<Characteristics>().UseOrRestoreStamina(staminaRequired);
        CustomUse();
    }

    public override string getDescription()
    {
        return $"Taleoff and levitate in the sky for {flightDuration} seconds. Levitation increases magic power by {buff.magicPowerBuff*100}% and all ranged skills distance by {buff.skillDistanceBuff} meters.";
    }
}
