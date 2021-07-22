using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GolemRockShard : MonoBehaviour
{
    Golem golem;

    public ParticleSystem vfx;
    public Transform col;
    public EnemyMeleeWeapon colWeapons;

    public AudioSource audioSource;

    public void Init(float distanceToPlayer, Golem _en) {golem = _en;

        var main = vfx.main;
        float duration = distanceToPlayer/30f;
        main.startLifetime = duration;

        col.DOLocalMoveZ(distanceToPlayer, duration * 0.9f).SetDelay(0.1f);
        colWeapons.enemy = golem;

        gameObject.SetActive(true);
        vfx.Play();

        Invoke("CameraShake", duration);

        audioSource.clip = golem.rockShardLoop;
        audioSource.pitch = 0.8f;
        audioSource.Play();

        Destroy(col.gameObject, duration + 0.5f);
        Destroy(gameObject, 4);
    }
    void CameraShake () {
        CameraControll.instance.CameraShake(0.15f, 1.5f, 0.1f, PlayerControlls.instance.transform.position);
        audioSource.clip = golem.rockShardExplosion;
        audioSource.pitch = 1;
        audioSource.Play();
    }
}
