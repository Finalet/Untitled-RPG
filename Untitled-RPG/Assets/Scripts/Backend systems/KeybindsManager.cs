using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeybindsManager : MonoBehaviour
{
    public static KeybindsManager instance;

    [Header("Character")]
    public KeyCode run = KeyCode.LeftShift;
    public KeyCode roll = KeyCode.V;
    public KeyCode crouch = KeyCode.C;
    public KeyCode jump = KeyCode.Space;
    public KeyCode toggleRunning = KeyCode.CapsLock;
    public KeyCode sheathe = KeyCode.H;
    [Header("Main")]
    public KeyCode quickAccessMenu = KeyCode.Tab;
    public KeyCode inventory = KeyCode.I;
    public KeyCode skills = KeyCode.K;
    public KeyCode waitTime = KeyCode.O;
    public KeyCode interact = KeyCode.F;
    [Header("Misc")]
    public KeyCode damageChat = KeyCode.F1;
    public KeyCode tooManyItems = KeyCode.F2;
    public KeyCode hideUI = KeyCode.F3;

    void Awake() {
        instance = this;
    }
    
}
