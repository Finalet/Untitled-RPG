using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : Skill
{
    [Header("Custom Vars")]
    public LayerMask ignorePlayer;
    public float speed;
    public float distance;

    public GameObject fireball;
    public Transform shootPosition;

    Vector3 shootPoint;

    public ParticleSystem HandsEffect;
    public Transform[] hands;

    [Header("Sounds")]
    public AudioClip castingSound;

    ParticleSystem[] instanciatedEffects = new ParticleSystem[2];

    protected override void CastingAnim() {

        animator.CrossFade("Attacks.Mage.Fireball", 0.25f);

        PlaySound(castingSound, 0.15f, 0.75f);

        instanciatedEffects[0] = Instantiate(HandsEffect, hands[0]);
        instanciatedEffects[1] = Instantiate(HandsEffect, hands[1]);

        instanciatedEffects[0].gameObject.SetActive(true);
        instanciatedEffects[1].gameObject.SetActive(true);
    }

    protected override void CustomUse() {}

    public void FireProjectile () {
        finishedCast = true;
         
        ParticleSystem[] left = instanciatedEffects[0].GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < left.Length; i++) {
            left[i].Stop();
        }
        ParticleSystem[] right = instanciatedEffects[0].GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < right.Length; i++) {
            right[i].Stop();
        }
        instanciatedEffects[0].Stop();
        instanciatedEffects[1].Stop();
        Destroy(instanciatedEffects[0].gameObject, 1f);
        Destroy(instanciatedEffects[1].gameObject, 1f);

        actualDamage = Mathf.RoundToInt(baseDamage * (float)characteristics.magicPower/100f);
        //play sound
        GameObject Fireball = Instantiate(fireball, shootPosition.position, Quaternion.identity);
        
        RaycastHit hit;
        if (Physics.Raycast(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward, out hit, distance, ignorePlayer)) {
            shootPoint = hit.point;        
        } else {
            shootPoint = PlayerControlls.instance.playerCamera.transform.forward * (distance + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance) + PlayerControlls.instance.playerCamera.transform.position;
        }

        Vector3 direction = shootPoint - shootPosition.position; 

        Fireball.GetComponent<Rigidbody>().AddForce(direction.normalized * speed, ForceMode.Impulse);
        Fireball.GetComponent<FireballProjectile>().distance = distance;
        Fireball.GetComponent<FireballProjectile>().actualDamage = actualDamage;
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

        ParticleSystem[] left = instanciatedEffects[0].GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < left.Length; i++) {
            left[i].Stop();
        }
        ParticleSystem[] right = instanciatedEffects[0].GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < right.Length; i++) {
            right[i].Stop();
        }
        instanciatedEffects[0].Stop();
        instanciatedEffects[1].Stop();
        Destroy(instanciatedEffects[0].gameObject, 1f);
        Destroy(instanciatedEffects[1].gameObject, 1f);
    }

    void DrawDebugs () {
        RaycastHit hit;
        if (Physics.Raycast(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward, out hit, distance, ignorePlayer)) {
            shootPoint = hit.point;        
        } else {
            shootPoint = PlayerControlls.instance.playerCamera.transform.forward * (distance + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance) + PlayerControlls.instance.playerCamera.transform.position;
        }
        Debug.DrawLine(shootPosition.position, shootPoint, Color.blue);
        Debug.DrawRay(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward * (distance + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance), Color.red);
    
    }
}
