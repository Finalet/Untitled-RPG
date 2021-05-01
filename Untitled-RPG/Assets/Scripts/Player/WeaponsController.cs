using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BothHandsStatus {RightHandSwordOnly, RightHandStaffOnly, RightStaffLeftSword, RightStaffLeftShield, LeftHandSwordOnly, DualSwords, SwordShield, ShieldOnly, TwoHandedSword, TwoHandedStaff, AllEmpty};
public enum SingleHandStatus {OneHandedSword, OneHandedStaff, TwoHandedSword, TwoHandedStaff, Shield, Empty};

public class WeaponsController : MonoBehaviour
{
    public static WeaponsController instance;

    Animator animator;

    float weight;
    float overlaySpeed = 3;

    public bool isWeaponOut;
    public bool isBowOut;
    [Header("Objects")]
    public GameObject LeftHandEquipObj;
    public GameObject RightHandEquipObj;
    public GameObject BowObj;
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
    public Transform bowSlot;

    public AudioClip[] sheathSounds;

    public bool sheathingUnsheathing;
    bool blockSheathe;

    void Awake() {
        if (instance == null)
            instance = this;
    }

    void Start () {
        animator = GetComponent<Animator>();
    }

    void Update() {
        CheckHandsEquip();
        ReleaseEmptyHandAnim();

        //Manual Sheath
        if (Input.GetKeyDown(KeybindsManager.instance.sheathe) && isWeaponOut) {
            StartCoroutine(Sheathe());
        }
    }

    public IEnumerator UnSheathe () {
        blockSheathe = true; //So that you cant immediately sheathe after unsheathing;
        
        sheathingUnsheathing = true;
        animator.SetTrigger("UnSheath");
        GetComponent<AudioSource>().clip = sheathSounds[0];
        GetComponent<AudioSource>().PlayDelayed(0.3f);
        weight = 0;
        while (weight <= 1) {
            animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), weight);
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
            
            animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), weight);
            animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), weight);
                
            weight += Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        isWeaponOut = true;

        animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), 1);
        animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 1);

        animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), 1);
        animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), 1);        

        weight = 1;
        while (weight >= 0) {
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
            animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), weight);
            
            weight -= Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 0);
        animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), 0);

        sheathingUnsheathing = false;

        yield return new WaitForSeconds(2);
        blockSheathe = false;
    }
    public IEnumerator Sheathe () {
        if (blockSheathe)
            yield break;

        sheathingUnsheathing = true;
        animator.SetTrigger("Sheath"); 

        GetComponent<AudioSource>().clip = sheathSounds[1];
        GetComponent<AudioSource>().PlayDelayed(0.3f);

        weight = 0;
        while (weight <= 1) {
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
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
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
            animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), weight);

            animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), weight);
            animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), weight);
            
            weight -= Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        weight = 0;

        animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 0);
        animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), 0);

        animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), 0);
        animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), 0);
        sheathingUnsheathing = false;
    }

    public void InstantUnsheathe () {
        if (isBowOut)
            SheathObj();

        animator.SetTrigger("UnSheath"); //Triggered to play animation of closed fist
        if (bothHandsStatus == BothHandsStatus.DualSwords) { //Currently not all weapons types supported, add ELSE IF to add more support 
            SetParentAndTransorm(ref RightHandEquipObj, RightHandTrans);
            SetParentAndTransorm(ref LeftHandEquipObj, LeftHandTrans);
            animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), 1);
            animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), 1);
        } else if (bothHandsStatus == BothHandsStatus.TwoHandedSword || bothHandsStatus == BothHandsStatus.TwoHandedStaff) {
            SetParentAndTransorm(ref RightHandEquipObj, RightHandTrans);
            animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), 1);
        } else {
            Debug.LogError("Instant unsheathing for this type of weapon is not implemented yet");
        }
        isWeaponOut = true;
        
        blockSheathe = true;
        Invoke("UnblockSheath", 2);
    }
    public void InstantUnsheatheBow() {
        if (isBowOut)   
            return;

        animator.SetTrigger("UnSheath"); //Triggered to play animation of closed fist
        SetParentAndTransorm(ref BowObj, LeftHandTrans);
        animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), 1);
        SheathObj();
        isBowOut = true;
        
        blockSheathe = true;
        Invoke("UnblockSheath", 2);
    }

    void SheathObj () {
        if (isBowOut) {
            SetParentAndTransorm(ref BowObj, bowSlot);
            isBowOut = false;
            return;
        }
        
        if (bothHandsStatus == BothHandsStatus.DualSwords) {
            SetParentAndTransorm(ref RightHandEquipObj, leftHipSlot);
            SetParentAndTransorm(ref LeftHandEquipObj, rightHipSlot);
        } else if (bothHandsStatus == BothHandsStatus.TwoHandedStaff || bothHandsStatus == BothHandsStatus.TwoHandedSword) {
            SetParentAndTransorm(ref RightHandEquipObj, twohandedWeaponSlot);
        } else {
            Debug.LogError("Sheathing for this type of weapon is not implemented yet");
        }
        isWeaponOut = false;
    }

    public void EnableTrails() {
        if (LeftHandEquipObj != null) LeftHandEquipObj.GetComponentInChildren<ParticleSystem>().Play();
        if (RightHandEquipObj != null) RightHandEquipObj.GetComponentInChildren<ParticleSystem>().Play();
    }
    public void DisableTrails() {
        if (LeftHandEquipObj != null) LeftHandEquipObj.GetComponentInChildren<ParticleSystem>().Stop();
        if (RightHandEquipObj != null) RightHandEquipObj.GetComponentInChildren<ParticleSystem>().Stop();
    }

    void CheckHandsEquip() {
        //Check weapons
        Weapon r = (Weapon)EquipmentManager.instance.mainHand.itemInSlot;
        Weapon l = (Weapon)EquipmentManager.instance.secondaryHand.itemInSlot;
        int animatorWeaponType;

        //Assign single hand status
        if (RightHandEquipObj != null && r != null) {
            if (r.weaponType == WeaponType.OneHandedSword) {
                rightHandStatus = SingleHandStatus.OneHandedSword;
            } else if (r.weaponType == WeaponType.TwoHandedSword) {
                rightHandStatus = SingleHandStatus.TwoHandedSword;
            } else if (r.weaponType == WeaponType.OneHandedStaff) {
                rightHandStatus = SingleHandStatus.OneHandedStaff;
            } else if (r.weaponType == WeaponType.TwoHandedStaff) {
                rightHandStatus = SingleHandStatus.TwoHandedStaff;
            } 
        } else {
            rightHandStatus = SingleHandStatus.Empty;
        }
        if (LeftHandEquipObj != null && l != null) {
            if (l.weaponType == WeaponType.OneHandedSword) {
                leftHandStatus = SingleHandStatus.OneHandedSword;
            } else if (l.weaponType == WeaponType.Shield) {
                leftHandStatus = SingleHandStatus.Shield;
            }
        } else {
            leftHandStatus = SingleHandStatus.Empty;
        }

        //Check if bow
        if (isBowOut) {
            animatorWeaponType = 2;
            PlayerControlls.instance.animator.SetInteger("weaponType", animatorWeaponType);
            return;
        }

        //Assign total status
        if (rightHandStatus == SingleHandStatus.Empty && leftHandStatus == SingleHandStatus.Empty) {
            bothHandsStatus = BothHandsStatus.AllEmpty;
            animatorWeaponType = -1;
        } else if (rightHandStatus == SingleHandStatus.OneHandedSword && leftHandStatus == SingleHandStatus.Empty) {
            bothHandsStatus = BothHandsStatus.RightHandSwordOnly;
            animatorWeaponType = -1; //FIX LATER
        } else if (rightHandStatus == SingleHandStatus.OneHandedSword && leftHandStatus == SingleHandStatus.OneHandedSword) {
            bothHandsStatus = BothHandsStatus.DualSwords;
            animatorWeaponType = 0;
        } else if (rightHandStatus == SingleHandStatus.TwoHandedSword && leftHandStatus == SingleHandStatus.Empty) {
            bothHandsStatus = BothHandsStatus.TwoHandedSword;
            animatorWeaponType = 1;
        } else if (rightHandStatus == SingleHandStatus.TwoHandedStaff && leftHandStatus == SingleHandStatus.Empty) {
            bothHandsStatus = BothHandsStatus.TwoHandedStaff;
            animatorWeaponType = 1;
        } else if (rightHandStatus == SingleHandStatus.Empty && leftHandStatus == SingleHandStatus.OneHandedSword) {
            bothHandsStatus = BothHandsStatus.LeftHandSwordOnly;
            animatorWeaponType = -1; //FIX LATER
        } else if (rightHandStatus == SingleHandStatus.OneHandedSword && leftHandStatus == SingleHandStatus.Shield) {
            bothHandsStatus = BothHandsStatus.SwordShield;
            animatorWeaponType = -1; //FIX LATER
        } else if (rightHandStatus == SingleHandStatus.OneHandedStaff && leftHandStatus == SingleHandStatus.Empty) {
            bothHandsStatus = BothHandsStatus.RightHandStaffOnly;
            animatorWeaponType = -1; //FIX LATER
        } else if (rightHandStatus == SingleHandStatus.OneHandedStaff && leftHandStatus == SingleHandStatus.OneHandedSword) {
            bothHandsStatus = BothHandsStatus.RightStaffLeftSword;
            animatorWeaponType = -1; //FIX LATER
        } else if (rightHandStatus == SingleHandStatus.OneHandedStaff && leftHandStatus == SingleHandStatus.Shield) {
            bothHandsStatus = BothHandsStatus.RightStaffLeftShield;
            animatorWeaponType = -1; //FIX LATER
        } else if (rightHandStatus == SingleHandStatus.Empty && leftHandStatus == SingleHandStatus.Shield) {
            bothHandsStatus = BothHandsStatus.ShieldOnly;
            animatorWeaponType = -1; //FIX LATER
        } else {
            animatorWeaponType = -1; //FIX LATER
        }

        PlayerControlls.instance.animator.SetInteger("weaponType", animatorWeaponType);
    }

    void SetParentAndTransorm (ref GameObject obj, Transform parent) {
        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localEulerAngles = Vector3.zero;
        obj.transform.localScale = Vector3.one;
    }

    void ReleaseEmptyHandAnim() {
        if (sheathingUnsheathing)
            return;
            
        if (rightHandStatus == SingleHandStatus.Empty) {
            animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), 0);
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 0);
        }
        if (leftHandStatus == SingleHandStatus.Empty) {
            animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), 0);
            animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), 0);
        }
    }

    void UnblockSheath() {
        blockSheathe = false;
    }
}
