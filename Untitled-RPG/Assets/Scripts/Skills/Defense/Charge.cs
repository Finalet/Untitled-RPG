using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Charge : Skill
{
    [Header("Custom vars")]
    public float duraiton;
    public GameObject VFX;

    public ParticleSystem lastParticles;
    public AudioClip soundFX;
    
    bool charging;

    List<Enemy> enemiesHit = new List<Enemy>();
    Collider hitCollider;

    protected override void CustomUse()
    {
        hitCollider = GetComponent<Collider>();
        StartCoroutine(Using());
    }

    IEnumerator Using () {
        animator.CrossFade("Attacks.Defense.Charge", 0.4f);
        enemiesHit.Clear();
        PlaySound(soundFX, 0, 1, 0, 0.5f);
        Combat.instanace.blockSkills = true;
        characteristics.immuneToDamage = true;
        characteristics.immuneToInterrupt = true;
        playerControlls.isAttacking = true;
        charging = true;
        VFX.SetActive(true);
        hitCollider.enabled = true;
        Material vfxMat = VFX.transform.GetChild(0).GetComponent<MeshRenderer>().material;
        vfxMat.SetFloat("Progress", 0);
        vfxMat.DOFloat(1, "Progress", 2);
        float timeStarted = Time.time;
        float cleanListTimer = Time.time;
        while (Time.time - timeStarted < duraiton) {
            if (Time.time - cleanListTimer > 1) {
                enemiesHit.Clear();
                cleanListTimer = Time.time;
            }
            yield return null;
        }
        vfxMat.DOFloat(0, "Progress", 0.5f);
        animator.CrossFade("Attacks.Defense.Empty", 0.4f);
        characteristics.immuneToDamage = false;
        characteristics.immuneToInterrupt = false;
        charging = false;
        playerControlls.isAttacking = false;
        hitCollider.enabled = false;
        yield return new WaitForSeconds(0.5f);
        lastParticles.Play();
        Combat.instanace.blockSkills = false;
        yield return new WaitForSeconds(0.7f);
        VFX.SetActive(false);
    }

    void OnTriggerEnter(Collider other) {
        if(!charging)
            return;

        Enemy en = other.GetComponentInParent<Enemy>();
        if (en == null || enemiesHit.Contains(en))
            return;

        en.GetHit(CalculateDamage.damageInfo(damageType, baseDamagePercentage), skillName, false, false, HitType.Kickback, new Vector3(), 30);
        enemiesHit.Add(en);
    }

    public override bool skillActive()
    {
        if (WeaponsController.instance.leftHandStatus != SingleHandStatus.Shield)
            return false;
        return base.skillActive();
    }

    public override string getDescription()
    {
        return $"Charge through enemies for {duraiton} seconds, dealing {baseDamagePercentage} {damageType} damage.\n\nYou cannot recieve damage while charging. Shield is required.";
    }
}
