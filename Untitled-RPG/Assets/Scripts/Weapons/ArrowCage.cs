using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowCage : Arrow
{
    public GameObject cage;
    public int duraiton;
    public ParticleSystem strongTrails;
    AudioSource[] audioSources;
    public GameObject colliders;

    protected override void Start() {
        base.Start();
        strongTrails.Stop();
        audioSources = GetComponentsInChildren<AudioSource>(true);
    }

    public override void Shoot (float _strength, Vector3 _shotPoint, DamageInfo _damageInfo) {
        base.Shoot(_strength, _shotPoint, _damageInfo);
        strongTrails.Play();
        PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().CameraShake(0.15f, 1.5f, 0.07f, transform.position);
    }

    protected override void Collision (Transform collisionObj) {
        base.Collision(collisionObj);
        strongTrails.Stop();

        Vector3 cagePos;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 20, LayerMask.GetMask("Terrain", "StaticLevel", "Default"))) {
            cagePos = hit.point;
        } else {
            cagePos = transform.position;
        }
        Vector3[] cornerPoints = new Vector3[4];
        if (Physics.Raycast(cagePos + Vector3.up * 3f + Vector3.forward * 3, Vector3.down, out hit, 5, LayerMask.GetMask("Terrain", "StaticLevel", "Default"))) {
            cornerPoints[0] = hit.point;
        }
        if (Physics.Raycast(cagePos + Vector3.up * 3f + -Vector3.forward * 3, Vector3.down, out hit, 5, LayerMask.GetMask("Terrain", "StaticLevel", "Default"))) {
            cornerPoints[1] = hit.point;
        }
        if (Physics.Raycast(cagePos + Vector3.up * 3f + Vector3.right * 3, Vector3.down, out hit, 5, LayerMask.GetMask("Terrain", "StaticLevel", "Default"))) {
            cornerPoints[2] = hit.point;
        }
        if (Physics.Raycast(cagePos + Vector3.up * 3f + -Vector3.right * 3, Vector3.down, out hit, 5, LayerMask.GetMask("Terrain", "StaticLevel", "Default"))) {
            cornerPoints[3] = hit.point;
        }
        cage.transform.SetParent(null);
        cage.transform.localScale = Vector3.one;
        cage.transform.position = cagePos + Vector3.up * 0.1f;
        Vector3 normal = Vector3.Cross(cornerPoints[0]-cornerPoints[1], cornerPoints[2]-cornerPoints[3]);
        cage.transform.up = Vector3.Angle(normal, Vector3.up) > 5 ? normal : Vector3.up;
        cage.SetActive(true);
        Destroy(cage, duraiton+5);
        Destroy(colliders, duraiton);

        for (int i = 0; i < audioSources.Length; i++) {
            audioSources[i].PlayDelayed(i*0.1f);
        }
    }
}
