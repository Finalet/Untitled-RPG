using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum HitType {Normal, Interrupt, Kickback, Knockdown};

public abstract class Skill : MonoBehaviour
{
    public int ID;
    [Space]
    public string skillName;
    public Sprite icon;
    [Tooltip("From what tree the skill is")] public SkillTree skillTree; 
    public SkillType skillType; 
    public DamageType damageType;
    public int staminaRequired; 
    [Tooltip("Damage in % from players attack")] public int baseDamagePercentage;
    public float coolDown;
    public float coolDownTimer;
    [DisplayWithoutEdit] public bool isCoolingDown;
    public bool weaponOutRequired;
    public bool bowOutRequired;

    [Header("Timings")]
    [Tooltip("Time needed to prepare and attack")] public float castingTime; 
    [Range(0, 1)] public float nomralizedCastingAnim;
    public bool finishedCast;

    protected PlayerControlls playerControlls;
    protected Animator animator;
    protected Characteristics characteristics;
    protected AudioSource audioSource;

    [Header("AOE Area Picking")]
    public bool needToPickArea;
    public GameObject areaPickerPrefab;
    public float areaPickerSize;
    protected Vector3 pickedPosition;
    protected GameObject instanciatedAP;
    bool skillCanceled;

    protected virtual void Start() {
        audioSource = GetComponent<AudioSource>();
        playerControlls = PlayerControlls.instance.GetComponent<PlayerControlls>();
        animator = PlayerControlls.instance.GetComponent<Animator>();
        characteristics = PlayerControlls.instance.GetComponent<Characteristics>();
    }

    public virtual void Use() {
        if (playerControlls.GetComponent<Characteristics>().stamina < staminaRequired) {
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
                yield return new WaitForFixedUpdate();
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

        UnsheatheWeapons();

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

    void UnsheatheWeapons() {
        if (weaponOutRequired && !WeaponsController.instance.isWeaponOut)
                WeaponsController.instance.InstantUnsheathe();
        if (bowOutRequired && !WeaponsController.instance.isBowOut)
                WeaponsController.instance.InstantUnsheatheBow();
    }

    protected virtual void LocalUse () {
        playerControlls.InterruptCasting();
        coolDownTimer = coolDown;
        //playerControlls.GetComponent<Characteristics>().UseOrRestoreStamina(staminaRequired);
        CustomUse();
        UnsheatheWeapons();
        playerControlls.isAttacking = true;
    }

    protected virtual void CastingAnim () {}
    protected virtual void InterruptCasting () {
        StopSounds();
        playerControlls.playerCamera.GetComponent<CameraControll>().isAiming = false;
        playerControlls.playerCamera.GetComponent<CameraControll>().isShortAiming = false;
    }

    protected virtual float actualDistance() { //this distance is for picking area
        Debug.LogError($"\"Actual distance\" for {skillName} not set");
        return 0; //Specified in the skill itself. If 0 then something is wrong
    }

    protected virtual void PickArea () {
        if (instanciatedAP == null) {
            instanciatedAP = Instantiate(areaPickerPrefab);
            CanvasScript.instance.PickAreaForSkill(this);
        }

        RaycastHit hit;
        Vector3 pickPosition;
        int layerMask =~ LayerMask.GetMask("Enemy", "Player");
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
    }

    public void ConfirmPickArea(){
        playerControlls.isPickingArea = false;
        pickedPosition = instanciatedAP.transform.position;
        Destroy(instanciatedAP);
    }

    public void CancelPickingArea () {
        playerControlls.isPickingArea = false;
        Destroy(instanciatedAP);
        skillCanceled = true;
    }

    protected abstract void CustomUse(); // Custom code that is overriden in each skill seperately.

    protected virtual void Update() {
        if (PeaceCanvas.instance.isGamePaused)
            return;

        if (coolDownTimer >= 0) {
            coolDownTimer -= Time.deltaTime;
            isCoolingDown = true;
        } else {
            isCoolingDown = false;
        }
    }

    public virtual bool skillActive () {
        if (playerControlls.isMounted || playerControlls.isPickingArea || PeaceCanvas.instance.anyPanelOpen || Combat.instanace.blockSkills || playerControlls.isSitting)
            return false;
        else 
            return true;
    }

    protected virtual void PlaySound(AudioClip clip, float timeOffest = 0, float pitch = 1, float delay = 0, float volume = 1) {
        audioSource.clip = clip;
        audioSource.time = timeOffest;
        audioSource.pitch = pitch;
        audioSource.volume = volume;
        if (delay == 0)
            audioSource.Play();
        else
            audioSource.PlayDelayed(delay);
    }

    protected virtual void StopSounds () {
        audioSource.Stop();
    }

    public virtual string getDescription() {return "";}

    public virtual void OverrideLMBAction(){}
    public virtual void OverrideLMBicon(UI_MiddleSkillPanelButtons slot){}
}

