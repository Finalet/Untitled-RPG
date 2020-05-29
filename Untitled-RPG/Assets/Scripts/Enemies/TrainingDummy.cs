using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingDummy : Enemy
{
    float rotationX;
    float rotationZ;
    
    float x;
    float z;
    
    Vector3 direction;
    GameObject rotationObj;
    
    protected override void Start() {
        rotationObj = transform.GetChild(0).gameObject;
        canGetHit = true;
        stabsAudioSource = GetComponent<AudioSource>();
    }

    protected override void Update() {
        if (PlayerControlls.instance == null) //Player instance is null when level is only loading
            return;

        transform.rotation = Quaternion.Euler(0,0,0);

        health = maxHealth;

        if (rotationX > 0.5f || rotationX < -0.5f) {
            rotationX = Mathf.Lerp(rotationX, 0, 10*Time.deltaTime);
        } else {
            rotationX = 0;
        }

        if (rotationZ > 0.5f || rotationZ < -0.5f) {
            rotationZ = Mathf.Lerp(rotationZ, 0, 10*Time.deltaTime);
        } else {
            rotationZ = 0;
        }
        x = rotationX;
        z = rotationZ;   
        

        direction = transform.position - PlayerControlls.instance.transform.position;
        rotationObj.transform.rotation = Quaternion.Euler(x, rotationObj.transform.eulerAngles.y, z);
    }

    public override void GetHit (int damage) {
        if (isDead || !canGetHit)
            return;

        int actualDamage = Mathf.RoundToInt( damage * (1 + TargetSkillDamagePercentage/100) ); 

        DisplayDamageNumber (actualDamage);
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.1f, 0.1f, actualDamage);
        HitParticles();
        StartCoroutine(HitStop());
        PlayGetHitSounds();
        PlayStabSounds();

        rotationX = direction.normalized.z * 30 * (1 + (float)damage/5000);
        rotationZ = -direction.normalized.x * 30 * (1 + (float)damage/5000);
    }

    IEnumerator HitStop () {
        float timer = Time.realtimeSinceStartup;
        Time.timeScale = 0.2f;
        while(Time.realtimeSinceStartup - timer < 0.13f) {
            yield return null;
        }
        Time.timeScale = 1;
    }

    public override void GetKnockedDown () {
        //Do nothing
    }

    protected override void PlayStabSounds () {
        int playID;
        float x = Random.Range(0f, 1f);
        if (x<0.7f) {
            playID = 0;
        } else {
            playID = 1;
        }
        stabsAudioSource.clip = stabsClips[playID];
        stabsAudioSource.pitch = 1 + Random.Range(-0.1f, 0.1f);
        stabsAudioSource.Play();
    }
}
