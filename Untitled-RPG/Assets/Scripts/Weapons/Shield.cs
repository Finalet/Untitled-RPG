using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    public GameObject mesh;
    [Space]
    public Vector3 localPosShiethed;

    public void ShiftMeshPos (bool objectOrigin = false) {
        mesh.transform.localPosition = objectOrigin ? Vector3.zero : localPosShiethed;
    }

}
