using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goblin : Enemy
{
    public GameObject healthBar;

    public override void Start() {
        base.Start();
    }

    public override void Update() {
        base.Update();

        ShowHealthBar();
    }

    void ShowHealthBar () {
        if (isDead) {
            healthBar.SetActive(false);
            return;
        }

        if (Vector3.Distance(transform.position, PlayerControlls.instance.transform.position) <= PlayerControlls.instance.playerCamera.GetComponent<LookingTarget>().viewDistance / 1.5f) {
            healthBar.transform.GetChild(0).localScale = new Vector3((float)health/maxHealth, transform.localScale.y, transform.localScale.z);
            healthBar.transform.LookAt (PlayerControlls.instance.playerCamera.transform);
            healthBar.SetActive(true);
        } else {
            healthBar.SetActive(false);
        }

    }
}
