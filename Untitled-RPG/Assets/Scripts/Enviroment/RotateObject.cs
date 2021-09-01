using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public Vector3 rotationDirection;
    public float rotationSpeed;
    void Update()
    {
        transform.Rotate(rotationDirection * rotationSpeed);
    }
}
