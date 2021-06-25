using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

//public enum BothHandsStatus {RightHandSwordOnly, RightHandStaffOnly, RightStaffLeftSword, RightStaffLeftShield, LeftHandSwordOnly, DualSwords, SwordShield, ShieldOnly, TwoHandedSword, TwoHandedStaff, AllEmpty};
//public enum SingleHandStatus {OneHandedSword, OneHandedStaff, TwoHandedSword, TwoHandedStaff, Shield, Empty};

public enum BothHandsStatus {TwoHanded, DualOneHanded, LeftOneHanded, RightOneHanded, OneHandedPlusShield, ShieldOnly, BothEmpty};
public enum SingleHandStatus {OneHanded, TwoHanded, Shield, Empty};

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
    public SingleHandStatus leftHandStatus;
    public SingleHandStatus rightHandStatus;
    public Transform LeftHandTrans;
    public Transform RightHandTrans;
    public Transform LeftHandShieldTrans;
    [Header("Slots on root")]
    public Transform leftHipSlot;
    public Transform rightHipSlot;
    public Transform twohandedStaffSlot;
    public Transform twohandedSwordSlot;
    public Transform bowSlot;
    public Transform shieldBackSlot;
    [Header("Sounds")]
    public AudioClip unsheathSword;
    public AudioClip sheathSword;
    public AudioClip unsheathBow;
    public AudioClip sheathBow;
    AudioSource audioSource;

    [Space]
    [DisplayWithoutEdit] public bool sheathingUnsheathing;
    bool blockSheathe;

    void Awake() {
        if (instance == null)
            instance = this;

        audioSource = GetComponent<AudioSource>();
    }

    void Start () {
        animator = GetComponent<Animator>();
    }

    void Update() {
        CheckHandsEquip();
        ReleaseEmptyHandAnim();

        //Manual Sheath
        if (Input.GetKeyDown(KeybindsManager.instance.sheathe) && (isWeaponOut || isBowOut) ) {
            StartCoroutine(Sheathe());
        }
    }

    public IEnumerator UnSheathe () { //LEGACY not using it anywhere for now
        blockSheathe = true; //So that you cant immediately sheathe after unsheathing;
        
        sheathingUnsheathing = true;
        animator.SetTrigger("UnSheath");
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

        PlaySheathSounds();
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
        if (bothHandsStatus == BothHandsStatus.DualOneHanded) { //Currently not all weapons types supported, add ELSE IF to add more support 
            SetParentAndTransorm(ref RightHandEquipObj, RightHandTrans);
            SetParentAndTransorm(ref LeftHandEquipObj, LeftHandTrans);
            animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), 1);
            animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), 1);
        } else if (bothHandsStatus == BothHandsStatus.TwoHanded) {
            SetParentAndTransorm(ref RightHandEquipObj, RightHandTrans);
            animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), 1);
        } else if (bothHandsStatus == BothHandsStatus.OneHandedPlusShield) {
            SetParentAndTransorm(ref RightHandEquipObj, RightHandTrans);
            SetParentAndTransorm(ref LeftHandEquipObj, LeftHandShieldTrans);
            animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), 1);
            animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), 1);
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
        
        if (bothHandsStatus == BothHandsStatus.DualOneHanded) {
            SetParentAndTransorm(ref RightHandEquipObj, leftHipSlot);
            SetParentAndTransorm(ref LeftHandEquipObj, rightHipSlot);
        } else if (bothHandsStatus == BothHandsStatus.TwoHanded) {
            Weapon w = (Weapon)EquipmentManager.instance.mainHand.itemInSlot;
            if (w.weaponType == WeaponType.TwoHandedSword)
                SetParentAndTransorm(ref RightHandEquipObj, twohandedSwordSlot);
            else if (w.weaponType == WeaponType.TwoHandedStaff)
                SetParentAndTransorm(ref RightHandEquipObj, twohandedStaffSlot);
        } if (bothHandsStatus == BothHandsStatus.OneHandedPlusShield) {
            SetParentAndTransorm(ref RightHandEquipObj, leftHipSlot);
            SetParentAndTransorm(ref LeftHandEquipObj, shieldBackSlot);
        } else {
            Debug.LogError("Sheathing for this type of weapon is not implemented yet");
        }
        isWeaponOut = false;
    }

    public void EnableTrails() {
        if (LeftHandEquipObj != null) {
            Weapon w = (Weapon)EquipmentManager.instance.secondaryHand.itemInSlot;
            if (w.weaponType == WeaponType.OneHandedSword || w.weaponType == WeaponType.TwoHandedSword)
                LeftHandEquipObj.GetComponentInChildren<ParticleSystem>().Play();
        }
        if (RightHandEquipObj != null) {
            Weapon w = (Weapon)EquipmentManager.instance.mainHand.itemInSlot;
            if (w.weaponType == WeaponType.OneHandedSword || w.weaponType == WeaponType.TwoHandedSword)            
                RightHandEquipObj.GetComponentInChildren<ParticleSystem>().Play();
        }
    }
    public void DisableTrails() {
        if (LeftHandEquipObj != null){
            Weapon w = (Weapon)EquipmentManager.instance.secondaryHand.itemInSlot;
            if (w.weaponType == WeaponType.OneHandedSword || w.weaponType == WeaponType.TwoHandedSword)
                LeftHandEquipObj.GetComponentInChildren<ParticleSystem>().Stop();
        }
        if (RightHandEquipObj != null){
            Weapon w = (Weapon)EquipmentManager.instance.mainHand.itemInSlot;
            if (w.weaponType == WeaponType.OneHandedSword || w.weaponType == WeaponType.TwoHandedSword)
                RightHandEquipObj.GetComponentInChildren<ParticleSystem>().Stop();
        }
    }

    void CheckHandsEquip() {
        //Check weapons
        Weapon r = (Weapon)EquipmentManager.instance.mainHand.itemInSlot;
        Weapon l = (Weapon)EquipmentManager.instance.secondaryHand.itemInSlot;
        int animatorWeaponType;

        //Assign single hand status
        if (RightHandEquipObj != null && r != null) {
            if (r.weaponType == WeaponType.OneHandedSword || r.weaponType == WeaponType.OneHandedStaff) {
                rightHandStatus = SingleHandStatus.OneHanded;
            } else if (r.weaponType == WeaponType.TwoHandedSword || r.weaponType == WeaponType.TwoHandedStaff) {
                rightHandStatus = SingleHandStatus.TwoHanded;
            }
        } else {
            rightHandStatus = SingleHandStatus.Empty;
        }
        if (LeftHandEquipObj != null && l != null) {
            if (l.weaponType == WeaponType.OneHandedSword || l.weaponType == WeaponType.OneHandedStaff) {
                leftHandStatus = SingleHandStatus.OneHanded;
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

        //Assign both hands status
        if (rightHandStatus == SingleHandStatus.Empty && leftHandStatus == SingleHandStatus.Empty) {
            bothHandsStatus = BothHandsStatus.BothEmpty;
            animatorWeaponType = -1;
        } else if (rightHandStatus == SingleHandStatus.OneHanded && leftHandStatus == SingleHandStatus.Empty) {
            bothHandsStatus = BothHandsStatus.RightOneHanded;
            animatorWeaponType = -1; //FIX LATER
        } else if (rightHandStatus == SingleHandStatus.OneHanded && leftHandStatus == SingleHandStatus.OneHanded) {
            bothHandsStatus = BothHandsStatus.DualOneHanded;
            animatorWeaponType = 0;
        } else if (rightHandStatus == SingleHandStatus.TwoHanded && leftHandStatus == SingleHandStatus.Empty) {
            bothHandsStatus = BothHandsStatus.TwoHanded;
            animatorWeaponType = 1;
        } else if (rightHandStatus == SingleHandStatus.Empty && leftHandStatus == SingleHandStatus.OneHanded) {
            bothHandsStatus = BothHandsStatus.LeftOneHanded;
            animatorWeaponType = -1; //FIX LATER
        } else if (rightHandStatus == SingleHandStatus.OneHanded && leftHandStatus == SingleHandStatus.Shield) {
            bothHandsStatus = BothHandsStatus.OneHandedPlusShield;
            animatorWeaponType = 0; //FIX THIS, WE NEED TO PLAY SPECIFIC ANIM FOR SHIELD AND SWORD;
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

    void UnblockSheath () {
        blockSheathe = false;
    }

    void PlaySounds (AudioClip clip) {
        audioSource.clip = clip;
        audioSource.Play();
    }
    void PlaySheathSounds () {
        if (isBowOut) {
            PlaySounds(sheathBow);
        } else {
            PlaySounds(sheathSword);
        }
    }
}
