using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ArrowExplosive : Arrow
{
    public SphereCollider explosionCollider;
    public ParticleSystem strongTrails;
    public ParticleSystem explostionVFX;
    bool exploded;

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
        print(en);
        if (!enemiesHit.Contains(en)) {
            en.GetHit(damageInfo, skillName, false, true, HitType.Knockdown);
            enemiesHit.Add(en);
        }
    }  

    protected override void Collision (Transform collisionObj) {
        base.Collision(collisionObj);
        strongTrails.Stop();
        explostionVFX.Play();
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.1f, 40f, 0.1f, transform.position);
        explosionCollider.enabled = true;
        DOTween.To(()=> explosionCollider.radius, x=> explosionCollider.radius = x, 3.2f, 0.2f).SetEase(Ease.OutSine);
        exploded = true;

        List<RagdollController> ragdolledEnemies = new List<RagdollController>();
        foreach (Collider col in Physics.OverlapSphere(transform.position, 3f, LayerMask.GetMask("Enemy"), QueryTriggerInteraction.Collide)){
            RagdollController rag = col.GetComponentInParent<RagdollController>();
            if (rag != null && !ragdolledEnemies.Contains(rag)) {
                rag.EnableExplosionRagdoll(3f, 50f, transform.position, 0);
                ragdolledEnemies.Add(rag);
            }
        }

        Destroy(explosionCollider, 0.3f);
    }

    protected override void OnTriggerEnter(Collider other) {
        if (!shot || other.isTrigger || other.CompareTag("Player"))
            return;
        
        if (!exploded) Collision(other.transform);

        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (en == null) 
            return;

        Hit(en);
    }
}
