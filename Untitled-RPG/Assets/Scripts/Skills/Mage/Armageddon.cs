using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armageddon : Skill
{
    [Header("Custom Vars")]
    public float distance = 15;
    public float duration = 5;
    public float meteorsPerSecond = 4;
    public float area = 10;
    [Range(0, 1)] public float chanceToHitEnemy = 0.1f;

    [Space]
    public GameObject projectile;
    public float projectileSpeed;

    public ParticleSystem handsVFX;
    public ParticleSystem cloudVFX;
    Transform[] hands;

    float lastShotTime;
    Vector3 mainPos;

    List<ParticleSystem> instanciatedEffects = new List<ParticleSystem>();
    Collider[] enemiesBelow;
    ParticleSystem instanciatedCloud;

    protected override void Start() {
        base.Start();
        hands = new Transform[2];
        hands[0] = PlayerControlls.instance.leftHandWeaponSlot;
        hands[1] = PlayerControlls.instance.rightHandWeaponSlot;
    }

    protected override float actualDistance () {
        return distance + characteristics.skillDistanceIncrease;
    } 

    protected override void CastingAnim() {
        animator.CrossFade("Attacks.Mage.Armageddon_start", 0.25f);

        AddParticles();

        audioSource.pitch = Characteristics.instance.castingSpeed.x;
        audioSource.time = 0.5f;
        audioSource.Play();
    }

    protected override void CustomUse() {}

    protected override void InterruptCasting() {
        base.InterruptCasting();
        RemoveParticles();
    }

    public void StartHell () {
        finishedCast = true;
        instanciatedCloud = Instantiate(cloudVFX, pickedPosition + Vector3.up * 10, Quaternion.identity);
        instanciatedCloud.gameObject.SetActive(true);
        RemoveParticles();
        StartCoroutine(hell());
    }

    IEnumerator hell () {
        mainPos = pickedPosition + Vector3.up * 10;
        float time = Time.time;
        while(Time.time - time <= duration) {
            if (Time.time - lastShotTime >= 1/meteorsPerSecond) {
                ShootOneMeteor();
                lastShotTime = Time.time;
            }
            if (playerControlls.castInterrupted) {
                playerControlls.castInterrupted = false;
                instanciatedCloud.Stop();
                Destroy(instanciatedCloud.gameObject, 5);
                yield break;
            }
            playerControlls.isCastingSkill = true;
            yield return null;
        }
        animator.CrossFade("Attacks.Mage.Armageddon_end", 0.25f);
        instanciatedCloud.Stop();
        playerControlls.isCastingSkill = false;
        Destroy(instanciatedCloud.gameObject, 5);
    }
    
    void ShootOneMeteor() {
        enemiesBelow = Physics.OverlapSphere(mainPos + Vector3.down * 10, area/2, LayerMask.GetMask("Enemy"));
        bool shotEnemy = enemiesBelow.Length == 0 ? false : Random.value < chanceToHitEnemy ? true : false;

        Vector3 pos;
        if (shotEnemy) {
            int enemyIndex = Random.Range(0, enemiesBelow.Length);
            pos.x = enemiesBelow[enemyIndex].transform.position.x + Random.Range(-enemiesBelow[enemyIndex].GetComponentInParent<Enemy>().globalEnemyBounds().x/2, enemiesBelow[enemyIndex].GetComponentInParent<Enemy>().globalEnemyBounds().x/2);
            pos.z = enemiesBelow[enemyIndex].transform.position.z + Random.Range(-enemiesBelow[enemyIndex].GetComponentInParent<Enemy>().globalEnemyBounds().z/2, enemiesBelow[enemyIndex].GetComponentInParent<Enemy>().globalEnemyBounds().z/2);
        } else {
            pos.x = mainPos.x + Random.Range(-area/2, area/2);
            pos.z = mainPos.z + Random.Range(-area/2, area/2);
        }
        pos.y = mainPos.y;

        GameObject go = Instantiate(projectile, pos, projectile.transform.rotation);
        go.GetComponent<ArmageddonProjectile>().damageInfo = CalculateDamage.damageInfo(skillTree, baseDamagePercentage);
        go.GetComponent<ArmageddonProjectile>().speed = projectileSpeed;
        go.SetActive(true);
        //playerControlls.GetComponent<Characteristics>().UseOrRestoreStamina(10);
    }

    void AddParticles() {
        for (int i = 0; i < hands.Length; i ++) {
            ParticleSystem ps = Instantiate(handsVFX, hands[i]);
            instanciatedEffects.Add(ps);
            ps.gameObject.SetActive(true);
        }
    }
    void RemoveParticles() {
        int x = instanciatedEffects.Count;
        for (int i = 0; i < x; i++) {
            instanciatedEffects[0].Stop();
            Destroy(instanciatedEffects[0].gameObject, 2f);
            instanciatedEffects.Remove(instanciatedEffects[0]);
        }
    }
}
