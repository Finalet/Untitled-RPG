using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BothHandsStatus {RightHandSwordOnly, LeftHandSwordOnly, DualSwords, SwordShield, TwoHandedWeapon, AllEmpty};
public enum SingleHandStatus {OneHanded, TwoHanded, Shield, Empty};

public class WeaponsController : MonoBehaviour
{
    public static WeaponsController instance;

    Animator animator;

    float weight;
    float overlaySpeed = 3;

    public bool isWeaponOut;
    [Header("Objects")]
    public GameObject LeftHandEquipObj;
    public GameObject RightHandEquipObj;
    [Header("Hands")]
    public BothHandsStatus bothHandsStatus;
    [Space]
    public SingleHandStatus leftHandStatus;
    public SingleHandStatus rightHandStatus;
    public Transform LeftHandTrans;
    public Transform RightHandTrans;
    [Header("Slots")]
    public Transform leftHipSlot;
    public Transform rightHipSlot;
    public Transform twohandedWeaponSlot;

    public AudioClip[] sheathSounds;

    void Awake() {
        if (instance == null)
            instance = this;
    }

    void Start () {
        animator = GetComponent<Animator>();
        DisableTrails();
    }

    void Update() {
        CheckHandsEquip();
    }

    public IEnumerator UnSheathe () {
        animator.SetTrigger("UnSheath");
        GetComponent<AudioSource>().clip = sheathSounds[0];
        GetComponent<AudioSource>().PlayDelayed(0.3f);
        weight = 0;
        while (weight <= 1) {
            if (rightHandStatus != SingleHandStatus.Empty) {
                animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), weight);
                animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
            }
            if (leftHandStatus != SingleHandStatus.Empty) {
                animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), weight);
                animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), weight);
            }
                
            weight += Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        isWeaponOut = true;
        
        if (rightHandStatus != SingleHandStatus.Empty) {
            animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), 1);
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 1);
        }
        if (leftHandStatus != SingleHandStatus.Empty) {
            animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), 1);
            animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), 1);
        }
        

        weight = 1;
        while (weight >= 0) {
            if (rightHandStatus != SingleHandStatus.Empty)
                animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
            if (leftHandStatus != SingleHandStatus.Empty)
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
            if (rightHandStatus != SingleHandStatus.Empty)
                animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
            if (leftHandStatus != SingleHandStatus.Empty)
                animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), weight);

            weight += Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        if (rightHandStatus != SingleHandStatus.Empty)
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 1);
        if (leftHandStatus != SingleHandStatus.Empty)
            animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), 1);

        SheathObj();
        weight = 1;
        while (weight >= 0) {
            if (rightHandStatus != SingleHandStatus.Empty) {
                animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
                animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), weight);
            }
            if (leftHandStatus != SingleHandStatus.Empty) {
                animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), weight);
                animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), weight);
            }
            
            weight -= Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        weight = 0;

        if (rightHandStatus != SingleHandStatus.Empty) {
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 0);
            animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), 0);
        }
        if (leftHandStatus != SingleHandStatus.Empty) {
            animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), 0);
            animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), 0);
        }
    }

    public void InstantUnsheathe () {
        animator.SetTrigger("UnSheath");
        if (bothHandsStatus == BothHandsStatus.DualSwords) { //Currently only dual swords supported, add ELSE IF to add more support 
            SetParentAndTransorm(ref RightHandEquipObj, RightHandTrans);
            SetParentAndTransorm(ref LeftHandEquipObj, LeftHandTrans);
            animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), 1);
            animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), 1);
        } else {
            //future support here
        }
        isWeaponOut = true;
    }
    void SheathObj () {
        if (bothHandsStatus == BothHandsStatus.DualSwords) { //Currently only dual swords supported, add ELSE IF to add more support 
            SetParentAndTransorm(ref RightHandEquipObj, leftHipSlot);
            SetParentAndTransorm(ref LeftHandEquipObj, rightHipSlot);
        } else {
            //future support here
        }
    }

    public void EnableTrails() {
        LeftHandEquipObj.GetComponentInChildren<ParticleSystem>().Play();
        RightHandEquipObj.GetComponentInChildren<ParticleSystem>().Play();
    }
    public void DisableTrails() {
        LeftHandEquipObj.GetComponentInChildren<ParticleSystem>().Stop();
        RightHandEquipObj.GetComponentInChildren<ParticleSystem>().Stop();
    }

    void CheckHandsEquip() {
        //Check weapons
        Weapon r = (Weapon)EquipmentManager.instance.mainHand.itemInSlot;
        Weapon l = (Weapon)EquipmentManager.instance.secondaryHand.itemInSlot;

        //Assign single hand status
        if (RightHandEquipObj != null) {
            if (r.weaponType == WeaponType.OneHanded) {
                rightHandStatus = SingleHandStatus.OneHanded;
            } else if (r.weaponType == WeaponType.TwoHanded) {
                rightHandStatus = SingleHandStatus.TwoHanded;
            }
        } else {
            rightHandStatus = SingleHandStatus.Empty;
        }
        if (LeftHandEquipObj != null) {
            if (l.weaponType == WeaponType.OneHanded) {
                leftHandStatus = SingleHandStatus.OneHanded;
            } else if (l.weaponType == WeaponType.Shield) {
                leftHandStatus = SingleHandStatus.Shield;
            }
        } else {
            leftHandStatus = SingleHandStatus.Empty;
        }

        //Assign total status
        if (rightHandStatus == SingleHandStatus.Empty && leftHandStatus == SingleHandStatus.Empty) {
            bothHandsStatus = BothHandsStatus.AllEmpty;
        } else if (rightHandStatus == SingleHandStatus.OneHanded && leftHandStatus == SingleHandStatus.Empty) {
            bothHandsStatus = BothHandsStatus.RightHandSwordOnly;
        } else if (rightHandStatus == SingleHandStatus.OneHanded && leftHandStatus == SingleHandStatus.OneHanded) {
            bothHandsStatus = BothHandsStatus.DualSwords;
        } else if (rightHandStatus == SingleHandStatus.TwoHanded && leftHandStatus == SingleHandStatus.Empty) {
            bothHandsStatus = BothHandsStatus.TwoHandedWeapon;
        } else if (rightHandStatus == SingleHandStatus.Empty && leftHandStatus == SingleHandStatus.OneHanded) {
            bothHandsStatus = BothHandsStatus.LeftHandSwordOnly;
        } else if (rightHandStatus == SingleHandStatus.OneHanded && leftHandStatus == SingleHandStatus.Shield) {
            bothHandsStatus = BothHandsStatus.SwordShield;
        }  
    }

    void SetParentAndTransorm (ref GameObject obj, Transform parent) {
        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localEulerAngles = Vector3.zero;
    }
}
