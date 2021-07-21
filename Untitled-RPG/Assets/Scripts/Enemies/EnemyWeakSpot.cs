using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeakSpot : MonoBehaviour, IDamagable
{
    Enemy parentEnemy;

    public List<RecurringEffect> recurringEffects => throw new System.NotImplementedException();

    public void AddRecurringEffect(RecurringEffect effect) {}

    public void GetHit(DamageInfo damageInfo, string skillName, bool stopHit = false, bool cameraShake = false, HitType hitType = HitType.Normal, Vector3 damageTextPos = default, float kickBackStrength = 50)
    {
        parentEnemy.OnWeakSpotHit();
    }

    public void RunRecurringEffects() {}

    void Awake() {
        parentEnemy = GetComponentInParent<Enemy>();
        if (!parentEnemy) throw new System.Exception("Enemy Weak Spot cannot find parent enemy");
    }
}
