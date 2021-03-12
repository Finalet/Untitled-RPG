using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class disableOnUnload : MonoBehaviour
{
    void Awake() {
        if (ScenesManagement.instance != null) {
            ScenesManagement.instance.GameobjectsToDisableOnUnload.Add(gameObject);
        }
    }
}
