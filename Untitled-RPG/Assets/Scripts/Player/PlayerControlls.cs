using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.HAP;
using Cinemachine;
using ECM.Controllers;

public class PlayerControlls : MonoBehaviour
{
    public static PlayerControlls instance;

    [Header("Movement")]
    public float baseWalkSpeed = 1;
    float walkSpeed;
    public bool toggleRunning = false;
    public int staminaReqToRoll = 50;

    [Header("State")]
    public bool isIdle;
    public bool isRunning;
    public bool isSprinting;
    public bool isCrouch;
    public bool isJumping;
    public bool isRolling;
    public bool isMounted;
    public bool isAttacking;
    public bool isGettingHit;
    public bool isGrounded;
    public bool isCastingSkill;
    public bool isFlying;
    public bool isPickingArea;
    public bool isAimingSkill;
    [Header("Battle")]
    public bool inBattle;
    public float inBattleExitTime = 7; //How long after exisiting the battle will the player remain "in battle"
    float inBattleTimer;
    public bool attackedByEnemies;

    [Space]
    public bool castInterrupted;
    [Space]
    public bool disableControl;

    float sprintingDirection;
    float desiredSprintingDirection;

    [System.NonSerialized] public float desiredLookDirection; //Accessed by M_Rider fix rotation when on animal
    [System.NonSerialized] public float lookDirection; 

    float rollDirection;
    float desiredRollDirection;

    [System.NonSerialized] public Rigidbody rb;
    [System.NonSerialized] public Camera playerCamera;
    [System.NonSerialized] public CameraControll cameraControl;
    [System.NonSerialized] public CinemachineFreeLook CM_Camera;
    [System.NonSerialized] public PlayerAudioController audioController;
    [System.NonSerialized] public BaseCharacterController baseCharacterController;

    [Space]
    public bool forceRigidbodyMovement; //when TRUE rootAnimations will be ignored;

    Vector3 lastCamRotation;

    [Header("Bodypart roots")]
    public Transform leftFootRoot;
    public Transform rightFootRoot;
    public Transform leftHandRoot;
    public Transform rightHandRoot;
    public Transform leftHandWeaponSlot;
    public Transform rightHandWeaponSlot;
    public ParticleSystem sprintTrails;
    public SkinnedMeshRenderer skinnedMesh;
    [System.NonSerialized] public Animator animator;

    bool rolled;

    public bool[] emptyAttackAnimatorStates = new bool[7];

    void Awake() {
        if (instance == null) 
            instance = this;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        audioController = GetComponent<PlayerAudioController>();
        baseCharacterController = GetComponent<BaseCharacterController>();
        rb = GetComponent<Rigidbody>();

        playerCamera = Camera.main;
        CM_Camera = playerCamera.GetComponent<CameraControll>().CM_cam;
        cameraControl = playerCamera.GetComponent<CameraControll>();
        walkSpeed = baseWalkSpeed;
    }

