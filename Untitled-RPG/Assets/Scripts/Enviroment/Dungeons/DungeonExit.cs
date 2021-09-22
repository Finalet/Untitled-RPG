using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonExit : MonoBehaviour {

    public void Exit() {
        ScenesManagement.instance.LoadLevel("City");
    }

}