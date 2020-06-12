using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HailstoneProjectile : MonoBehaviour
{
    void Start() {
        GetComponent<Rigidbody>().AddForce(transform.right * 30, ForceMode.Impulse);
    }
}
