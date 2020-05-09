using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWaterWheel : MonoBehaviour
{
    void Start() {
        
    }
    void Update()
    {
        transform.Rotate(10*Time.deltaTime, 0,0);
    }
}
