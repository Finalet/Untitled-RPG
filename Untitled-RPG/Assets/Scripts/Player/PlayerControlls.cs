using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControlls : MonoBehaviour
{
    [Header("Movement")]
    public float currentSpeed = 0;
    public float walkSpeed = 5;
    public float runSpeed = 10;
    public float jumpHeight = 10;
    public bool toggleRunning = false;
    public bool isIdle;

    [Header("Jumping")]
    public bool airDash;
    public float gravity = -12;
    float velocityY;
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

    bool leftAltPressed;
    Vector3 lastCamRotation;

    [Header("Animations")]
    public Animator animator;
    public Transform leftFoot;
    public Transform rightFoot;

    public float InputDirection;
    public float lastInputDirection;
    float InputMagnitude;
    float RotationDirection;

    public float x;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = Camera.main;
    }

    void Update()
    {
        Movement();
        DashCoolDown();
        Animations();

        if (Input.GetKeyDown(KeyCode.CapsLock))
            toggleRunning = !toggleRunning;

        if (!Input.GetButton("Horizontal") && !Input.GetButton("Vertical")) 
            isIdle = true;
        else
            isIdle = false;
    }

    void Movement()
    {
        x = velocity.x;


        if (Input.GetButton("Run"))
        {
            if (!toggleRunning)
                currentSpeed = runSpeed;
            else
                currentSpeed = walkSpeed;
        } else
        {
            if (toggleRunning)
                currentSpeed = runSpeed;
            else
                currentSpeed = walkSpeed;
        }

        velocityY += gravity * Time.deltaTime;
        velocityY = Mathf.Clamp(velocityY, gravity * 3, -gravity * 3);
        
        dashVector = dashMultiplier * (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal") );

        velocity = currentSpeed * (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal") + dashVector) + Vector3.up * velocityY;

        if (Input.GetButton("Horizontal") && Input.GetButton("Vertical"))
            velocity = velocity / 1.4f;

        controller.Move(velocity * Time.deltaTime);

        if (Input.GetButtonDown("Jump"))
            Jump();

        if (Input.GetButtonDown("Dash"))
            Dash();

        if (dashMultiplier >= 0)
            dashMultiplier -= 7.5f * Time.deltaTime;

        if (controller.isGrounded)
        {
            doubleJumped = false;
            AirDashed = false;
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt)) {
            leftAltPressed = true;
            lastCamRotation = playerCamera.transform.eulerAngles;
        } else if (Input.GetKeyUp(KeyCode.LeftAlt)) {
            leftAltPressed = false;
            playerCamera.transform.eulerAngles = lastCamRotation;
        }
            

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, playerCamera.transform.eulerAngles.y, transform.eulerAngles.z);
    }

    void Animations() {;
        if (!isIdle) {
            InputDirection = Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * Mathf.Rad2Deg;
            lastInputDirection = InputDirection;

            CheckWhichFootIsUp();
            
            animator.speed = 1;

        } else {
            InputDirection = lastInputDirection;

             if (RotationDirection > 5 || RotationDirection < -5)
                animator.speed = 1 + Mathf.Abs(RotationDirection / 100);
            else 
                animator.speed = 1;
        }

        InputMagnitude = Mathf.Clamp01( Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical")) );
        animator.SetFloat("InputMagnitude", InputMagnitude);
       
        animator.SetFloat("InputDirection", InputDirection);


        if (Input.GetAxis("Mouse X") == 0) {
            if (RotationDirection > 0.1f) 
                RotationDirection -= Time.deltaTime * 50;
            else if (RotationDirection < -0.1f)
                RotationDirection += Time.deltaTime * 50;
        } else {
            RotationDirection += Input.GetAxis("Mouse X");
        }
        RotationDirection = Mathf.Clamp(RotationDirection, -45, 45);       

        animator.SetFloat("RotationDirection", RotationDirection);
    }

    void CheckWhichFootIsUp() {
        if (leftFoot.transform.position.y < rightFoot.transform.position.y) {
            animator.SetBool("IsRightLegUp", true);
        } else {
            animator.SetBool("IsRightLegUp", false);
        }
    }

    bool doubleJumped = false;
    void Jump ()
    {
        if (controller.isGrounded)
        {
            velocityY = Mathf.Sqrt(-2 * gravity * jumpHeight);
        } else
        { 
            if (airDash && !doubleJumped)
            {
                doubleJumped = true;
                AirDash();
            }
        }
    }

    bool dashed;
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

    bool AirDashed;
    void AirDash ()
    {
        if (!AirDashed && !controller.isGrounded)
        {
            AirDashed = true;
            dashMultiplier = airDashDistance;
        }
    }
}
