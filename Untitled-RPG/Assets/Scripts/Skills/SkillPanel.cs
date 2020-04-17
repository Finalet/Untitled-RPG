using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillPanel : MonoBehaviour
{
    public GameObject[] displaySlots;
    public GameObject[] actualSlots;

    Color baseColor;
    void Awake() {
        baseColor = displaySlots[1].GetComponent<Image>().color;
    }
    
    void Update() {
        DisplaySlots();
        UseSlots();
    }

    void DisplaySlots() {
        for (int i = 0; i < actualSlots.Length; i++) {
            if (actualSlots[i] != null && actualSlots[i].GetComponent<Skill>() != null) { //if there is a skill in the slot
                displaySlots[i].GetComponent<Image>().sprite = actualSlots[i].GetComponent<Skill>().icon;
                displaySlots[i].GetComponent<Image>().color = Color.white;

                if(actualSlots[i].GetComponent<Skill>().isCoolingDown) {
                    displaySlots[i].transform.GetChild(1).GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
                    displaySlots[i].transform.GetChild(1).GetComponent<Image>().fillAmount = actualSlots[i].GetComponent<Skill>().coolDownTimer/actualSlots[i].GetComponent<Skill>().coolDown;
                } else {
                    displaySlots[i].transform.GetChild(1).GetComponent<Image>().color = new Color(0, 0, 0, 0);
                    displaySlots[i].transform.GetChild(1).GetComponent<Image>().fillAmount = 1;
                }
            } else {
                displaySlots[i].GetComponent<Image>().color = baseColor;
            }
        }
    }

    void UseSlots() {
        //FIRST ROW
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            StartCoroutine(PressAnimation(0));
        } else if (Input.GetKeyUp(KeyCode.Alpha1)) {
            StartCoroutine(UnpressAnimation(0));
            if (actualSlots[0] != null && actualSlots[0].GetComponent<Skill>() != null) {
                actualSlots[0].GetComponent<Skill>().Use();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            StartCoroutine(PressAnimation(1));
        } else if (Input.GetKeyUp(KeyCode.Alpha2)) {
            StartCoroutine(UnpressAnimation(1));
            if (actualSlots[1] != null && actualSlots[1].GetComponent<Skill>() != null) {
                actualSlots[1].GetComponent<Skill>().Use();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            StartCoroutine(PressAnimation(2));
        } else if (Input.GetKeyUp(KeyCode.Alpha3)) {
            StartCoroutine(UnpressAnimation(2));
            if (actualSlots[2] != null && actualSlots[2].GetComponent<Skill>() != null) {
                actualSlots[2].GetComponent<Skill>().Use();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            StartCoroutine(PressAnimation(3));
        } else if (Input.GetKeyUp(KeyCode.Alpha4)) {
            StartCoroutine(UnpressAnimation(3));
            if (actualSlots[3] != null && actualSlots[3].GetComponent<Skill>() != null) {
                actualSlots[3].GetComponent<Skill>().Use();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            StartCoroutine(PressAnimation(4));
        } else if (Input.GetKeyUp(KeyCode.Alpha5)) {
            StartCoroutine(UnpressAnimation(4));
            if (actualSlots[4] != null && actualSlots[4].GetComponent<Skill>() != null) {
                actualSlots[4].GetComponent<Skill>().Use();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha6)) {
            StartCoroutine(PressAnimation(5));
        } else if (Input.GetKeyUp(KeyCode.Alpha6)) {
            StartCoroutine(UnpressAnimation(5));
            if (actualSlots[5] != null && actualSlots[5].GetComponent<Skill>() != null) {
                actualSlots[5].GetComponent<Skill>().Use();
            }
        }

        // SECOND ROW
        if (Input.GetKeyDown(KeyCode.Q)) {
            StartCoroutine(PressAnimation(6));
        } else if (Input.GetKeyUp(KeyCode.Q)) {
            StartCoroutine(UnpressAnimation(6));
            if (actualSlots[6] != null && actualSlots[6].GetComponent<Skill>() != null) {
                actualSlots[6].GetComponent<Skill>().Use();
            }
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            StartCoroutine(PressAnimation(7));
        } else if (Input.GetKeyUp(KeyCode.E)) {
            StartCoroutine(UnpressAnimation(7));
            if (actualSlots[7] != null && actualSlots[7].GetComponent<Skill>() != null) {
                actualSlots[7].GetComponent<Skill>().Use();
            }
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            StartCoroutine(PressAnimation(8));
        } else if (Input.GetKeyUp(KeyCode.R)) {
            StartCoroutine(UnpressAnimation(8));
            if (actualSlots[8] != null && actualSlots[8].GetComponent<Skill>() != null) {
                actualSlots[8].GetComponent<Skill>().Use();
            }
        }

        if (Input.GetKeyDown(KeyCode.T)) {
            StartCoroutine(PressAnimation(9));
        } else if (Input.GetKeyUp(KeyCode.T)) {
            StartCoroutine(UnpressAnimation(9));
            if (actualSlots[9] != null && actualSlots[9].GetComponent<Skill>() != null) {
                actualSlots[9].GetComponent<Skill>().Use();
            }
        }

        if (Input.GetKeyDown(KeyCode.Z)) {
            StartCoroutine(PressAnimation(10));
        } else if (Input.GetKeyUp(KeyCode.Z)) {
            StartCoroutine(UnpressAnimation(10));
            if (actualSlots[10] != null && actualSlots[10].GetComponent<Skill>() != null) {
                actualSlots[10].GetComponent<Skill>().Use();
            }
        }

        if (Input.GetKeyDown(KeyCode.X)) {
            StartCoroutine(PressAnimation(11));
        } else if (Input.GetKeyUp(KeyCode.X)) {
            StartCoroutine(UnpressAnimation(11));
            if (actualSlots[11] != null && actualSlots[11].GetComponent<Skill>() != null) {
                actualSlots[11].GetComponent<Skill>().Use();
            }
        }
        

    }

    IEnumerator PressAnimation (int slotIndex) {
        Vector2 currentSize = displaySlots[slotIndex].GetComponent<RectTransform>().localScale;
        while (currentSize.x > 0.9f) {
            displaySlots[slotIndex].GetComponent<RectTransform>().localScale = new Vector2(Mathf.Lerp(currentSize.x, 0.85f, 0.3f), Mathf.Lerp(currentSize.y, 0.85f, 0.3f));
            currentSize = displaySlots[slotIndex].GetComponent<RectTransform>().localScale;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        displaySlots[slotIndex].GetComponent<RectTransform>().localScale = new Vector2(0.9f, 0.9f);
    }
    IEnumerator UnpressAnimation (int slotIndex) {
        Vector2 currentSize = displaySlots[slotIndex].GetComponent<RectTransform>().localScale;
        while (currentSize.x < 1f) {
            displaySlots[slotIndex].GetComponent<RectTransform>().localScale = new Vector2(Mathf.Lerp(currentSize.x, 1.05f, 0.3f), Mathf.Lerp(currentSize.y, 1.05f, 0.3f));
            currentSize = displaySlots[slotIndex].GetComponent<RectTransform>().localScale;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        displaySlots[slotIndex].GetComponent<RectTransform>().localScale = new Vector2(1f, 1f);
    }
}
