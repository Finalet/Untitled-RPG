using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Skill : MonoBehaviour
{
    public int ID;
    [Space]
    public string skillName;
    public string description;
    public Sprite icon;
    [Tooltip("From what tree the skill is")] public SkillTree skillTree; 
    public SkillType skillType; 
    public int staminaRequired; 
    [Tooltip("Base damage without player's characteristics")] public int baseDamage;
    [Tooltip("Damage after applying player's characteristics")] protected int actualDamage;
    public float coolDown;
    public float coolDownTimer;
    public bool isCoolingDown;

    [Header("Timings")]
    [Tooltip("Time needed to prepare and attack")] public float castingTime; public float nomralizedCastingAnim;
    [Tooltip("Total attack time, excluding casting (enemy can get hit during attackTime*(1-attackTimeOffset)")] public float totalAttackTime;
    public bool finishedCast;


    protected PlayerControlls playerControlls;
    protected Animator animator;
    protected Characteristics characteristics;
    protected AudioSource audioSource;

    public enum SkillTree {Knight, Hunter, Mage, Agnel, Stealth, Shield, Summoner };
    public enum SkillType {Damaging, Healing, Buff };

    [Header("AOE Area Picking")]
    public bool needToPickArea;
    public GameObject areaPickerPrefab;
    GameObject instanciatedAP;
    public float areaPickerSize;
    protected Vector3 pickedPosition;

    protected virtual void Start() {
        audioSource = GetComponent<AudioSource>();
        playerControlls = PlayerControlls.instance.GetComponent<PlayerControlls>();
        animator = PlayerControlls.instance.GetComponent<Animator>();
        characteristics = PlayerControlls.instance.GetComponent<Characteristics>();
    }

    public virtual void Use() { //Virtual, because sometimes need to be overriden, for instance in the Target skill.
        if (playerControlls.GetComponent<Characteristics>().Stamina < staminaRequired) {
            CanvasScript.instance.DisplayWarning("Not enough stamina!");
            return;
        }
        if (!canBeUsed() || isCoolingDown || playerControlls.isRolling || playerControlls.isGettingHit)
            return;

        StartCoroutine(StartUse());
    }

    protected virtual IEnumerator StartUse () {
        if (needToPickArea) {
            playerControlls.isPickingArea = true;
            while (playerControlls.isPickingArea) {
                PickArea();
                yield return null;
            }
        }
        if (castingTime != 0)
            StartCoroutine(UseCoroutine());
        else 
            LocalUse();
    }

    protected virtual IEnumerator UseCoroutine (){
        if (playerControlls.isCastingSkill || !playerControlls.isIdle)
            yield break;

        CastingAnim();
        playerControlls.isCastingSkill = true;
        finishedCast = false;
        CanvasScript.instance.DisplayCastingBar(nomralizedCastingAnim);
        while (!finishedCast) {
            if (playerControlls.castInterrupted) { 
                InterruptCasting();
                playerControlls.castInterrupted = false;
                yield break;
            }
            yield return null;
        }
        playerControlls.isCastingSkill = false;
        LocalUse();
    }

    protected virtual void LocalUse () {
        playerControlls.InterruptCasting();
        coolDownTimer = coolDown;
        //playerControlls.isAttacking = true;
        playerControlls.GetComponent<Characteristics>().UseOrRestoreStamina(staminaRequired);
        CustomUse();
    }

    protected virtual void CastingAnim () {}
    protected virtual void InterruptCasting () {}

    protected virtual float actualDistance() {
        return 0;
    }

    protected virtual void PickArea () {

        RaycastHit hit;
        Vector3 pickPosition;
        if (Physics.Raycast(playerControlls.playerCamera.transform.position, playerControlls.playerCamera.transform.forward, out hit, actualDistance())) {
            pickPosition = hit.point + hit.normal * 0.2f;
        } else if (Physics.Raycast(playerControlls.playerCamera.transform.position + playerControlls.playerCamera.transform.forward * actualDistance(), Vector3.down, out hit, 10)) {
            pickPosition = hit.point + hit.normal * 0.2f;
        } else {
            pickPosition = playerControlls.transform.position + playerControlls.transform.forward * 10;
        }

        if (instanciatedAP == null) instanciatedAP = Instantiate(areaPickerPrefab, pickPosition, Quaternion.identity);
        instanciatedAP.transform.position = pickPosition;
        instanciatedAP.transform.localScale = Vector3.one * areaPickerSize / 10f;

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            playerControlls.isPickingArea = false;
            pickedPosition = instanciatedAP.transform.position;
            instanciatedAP.transform.localPosition = Vector3.zero;
            Destroy(instanciatedAP);
        }
    }

    protected abstract void CustomUse(); // Custom code that is overriden in each skill seperately.

    protected virtual void Update() {
        if (coolDownTimer >= 0) {
            coolDownTimer -= Time.deltaTime;
            isCoolingDown = true;
        } else {
            isCoolingDown = false;
        }
    }

    public virtual bool canBeUsed () {
        if (playerControlls.isMounted || playerControlls.isPickingArea)
            return false;

        if (skillTree == SkillTree.Knight) {
            if (playerControlls.isWeaponOut)
                return true;
            else 
                return false;
        } else if (skillTree == SkillTree.Mage) {
            return true;
        } else {
            return false;
        }
    }

    protected virtual void PlaySound(AudioClip clip, float timeOffest, float pitch) {
        audioSource.clip = clip;
        audioSource.time = timeOffest;
        audioSource.pitch = pitch;
        audioSource.Play();
    }
}
