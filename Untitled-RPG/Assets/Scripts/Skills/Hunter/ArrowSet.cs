using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowSet : Skill
{
    [Header("Skill vars")]
    public GameObject arrowPrefab;
    public int numberOfArrows = 6;
    public float strength = 100;
    [Range(0,1)] public float distanceBetweenArrows;
    [Header("Sounds")]
    public AudioClip castingSound;

    LayerMask ignorePlayer;
    public Arrow[] newArrows;
    Transform shootPosition;
    Vector3 shootPoint;
    bool grabBowstring;

    protected override void Start()
    {
        base.Start();
        shootPosition = PlayerControlls.instance.rightHandWeaponSlot;
    }

    protected override void CustomUse() {}

    protected override void CastingAnim() {
        animator.CrossFade("Attacks.Hunter.Arrow Set", 0.25f);
        StartCoroutine(SpawnArrowIE());
        PlaySound(castingSound, 0, characteristics.castingSpeed.x);
        
        playerControlls.playerCamera.GetComponent<CameraControll>().isShortAiming = true;
    }

    public void Shoot (bool spawnArrows = false) {
        if (spawnArrows) {
            for (int i = 0; i < numberOfArrows; i++) {
                newArrows[i] = Instantiate(arrowPrefab, WeaponsController.instance.BowObj.transform).GetComponent<Arrow>();
            }
        }
        
        finishedCast = true;
        ignorePlayer =~ LayerMask.GetMask("Player");
        //play sound
        
        RaycastHit hit;
        if (Physics.Raycast(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward, out hit, strength, ignorePlayer)) {
            shootPoint = hit.point;        
        } else {
            shootPoint = PlayerControlls.instance.playerCamera.transform.forward * (strength/2 + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance) + PlayerControlls.instance.playerCamera.transform.position;
        }

        Vector3 maxPossibleShot = PlayerControlls.instance.playerCamera.transform.forward * (strength/2 + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance) + PlayerControlls.instance.playerCamera.transform.position;
        float adjustingDistanceBetweenArrows = Vector3.Distance(PlayerControlls.instance.playerCamera.transform.position, shootPoint) / Vector3.Distance(PlayerControlls.instance.playerCamera.transform.position, maxPossibleShot);
    
        for (int i = 0; i < numberOfArrows; i++) {
            if (newArrows[i] != null) {
                Vector3 newShootPoint = shootPoint + transform.right * (i-numberOfArrows/2f) * adjustingDistanceBetweenArrows * (0.5f + distanceBetweenArrows*2);
                newArrows[i].instantShot = true;
                newArrows[i].Shoot(strength, newShootPoint, CalculateDamage.damageInfo(damageType, baseDamagePercentage), skillName); //could be null if just canceled skill
                newArrows[i] = null;
            }
        }
        playerControlls.isAttacking = false;
        grabBowstring = false;
        WeaponsController.instance.BowObj.GetComponent<Bow>().ReleaseString();
        Invoke("StopAiming", 0.5f);
    }

    public void GrabBowstring () {
        grabBowstring = true;
        StartCoroutine(GrabBowstringIE());
    }

    void StopAiming () {
        playerControlls.playerCamera.GetComponent<CameraControll>().isShortAiming = false;
    }

    protected override void InterruptCasting() {
        base.InterruptCasting();

        if (newArrows[0] != null) {
            for (int i = 0; i < numberOfArrows; i++) {
                Destroy(newArrows[i].gameObject);
                newArrows[i] = null;
            }
        }
        
        grabBowstring = false;
        WeaponsController.instance.BowObj.GetComponent<Bow>().ReleaseString();
    }

    IEnumerator GrabBowstringIE () {
        while (grabBowstring) {
            WeaponsController.instance.BowObj.GetComponent<Bow>().bowstring.position = shootPosition.transform.position;
            yield return null;
        }
    }
    IEnumerator SpawnArrowIE () {
        newArrows = new Arrow[numberOfArrows];
        for (int i = 0; i < numberOfArrows; i++) {
            newArrows[i] = Instantiate(arrowPrefab, WeaponsController.instance.BowObj.transform).GetComponent<Arrow>();
        }
        while (newArrows[0] != null) {
            for (int i = 0; i < numberOfArrows; i++) {
                if (newArrows[i] == null)
                    break;

                newArrows[i].transform.position = shootPosition.position + 0.03f * newArrows[i].transform.forward; 
                newArrows[i].transform.LookAt(WeaponsController.instance.BowObj.transform.position + WeaponsController.instance.BowObj.transform.up * 0.1f* (i-numberOfArrows/2f)); 
            }
            yield return null; 
        }
    }

    public override string getDescription()
    {
        DamageInfo dmg = CalculateDamage.damageInfo(damageType, baseDamagePercentage, 0, 0);
        return $"Cover vast area in front by launching multiple arrows and dealing {dmg.damage} {dmg.damageType} damage with each arrow.";
    }
}
