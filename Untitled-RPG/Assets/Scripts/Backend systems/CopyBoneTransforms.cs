using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class CopyBoneTransforms : MonoBehaviour
{
    public Transform parent;

    Vector3[] copyPos;
    Quaternion[] copyRot;

    [Button("Copy all transforms")]
    void CopyAllTransforms () {
        copyPos = new Vector3[parent.childCount];
        copyRot = new Quaternion[parent.childCount];
        for (int i = 0; i < parent.childCount; i++) {
            copyPos[i] = parent.GetChild(i).transform.position;
            copyRot[i] = parent.GetChild(i).transform.rotation;
        }
    }

    [Button("Paste all transforms")]
    void PasteAllTransforms () {
        if (copyPos == null || copyRot == null || copyPos.Length == 0 || copyRot.Length == 0) throw new System.Exception("No position or rotation in clipboard.") ;

        for (int i = 0; i < parent.childCount; i++) {
            parent.GetChild(i).transform.position = copyPos[i];
            parent.GetChild(i).transform.rotation = copyRot[i];
        }
    }

}