    void Update()
    {
        isGrounded = baseCharacterController.isGrounded; //IsGrounded(); LEGACY
        isJumping = baseCharacterController.isJumping;

        if (!isMounted && !isFlying && !disableControl) {
            GroundMovement();
            GroundAnimations();
        } else if (isFlying && !disableControl) {
            FlyingAnimations();
        }
        UpdateRotation();
        Sprinting();
        MountAnimal();
        SetAnimatorVarialbes();
        InputMagnitudeFunc();
        CheckIsAttacking();
        InBattleCheck();

        if (Input.GetKeyDown(KeyCode.CapsLock))
            toggleRunning = !toggleRunning;

        if (!Input.GetButton("Horizontal") && !Input.GetButton("Vertical") && !isAttacking) {
            isIdle = true;
        } else {
            isIdle = false;
        }

        //Smooth rotation after rolling
        if (Mathf.Abs(rollDirection - desiredRollDirection) > 1) {
            rollDirection = Mathf.Lerp(rollDirection, desiredRollDirection, Time.deltaTime * 15);
        } else {
            rollDirection = desiredRollDirection;
        }

        //Smooth rotation sprinting
        //LerpAngle since need to go from -179 to 179 smoothely
        if (Mathf.Abs(sprintingDirection - desiredSprintingDirection) > 1) {
            sprintingDirection = Mathf.LerpAngle(sprintingDirection, desiredSprintingDirection, Time.deltaTime * 15);
        } else {
            sprintingDirection = desiredSprintingDirection;
        }

        //Smooth rotation after idle
        if (Mathf.Abs(lookDirection - desiredLookDirection) > 1) {
            lookDirection = Mathf.LerpAngle(lookDirection, desiredLookDirection, Time.deltaTime * 15);
        } else {
            lookDirection = desiredLookDirection;
        }

    }

#region Ground movement
    float runningStaminaTimer;
    void GroundMovement()
    {
        //if (Input.GetButtonDown("Jump"))  //JUMPING IS NOW FONE THROUGH "BASE CHARACTER CONTROLLER"
            //Jump();

        if (Input.GetButtonDown("Crouch"))
            Crouch();

        if (Input.GetButtonDown("Roll"))
            Roll();

        if (!isIdle) {
            if (Input.GetButton("Run"))
            {
                if (!toggleRunning)
                    isRunning = true;
                else if (!isCrouch && !isAttacking && !isRolling)
                    isSprinting = true; //Start sprinting
            } else if (Input.GetButtonUp("Run"))
            {
                if (toggleRunning && !isRolling) {
                    isSprinting = false;
                } else {
                    isRunning = false;
                }
            } else {
                if (!isRolling) {
                    isSprinting = false;
                }
                if (toggleRunning){
                    isRunning = true;
                } else {
                    isRunning = false;
                }
            }
        } else {
            if (!isRolling)
                isSprinting = false;
        }

        if( (Characteristics.instance.stamina <= 0 || !Characteristics.instance.canUseStamina) && !isRolling) {
            isSprinting = false;
        }
 
        if (Input.GetButtonDown("FreeCamera")) {
            lastCamRotation = playerCamera.transform.eulerAngles;
        } else if (Input.GetButtonUp("FreeCamera")) {
            playerCamera.transform.eulerAngles = lastCamRotation;
        }

        if (isSprinting) {
            if (InputDirection > 0) {
                //sprintingDirection = InputDirection;
                desiredSprintingDirection = InputDirection;
            } else {
                //sprintingDirection = 360 + InputDirection;
                desiredSprintingDirection =  InputDirection;
            }

            if (Time.time - runningStaminaTimer >= 0.02f) {
                Characteristics.instance.UseOrRestoreStamina(1);
                runningStaminaTimer = Time.time;
            }
        } else {
            // sprintingDirection = 0;
            desiredSprintingDirection = 0;
        }

        if (isAttacking) {
            SprintOff();
        }
    }

    void UpdateRotation () {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, lookDirection + sprintingDirection + rollDirection, transform.eulerAngles.z);
        
        if ( (!PeaceCanvas.instance.anyPanelOpen && !isIdle) || cameraControl.isAiming || cameraControl.isShortAiming) { 
            desiredLookDirection = CM_Camera.m_XAxis.Value; //rotate player with camera, unless idle, aiming, or in inventory
        }
    }


    //Animation variables
    float InputDirection;
    float lastInputDirection;
    float InputMagnitude;
    float isRunningFloat;
    float isSprintingFloat;
    
    void GroundAnimations() {
        if ( (InputDirection == 90 || InputDirection == -90) && isRunning && !isSprinting)
            walkSpeed = baseWalkSpeed * 1.2f;
        else 
            walkSpeed = baseWalkSpeed;

        if (!isIdle) {
            InputDirection = Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * Mathf.Rad2Deg;
            lastInputDirection = InputDirection;

            CheckWhichFootIsUp();

        } else {
            InputDirection = lastInputDirection;
        }


        if (isRunning) {
            isRunningFloat += Time.deltaTime * 5;
        } else {
            isRunningFloat -= Time.deltaTime * 5;
        }
        isRunningFloat = Mathf.Clamp01(isRunningFloat);

        if (isSprinting) {
            isSprintingFloat = 1;
        } else {
            isSprintingFloat = 0;
        }
    }

    void InputMagnitudeFunc () {
        if (disableControl){
            InputMagnitude = 0;
            return;
        }

        if (Input.GetButton("Horizontal") || Input.GetButton("Vertical")) {
            InputMagnitude = 1;
            InterruptCasting();
        } else {
            InputMagnitude = 0;
        }
    }

    void CheckWhichFootIsUp() {
        if (leftFootRoot.position.y < rightFootRoot.position.y) {
            animator.SetBool("IsRightLegUp", true);
        } else {
            animator.SetBool("IsRightLegUp", false);
        }
    }
    
    float jumpDis;
    public bool canJump() {
        if (!isGrounded || isFlying || isCrouch || isRolling || isAttacking || isGettingHit || isJumping) {
            return false;
        } else {
            return true;
        }
    }
    public void Jump ()
    {
        audioController.PlayJumpRollSound();
        animator.SetTrigger("Jump");
        
        InterruptCasting();
    }
    void Crouch() {
        if (isGrounded)
            isCrouch = !isCrouch;
        
        InterruptCasting();
    }

    void Roll() {
        if (isAttacking || isRolling || isFlying) {
            return;
        }
        if (!Characteristics.instance.canUseStamina) {
            CanvasScript.instance.DisplayWarning("Not enough stamina!");
            return;
        }

        GetComponent<Characteristics>().UseOrRestoreStamina(staminaReqToRoll);
        isRolling = true;
        if (InputDirection > 0 && !isSprinting && !isIdle) {
            desiredRollDirection = InputDirection;
        } else if (!isSprinting && !isIdle) {
            desiredRollDirection = InputDirection;
        } 
        UpdateRotation();
        animator.SetTrigger("Roll");
        audioController.PlayJumpRollSound();

        InterruptCasting();
    }
    public void StopRoll() { //Called from rolling animation
        isRolling = false;
        desiredRollDirection = 0;
    }

    void Sprinting () {
        if (disableControl)
            isSprinting = false;

        if (isSprinting && !sprintTrails.isPlaying)
            sprintTrails.Play();
        else if (!isSprinting && sprintTrails.isPlaying) 
            sprintTrails.Stop();
    }
    public void SprintOff () {
        isSprinting = false;
    }

    void MountAnimal () {
        if (Input.GetKeyDown(KeyCode.F)) {
            if (!isMounted && !isFlying) {
                GetComponent<MRider>().MountAnimal();
                SprintOff();
            }
            else {
                GetComponent<MRider>().DismountAnimal();
            } 
        }
    }

