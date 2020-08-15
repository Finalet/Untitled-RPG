using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCollider : MonoBehaviour
{
    public Transform mainShip;
    public Collider crestCollider;

    void Start() {
        Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), crestCollider, true);
    }

    void FixedUpdate() {
        transform.position = mainShip.position;
        transform.rotation = mainShip.rotation;
        transform.localScale = mainShip.localScale;
    }
}
