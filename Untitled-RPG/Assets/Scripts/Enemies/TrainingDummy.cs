using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingDummy : StaticEnemy
{
    float rotationX;
    float rotationZ;
    
    float x;
    float z;
    
    Vector3 direction;
    GameObject rotationObj;
    
    protected override void Start() {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;

        SetInitialPosition();
        
        rotationObj = transform.GetChild(0).gameObject;
        canGetHit = true;
    }

    protected override void Update() {
        if (PlayerControlls.instance == null) //Player instance is null when level is only loading
            return;

        transform.rotation = Quaternion.Euler(0,0,0);

        currentHealth = maxHealth;

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

    public override void GetHit (int damage, string skillName, bool stopHit = false, bool cameraShake = false, HitType hitType = HitType.Normal, Vector3 damageTextPos = new Vector3 ()) {
        if (isDead || !canGetHit)
            return;
        
        int actualDamage = calculateActualDamage(damage);

        rotationX = direction.normalized.z * 30 * (1 + (float)damage/5000);
        rotationZ = -direction.normalized.x * 30 * (1 + (float)damage/5000);

        currentHealth -= actualDamage;
        PlayHitParticles(); 
        PlayGetHitSounds();
        PlayStabSounds();
        
        if (stopHit) StartCoroutine(HitStop());
        if (cameraShake) PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.2f, 1*(1+actualDamage/3000), 0.1f, transform.position);
        DisplayDamageNumber(actualDamage, damageTextPos);

        PeaceCanvas.instance.DebugChat($"[{System.DateTime.Now.Hour}:{System.DateTime.Now.Minute}:{System.DateTime.Now.Second}] {enemyName} was hit <color=red>{actualDamage}</color> points by <color=#80FFFF>{skillName}</color>.");
    }

    public override void Hit(){
        //Does nothing
    }
    
    protected override void PlayStabSounds () {
        int playID;
        float x = Random.Range(0f, 1f);
        if (x<0.7f) {
            playID = 0;
        } else {
            playID = 1;
        }
        audioSource.pitch = 1 + Random.Range(-0.1f, 0.1f);
        audioSource.PlayOneShot(stabSounds[playID]);
    } 
}
