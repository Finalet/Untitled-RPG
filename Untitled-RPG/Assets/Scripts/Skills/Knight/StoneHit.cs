using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneHit : Skill
{
    bool yes;

    Vector3 baseCenter;
    Vector3 newCenter;

    List<IDamagable> damagablesHit = new List<IDamagable>();

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
        if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.TwoHanded)
            animator.CrossFade("Attacks.Knight.StoneHit Two handed", 0.25f);
        else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.DualOneHanded)
            animator.CrossFade("Attacks.Knight.StoneHit Dual swords", 0.25f);
        else if (WeaponsController.instance.bothHandsStatus == BothHandsStatus.OneHandedPlusShield)
            animator.CrossFade("Attacks.Knight.StoneHit OneHanded", 0.25f);
        else 
            animator.CrossFade("Attacks.Knight.StoneHit OneHanded", 0.25f);

        PlaySound(first, 0f, characteristics.attackSpeed.x, 0.2f);
        while(!yes) {
            yield return null;
        }
        transform.GetChild(0).GetComponent<ParticleSystem>().Play();

        GameObject soundFX = Instantiate(SFX.gameObject, transform.position, Quaternion.identity);

        playerControlls.playerCamera.GetComponent<CameraControll>().CameraShake();

        GetComponent<BoxCollider>().enabled = true;
        while (GetComponent<BoxCollider>().center != newCenter) {
            GetComponent<BoxCollider>().center = Vector3.MoveTowards(GetComponent<BoxCollider>().center, newCenter, 15 * Time.deltaTime);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        yes = false;
        GetComponent<BoxCollider>().center = baseCenter;
        GetComponent<BoxCollider>().enabled = false;
        damagablesHit.Clear();
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

        IDamagable en = other.transform.GetComponentInParent<IDamagable>();
        if (en == null || other.isTrigger)
            return;

        if (!damagablesHit.Contains(en)) {
            en.GetHit(CalculateDamage.damageInfo(damageType, baseDamagePercentage, skillName), true, true, HitType.Knockdown);
            damagablesHit.Add(en);
        }
    }

    void ClearTrigger () {
        for (int i = 0; i < damagablesHit.Count; i++) {
            if (damagablesHit[i] == null) {
                damagablesHit.RemoveAt(i);
            }
        }
    }
    public override string getDescription()
    {
        DamageInfo dmg = CalculateDamage.damageInfo(damageType, baseDamagePercentage, skillName, 0, 0);
        return $"Hit the ground in front of you, knicking down enemies and dealing {dmg.damage} {dmg.damageType} damage.";
    }
}