#endregion

#region Flying movement

    void FlyingMovement() {
        UpdateRotation();
    }

    void FlyingAnimations () {
        if (!isIdle) {
            InputDirection = Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * Mathf.Rad2Deg;
            lastInputDirection = InputDirection;
        } else {
            InputDirection = lastInputDirection;
        }
    }

    public void TakeOff () {
        isCrouch = false;

        animator.CrossFade("Main.Flying.Takeoff", 0.1f);
        baseCharacterController.SwitchRootAnimation(false);
        isFlying = true;
    }
    public void LandFromFlying () {
        animator.CrossFade("Main.Flying.Land", 0.1f);
    }
    public void FlyUp () { //Called from take off animation
        baseCharacterController.allowVerticalMovement = true;
        baseCharacterController.movement.DisableGroundDetection();
        baseCharacterController.movement.ApplyVerticalImpulse(10);
        SprintOff();
    }
    public void LandDown () { //Called from landing animation
        baseCharacterController.allowVerticalMovement = false;
        baseCharacterController.movement.EnableGroundDetection();
        baseCharacterController.SwitchRootAnimation(true);
        isFlying = false;
    }


#endregion
    
    void SetAnimatorVarialbes () {
        animator.SetFloat("InputMagnitude", InputMagnitude);    
        animator.SetFloat("InputDirection", InputDirection);
        animator.SetFloat("isSprinting", isSprintingFloat);
        animator.SetFloat("isRunning", isRunningFloat);
        animator.SetFloat("WalkSpeed", walkSpeed);
        animator.SetFloat("AttackSpeed", Characteristics.instance.attackSpeed.x);
        animator.SetFloat("CastingSpeed", Characteristics.instance.castingSpeed.x);
        
        animator.SetBool("Idle", isIdle);
        animator.SetBool("isCrouch", isCrouch);
        animator.SetBool("isGrounded", isGrounded);
    }
    
    public void InterruptCasting () {
        if (!isCastingSkill)
            return;
        animator.CrossFade("Attacks.Mage.Empty", 0.15f);
        castInterrupted = true;
        isCastingSkill = false;
    }

    void CheckIsAttacking () {
        if ( (emptyAttackAnimatorStates[0] || emptyAttackAnimatorStates[1] || emptyAttackAnimatorStates[6]) && (emptyAttackAnimatorStates[2] || emptyAttackAnimatorStates[3]) ) {
            isAttacking = false;
            isCastingSkill = false;
        }

        if (emptyAttackAnimatorStates[4] && emptyAttackAnimatorStates[5]) {
            isAttacking = false;
            isCastingSkill = false;
        }

        if ( emptyAttackAnimatorStates[5] && (emptyAttackAnimatorStates[0] || emptyAttackAnimatorStates[1]  || emptyAttackAnimatorStates[6]) ) {
            isAttacking = false;
            isCastingSkill = false;
        }

        if ( emptyAttackAnimatorStates[4] && (emptyAttackAnimatorStates[2] || emptyAttackAnimatorStates[3]) ) {
            isAttacking = false;
            isCastingSkill = false;
        }
    }

    void InBattleCheck () {
        if (isAttacking || isGettingHit || attackedByEnemies || cameraControl.isAiming || cameraControl.isShortAiming) {
            inBattle = true;
            inBattleTimer = Time.time;
        } else if (Time.time - inBattleTimer >= inBattleExitTime && !WeaponsController.instance.sheathingUnsheathing) {
            if (WeaponsController.instance.isWeaponOut || WeaponsController.instance.isBowOut) {
                StartCoroutine(WeaponsController.instance.Sheathe());
            }
            inBattle = false;
        }
    }

    public void PlayGeneralAnimation (int animationID) {
        animator.SetInteger("GeneralID", animationID);
        animator.SetTrigger("GeneralTrigger");
    }
}
