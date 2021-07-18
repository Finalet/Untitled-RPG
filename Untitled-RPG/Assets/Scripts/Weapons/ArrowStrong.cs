using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowStrong : Arrow
{
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

    protected override void Hit (IDamagable en) {
        if (!damagablesHit.Contains(en)) {
            en.GetHit(damageInfo, skillName, true, true, HitType.Kickback, transform.position);
            damagablesHit.Add(en);

            bl_UCrosshair.Instance.OnHit();
        }
    }  

    protected override void Collision (Transform collisionObj) {
        base.Collision(collisionObj);
        strongTrails.Stop();
        explostionVFX.Play();
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.2f, 2f, 0.1f, transform.position);
    }

}
