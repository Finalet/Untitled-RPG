using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crest;
using QFSW.QC;

public class FreeCameraController : MonoBehaviour
{
    public float speed = 1;
    public KeyCode takeScreenshot = KeyCode.F12;
    public bool startCursorLock = false;
    Vector3 move;
    
    float xRotation = 0f;
    float yRotation = 0f;

    public bool disableControl;

    QuantumConsole console;

    void Start() {
        console = QuantumConsole.Instance;

        QuantumConsole.Instance.OnActivate += ShowCursor;
        QuantumConsole.Instance.OnDeactivate += HideCursor;
    }

    void OnEnable() {
        xRotation = transform.rotation.eulerAngles.x;
        yRotation = transform.rotation.eulerAngles.y;

        if (startCursorLock) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update () {
        if (disableControl) return;
        
        move = (Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward + Input.GetAxis("QE") * transform.up )* speed / 10f;
        transform.Translate(move, Space.World);

        float mouseX = Input.GetAxis("Mouse X") * (SettingsManager.instance ? SettingsManager.instance.mouseSensitivity : 50) * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * (SettingsManager.instance ? SettingsManager.instance.mouseSensitivity : 50) * Time.deltaTime * ((SettingsManager.instance ? SettingsManager.instance.invertY  ? 1 : -1 : 1));

        xRotation += mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        yRotation += mouseX;

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.LeftControl)){
            speed *= 4;
        } else if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.LeftControl)) {
            speed *= 0.25f;
        }

        speed += Input.mouseScrollDelta.y * 0.5f;
        speed = Mathf.Clamp(speed, 0, Mathf.Infinity);

        if (Input.GetKeyDown(takeScreenshot)) ScreenCapture.CaptureScreenshot($"{Application.dataPath}/Screenshots/{System.DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")} screenshot.png", 3);
    }

    void ShowCursor () {
        disableControl = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    void HideCursor() {
        disableControl = false;
        if (startCursorLock) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
