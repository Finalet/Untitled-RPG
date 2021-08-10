using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrainingDummyDPSIndicator : TrainingDummy
{
    
    public TextMeshPro dpsLabel;

    float timeLastCalculated;

    int totalDamageTaken;

    protected override void Update() {
        base.Update();
        CalculateDPS();
    }

    void CalculateDPS() {
        if (Time.time - timeLastCalculated < 1)
            return;
        
        dpsLabel.text = totalDamageTaken != 0 ? totalDamageTaken.ToString() : dpsLabel.text;
        timeLastCalculated = Time.time;
        totalDamageTaken = 0;
    }

    public override void GetHit (DamageInfo damageInfo, bool stopHit = false, bool cameraShake = false, HitType hitType = HitType.Normal, Vector3 damageTextPos = new Vector3 (), float kickBackStrength = 50) {
        if (isDead || !canGetHit)
            return;
        base.GetHit(damageInfo, stopHit, cameraShake, hitType, damageTextPos, kickBackStrength);
        totalDamageTaken += calculateActualDamage(damageInfo.damage);
    }
}
