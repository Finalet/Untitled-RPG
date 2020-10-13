using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsController : MonoBehaviour
{
    public static WeaponsController instance;

    public bool isWeaponOut;
    Animator animator;

    float weight;
    float overlaySpeed = 3;

    public bool isLeftHandEquiped;
    public bool isRightHandEquiped;
    public bool isDualHands;

    public GameObject LeftHandEquipement;
    public GameObject RightHandEquipement;

    public Transform LeftHand;
    public Transform RightHand;

    public AudioClip[] sheathSounds;

    void Start () {
        if (instance == null)
            instance = this;

        animator = GetComponent<Animator>();
        DisableTrails();
    }

    public IEnumerator UnSheathe () {
        animator.SetTrigger("UnSheath");
        GetComponent<AudioSource>().clip = sheathSounds[0];
        GetComponent<AudioSource>().PlayDelayed(0.3f);
        weight = 0;
        while (weight <= 1) {
            if (isRightHandEquiped) {
                animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), weight);
                animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
            }
            if (isLeftHandEquiped) {
                animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), weight);
                animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), weight);
            }
                
            weight += Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        isWeaponOut = true;
        
        if (isRightHandEquiped) {
            animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), 1);
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 1);
        }
        if (isLeftHandEquiped) {
            animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), 1);
            animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), 1);
        }
        

        weight = 1;
        while (weight >= 0) {
            if (isRightHandEquiped)
                animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
            if (isLeftHandEquiped)
                animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), weight);
            weight -= Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 0);
        animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), 0);
    }
    public IEnumerator Sheathe () {
        isWeaponOut = false;
        animator.SetTrigger("Sheath");

        GetComponent<AudioSource>().clip = sheathSounds[1];
        GetComponent<AudioSource>().PlayDelayed(0.3f);

        weight = 0;
        while (weight <= 1) {
            if (isRightHandEquiped)
                animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
            if (isLeftHandEquiped)
                animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), weight);

            weight += Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        if (isRightHandEquiped)
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 1);
        if (isLeftHandEquiped)
            animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), 1);


        weight = 1;
        while (weight >= 0) {
            if (isRightHandEquiped) {
                animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
                animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), weight);
            }
            if (isLeftHandEquiped) {
                animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), weight);
                animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), weight);
            }
            
            weight -= Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        weight = 0;

        if (isRightHandEquiped) {
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 0);
            animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), 0);
        }
        if (isLeftHandEquiped) {
            animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), 0);
            animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), 0);
        }
    }

    public void InstantUnsheathe () {
        EquipmentManager.instance.MainHandSlot.GetChild(0).SetParent(RightHand);
        RightHand.GetChild(0).transform.localPosition = Vector3.zero;
        RightHand.GetChild(0).transform.localEulerAngles = Vector3.zero;
        EquipmentManager.instance.SecondaryHandSlot.GetChild(0).SetParent(LeftHand);
        LeftHand.GetChild(0).transform.localPosition = Vector3.zero;
        LeftHand.GetChild(0).transform.localEulerAngles = Vector3.zero;
        isWeaponOut = true;
    }

    public void EnableTrails() {
        //LeftHandEquipement.GetComponentInChildren<ParticleSystem>().Play();
        //RightHandEquipement.GetComponentInChildren<ParticleSystem>().Play();
    }
    public void DisableTrails() {
        //LeftHandEquipement.GetComponentInChildren<ParticleSystem>().Stop();
        //RightHandEquipement.GetComponentInChildren<ParticleSystem>().Stop();
    }
}
