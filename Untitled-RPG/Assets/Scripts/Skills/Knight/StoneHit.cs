using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneHit : Skill
{
    bool yes;

    Vector3 baseCenter;
    Vector3 newCenter;

    List<Enemy> enemiesHit = new List<Enemy>();

    public AudioClip first;
    public AudioClip last;

    public GameObject SFX;

    protected override void Start() {
        base.Start();
        
        GetComponent<BoxCollider>().enabled = false;
        
        baseCenter = GetComponent<BoxCollider>().center;
        newCenter = baseCenter + Vector3.forward * 4.5f;
    }

    protected override void Update() {
        base.Update();
        ClearTrigger();
    }

    protected override void CustomUse() {
        StartCoroutine(Using());
    }

    IEnumerator Using () {
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHandedSword)
            animator.CrossFade("Attacks.Knight.StoneHit Two handed", 0.25f);
        else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualSwords)
            animator.CrossFade("Attacks.Knight.StoneHit Dual swords", 0.25f);
        else 
            animator.CrossFade("Attacks.Knight.StoneHit Dual swords", 0.25f);

        while(!yes) {
            yield return null;
        }
        transform.GetChild(0).GetComponent<ParticleSystem>().Play();

        GameObject soundFX = Instantiate(SFX.gameObject, transform.position, Quaternion.identity);
        soundFX.GetComponent<AudioSource>().clip = first;
        soundFX.GetComponent<AudioSource>().Play();

        playerControlls.playerCamera.GetComponent<CameraControll>().CameraShake();

        GetComponent<BoxCollider>().enabled = true;
        while (GetComponent<BoxCollider>().center != newCenter) {
            GetComponent<BoxCollider>().center = Vector3.MoveTowards(GetComponent<BoxCollider>().center, newCenter, 15 * Time.deltaTime);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        yes = false;
        GetComponent<BoxCollider>().center = baseCenter;
        GetComponent<BoxCollider>().enabled = false;
        enemiesHit.Clear();
        yield return new WaitForSeconds(1.1f);

        soundFX.GetComponent<AudioSource>().clip = last;
        soundFX.GetComponent<AudioSource>().Play();
        while(soundFX.GetComponent<AudioSource>().isPlaying){
            yield return null;
        }
        Destroy(soundFX);
    }
    
    public void ApplyDamage() {
        yes = true;
    }

    void OnTriggerEnter(Collider other) {
        if (!yes)
            return;

        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (en == null || other.isTrigger)
            return;

        if (!enemiesHit.Contains(en)) {
            en.GetHit(CalculateDamage.damageInfo(skillTree, baseDamagePercentage), skillName, true, true, HitType.Knockdown);
            enemiesHit.Add(en);
        }
    }

    void ClearTrigger () {
        for (int i = 0; i < enemiesHit.Count; i++) {
            if (enemiesHit[i] == null) {
                enemiesHit.RemoveAt(i);
            }
        }
    }
    public override string getDescription()
    {
        DamageInfo dmg = CalculateDamage.damageInfo(skillTree, baseDamagePercentage, 0, 0);
        return $"Hit the ground in front of you, knicking down enemies and dealing {dmg.damage} {dmg.damageType} damage.";
    }
}
