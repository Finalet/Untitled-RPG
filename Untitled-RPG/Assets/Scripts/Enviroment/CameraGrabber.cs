using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AwesomeTechnologies.VegetationSystem;
using StylizedGrass;
using Crest;

//Drop on GameObject which needs to reference Players position or camera
public class CameraGrabber : MonoBehaviour
{

    void Awake() {
        StartCoroutine(waitForPlayerLoad());
    }

    IEnumerator waitForPlayerLoad () {
        while (PlayerControlls.instance == null) {
            //print("waiting");
            yield return null;
        }
        
        if (TryGetComponent(out VegetationSystemPro VSP)) VSP.AddCamera(PlayerControlls.instance.playerCamera);
        if (TryGetComponent(out StylizedGrassRenderer SGR)) SGR.followTarget = PlayerControlls.instance.transform; 
        if (TryGetComponent(out OceanRenderer OR)) {
            OR.Viewpoint = PlayerControlls.instance.transform;
            OR.ViewCamera = PlayerControlls.instance.playerCamera;
        }
    }
}
