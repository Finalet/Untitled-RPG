using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Charge : Skill
{
    [Header("Custom vars")]
    public float duraiton;
    public GameObject VFX;
    
    bool charging;

    List<Enemy> enemiesHit = new List<Enemy>();

    protected override void CustomUse()
    {
        StartCoroutine(Using());
    }

    IEnumerator Using () {
        animator.CrossFade("Attacks.Defense.Charge", 0.4f);
        enemiesHit.Clear();
        characteristics.immuneToDamage = true;
        characteristics.immuneToInterrupt = true;
        charging = true;
        VFX.SetActive(true);
        VFX.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetFloat("Progress", 0);
        VFX.transform.GetChild(0).GetComponent<MeshRenderer>().material.DOFloat(1, "Progress", 1).SetEase(Ease.OutSine);
        float timeStarted = Time.time;
        float cleanListTimer = Time.time;
        while (Time.time - timeStarted < duraiton) {
            if (Time.time - cleanListTimer > 1) {
                enemiesHit.Clear();
                cleanListTimer = Time.time;
            }
            yield return null;
        }
        animator.CrossFade("Attacks.Defense.Empty", 0.4f);
        characteristics.immuneToDamage = false;
        characteristics.immuneToInterrupt = false;
        charging = false;
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
