using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptainThrow : Skill
{
    [Header("Custom vars")]
    public float maxDistance = 10;
    public float strength = 20;
    [Space]
    public AudioClip shootSound;
    public AudioClip hitSound;
    public AudioClip catchSound;

    Vector3 throwPoint;
    Transform shootPosition;
    LayerMask ignorePlayer;
    CaptainThrowProjectile projectile;

    protected override void CustomUse()
    {
        Combat.instanace.blockSkills = true;
        
        shootPosition = WeaponsController.instance.LeftHandShieldTrans;
        animator.CrossFade("Attacks.Defense.Captain Throw", 0.25f);

        playerControlls.playerCamera.GetComponent<CameraControll>().isShortAiming = true;
        ignorePlayer =~ LayerMask.GetMask("Player");

        projectile = WeaponsController.instance.LeftHandEquipObj.GetComponent<CaptainThrowProjectile>();
        projectile.hitSound = hitSound;
        projectile.catchSound = catchSound;
        projectile.returnTransform = shootPosition;
        projectile.skill = this;
        projectile.damageInfo = CalculateDamage.damageInfo(damage());
        PlaySound(shootSound, 0, 1, characteristics.attackSpeed.x * 0.45f);
    }

    public void ThrowShield () {
        Combat.instanace.blockSkills = false;
        RaycastHit hit;
        if (Physics.Raycast(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward, out hit, maxDistance, ignorePlayer)) {
            throwPoint = hit.point;        
        } else {
            throwPoint = PlayerControlls.instance.playerCamera.transform.forward * (maxDistance + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance) + PlayerControlls.instance.playerCamera.transform.position;
        }

        projectile.Throw(throwPoint, strength);
        Invoke("ReturnCamera", 0.5f);
    }
    void ReturnCamera () {
        playerControlls.playerCamera.GetComponent<CameraControll>().isShortAiming = false;
    }

    public override bool skillActive()
    {
        if (WeaponsController.instance.leftHandStatus != SingleHandStatus.Shield)
            return false;
        return base.skillActive();
    }

    float damage () {
        Weapon equipedShield = (Weapon)EquipmentManager.instance.secondaryHand.itemInSlot;
        if (equipedShield == null) return 0;
        return equipedShield.Defense * 3f;
    }

    public override string getDescription()
    {
        DamageInfo damageInfo = CalculateDamage.damageInfo(damage(), 0, 0);
        return $"Throws shield at an enemy, dealing {damageInfo.damage} {damageInfo.damageType} damage and knocking it down.\n\nShield is required.";
    }
}
