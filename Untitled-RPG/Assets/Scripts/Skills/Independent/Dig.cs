using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dig : Skill
{
    [Header("Custom Vars")]
    public AudioClip digSound;
    public AudioClip hitSound;
    public ParticleSystem digVFX;
    public ParticleSystem hitVFX;

    Collider[] col;

    protected override void CustomUse(){
        StartCoroutine(StartDig());
    }

    IEnumerator StartDig () {
        PlaySound(digSound, 0, 1.5f, 0.5f);
        PlayerControlls.instance.PlayGeneralAnimation(4);
        yield return new WaitForSeconds(0.5f);
        digVFX.Play();
        yield return new WaitForSeconds(3f);
    }

    public void DigHit () {
        ParticleSystem ps = Instantiate(hitVFX, WeaponsController.instance.RightHandEquipObj.transform);
        ps.transform.localPosition = Vector3.up;
        ps.Play();
        Destroy(ps.gameObject, 1);

        col = Physics.OverlapSphere(transform.position + transform.forward * 1.5f, 1, ~LayerMask.GetMask("Player"), QueryTriggerInteraction.Collide);

        foreach (Collider c in col) {
            if (c.TryGetComponent(out BurriedObject b)) {
                PlayerControlls.instance.cameraControl.CameraShake(0.1f, 0.7f);
                audioSource.PlayOneShot(hitSound);
            }
        }
    }

    public void DigOut () {
        foreach (Collider c in col) {
            if (c.TryGetComponent(out BurriedObject b)) {
                b.DigOut();
            }
        }
    }
}
