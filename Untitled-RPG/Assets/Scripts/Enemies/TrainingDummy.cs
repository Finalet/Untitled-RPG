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
    }

    protected override void Update() {
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

    public void CustomGetHit (float damage) {
        rotationX = direction.normalized.z * 30 * (1 + damage/5000);
        rotationZ = -direction.normalized.x * 30 * (1 + damage/5000);
    }

    protected override void PlayGetHitNext () {
        int playID;
        float x = Random.Range(0f, 1f);
        if (x<0.7f) {
            playID = 0;
        } else {
            playID = 1;
        }
        GetComponent<AudioSource>().clip = getHitClips[playID];
        GetComponent<AudioSource>().pitch = 1 + Random.Range(-0.1f, 0.1f);
        GetComponent<AudioSource>().Play();
    }
}
