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

    protected override void BasicGetHit(int damage) {
        actualDamage = calculateActualDamage(damage);

        rotationX = direction.normalized.z * 30 * (1 + (float)damage/5000);
        rotationZ = -direction.normalized.x * 30 * (1 + (float)damage/5000);
        HitParticles();
        PlayGetHitSounds();
        PlayStabSounds();
    
        PeaceCanvas.instance.DebugChat($"[{System.DateTime.Now.Hour}:{System.DateTime.Now.Minute}:{System.DateTime.Now.Second}] {enemyName} was hit <color=red>{actualDamage}</color> points.");
    }

    public override void GetKnockedDown () {
        //Do nothing
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
        stabsAudioSource.clip = stabsClips[playID];
        stabsAudioSource.pitch = 1 + Random.Range(-0.1f, 0.1f);
        stabsAudioSource.Play();
    }
}
