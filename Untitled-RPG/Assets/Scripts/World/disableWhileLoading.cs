using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class disableWhileLoading : MonoBehaviour
{
    void Awake() {
        if (ScenesManagement.instance != null) {
            if (ScenesManagement.instance.isLoading) {
                ScenesManagement.instance.GameobjectsToEnableOnLoad.Add(gameObject);
                gameObject.SetActive(false);
            }
        }
    }
}
