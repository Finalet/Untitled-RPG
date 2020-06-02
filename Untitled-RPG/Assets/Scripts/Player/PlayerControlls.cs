using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.HAP;

[RequireComponent(typeof(CharacterController))]
public class PlayerControlls : MonoBehaviour
{
    public static PlayerControlls instance;

    [Header("Movement")]
    public float currentSpeed = 0;
    public float baseWalkSpeed;
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
    public bool isWeaponOut;
    public bool isGrounded;
    //public bool isUsingSkill;
    public bool isCastingSkill;

    [Space]
    public bool castInterrupted;

    float sprintingDirection;
    float rollDirection;

    [Header("Jumping")]
    public float jumpHeight = 2;
    public float jumpDistance = 1.5f;
    public float gravity = -50;
    public float velocityY;
    public float fwd;
    public float independentFromInputFwd;
    public float sideways;
    Vector3 velocity;
    
    Vector3 forward;
    Vector3 side;

    [Header("Rolling")]
    public float rollDistance;    

    [System.NonSerialized] public CharacterController controller;
    [System.NonSerialized] public Camera playerCamera;
    [System.NonSerialized] public PlayerAudioController audioController;

    Vector3 lastCamRotation;

    [Header("Animations")]
    public Animator animator;
    public Transform leftFoot;
    public Transform rightFoot;
    public ParticleSystem sprintTrails;

    float baseHeight;
    float baseColliderCenterY;

    bool rolled;

    void Awake() {
        if (instance == null) 
            instance = this;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audioController = GetComponent<PlayerAudioController>();

        playerCamera = Camera.main;

        prevPos = transform.position;
        walkSpeed = baseWalkSpeed;

        baseHeight = controller.height;
        baseColliderCenterY = controller.center.y;
    }

    Vector3 prevPos;
    void CalculateCurrentSpeed () {
        currentSpeed = Vector3.Magnitude(transform.position - prevPos) * 120;
        prevPos = transform.position;
    }

    void Update()
    {

        if (!isMounted) {
            Movement();
            Animations();
        }
        Sprinting();
        MountAnimal();

        if (Input.GetKeyDown(KeyCode.CapsLock))
            toggleRunning = !toggleRunning;

        if (!Input.GetButton("Horizontal") && !Input.GetButton("Vertical") && !isAttacking) {
            isIdle = true;
        } else {
            isIdle = false;
        }

        isWeaponOut = GetComponent<WeaponsController>().isWeaponOut;
    }

    void FixedUpdate() {
        CalculateCurrentSpeed();
    }

