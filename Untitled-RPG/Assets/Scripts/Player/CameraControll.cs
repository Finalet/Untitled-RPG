using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControll : MonoBehaviour
{
    public Transform Player;
    float cameraX;
    float cameraY;

    public Vector3 offset;
    public float mouseSensitivity = 1;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        cameraX += Input.GetAxis("Mouse X") * mouseSensitivity;
        cameraY += Input.GetAxis("Mouse Y") * mouseSensitivity;
    }

    void LateUpdate()
    {
        Quaternion rotation = Quaternion.Euler(cameraY, cameraX, 0);
        transform.position = Player.position + rotation * offset;
        transform.LookAt(Player.position);
    }
}
