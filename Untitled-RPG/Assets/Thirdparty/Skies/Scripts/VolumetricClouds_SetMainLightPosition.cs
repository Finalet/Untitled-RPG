using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class VolumetricClouds_SetMainLightPosition : MonoBehaviour
{
    private void Update()
    {
        Shader.SetGlobalVector("_SUNDIRECTION", -transform.forward);
    }
}
