using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControll : MonoBehaviour
{
    public Transform Player;
    float cameraX;
    float cameraY;

    public Vector3 offset;
    public float camDistance;
    public float maxCamDistance = 10;
    public float camDesiredPosition;
    float scroll;
    public float mouseSensitivity = 1;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        camDesiredPosition = maxCamDistance/2;
    }

    void Update()
    {
        cameraX = Input.GetAxis("Mouse Y") * mouseSensitivity;
        cameraY = Input.GetAxis("Mouse X") * mouseSensitivity;

        camDesiredPosition -= Input.GetAxis("Mouse ScrollWheel") * 3;
        camDesiredPosition = Mathf.Clamp(camDesiredPosition, 1, maxCamDistance);

        float camDifference = camDesiredPosition - camDistance;

        camDistance += camDifference * Time.deltaTime * 20;
    }

    void LateUpdate()
    { 
        transform.eulerAngles = new Vector3(Mathf.Clamp(transform.eulerAngles.x + cameraX, 0, 90), transform.eulerAngles.y + cameraY, transform.eulerAngles.z);     
        transform.position = Player.transform.position - (transform.forward * camDistance) + offset;
    }
}
