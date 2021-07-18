using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningProjectile : MonoBehaviour
{
    public DamageInfo damageInfo;
    public int shots;

    List<IDamagable> damagablesHit = new List<IDamagable>();

    void Start() {
        Invoke("EnableCollider", 0.1f);
        Destroy(gameObject, 1f);
    }

    void OnTriggerEnter(Collider other) {
        IDamagable en = other.transform.GetComponentInParent<IDamagable>();
        if (other.isTrigger || other.CompareTag("Player") || en == null)
            return;
        
        if (!damagablesHit.Contains(en)) {
            en.GetHit(damageInfo, "Lightning", false, true, HitType.Normal, transform.position);
            damagablesHit.Add(en);

            bl_UCrosshair.Instance.OnHit();
        }
    }

    void EnableCollider () {
        GetComponent<SphereCollider>().enabled = true;
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.2f, 2*(1+damageInfo.damage/2000), 0.1f, transform.position);
    }
}
