using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enough : Skill
{
    [Header("Custom vars")]
    public float radius;
    public float strength;
    public ParticleSystem vfx;
    public AudioClip sfx;

    List<Enemy> enemiesInRadius = new List<Enemy>();
    protected override void CustomUse() {
        animator.CrossFade("Attacks.Defense.Enough start", 0.25f);
        PlaySound(sfx, 0, 0.7f * characteristics.attackSpeed.y);
        Invoke("PushAway", 0.5f * characteristics.attackSpeed.y);
    }

    public void PushAway () {
        vfx.Play();

        enemiesInRadius.Clear();
        foreach (Collider col in Physics.OverlapSphere(transform.position, radius, LayerMask.GetMask("Enemy"))) {
            Enemy en = col.GetComponentInParent<Enemy>();
            if (en != null && col.GetComponentInParent<Rigidbody>() != null && !enemiesInRadius.Contains(en))
                enemiesInRadius.Add(col.GetComponentInParent<Enemy>());
        }
        foreach (Enemy en in enemiesInRadius) {
            if (en.TryGetComponent(out RagdollController ragdoll)) {
                ragdoll.EnableRagdoll(4, (en.transform.position - transform.position).normalized * strength);
            } else if (en.TryGetComponent(out Rigidbody rb)) {
                rb.AddForce((en.transform.position - transform.position).normalized * strength, ForceMode.VelocityChange);
            }
        }
    }

    public override string getDescription()
    {
        return $"You aren't a people's person. You push away all creatures within a {radius} meter radius.";
    }
}
