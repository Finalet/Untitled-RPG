using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowPoison : Arrow
{
    public RecurringEffect poisonEffect;
    public ParticleSystem strongTrails;
    public ParticleSystem explostionVFX;

    protected override void Start() {
        base.Start();
        strongTrails.Stop();
    }

    public override void Shoot (float _strength, Vector3 _shotPoint, DamageInfo _damageInfo, string _skillName) {
        base.Shoot(_strength, _shotPoint, _damageInfo, _skillName);
        strongTrails.Play();
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.15f, 1.5f, 0.07f, transform.position);
    }

    protected override void Hit (Enemy en) {
        if (!enemiesHit.Contains(en)) {
            en.GetHit(damageInfo, skillName, false, true, HitType.Normal, transform.position);
            en.AddRecurringEffect(poisonEffect);
            enemiesHit.Add(en);
        }
    }  

    protected override void Collision (Transform collisionObj) {
        base.Collision(collisionObj);
        strongTrails.Stop();
        explostionVFX.Play();
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.2f, 2f, 0.1f, transform.position);
    }
}
