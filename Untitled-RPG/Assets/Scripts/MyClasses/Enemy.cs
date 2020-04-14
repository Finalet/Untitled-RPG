using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class Enemy : MonoBehaviour
{
    public string enemyName;
    public int health;

    float prevHitID;

    void Update() {
        if (health <= 0)
            Die();
    }

    public virtual void GetHit(int damage, float hitID) {
        if (hitID == prevHitID)
            return;

        prevHitID = hitID;
        health -= damage;
        DisplayDamageNumber (damage);
        print ("Got damaged by " + damage + "HP. " + health + " HP left.");
    }

    public virtual void Die() {
        print("Died");
        Destroy(gameObject);
    }

    void DisplayDamageNumber(int damage) {
        TextMeshProUGUI ddText = Instantiate(AssetHolder.instance.ddText);
        ddText.text = damage.ToString();
    }
}
