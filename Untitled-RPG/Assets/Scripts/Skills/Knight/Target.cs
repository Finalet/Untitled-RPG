using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : Skill
{

    [Header("Skill Specific Vars")]
    public float skillDistance;
    public float duration;
    public GameObject targetPrefab;
    public Enemy SkillTarget;
    public float damageIncrease = 20;

    List<Enemy> enemiesInTarget = new List<Enemy>();
    List<float> enemiesInTargetAngles = new List<float>();

    protected override void LocalUse () {
        var enemiesInTarget = Physics.OverlapSphere(pickedPosition, 1);
        foreach (Collider en in enemiesInTarget) {
            if (en.gameObject.GetComponent<Enemy>() != null) {
                SkillTarget = en.gameObject.GetComponent<Enemy>();
                break;
            }
        }

        if (SkillTarget == null) {
           CanvasScript.instance.DisplayWarning("No enemy target");
           return;
        }

        playerControlls.InterruptCasting();
        coolDownTimer = coolDown;
        //playerControlls.GetComponent<Characteristics>().UseOrRestoreStamina(staminaRequired);
        CustomUse();
        if (weaponOutRequired && !WeaponsController.instance.isWeaponOut)
                WeaponsController.instance.InstantUnsheathe();
    }

    protected override void PickArea () {
        if (instanciatedAP == null) {
            instanciatedAP = Instantiate(areaPickerPrefab);
            CanvasScript.instance.PickAreaForSkill(this);
        }

        enemiesInTarget.Clear();
        enemiesInTargetAngles.Clear();

        // Find all colliders within ViewRange
        var hits = Physics.OverlapSphere(transform.position, skillDistance + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance);

        
        foreach (var hit1 in hits) {
            if ( hit1.gameObject == this.gameObject ) continue; // don't hit self
            if ( hit1.gameObject.GetComponentInParent<Enemy>() == null) continue; // not an enemy

            // Check FOV
            var direction = (playerControlls.playerCamera.transform.position - hit1.transform.position);
            var hDirn = Vector3.ProjectOnPlane(direction, playerControlls.playerCamera.transform.up).normalized; // Project onto transform-relative XY plane to check hFov
            var vDirn = Vector3.ProjectOnPlane(direction, playerControlls.playerCamera.transform.right).normalized; // Project onto transform-relative YZ plane to check vFov

            var hOffset = 180 - Vector3.Angle(hDirn, playerControlls.playerCamera.transform.forward); //Vector3.Dot(hDirn, playerControlls.playerCamera.transform.forward) * Mathf.Rad2Deg; // Calculate horizontal angular offset in Degrees
            var vOffset = 180 - Vector3.Angle(vDirn, playerControlls.playerCamera.transform.forward); // Calculate vertical angular offset in Degrees

            if (hOffset > 10 || vOffset > 10) continue; // Outside of FOV

            enemiesInTarget.Add(hit1.GetComponentInParent<Enemy>());
            enemiesInTargetAngles.Add(hOffset);
        }

        if (enemiesInTarget.Count <= 0) {
            SkillTarget = null;
        } else {
            SkillTarget = enemiesInTarget[0];
            for (int i = 0; i < enemiesInTargetAngles.Count; i++) {
                if (i == 0) continue;
                if (enemiesInTargetAngles[i] < enemiesInTargetAngles[i-1]) {
                    SkillTarget = enemiesInTarget[i];
                }
            }
        }

        if (SkillTarget != null) {
            Vector3 pos = SkillTarget.transform.position + Vector3.up * (0.1f + SkillTarget.globalEnemyBounds().y);
            instanciatedAP.transform.position = pos;
            instanciatedAP.transform.localScale = Vector3.one * areaPickerSize / 10f;
        } else {
            instanciatedAP.transform.localScale = Vector3.zero;
        }
    }

    protected override float actualDistance () {
        return skillDistance;
    } 

    protected override void CustomUse() { 
        StartCoroutine(Using());
    }

    float startTime;
    IEnumerator Using () {
        audioSource.time = 0.21f;
        audioSource.Play();
        
        startTime = Time.realtimeSinceStartup;
        
        GameObject newTargetPrefab = Instantiate(targetPrefab, Vector3.zero, Quaternion.identity, SkillTarget.transform);
        SkillTarget.TargetSkillDamagePercentage = damageIncrease;

        float height = 0.3f + SkillTarget.globalEnemyBounds().y;

        newTargetPrefab.SetActive(true);
        newTargetPrefab.transform.localPosition = Vector3.up * height;

        float y = 0;
        while (Time.realtimeSinceStartup - startTime < duration) {
            y += Time.fixedDeltaTime;
            newTargetPrefab.transform.localPosition = Vector3.up * (height + Mathf.Sin(y)/10);
            newTargetPrefab.transform.Rotate(Vector3.up, 1f);
            if (SkillTarget.isDead)
                break;
            yield return null;
        }
        Destroy(newTargetPrefab);
        SkillTarget.TargetSkillDamagePercentage = 0;
    }

    public override string getDescription()
    {
        DamageInfo dmg = CalculateDamage.damageInfo(skillTree, baseDamagePercentage, 0, 0);
        return $"Mark an enemy as your primary target, increasing all damage it takes by {damageIncrease}% for {duration} seconds.";
    }
}