    bool staminaRanOutFromSprinting;
    void Movement()
    {
        isGrounded = IsGrounded();

        if (Input.GetButtonDown("Jump"))
            Jump();

        if (Input.GetButtonDown("Crouch"))
            Crouch();

        if (Input.GetButtonDown("Roll"))
            Roll();

        if (!isIdle) {
            if (Input.GetButton("Run"))
            {
                if (!toggleRunning)
                    isRunning = true;
                else if (!isCrouch && !isAttacking && !isRolling && !staminaRanOutFromSprinting)
                    isSprinting = true; //Start sprinting
            } else if (Input.GetButtonUp("Run"))
            {
                if (toggleRunning && !isRolling) {
                    isSprinting = false;
                    staminaRanOutFromSprinting = false;
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

        if(GetComponent<Characteristics>().Stamina <= 0) {
            isSprinting = false;
            staminaRanOutFromSprinting = true;
        }

        if(isCrouch) {
            controller.height = 1.1f;
            controller.center = new Vector3(controller.center.x, 0.6f, controller.center.z);
        } else {
            controller.height = baseHeight;
            controller.center = new Vector3(controller.center.x, baseColliderCenterY, controller.center.z);
        }
 
        velocityY += gravity * Time.deltaTime;
        velocityY = Mathf.Clamp(velocityY, gravity * 3, -gravity * 3);

        if (isSprinting || isRolling) {
            forward = fwd * transform.forward;
            side = Vector3.zero;
        } else {
            forward = transform.forward * fwd * Input.GetAxis("Vertical") + independentFromInputFwd * transform.forward;
            side = transform.right * sideways * Input.GetAxis("Horizontal");
        }

        velocity = Vector3.up * velocityY + forward + side;
        controller.Move(velocity * Time.deltaTime); 

        if (Input.GetKeyDown(KeyCode.LeftAlt)) {
            lastCamRotation = playerCamera.transform.eulerAngles;
        } else if (Input.GetKeyUp(KeyCode.LeftAlt)) {
            playerCamera.transform.eulerAngles = lastCamRotation;
        }
            
        UpdateRotation();

        if (isSprinting) {
            if (InputDirection > 0) {
                sprintingDirection = InputDirection;
            } else {
                sprintingDirection = 360 + InputDirection;
            }
        } else {
            sprintingDirection = 0;
        }

        if (isAttacking) {
            SprintOff();
        }
    }

    void UpdateRotation () {
        if (!Input.GetKey(KeyCode.LeftAlt) && !PeaceCanvas.instance.anyPanelOpen) {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, playerCamera.transform.eulerAngles.y + sprintingDirection + rollDirection, transform.eulerAngles.z);
        } else {
            RotationDirection = 0;
        }
    }


    [Header("Animations variables")]
    float InputDirection;
    float lastInputDirection;
    float InputMagnitude;
    float RotationDirection;
    float isRunningFloat;
    float isSprintingFloat;
    
    void Animations() {
        if ( (InputDirection == 90 || InputDirection == -90) && isRunning && !isSprinting)
            walkSpeed = baseWalkSpeed * 1.2f;
        else 
            walkSpeed = baseWalkSpeed;

        if (!isIdle) {
            InputDirection = Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * Mathf.Rad2Deg;
            lastInputDirection = InputDirection;

            CheckWhichFootIsUp();
            
            animator.speed = walkSpeed;

        } else {
            InputDirection = lastInputDirection;

             if (RotationDirection > 5 || RotationDirection < -5)
                animator.speed = (1 + Mathf.Abs(RotationDirection / 100));
            else 
                animator.speed = walkSpeed;
        }

        InputMagnitudeFunc();
        animator.SetFloat("InputMagnitude", InputMagnitude);    
        animator.SetFloat("InputDirection", InputDirection);


        if (!Input.GetKey(KeyCode.LeftAlt)) {
            if (Input.GetAxis("Mouse X") == 0) {
                if (RotationDirection > 0.1f) 
                    RotationDirection -= Time.deltaTime * 50;
                else if (RotationDirection < -0.1f)
                    RotationDirection += Time.deltaTime * 50;
            } else {
                RotationDirection += Input.GetAxis("Mouse X");
            }
            RotationDirection = Mathf.Clamp(RotationDirection, -45, 45);       
        } else {
            if (RotationDirection > 0.1f) 
                RotationDirection -= Time.deltaTime * 50;
            else if (RotationDirection < -0.1f) {
                RotationDirection += Time.deltaTime * 50;
            }
        }
        animator.SetFloat("RotationDirection", RotationDirection);

        if (isRunning) {
            isRunningFloat += Time.deltaTime * 5;
        } else {
            isRunningFloat -= Time.deltaTime * 5;
        }
        isRunningFloat = Mathf.Clamp01(isRunningFloat);
        animator.SetFloat("isRunning", isRunningFloat);

        if (isSprinting) {
            isSprintingFloat = 1;
        } else {
            isSprintingFloat = 0;
        }
        animator.SetFloat("isSprinting", isSprintingFloat);

        animator.SetBool("Idle", isIdle);
        animator.SetBool("isCrouch", isCrouch);
    }

    void InputMagnitudeFunc () {
        if (Input.GetButton("Horizontal") || Input.GetButton("Vertical")) {
            InputMagnitude = 1;
            InterruptCasting();
        } else {
            InputMagnitude = 0;
        }
    }

    void CheckWhichFootIsUp() {
        if (leftFoot.transform.position.y < rightFoot.transform.position.y) {
            animator.SetBool("IsRightLegUp", true);
        } else {
            animator.SetBool("IsRightLegUp", false);
        }
    }
    
    float jumpDis;
    void Jump ()
    {
        if (!isGrounded || isCrouch || isRolling || isAttacking || isGettingHit) {
            return;
        }

        audioController.PlayJumpRollSound();
        animator.SetTrigger("Jump");
        isJumping = true;
        jumpDis = jumpDistance * Mathf.Abs(currentSpeed/3);
        velocityY = Mathf.Sqrt(-2 * gravity * jumpHeight);
        fwd += jumpDis;
        sideways += jumpDis;
        animator.applyRootMotion = false;
        StartCoroutine(DetectLanding());

        InterruptCasting();
    }

    IEnumerator DetectLanding () {
        yield return new WaitForSeconds(0.1f);
        while (!isGrounded) {
            yield return null;
        }
        fwd -= jumpDis;
        sideways -= jumpDis;
        animator.applyRootMotion = true;
        isJumping = false;
        animator.SetTrigger("Landed");
    }

    void Crouch() {
        if (isGrounded)
            isCrouch = !isCrouch;
        
        InterruptCasting();
    }

    public void SprintOff () {
        isSprinting = false;
    }

    float rollDis;
    void Roll() {
        if (isAttacking || isRolling) {
            return;
        }
        if (GetComponent<Characteristics>().Stamina < staminaReqToRoll) {
            CanvasScript.instance.DisplayWarning("Not enough stamina!");
            return;
        }

        GetComponent<Characteristics>().UseOrRestoreStamina(staminaReqToRoll);
        isRolling = true;
        if (InputDirection > 0 && !isSprinting && !isIdle) {
            rollDirection = InputDirection;
        } else if (!isSprinting && !isIdle) {
            rollDirection = 360 + InputDirection;
        } 
        UpdateRotation();
        animator.SetTrigger("Roll");
        rollDis = rollDistance + Mathf.Abs(currentSpeed/6);
        fwd += rollDis;
        sideways += rollDis;
        audioController.PlayJumpRollSound();

        InterruptCasting();
    }
    public void StopRoll() {
        isRolling = false;
        fwd -= rollDis;
        sideways -= rollDis;
        rollDirection = 0;
    }

    bool IsGrounded () {
        Debug.DrawRay(transform.position + Vector3.up * 0.1f, -Vector3.up * 0.15f, Color.red);
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, -Vector3.up, 0.15f)) {
            return true;
        } else {
            return false;
        }
    }

    void Sprinting () {
        if (isSprinting && !sprintTrails.isPlaying)
            sprintTrails.Play();
        else if (!isSprinting && sprintTrails.isPlaying) 
            sprintTrails.Stop();
    }

    void MountAnimal () {
        if (Input.GetKeyDown(KeyCode.F)) {
            if (!isMounted)
                GetComponent<MRider>().MountAnimal();
            else 
                GetComponent<MRider>().DismountAnimal();
        }
    }

    public void InterruptCasting () {
        if (!isCastingSkill)
            return;
        animator.CrossFade("Attacks.Empty", 0.25f);
        castInterrupted = true;
        isCastingSkill = false;
    }
}
