using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Settings_KeybindsTemplate : MonoBehaviour
{
    public TextMeshProUGUI actionLabel;
    public TextMeshProUGUI keyLabel;
    public Button keyButton;

    float blinkRate = 3;
    float lastBlinkTime;

    public void Init(string action, bool isSpacer = false) {
        if (isSpacer) {
            Destroy(actionLabel.gameObject);
            Destroy(keyButton.gameObject);
            Destroy(this);
            return;
        }
        actionLabel.text = action;
        keyLabel.text = KeyCodeDictionary.keys[KeybindsManager.instance.currentKeyBinds[action]];
        keyButton.onClick.AddListener(delegate{KeyButton(action);});
    }

    public void KeyButton(string action) {
        keyLabel.text = "_";
        StartCoroutine(WaitForInput(action));
    }

    IEnumerator WaitForInput (string action) {
        bool gotInput = false;
        KeyCode bind = KeybindsManager.instance.currentKeyBinds[action];
        while (!gotInput) {
            foreach(KeyCode kcode in KeyCode.GetValues(typeof(KeyCode))) {
                if (Input.GetKeyDown(kcode)) {
                    gotInput = true;
                    if (isAnyAllowedKey(kcode)) bind = kcode;
                    break;
                }
            }
            if (Time.realtimeSinceStartup - lastBlinkTime > 1/blinkRate) {
                keyLabel.enabled = !keyLabel.enabled;
                lastBlinkTime = Time.realtimeSinceStartup;
            }
            yield return null;
        }
        keyLabel.enabled = true;
        keyLabel.text = KeyCodeDictionary.keys[bind];
        KeybindsManager.instance.currentKeyBinds[action] = bind;
    }

    bool isSlotKeyAllowed (KeyCode key) {
        return isLetterKey(key) || isFunctionKey(key) || isNumberKey(key) || isCharKey(key) || isNumberPadKey(key);
    }

    bool isAnyAllowedKey (KeyCode key) {
        return isLetterKey(key) || isFunctionKey(key) || isNumberKey(key) || isCharKey(key) || isNumberPadKey(key) || isLargeKey(key);
    }

    bool isLetterKey (KeyCode key) {
        return key >= KeyCode.A && key <= KeyCode.Z && (key != KeyCode.W && key != KeyCode.A && key != KeyCode.S && key != KeyCode.D);
    }
    bool isFunctionKey (KeyCode key) {
        return key >= KeyCode.F1 && key <= KeyCode.F15;
    }
    bool isNumberKey (KeyCode key) {
        return key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9;
    }
    bool isCharKey (KeyCode key) {
        return key == KeyCode.BackQuote || key == KeyCode.Minus || key == KeyCode.Equals;
    }
    bool isNumberPadKey (KeyCode key) {
        return key >= KeyCode.Keypad0 && key <= KeyCode.KeypadEquals && key != KeyCode.KeypadEnter;
    }
    bool isLargeKey (KeyCode key) {
        return key == KeyCode.Tab || key == KeyCode.CapsLock || key == KeyCode.LeftShift || key == KeyCode.LeftControl || key == KeyCode.LeftAlt || key == KeyCode.Space || key == KeyCode.RightAlt || key == KeyCode.RightControl || key == KeyCode.RightShift;
    }
}
