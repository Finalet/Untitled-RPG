using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ddText : MonoBehaviour
{
    float x = 1;
    Color c;

    public float speed = 1;
    public float lifeTime = 2;

    Vector2 offset;
    Color orange;

    public int damage;

    void Start() {
        float x = Random.Range(-1f, 1f);
        x = ((x >= 0) ? 1:-1);

        Vector3 dir = (PlayerControlls.instance.transform.right * x  + Vector3.up) * Random.Range(speed*0.5f, speed*1.3f) * (1+(float)damage/3000f);
        GetComponent<Rigidbody>().AddForce(dir, ForceMode.Impulse);
        timer = lifeTime;
        transform.GetChild(0).GetComponent<TextMeshPro>().text = damage.ToString();

        orange = new Color(1, 0.5f, 0,1);
        transform.GetChild(0).GetComponent<TextMeshPro>().color = Color.Lerp(orange, Color.red, (float)damage/3000f);
        transform.localScale = Vector3.zero;
    }

    float timer;
    void Update() {
        transform.LookAt(PlayerControlls.instance.playerCamera.transform);

        if (timer > 1) {
            timer -= Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1,1,1), Time.deltaTime * 10);
        } else if (timer <= 1.5f && timer > 0) {
            transform.GetChild(0).GetComponent<TextMeshPro>().color = Color.Lerp(transform.GetChild(0).GetComponent<TextMeshPro>().color, new Color(c.r,c.g,c.b,0), Time.deltaTime*2f);
            timer -= Time.deltaTime;
        } else {
            Destroy(gameObject);
        }
    }
}
