using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DancingSwords : Skill
{
    [Header("Custom vars")]
    public GameObject swordPrefab;
    public float skillDuration = 10;
    public float strength;
    [DisplayWithoutEdit] public bool skillInAction;
    [Space]
    public float noiseSpeed = 1;
    public float noiseStrength = 0.1f;
    public Buff buff;
    [Space]
    public AudioClip[] initialSounds;
    public AudioClip[] shootSounds;
    [Space]
    public Transform[] spawnedSwords;
    public Transform[] swordPositions;

    Vector3[] baseSwordPositions;
    Vector3[] noisedSwordPositions;
    bool[] spawned;
    int currentShotIndex;
    LayerMask ignorePlayer;
    Vector3 shootPoint;

    float lastShotTime;
    float skillStartedTime;

    float lerpValue = 20;

    protected override void CustomUse()
    {
        StartCoroutine(SpawnSwords());
    }

    protected override void Update() {
        base.Update();
        if (!skillInAction)
            return;

        if (Time.time - skillStartedTime > skillDuration && skillInAction){
            skillInAction = false;
            DropSwords();
            CanvasScript.instance.LMB.overrideSkill = null;
            CanvasScript.instance.LMB.overrideLMB = false;
            playerControlls.playerCamera.GetComponent<CameraControll>().isShortAiming = false;
            return;
        }

        OverrideMouseButtons();

        if(Time.time - lastShotTime >= 1) {
            playerControlls.playerCamera.GetComponent<CameraControll>().isShortAiming = false;
        }
    }
    void FixedUpdate() {
        for (int i = 0; i < spawnedSwords.Length; i++)
            MatchSwordPositions(i);
    }

    IEnumerator SpawnSwords() {
        buff.duration = skillDuration;
        buff.name = skillName;
        buff.icon = icon;

        lerpValue = 5;
        spawnedSwords = new Transform[swordPositions.Length];
        baseSwordPositions = new Vector3[swordPositions.Length];
        noisedSwordPositions = new Vector3[swordPositions.Length];
        spawned = new bool[swordPositions.Length];

        for (int i = 0; i < swordPositions.Length; i++) {
            baseSwordPositions[i] = swordPositions[i].localPosition;
            noisedSwordPositions[i] = baseSwordPositions[i];
        }

        for (int i = 0; i < swordPositions.Length; i++) {
            spawnedSwords[i] = Instantiate(swordPrefab, transform.position + Vector3.up, swordPositions[i].transform.rotation).transform;
            spawnedSwords[i].SetParent(null);
            spawnedSwords[i].gameObject.SetActive(true);
            spawnedSwords[i].localScale = Vector3.zero;
            spawnedSwords[i].DOScale(Vector3.one, 0.3f);
            spawned[i] = true;
            PlaySound(initialSounds[i]);
            yield return new WaitForSeconds(0.2f);
        }

        skillInAction = true;
        skillStartedTime = Time.time;
        lerpValue = 20;
        characteristics.AddBuff(buff);
    }
    IEnumerator RespawnSword (int swordIndex) {
        float startTime = Time.time;
        while (Time.time - startTime < 1) {
            if (!skillInAction)
                yield break;
            yield return null;
        }
        spawnedSwords[swordIndex] = Instantiate(swordPrefab, transform.position + Vector3.up, swordPositions[swordIndex].transform.rotation).transform;
        spawnedSwords[swordIndex].SetParent(null);
        spawnedSwords[swordIndex].gameObject.SetActive(true);
        spawnedSwords[swordIndex].localScale = Vector3.zero;
        spawnedSwords[swordIndex].DOScale(Vector3.one, 0.3f);
        spawned[swordIndex] = true;
    }

    void MatchSwordPositions (int swordIndex) {
        if (!spawned[swordIndex])
            return;

        spawnedSwords[swordIndex].position = Vector3.Lerp(spawnedSwords[swordIndex].position, swordPositions[swordIndex].position, Time.deltaTime * lerpValue);
        spawnedSwords[swordIndex].rotation = Quaternion.Lerp(spawnedSwords[swordIndex].rotation, swordPositions[swordIndex].rotation, Time.deltaTime * lerpValue);
        
        if (Vector3.Distance(swordPositions[swordIndex].localPosition, noisedSwordPositions[swordIndex]) <= 0) {
            noisedSwordPositions[swordIndex] = baseSwordPositions[swordIndex] + new Vector3(Random.value - 0.5f, Random.value - 0.5f, 0) * noiseStrength;
        }

        swordPositions[swordIndex].localPosition = Vector3.MoveTowards(swordPositions[swordIndex].localPosition, noisedSwordPositions[swordIndex], Time.deltaTime * noiseSpeed);
    }

    public void Shoot()
    {
        if (!skillInAction)
            return;

        PlayAnimation();
        lastShotTime = Time.time;

        playerControlls.playerCamera.GetComponent<CameraControll>().isShortAiming = true;

        ignorePlayer =~ LayerMask.GetMask("Player");

        RaycastHit hit;
        if (Physics.Raycast(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward, out hit, strength*2, ignorePlayer)) {
            shootPoint = hit.point;        
        } else {
            shootPoint = PlayerControlls.instance.playerCamera.transform.forward * (strength/2 + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance) + PlayerControlls.instance.playerCamera.transform.position;
        }

        int tryNumber = 0;
        while (spawnedSwords[currentShotIndex] == null){
            currentShotIndex = currentShotIndex < swordPositions.Length - 1 ? currentShotIndex+1 : 0;
            tryNumber ++;
            if(tryNumber >=4) return;
        }
        spawnedSwords[currentShotIndex].GetComponent<DancingSwordProjectile>().Shoot(strength, shootPoint, CalculateDamage.damageInfo(DamageType.Melee, baseDamagePercentage/2, skillName), CalculateDamage.damageInfo(DamageType.Ranged, baseDamagePercentage/2, skillName));
        spawnedSwords[currentShotIndex] = null;
        spawned[currentShotIndex] = false;
        StartCoroutine(RespawnSword(currentShotIndex));

        PlaySound(shootSounds[Random.Range(0, shootSounds.Length)]);

        playerControlls.isAttacking = false;
        currentShotIndex = currentShotIndex < swordPositions.Length - 1 ? currentShotIndex+1 : 0;
    }

    void PlayAnimation () {
        if (!playerControlls.isFlying) {
            if (currentShotIndex % 2 == 0) 
                animator.CrossFade("AttacksUpperBody.Mage.Lightning_right", 0.25f);
            else 
                animator.CrossFade("AttacksUpperBody.Mage.Lightning_left", 0.25f);
        } else {
            if (currentShotIndex % 2 == 0) 
                animator.CrossFade("Attacks.Mage.Lightning_flying_right", 0.25f);
            else 
                animator.CrossFade("Attacks.Mage.Lightning_flying_left", 0.25f);
        }
    }

    void DropSwords(){
        for (int i = 0; i < swordPositions.Length; i++) {
            if (spawnedSwords[i] != null) {
                Rigidbody rb = spawnedSwords[i].GetComponent<Rigidbody>();
                spawnedSwords[i].GetComponent<DancingSwordProjectile>().enabled = false;
                rb.isKinematic = false; 
                rb.useGravity = true; 
                spawnedSwords[i].GetComponent<Collider>().isTrigger = false; 
                rb.AddExplosionForce(100, transform.position + Vector3.up, 5);
                Destroy(spawnedSwords[i].gameObject, 10);
            }
            spawned[i] = false;
        }
    }

    void OverrideMouseButtons () {
        CanvasScript.instance.LMB.overrideSkill = this;
        CanvasScript.instance.LMB.overrideLMB = true;
    }

    public override void OverrideLMBAction()
    {
        if (Time.time - lastShotTime >= 0.4f * Characteristics.instance.attackSpeed.y)
            Shoot();
    }
    public override void OverrideLMBicon(UI_MiddleSkillPanelButtons slot)
    {
        slot.slotIcon.sprite = icon;

        slot.cooldownImage.color = new Color(0, 0, 0, 0);
        slot.cooldownImage.fillAmount = 1;
        slot.cooldownTimerText.text = "";

        slot.slotIcon.color = Color.white;
    }

    public override string getDescription()
    {
        DamageInfo dmgMelee = CalculateDamage.damageInfo(DamageType.Melee, baseDamagePercentage/2, skillName, 0, 0);
        DamageInfo dmgHunter = CalculateDamage.damageInfo(DamageType.Ranged, baseDamagePercentage/2, skillName, 0, 0);
        return $"Shoot levitating swords around you for {skillDuration} seconds dealing {dmgMelee.damage} {dmgMelee.damageType} and {dmgHunter.damage} {dmgHunter.damageType} damage.";
    }
}
