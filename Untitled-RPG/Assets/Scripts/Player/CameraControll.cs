using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControll : MonoBehaviour
{
    public Transform Player;
    float cameraX;
    float cameraY;

    public Vector3 offset;
    Vector3 shake;
    public float camDistance;
    public float maxCamDistance = 10;
    public float camDesiredPosition;
    float scroll;
    public float mouseSensitivity = 1;

    Camera cam;

    void Start()
    {
        camDesiredPosition = maxCamDistance;
        cam = GetComponent<Camera>();
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
        
        if (!PeaceCanvas.instance.anyPanelOpen)
            transform.eulerAngles = new Vector3(rotationX, transform.eulerAngles.y + cameraY, transform.eulerAngles.z);     

        transform.position = Player.transform.position - (transform.forward * camDistance) + offset + shake;

        RaycastHit hit;
        if (Physics.Linecast(Player.transform.position + offset, transform.position, out hit)) {
            if (hit.transform.tag != "Player" && !hit.collider.isTrigger) {
                transform.position = new Vector3(hit.point.x + hit.normal.x * 0.2f, hit.point.y, hit.point.z + hit.normal.z * 0.2f);
            }
        }

        SprintCameraFOV();
    }

    void SprintCameraFOV () {
        if (PlayerControlls.instance.isSprinting) {
            if (cam.fieldOfView < 80)
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 82, 10 * Time.deltaTime);
            else 
                cam.fieldOfView = 80;
        } else {
            if (cam.fieldOfView > 60)
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 58, 10 * Time.deltaTime);
            else cam.fieldOfView = 60;
        }
    }

    public void CameraMounted() {
        camDesiredPosition += 3;
        maxCamDistance += 5;
    }
    public void CameraDismount () {
        camDesiredPosition -= 3;
        maxCamDistance -= 5;
    }

    bool isShaking;
    public void CameraShake (float duration, float magnitude, float damage){
        if (isShaking)
            return;

        StartCoroutine(Shake(duration, magnitude, damage));
    }

    IEnumerator Shake (float duration, float magnitude, float damage) {
        float elapsed = 0;
    
        Vector3 newPos = Vector3.zero;
        isShaking = true;
        while (elapsed < duration * (1 + damage / 4000)) {   

            if (Vector3.Distance(shake, newPos) <= magnitude/4f) {
                float x = Random.Range(-1f, 1f) * magnitude * (1 + damage / 2000);
                float y = Random.Range(-1f, 1f) * magnitude * (1 + damage / 2000);
                newPos = new Vector3(x, y, 0);
            }
            
            shake = Vector3.Lerp(shake, newPos, Time.deltaTime * 50);

            elapsed += Time.deltaTime;

            yield return null;
        }
        while (Vector3.Distance(shake, Vector3.zero) >= magnitude/4f) {
            shake = Vector3.Lerp(shake, Vector3.zero, 0.5f);
        }
        shake = Vector3.zero;
        isShaking = false;
    }
}
