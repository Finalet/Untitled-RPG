using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControlls : MonoBehaviour
{
    [Header("Movement")]
    public int currentSpeed = 0;
    public int walkSpeed = 5;
    public int runSpeed = 10;
    public float jumpHeight = 10;
    public bool toggleRunning = false;

    public float gravity = -12;
    public float velocityY;
    Vector3 velocity;

    CharacterController controller;

    Camera playerCamera;

    public bool doubleJump;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = Camera.main;
    }

    void Update()
    {
        Movement();

        if (Input.GetKeyDown(KeyCode.CapsLock))
            toggleRunning = !toggleRunning;
    }

    void Movement()
    {
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

        velocity = transform.forward * currentSpeed * Input.GetAxis("Vertical") + transform.right * currentSpeed * Input.GetAxis("Horizontal") + Vector3.up * velocityY;
        controller.Move(velocity * Time.deltaTime);

        if (Input.GetButtonDown("Jump"))
            Jump();

        if (controller.isGrounded)
            doubleJumped = false;

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, playerCamera.transform.eulerAngles.y, transform.eulerAngles.z);
    }

    bool doubleJumped = false;
    void Jump ()
    {
        if (controller.isGrounded)
        {
            velocityY = Mathf.Sqrt(-2 * gravity * jumpHeight);
        } else
        { 
            if (doubleJump && !doubleJumped)
            {
                doubleJumped = true;
                velocityY = Mathf.Sqrt(-2 * gravity * jumpHeight);
            }
        }
    }
}
