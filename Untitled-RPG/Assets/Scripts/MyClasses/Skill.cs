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
    public float coolDown;
    public float coolDownTimer;
    public bool isCoolingDown;

    [Header("Timings")]
    [Tooltip("Time needed to prepare and attack")] public float castingTime; 
    [Range(0, 1)] public float nomralizedCastingAnim;
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
    public float areaPickerSize;
    protected Vector3 pickedPosition;
    GameObject instanciatedAP;
    bool skillCanceled;

    protected virtual void Start() {
        audioSource = GetComponent<AudioSource>();
        playerControlls = PlayerControlls.instance.GetComponent<PlayerControlls>();
        animator = PlayerControlls.instance.GetComponent<Animator>();
        characteristics = PlayerControlls.instance.GetComponent<Characteristics>();
    }

    public virtual void Use() {
        if (playerControlls.GetComponent<Characteristics>().Stamina < staminaRequired) {
            CanvasScript.instance.DisplayWarning("Not enough stamina!");
            return;
        }
        if (!skillActive() || isCoolingDown || playerControlls.isRolling || playerControlls.isGettingHit || playerControlls.isCastingSkill || playerControlls.isAttacking)
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
            if (skillCanceled) {
                skillCanceled = false;
                yield break;
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
        playerControlls.GetComponent<Characteristics>().UseOrRestoreStamina(staminaRequired);
        CustomUse();
    }

    protected virtual void CastingAnim () {}
    protected virtual void InterruptCasting () {}

    protected virtual float actualDistance() { //this distance is for picking area
        Debug.LogError($"\"Actual distance\" for {skillName} not set");
        return 0; //Specified in the skill itself. If 0 then something is wrong
    }

    protected virtual void PickArea () {
        if (instanciatedAP == null) {
            instanciatedAP = Instantiate(areaPickerPrefab);
            CanvasScript.instance.ShowSCB(this);
        }

        RaycastHit hit;
        Vector3 pickPosition;
        int layerMask =~ LayerMask.GetMask("Enemy");
        float dis = actualDistance() + playerControlls.playerCamera.GetComponent<CameraControll>().camDistance;
        if (Physics.Raycast(playerControlls.playerCamera.transform.position, playerControlls.playerCamera.transform.forward, out hit, dis, layerMask)) {
            pickPosition = hit.point + hit.normal * 0.3f;
        } else if (Physics.Raycast(playerControlls.playerCamera.transform.position + playerControlls.playerCamera.transform.forward * dis, Vector3.down, out hit, 10, layerMask)) {
            pickPosition = hit.point + hit.normal * 0.3f;
        } else {
            pickPosition = playerControlls.transform.position + playerControlls.transform.forward * 10;
        }

        instanciatedAP.transform.position = pickPosition;
        instanciatedAP.transform.localScale = Vector3.one * areaPickerSize / 10f;


        if (Input.GetKeyUp(KeyCode.Mouse0)) {
            playerControlls.isPickingArea = false;
            pickedPosition = instanciatedAP.transform.position;
            CanvasScript.instance.HideSCB();
            Destroy(instanciatedAP);
        } else if (Input.GetKeyUp(KeyCode.Mouse1)) {
            CancelPickingArea();
        }
    }

    void CancelPickingArea () {
        playerControlls.isPickingArea = false;
        CanvasScript.instance.HideSCB();
        Destroy(instanciatedAP);
        skillCanceled = true;
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

    public virtual bool skillActive () {
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

    protected virtual int damage () {
        return Mathf.RoundToInt(Random.Range(actualDamage()*0.85f, actualDamage()*1.15f));
    } 

    protected virtual int actualDamage () {
        switch (skillTree) {
            case SkillTree.Knight:
                return Mathf.RoundToInt(baseDamage * (float)characteristics.meleeAttack/100f);
            case SkillTree.Mage:
                return Mathf.RoundToInt(baseDamage * (float)characteristics.magicPower/100f);
            default: 
                Debug.LogError("Fuck you this can never happen");
                return 0;
        }
    }


    protected virtual void PlaySound(AudioClip clip) {
        audioSource.volume = 1;
        audioSource.pitch = 1;
        audioSource.time = 0;
        audioSource.clip = clip;
        audioSource.Play();
    }
    protected virtual void PlaySound(AudioClip clip, float timeOffest, float pitch) {
        audioSource.time = timeOffest;
        audioSource.pitch = pitch;
        audioSource.clip = clip;
        audioSource.Play();
    }
    protected virtual void PlaySound(AudioClip clip, float timeOffest, float pitch, float delay) {
        audioSource.clip = clip;
        audioSource.time = timeOffest;
        audioSource.pitch = pitch;
        audioSource.PlayDelayed(delay);
    }
    protected virtual void PlaySound(AudioClip clip, float timeOffest, float pitch, float delay, float volume) {
        audioSource.clip = clip;
        audioSource.time = timeOffest;
        audioSource.pitch = pitch;
        audioSource.volume = volume;
        audioSource.PlayDelayed(delay);
    }
}
