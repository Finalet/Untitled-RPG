using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControll : MonoBehaviour
{
    public Transform Player;
    float cameraX;
    float cameraY;

    Vector3 offset;
    public Vector3 desiredOffset;
    Vector3 shake;
    public float mouseSensitivity = 1;
    public float maxCamDistance = 10;
    public float camDistance;
    float camDesiredPosition;
    float scroll;

    Camera cam;

    const float leftShoulder = -0.5f;
    const float rightShoulder = 0.5f;
    const float center = 0;

    void Start()
    {
        camDesiredPosition = maxCamDistance;
        offset = desiredOffset;
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

        //Shoulder camera switching
        if (Input.GetKeyUp(KeyCode.Mouse2))
            SwitchShouderCam();
        
        //Turn on/off UI
        if (Input.GetKeyUp(KeyCode.F3)) {
            CanvasScript.instance.gameObject.SetActive(!CanvasScript.instance.gameObject.activeInHierarchy);
            PeaceCanvas.instance.gameObject.SetActive(!PeaceCanvas.instance.gameObject.activeInHierarchy);
        }
    }

    float rotationX;
    void LateUpdate()
    { 
        rotationX = transform.eulerAngles.x + cameraX;
        
        if (rotationX <= 280 && rotationX >= 270)
            rotationX = 280;
        else if (rotationX >= 90 && rotationX <= 100) 
            rotationX = 90;    
        
        if (!PeaceCanvas.instance.anyPanelOpen)
            transform.eulerAngles = new Vector3(rotationX, transform.eulerAngles.y + cameraY, transform.eulerAngles.z);     

        transform.position = Player.transform.position  -transform.forward * (camDistance + offset.z) + transform.right * offset.x + Vector3.up * offset.y + shake;

        RaycastHit hit;
        if (Physics.Linecast(Player.transform.position + Vector3.up, transform.position, out hit)) {
            if (hit.transform.tag != "Player" && !hit.collider.isTrigger) {
                transform.position = hit.point + hit.normal * 0.5f;
            }
        }

        SprintCameraFOV();

        if (offset != desiredOffset) {
            offset = Vector3.MoveTowards(offset, desiredOffset, 4 * Time.deltaTime);
        }
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

    void SwitchShouderCam() {
        switch (desiredOffset.x) {
            case center: 
                desiredOffset.x = rightShoulder;
                break;
            case rightShoulder:
                desiredOffset.x = leftShoulder;
                break;
            case leftShoulder:
                desiredOffset.x = center;
                break;
            default: desiredOffset.x = center;
                break;
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


#region Camera shake overloads
    bool isShaking;
    public void CameraShake (float duration, float magnitude){
        if (isShaking)
            return;

        StartCoroutine(Shake(duration, magnitude));
    }
    public void CameraShake (float duration, float magnitude, float damage){
        if (isShaking)
            return;

        StartCoroutine(Shake(duration, magnitude, damage));
    }

    IEnumerator Shake (float duration, float magnitude) {
        float elapsed = 0;
    
        Vector3 newPos = Vector3.zero;
        isShaking = true;
        while (elapsed < duration) {   

            if (Vector3.Distance(shake, newPos) <= magnitude/4f) {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                newPos = new Vector3(x, y, 0);
            }
            
            shake = Vector3.Lerp(shake, newPos, Time.deltaTime * 50);

            elapsed += Time.deltaTime;

            yield return null;
        }
        while (Vector3.Distance(shake, Vector3.zero) >= magnitude/4f) {
            shake = Vector3.Lerp(shake, Vector3.zero, 0.4f);
        }
        shake = Vector3.zero;
        isShaking = false;
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
            shake = Vector3.Lerp(shake, Vector3.zero, 0.4f);
        }
        shake = Vector3.zero;
        isShaking = false;
    }

#endregion
}
