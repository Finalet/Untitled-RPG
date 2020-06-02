using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : Skill
{
    [Header("Custom Vars")]
    public float speed;
    public float distance;

    public GameObject fireball;
    public Transform shootPosition;

    Vector3 shootPoint;

    protected override void CastingAnim() {
        animator.CrossFade("Attacks.Mage.Fireball", 0.25f);
    }

    protected override void CustomUse () {
        actualDamage = Mathf.RoundToInt(baseDamage * (float)characteristics.magicPower/100f);
        //play sound
        GameObject Fireball = Instantiate(fireball, shootPosition.position, Quaternion.identity);
        
        RaycastHit hit;
        if (Physics.Raycast(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward, out hit, distance)) {
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

    void DrawDebugs () {
        RaycastHit hit;
        if (Physics.Raycast(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward, out hit, distance)) {
            shootPoint = hit.point;        
        } else {
            shootPoint = PlayerControlls.instance.playerCamera.transform.forward * (distance + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance) + PlayerControlls.instance.playerCamera.transform.position;
        }
        Debug.DrawLine(shootPosition.position, shootPoint, Color.blue);
        Debug.DrawRay(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward * (distance + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance), Color.red);
    
    }
}
