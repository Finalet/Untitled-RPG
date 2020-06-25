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

    [Space]
    public GameObject projectile;
    public float projectileSpeed;

    public ParticleSystem handsVFX;
    public ParticleSystem cloudVFX;
    public Transform[] hands;

    float lastShotTime;
    Vector3 mainPos;

    public List<ParticleSystem> instanciatedEffects = new List<ParticleSystem>();
    ParticleSystem instanciatedCloud;

    protected override float actualDistance () {
        return distance + characteristics.magicSkillDistanceIncrease;
    } 

    protected override void CastingAnim() {
        animator.CrossFade("Attacks.Mage.Armageddon_start", 0.25f);

        AddParticles();


        //PlaySound(castingSound, 0, 0.3f, 0.3f);
    }

    protected override void CustomUse() {}

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
            yield return null;
        }
        animator.CrossFade("Attacks.Mage.Armageddon_end", 0.25f);
        instanciatedCloud.Stop();
        yield return new WaitForSeconds (5);
        Destroy(instanciatedCloud.gameObject);
    }
    
    void ShootOneMeteor() {
        Vector3 randPos = new Vector3(Random.Range(-area/2, area/2), 0, Random.Range(-area/2, area/2));
        GameObject go = Instantiate(projectile, mainPos + randPos, projectile.transform.rotation);
        go.GetComponent<ArmageddonProjectile>().actualDamage = actualDamage();
        go.GetComponent<ArmageddonProjectile>().speed = projectileSpeed;
        go.SetActive(true);
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
            Destroy(instanciatedEffects[0].gameObject, 1f);
            instanciatedEffects.Remove(instanciatedEffects[0]);
        }
    }
}
