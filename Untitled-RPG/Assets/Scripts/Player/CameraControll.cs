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

    public float rotationX;
    void LateUpdate()
    { 
        rotationX = transform.eulerAngles.x + cameraX;
        
        if (rotationX <= 280 && rotationX >= 270)
            rotationX = 280;
        else if (rotationX >= 90 && rotationX <= 100) 
            rotationX = 90;    
        
        transform.eulerAngles = new Vector3(rotationX, transform.eulerAngles.y + cameraY, transform.eulerAngles.z);     
        transform.position = Player.transform.position - (transform.forward * camDistance) + offset;

        RaycastHit hit;
        if (Physics.Linecast(Player.transform.position + offset, transform.position, out hit)) {
            transform.position = new Vector3(hit.point.x + hit.normal.x * 0.2f, hit.point.y, hit.point.z + hit.normal.z * 0.2f);
        }
    }
}
