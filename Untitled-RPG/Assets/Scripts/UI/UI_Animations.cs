using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class UI_Animations
{
    public static IEnumerator PressAnimation (Image image, KeyCode pressedKey) {
        float animationDepth = 0.8f;
        float animationSpeed = 10f;
        Vector2 currentSize = image.GetComponent<RectTransform>().localScale;
        while (currentSize.x > animationDepth) {
            image.GetComponent<RectTransform>().localScale = Vector2.MoveTowards(currentSize, Vector2.one * animationDepth, Time.deltaTime * animationSpeed);
            currentSize = image.GetComponent<RectTransform>().localScale;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        while (Input.GetKey(pressedKey)) {
            yield return null;
        }

        while (currentSize.x < 1) {
            image.GetComponent<RectTransform>().localScale = Vector2.MoveTowards(currentSize, Vector2.one, Time.deltaTime * animationSpeed);
            currentSize = image.GetComponent<RectTransform>().localScale;
            yield return new WaitForSeconds(Time.deltaTime);
        } 
        image.GetComponent<RectTransform>().localScale = Vector2.one;
    }
}
