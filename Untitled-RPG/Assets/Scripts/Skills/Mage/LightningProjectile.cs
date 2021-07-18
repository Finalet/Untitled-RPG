using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningProjectile : MonoBehaviour
{
    public DamageInfo damageInfo;
    public int shots;

    List<Enemy> enemiesHit = new List<Enemy>();

    void Start() {
        Invoke("EnableCollider", 0.1f);
        Destroy(gameObject, 1f);
    }

    void OnTriggerEnter(Collider other) {
        Enemy en = other.transform.GetComponentInParent<Enemy>();
        if (other.isTrigger || other.CompareTag("Player") || en == null)
            return;
        
        if (!enemiesHit.Contains(en)) {
            en.GetHit(damageInfo, "Lightning", false, true, HitType.Normal, transform.position);
            enemiesHit.Add(en);

            bl_UCrosshair.Instance.OnHit();
        }
    }

    void EnableCollider () {
        GetComponent<SphereCollider>().enabled = true;
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.2f, 2*(1+damageInfo.damage/2000), 0.1f, transform.position);
    }
}
