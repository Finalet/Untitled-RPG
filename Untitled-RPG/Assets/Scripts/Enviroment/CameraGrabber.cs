using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AwesomeTechnologies.VegetationSystem;
using StylizedGrass;
using Crest;

public class CameraGrabber : MonoBehaviour
{

    void Awake() {
        StartCoroutine(waitForPlayerLoad());
    }

    IEnumerator waitForPlayerLoad () {
        VegetationSystemPro VSP = GetComponent<VegetationSystemPro>();
        StylizedGrassRenderer SGR = GetComponent<StylizedGrassRenderer>();
        OceanRenderer OR = GetComponent<OceanRenderer>();
        while (PlayerControlls.instance == null) {
            //print("waiting");
            yield return null;
        }
        
        if (VSP != null) VSP.AddCamera(PlayerControlls.instance.playerCamera);
        if (SGR != null) SGR.followTarget = PlayerControlls.instance.transform; 
        if (OR != null) OR.Viewpoint = PlayerControlls.instance.playerCamera.transform;
    }
}
