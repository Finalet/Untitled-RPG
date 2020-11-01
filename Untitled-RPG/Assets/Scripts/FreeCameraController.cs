using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crest;

public class FreeCameraController : MonoBehaviour
{
    public PlayerControlls instance;

    public float speed = 1;
    public float mouseSensitivity = 1;
    Vector3 move;
    
    float xRotation = 0f;
    float yRotation = 0f;

    void Update () {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        move = (Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward + Input.GetAxis("QE") * transform.up )* speed / 10f;
        transform.Translate(move, Space.World);

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime * 100f;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime * 100f;

        xRotation += mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        yRotation += mouseX;

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.LeftControl)){
            speed *= 4;
        } else if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.LeftControl)) {
            speed *= 0.25f;
        }
    }
}
