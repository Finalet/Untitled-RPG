using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : Skill
{
    [Header("Custom Vars")]
    LayerMask ignorePlayer;
    public float speed;
    public float distance;

    public GameObject fireball;
    Transform shootPosition;

    Vector3 shootPoint;

    public ParticleSystem HandsEffect;
    Transform hand;

    [Header("Sounds")]
    public AudioClip castingSound;

    List<ParticleSystem> instanciatedEffects = new List<ParticleSystem>();

        
    protected override void Start() {
        base.Start();
        hand = PlayerControlls.instance.rightHandRoot;
        shootPosition = PlayerControlls.instance.rightHandWeaponSlot;
    }

    protected override void CastingAnim() {

        if (playerControlls.isFlying)
            animator.CrossFade("Attacks.Mage.Fireball_flying", 0.25f);
        else 
            animator.CrossFade("Attacks.Mage.Fireball", 0.25f);

        PlaySound(castingSound, 0, characteristics.castingSpeed.x);

        ParticleSystem ps = Instantiate(HandsEffect, hand);
        instanciatedEffects.Add(ps);
        ps.gameObject.SetActive(true);

        playerControlls.playerCamera.GetComponent<CameraControll>().isAiming = true;
    }

    protected override void CustomUse() {}

    public void FireProjectile () {
        float actualDistance = distance + characteristics.skillDistanceIncrease;

        finishedCast = true;
        playerControlls.playerCamera.GetComponent<CameraControll>().isAiming = false;
        ignorePlayer =~ LayerMask.GetMask("Player");
        StopEffects();
        //play sound
        GameObject Fireball = Instantiate(fireball, shootPosition.position, Quaternion.identity);
        
        RaycastHit hit;
        if (Physics.Raycast(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward, out hit, actualDistance, ignorePlayer)) {
            shootPoint = hit.point;        
        } else {
            shootPoint = PlayerControlls.instance.playerCamera.transform.forward * (actualDistance + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance) + PlayerControlls.instance.playerCamera.transform.position;
        }

        Vector3 direction = shootPoint - shootPosition.position; 

        Fireball.SetActive(true);
        Fireball.GetComponent<Rigidbody>().AddForce(direction.normalized * speed, ForceMode.Impulse);
        Fireball.GetComponent<FireballProjectile>().distance = actualDistance;
        Fireball.GetComponent<FireballProjectile>().damageInfo = CalculateDamage.damageInfo(damageType, baseDamagePercentage, skillName);
        Fireball.GetComponent<FireballProjectile>().doNotDestroy = false;
        playerControlls.isAttacking = false;
    }

    /*
    protected override void Update() {
        base.Update();
        DrawDebugs();
    } */

    protected override void InterruptCasting() {
        base.InterruptCasting();

        StopEffects();
    }

    void StopEffects() {
        int x = instanciatedEffects.Count;
        for (int i = 0; i < x; i++) {
            instanciatedEffects[0].Stop();
            Destroy(instanciatedEffects[0].gameObject, 1f);
            instanciatedEffects.Remove(instanciatedEffects[0]);
        }
    }

    void DrawDebugs () {
        RaycastHit hit;
        if (Physics.Raycast(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward, out hit, distance+characteristics.skillDistanceIncrease, ignorePlayer)) {
            shootPoint = hit.point;        
        } else {
            shootPoint = PlayerControlls.instance.playerCamera.transform.forward * (distance + characteristics.skillDistanceIncrease + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance) + PlayerControlls.instance.playerCamera.transform.position;
        }
        Debug.DrawLine(shootPosition.position, shootPoint, Color.blue);
        Debug.DrawRay(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward * (distance + characteristics.skillDistanceIncrease + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance), Color.red);
    
    }

    public override string getDescription()
    {
        DamageInfo dmg = CalculateDamage.damageInfo(damageType, baseDamagePercentage, skillName, 0, 0);
        return $"Cast a fireball that deals {dmg.damage} {dmg.damageType} damage.";
    }
}
