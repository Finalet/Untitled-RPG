using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class UI_Animations
{

    public static IEnumerator PressAnimation (Image image, KeyCode pressedKey) {
        Vector2 currentSize = image.GetComponent<RectTransform>().localScale;
        while (currentSize.x > 0.9f) {
            image.GetComponent<RectTransform>().localScale = Vector2.MoveTowards(currentSize, Vector2.one * 0.9f, Time.deltaTime * 7f);
            currentSize = image.GetComponent<RectTransform>().localScale;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        while (Input.GetKey(pressedKey)) {
            yield return null;
        }

        while (currentSize.x < 1) {
            image.GetComponent<RectTransform>().localScale = Vector2.MoveTowards(currentSize, Vector2.one, Time.deltaTime * 7f);
            currentSize = image.GetComponent<RectTransform>().localScale;
            yield return new WaitForSeconds(Time.deltaTime);
        } 
        image.GetComponent<RectTransform>().localScale = Vector2.one;
    }
}
