using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmageddonProjectile : MonoBehaviour
{
    public float speed;
    public float actualDamage;

    List<Enemy> enemiesHit = new List<Enemy>();

    void Start() {
        GetComponent<Rigidbody>().AddForce(Vector3.down * speed, ForceMode.Impulse);
        Destroy(gameObject, 2);
    }

    void OnTriggerEnter(Collider other) {
        if (other.isTrigger || other.CompareTag("Player"))
            return;
        
        if (other.gameObject.GetComponent<Enemy>() != null) {
            if (!enemiesHit.Contains(other.GetComponent<Enemy>())) {
                other.GetComponent<Enemy>().GetHit(damage(), "Armageddon");
                enemiesHit.Add(other.GetComponent<Enemy>());
            }
        }
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.2f, 0.1f, damage());
    }

    int damage () {
        return Mathf.RoundToInt(Random.Range(actualDamage*0.85f, actualDamage*1.15f));
    }
}
