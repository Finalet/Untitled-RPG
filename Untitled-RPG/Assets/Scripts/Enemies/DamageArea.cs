using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageArea : MonoBehaviour
{
    public string areaName;
    public int damage;
    public float damageFrequency = 2;

    float lastDamagePlayerTime;
    float lastDamageDamagablesTime;

    List<IDamagable> damagablesInTrigger = new List<IDamagable>();

    void OnTriggerStay(Collider other) {
        if (other.CompareTag("Player") && other.GetType() == typeof(CapsuleCollider)) {
            DamagePlayer();
            return;
        }

        IDamagable en = other.transform.GetComponentInParent<IDamagable>();
        if (en == null || other.isTrigger)
            return;

        if (!damagablesInTrigger.Contains(en)) damagablesInTrigger.Add(en);

        DamageDamagables();
    }

    void OnTriggerExit(Collider other) {
        IDamagable en = other.transform.GetComponentInParent<IDamagable>();
        if (en == null || other.isTrigger)
            return;

        if (damagablesInTrigger.Contains(en)) damagablesInTrigger.Remove(en);
    }

    void DamageDamagables () {
        if (Time.time - lastDamageDamagablesTime > 1/damageFrequency) {
            lastDamageDamagablesTime = Time.time;
            for (int i = 0; i < damagablesInTrigger.Count; i++) {
                damagablesInTrigger[i].GetHit(CalculateDamage.damageInfo(DamageType.Raw, damage, areaName));
            }
        }
    }

    void DamagePlayer () {
        if (Time.time - lastDamagePlayerTime > 1/damageFrequency) {
            lastDamagePlayerTime = Time.time;
            DamageInfo damageInfo = CalculateDamage.enemyDamageInfo(damage, areaName);
            Characteristics.instance.GetHit(damageInfo, HitType.Normal);
        }
    }
}
