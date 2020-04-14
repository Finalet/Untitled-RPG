using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillPanel : MonoBehaviour
{
    public Skill skill1;
    public Skill skill2;
    public Skill skill3;
    public Skill skill4;
    public Skill skill5;

    void Update() {
        if (!PlayerControlls.instance.isWeaponOut)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && skill1 != null)
            skill1.Use();
        if (Input.GetKeyDown(KeyCode.Alpha2) && skill2 != null)
            skill2.Use();
        if (Input.GetKeyDown(KeyCode.Alpha3) && skill3 != null)
            skill3.Use();
        if (Input.GetKeyDown(KeyCode.Alpha4) && skill4 != null)
            skill4.Use();
        if (Input.GetKeyDown(KeyCode.Alpha5) && skill5 != null)
            skill5.Use();
    }
}
