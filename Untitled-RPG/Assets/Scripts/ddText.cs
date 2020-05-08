using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ddText : MonoBehaviour
{
    float x = 1;
    Color c;

    public float upwardSpeed = 1;
    public float disolveSpeed = 1;
    public float lifeTime = 1;
    
    Vector2 offset;

    float break1;

    void Awake() {
        transform.SetParent(AssetHolder.instance.canvas.gameObject.transform);
    }

    void Start() {
        c = GetComponent<TextMeshProUGUI>().color;
        StartCoroutine(work());
        GetComponent<RectTransform>().anchoredPosition = new Vector2(Random.Range(-200f, 200f), Random.Range(0, 200f));

        break1 = lifeTime * 0.8f;
    }

    IEnumerator work() {
        while (lifeTime > break1) {
            lifeTime -= Time.deltaTime;
            GetComponent<RectTransform>().Translate(Vector3.up * Time.deltaTime * upwardSpeed / 4);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        while (lifeTime > 0) {
            lifeTime -= Time.deltaTime;
            upwardSpeed += Time.deltaTime * 10;
            GetComponent<RectTransform>().Translate(Vector3.up * Time.deltaTime * upwardSpeed);
            x -= Time.deltaTime * disolveSpeed;
            GetComponent<TextMeshProUGUI>().color = new Color(c.r, c.g, c.b, x);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        Destroy(gameObject);
    }
}
