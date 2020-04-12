using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControlls : MonoBehaviour
{
    [Header("Movement")]
    public float currentSpeed = 0;
    public float walkSpeed = 1;
    public bool toggleRunning = false;
    public bool isRunning;
    public bool isSprinting;
    public bool isIdle;
    public bool isCrouch;

    float sprintingDirection;
    float desiredSprintingDirection;

    [Header("Jumping")]
    public bool airDash;
    public float jumpHeight = 2;
    public float jumpDistance = 1.5f;
    public float gravity = -50;
    float velocityY;
    float velocityX;
    Vector3 velocity;

    [Header("Dash & Air dash")]
    public float dashDistance = 5;
    public float dashCoolDownTime = 2;
    float dashCoolDownTimer = 0;
    public float airDashDistance = 5;
    float dashMultiplier;
    Vector3 dashVector;

    CharacterController controller;
    Camera playerCamera;

    Vector3 lastCamRotation;

    [Header("Animations")]
    Animator animator;
    public Transform leftFoot;
    public Transform rightFoot;

    float baseHeight;
    float baseColliderCenterY;

    bool dashed;
    bool doubleJumped = false;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerCamera = Camera.main;

        prevPos = transform.position;

        baseHeight = controller.height;
        baseColliderCenterY = controller.center.y;
    }

    Vector3 prevPos;
    void CalculateCurrentSpeed () {
        currentSpeed = Vector3.Magnitude(transform.position - prevPos) * 500;
        prevPos = transform.position;
    }

    void Update()
    {
        Movement();
        DashCoolDown();
        Animations();

        CalculateCurrentSpeed();

        if (Input.GetKeyDown(KeyCode.CapsLock))
            toggleRunning = !toggleRunning;

        if (!Input.GetButton("Horizontal") && !Input.GetButton("Vertical")) {
            isIdle = true;
        } else {
            isIdle = false;
        }
    }

    Vector3 forward;
    Vector3 side;
    void Movement()
    {
        if (!isIdle) {
                if (Input.GetButton("Run"))
            {
                if (!toggleRunning)
                    isRunning = true;
                else if (!isCrouch)
                    isSprinting = true;
            } else if (Input.GetButtonUp("Run"))
            {
                if (toggleRunning) {
                    isSprinting = false;
                } else {
                    isRunning = false;
                }
            } else {
                if (toggleRunning){
                    isRunning = true;
                } else {
                    isRunning = false;
                }
            }
        } else {
            isSprinting = false;
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
        
        dashVector = dashMultiplier * (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal") );

        if (isSprinting) {
            forward = velocityX * transform.forward;
            side = Vector3.zero;
        } else {
            forward = velocityX * transform.forward * Input.GetAxis("Vertical");
            side = velocityX * transform.right * Input.GetAxis("Horizontal");
        }
        

        velocity = Vector3.up * velocityY + forward + side;
        controller.Move(velocity * Time.deltaTime); 

        if (Input.GetButtonDown("Jump"))
            Jump();

        if (Input.GetButtonDown("Dash"))
            Dash();

        if (Input.GetButtonDown("Crouch"))
            Crouch();

        if (dashMultiplier >= 0)
            dashMultiplier -= 7.5f * Time.deltaTime;

        if (controller.isGrounded)
        {
            doubleJumped = false;
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt)) {
            lastCamRotation = playerCamera.transform.eulerAngles;
        } else if (Input.GetKeyUp(KeyCode.LeftAlt)) {
            playerCamera.transform.eulerAngles = lastCamRotation;
        }
            
        if (!Input.GetKey(KeyCode.LeftAlt)) {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, playerCamera.transform.eulerAngles.y + sprintingDirection, transform.eulerAngles.z);
        }

        if (isSprinting) {
            if (InputDirection > 0) {
                sprintingDirection = InputDirection;
            } else {
                sprintingDirection = 360 + InputDirection;
            }
        } else {
            sprintingDirection = 0;
        }
    }

    [Header("Animations variables")]
    float InputDirection;
    float lastInputDirection;
    float InputMagnitude;
    float RotationDirection;
    float isRunningFloat;
    float isSprintingFloat;
    

    void Animations() {;
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
    
    void Jump ()
    {
        if (controller.isGrounded && !isCrouch)
        {
            animator.SetTrigger("Jump");
            velocityY = Mathf.Sqrt(-2 * gravity * jumpHeight);
            velocityX = jumpDistance * Mathf.Abs(currentSpeed/3);
            animator.applyRootMotion = false;
            StartCoroutine(DetectLanding());
        } else
        { 
            if (airDash && !doubleJumped)
            {
                doubleJumped = true;
                // Double Jump
            }
        }
    }

    IEnumerator DetectLanding () {
        yield return new WaitForSeconds(0.1f);
        while (!controller.isGrounded) {
            yield return null;
        }
        velocityX = 0;
        animator.applyRootMotion = true;
        animator.SetTrigger("Landed");
    }

    void Dash ()
    {
        if (!dashed && controller.isGrounded)
        {
            dashCoolDownTimer = dashCoolDownTime;
            dashed = true;
            dashMultiplier = dashDistance;
        }
    }

    void DashCoolDown()
    {
        if (dashCoolDownTimer >= 0)
        {
            dashCoolDownTimer -= Time.deltaTime;
        } else if (dashMultiplier < 0)
        {
            dashed = false;
        }

    }

    void Crouch() {
        if (controller.isGrounded)
            isCrouch = !isCrouch;
    }

    public void SprintOff () {
        isSprinting = false;
    }
}
