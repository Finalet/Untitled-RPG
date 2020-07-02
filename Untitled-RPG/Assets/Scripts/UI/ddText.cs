using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ddText : MonoBehaviour
{
    Color baseColor;

    public float speed = 1;
    public float lifeTime = 2;

    Vector2 offset;
    Color orange;

    public int damage;
    public int healAmount;
    public int staminaAmount;
    public bool isPlayer;


    void Start() {
        float x = Random.Range(-1f, 1f);
        x = ((x >= 0) ? 1:-1);

        Vector3 dir = (PlayerControlls.instance.transform.right * x  + Vector3.up) * Random.Range(speed*0.5f, speed*1.3f) * (1+(float)damage/3000f);
        GetComponent<Rigidbody>().AddForce(dir, ForceMode.Impulse);
        timer = lifeTime;
        if (healAmount == 0 && damage != 0) {
            transform.GetChild(0).GetComponent<TextMeshPro>().text = damage.ToString();

            orange = new Color(1, 0.5f, 0,1);
            if (isPlayer)
                transform.GetChild(0).GetComponent<TextMeshPro>().color = Color.red;
            else 
                transform.GetChild(0).GetComponent<TextMeshPro>().color = Color.Lerp(orange, Color.red, (float)damage/3000f);
        } else if (healAmount != 0 && damage == 0) {
            transform.GetChild(0).GetComponent<TextMeshPro>().text = healAmount.ToString();
            transform.GetChild(0).GetComponent<TextMeshPro>().color = Color.green;
        } else {
            transform.GetChild(0).GetComponent<TextMeshPro>().text = staminaAmount.ToString();
            transform.GetChild(0).GetComponent<TextMeshPro>().color = Color.cyan;
        }

        transform.localScale = Vector3.zero;

        baseColor = transform.GetChild(0).GetComponent<TextMeshPro>().color;
        
    }

    float timer;
    void Update() {
        transform.LookAt(PlayerControlls.instance.playerCamera.transform);

        if (timer > 1) {
            timer -= Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1,1,1), Time.deltaTime * 10);
        } else if (timer <= 1.5f && timer > 0) {
            transform.GetChild(0).GetComponent<TextMeshPro>().color = Color.Lerp(transform.GetChild(0).GetComponent<TextMeshPro>().color, new Color(baseColor.r,baseColor.g,baseColor.b,0), Time.deltaTime*2f);
            timer -= Time.deltaTime;
        } else {
            Destroy(gameObject);
        }
    }
}
