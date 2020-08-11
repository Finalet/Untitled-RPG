using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.HAP;
using Cinemachine;


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
    public bool isCastingSkill;
    public bool isFlying;
    public bool isPickingArea;

    [Space]
    public bool castInterrupted;

    float sprintingDirection;
    float desiredSprintingDirection;

    [System.NonSerialized] public float desiredLookDirection; //Accessed by M_Rider fix rotation when on animal
    [System.NonSerialized] public float lookDirection; 

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
    float rollDirection;
    float desiredRollDirection;

    [System.NonSerialized] public CharacterController controller;
    [System.NonSerialized] public Camera playerCamera;
    [System.NonSerialized] public CinemachineFreeLook CM_Camera;
    [System.NonSerialized] public PlayerAudioController audioController;

    Vector3 lastCamRotation;

    [Header("Animations")]
    public Animator animator;
    public Transform leftFoot;
    public Transform rightFoot;
    public ParticleSystem sprintTrails;

    public SkinnedMeshRenderer skinnedMesh;

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
        CM_Camera = playerCamera.GetComponentInChildren<CinemachineFreeLook>();

        prevPos = transform.position;
        walkSpeed = baseWalkSpeed;

        baseHeight = controller.height;
        baseColliderCenterY = controller.center.y;
    }

    Vector3 prevPos;
    float calculatingCurrentSpeedTimer;
    float currentSpeedSums;
    float instantCurrentSpeed;
    void CalculateCurrentSpeed () {
        if (calculatingCurrentSpeedTimer < 0.25f) {
            calculatingCurrentSpeedTimer += Time.fixedDeltaTime;
            instantCurrentSpeed = Vector3.Magnitude(transform.position - prevPos - new Vector3(0, transform.position.y, 0)) * 120;
            prevPos = transform.position - new Vector3(0, transform.position.y, 0);
            currentSpeedSums += instantCurrentSpeed;
        } else {
            currentSpeed = Mathf.Round( (currentSpeedSums/(0.25f/Time.fixedDeltaTime)) * 100f ) / 100f;
            calculatingCurrentSpeedTimer = 0;
            currentSpeedSums = 0;
        }
        
    }

    void Update()
    {
        isGrounded = IsGrounded();

        if (!isMounted && !isFlying) {
            GroundMovement();
            GroundAnimations();
        } else if (isFlying) {
            FlyingMovement();
            FlyingAnimations();
        }
        Sprinting();
        MountAnimal();
        SetAnimatorVarialbes();
        InputMagnitudeFunc();

        if (Input.GetKeyDown(KeyCode.CapsLock))
            toggleRunning = !toggleRunning;

        if (!Input.GetButton("Horizontal") && !Input.GetButton("Vertical") && !isAttacking) {
            isIdle = true;
        } else {
            isIdle = false;
        }

        isWeaponOut = GetComponent<WeaponsController>().isWeaponOut;


        //Smooth rotation after rolling
        if (Mathf.Abs(rollDirection - desiredRollDirection) > 1) {
            rollDirection = Mathf.Lerp(rollDirection, desiredRollDirection, Time.deltaTime * 10);
        } else {
            rollDirection = desiredRollDirection;
        }

        //Smooth rotation after sprinting
        //LerpAngle since need to go from -179 to 179 smoothely
        if (Mathf.Abs(sprintingDirection - desiredSprintingDirection) > 1) {
            sprintingDirection = Mathf.LerpAngle(sprintingDirection, desiredSprintingDirection, Time.deltaTime * 7);
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

    void FixedUpdate() {
        CalculateCurrentSpeed();
    }

#region Ground movement
    bool staminaRanOutFromSprinting;
    void GroundMovement()
    {
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

        if (Input.GetButtonDown("FreeCamera")) {
            lastCamRotation = playerCamera.transform.eulerAngles;
        } else if (Input.GetButtonUp("FreeCamera")) {
            playerCamera.transform.eulerAngles = lastCamRotation;
        }
            
        UpdateRotation();

        if (isSprinting) {
            if (InputDirection > 0) {
                //sprintingDirection = InputDirection;
                desiredSprintingDirection = InputDirection;
            } else {
                //sprintingDirection = 360 + InputDirection;
                desiredSprintingDirection =  InputDirection;
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
        
        if (!PeaceCanvas.instance.anyPanelOpen && !isIdle) { //If idle, player show not rotate
            desiredLookDirection = CM_Camera.m_XAxis.Value;
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
        if (!isGrounded || isFlying || isCrouch || isRolling || isAttacking || isGettingHit || isJumping) {
            return;
        }

        audioController.PlayJumpRollSound();
        animator.SetTrigger("Jump");
        isJumping = true;
        jumpDis = jumpDistance * currentSpeed/3;
        jumpDis = Mathf.Clamp(jumpDis, 0, 8); //Limit max jumping distance
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
            if (isFlying) {
                fwd -= jumpDis;
                sideways -= jumpDis;
                isJumping = false;
                animator.CrossFade("Jump/Roll.Empty", 0.25f);
                yield break;
            }
            yield return null;
        }
        fwd -= jumpDis;
        sideways -= jumpDis;
        animator.applyRootMotion = true;
        yield return new WaitForSeconds(0.5f);
        isJumping = false;
    }
    IEnumerator DetectLanding (bool afterFlying) {
        yield return new WaitForSeconds(0.1f);
        while (!isGrounded) {
            velocityY += gravity/2.5f * Time.deltaTime;
            yield return null;
        }
        isFlying = false;
        animator.applyRootMotion = true;
    }

    void Crouch() {
        if (isGrounded)
            isCrouch = !isCrouch;
        
        InterruptCasting();
    }

    float rollDis;
    void Roll() {
        if (isAttacking || isRolling || isFlying) {
            return;
        }
        if (GetComponent<Characteristics>().Stamina < staminaReqToRoll) {
            CanvasScript.instance.DisplayWarning("Not enough stamina!");
            return;
        }

        animator.applyRootMotion = false;
        GetComponent<Characteristics>().UseOrRestoreStamina(staminaReqToRoll);
        isRolling = true;
        if (InputDirection > 0 && !isSprinting && !isIdle) {
            desiredRollDirection = InputDirection;
        } else if (!isSprinting && !isIdle) {
            desiredRollDirection = InputDirection;
        } 
        UpdateRotation();
        animator.SetTrigger("Roll");
        rollDis = rollDistance + currentSpeed/6;
        rollDis = Mathf.Clamp(rollDis, 0, 8);
        fwd += rollDis;
        sideways += rollDis;
        audioController.PlayJumpRollSound();

        InterruptCasting();
    }
    public void StopRoll() {
        isRolling = false;
        fwd -= rollDis;
        sideways -= rollDis;
        desiredRollDirection = 0;
        animator.applyRootMotion = true;
    }

    bool IsGrounded () {
        Vector3 rayOffset;
        for (int i = 0; i < 4; i++) {
            if (i==0)
                rayOffset = transform.forward * 0.4f;
            else if (i==1)
                rayOffset = -transform.forward * 0.4f;
            else if (i==2)
                rayOffset = transform.right * 0.4f;
            else
                rayOffset = -transform.right * 0.4f;

            Debug.DrawRay(transform.position + Vector3.up*0.1f + rayOffset, -Vector3.up*0.4f, Color.red);

            if (Physics.Raycast(transform.position + Vector3.up * 0.1f + rayOffset, -Vector3.up, 0.4f))
                return true;
        }
        //if all of the above fails, then still not grounded;
        return false;
    }

    void Sprinting () {
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
            if (!isMounted) {
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

    [Header("Flying variables")]
    public float flySpeed = 2;

    void FlyingMovement() {
        forward = flySpeed * transform.forward * (1+fwd) * Input.GetAxis("Vertical") + independentFromInputFwd * transform.forward;
        side = flySpeed * transform.right * (1+sideways) * Input.GetAxis("Horizontal");

        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftShift)) {
            velocityY = Mathf.MoveTowards(velocityY, 1, 0.2f);
        } else if (Input.GetKey(KeyCode.LeftControl)) {
            velocityY = Mathf.MoveTowards(velocityY, -1, 0.2f);;
        } else {
            velocityY = Mathf.MoveTowards(velocityY, 0, 0.2f);
        }
        
        velocity = Vector3.up * velocityY * 2 + forward + side;
        controller.Move(velocity * flySpeed * Time.deltaTime); 

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
        animator.applyRootMotion = false;
        isFlying = true;
    }
    public void LandFromFlying () {
        animator.CrossFade("Main.Flying.Land", 0.1f);
    }
    public void FlyUp () { //Called from take off animation
        velocityY = 5f;
        SprintOff();
    }
    public void LandDown () { //Called from landing animation
        StartCoroutine(DetectLanding(true));
    }


#endregion
    
    void SetAnimatorVarialbes () {
        animator.SetFloat("InputMagnitude", InputMagnitude);    
        animator.SetFloat("InputDirection", InputDirection);
        animator.SetFloat("isSprinting", isSprintingFloat);
        animator.SetFloat("isRunning", isRunningFloat);
        animator.SetFloat("WalkSpeed", walkSpeed);
        
        animator.SetBool("Idle", isIdle);
        animator.SetBool("isCrouch", isCrouch);
        animator.SetBool("isGrounded", isGrounded);
    }
    
    public void InterruptCasting () {
        if (!isCastingSkill)
            return;
        animator.CrossFade("Attacks.Empty", 0.15f);
        castInterrupted = true;
        isCastingSkill = false;
    }
}
