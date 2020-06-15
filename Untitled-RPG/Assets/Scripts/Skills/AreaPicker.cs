using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaPicker : MonoBehaviour
{
    public Transform child0;
    public Transform child1;
    public Transform arrow;

    void Update()
    {
        child0.Rotate(Vector3.forward * Time.deltaTime * 50);
        child1.Rotate(-Vector3.forward * Time.deltaTime * 50);
        arrow.transform.localPosition = Vector3.up * Mathf.Sin(Time.time * 5) / 2;
        arrow.Rotate (Vector3.forward * Time.deltaTime * 30);
    }
}