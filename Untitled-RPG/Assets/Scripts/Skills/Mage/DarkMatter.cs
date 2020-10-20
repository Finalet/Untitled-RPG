using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkMatter : Skill
{
    LayerMask ignorePlayer;
    [Header("Custom Vars")]
    public float speed;
    public float distance;

    public GameObject projectile;
    public Transform[] hands;
    
    Vector3 shootPoint;
    bool left;
    float aimingTimer;

    protected override void CustomUse()
    {
        playerControlls.playerCamera.GetComponent<CameraControll>().isShortAiming = true;
        string anim = left ? "AttacksUpperBody.Mage.DarkMatter_left" : "AttacksUpperBody.Mage.DarkMatter_right";
        animator.CrossFade(anim, 0.25f);

        Invoke("FireProjectile", 0.15f * characteristics.attackSpeed.z);

        aimingTimer = Time.time;
        Invoke("Timer", 0.7f);
    }

    public void FireProjectile () {
        float actualDistance = distance + characteristics.magicSkillDistanceIncrease;
        Transform shootPosition = left ? hands[0] : hands[1];
        ignorePlayer =~ LayerMask.GetMask("Player");

        //play sound
        GameObject prj = Instantiate(projectile, shootPosition.position, Quaternion.identity);
        
        RaycastHit hit;
        if (Physics.Raycast(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward, out hit, actualDistance, ignorePlayer)) {
            shootPoint = hit.point;        
        } else {
            shootPoint = PlayerControlls.instance.playerCamera.transform.forward * (actualDistance + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance) + PlayerControlls.instance.playerCamera.transform.position;
        }

        Vector3 direction = shootPoint - shootPosition.position; 

        prj.SetActive(true);
        prj.GetComponent<Rigidbody>().AddForce(direction.normalized * speed, ForceMode.Impulse);
        prj.GetComponent<DarkMatterProjectile>().distance = actualDistance;
        prj.GetComponent<DarkMatterProjectile>().actualDamage = actualDamage();
        prj.GetComponent<DarkMatterProjectile>().doNotDestroy = false;
        playerControlls.isAttacking = false;

        left = !left;
    }

    void Timer () {
        if (Time.time - aimingTimer >= 0.6f) {
            playerControlls.playerCamera.GetComponent<CameraControll>().isShortAiming = false;
        }
    }

    protected override void LocalUse () {
        playerControlls.InterruptCasting();
        coolDownTimer = coolDown * characteristics.attackSpeed.z;
        //playerControlls.GetComponent<Characteristics>().UseOrRestoreStamina(staminaRequired);
        CustomUse();
        if (weaponOutRequired && !playerControlls.isWeaponOut)
                WeaponsController.instance.InstantUnsheathe();
        playerControlls.isAttacking = true;
    }
}
