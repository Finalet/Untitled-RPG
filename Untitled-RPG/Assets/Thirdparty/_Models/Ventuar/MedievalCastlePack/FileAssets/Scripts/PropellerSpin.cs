using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script spins the propeller of the WindmillBase object spin along its local Z axis.
public class PropellerSpin : MonoBehaviour
{
    
    public float spinSpeed = 13;

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(transform.position, transform.forward, Time.deltaTime * -spinSpeed);
    }
}
