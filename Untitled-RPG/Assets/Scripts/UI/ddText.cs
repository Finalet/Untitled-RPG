using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class ddText : MonoBehaviour
{
    public float speed = 1;
    public float lifeTime = 2;

    Vector2 offset;
    Color orange;
    Color red;

    public DamageInfo damageInfo;
    public int healAmount;
    public int staminaAmount;
    public bool isPlayer;


    void Start() {
        float x = Random.Range(-1f, 1f);
        x = ((x >= 0) ? 1:-1);

        Vector3 dir = (PlayerControlls.instance.transform.right * x  + Vector3.up) * Random.Range(speed*0.5f, speed*1.3f) * (1+(float)damageInfo.damage/3000f);
        GetComponent<Rigidbody>().AddForce(dir, ForceMode.Impulse);

        TextMeshPro tmp = transform.GetChild(0).GetComponent<TextMeshPro>();

        if (healAmount == 0 && damageInfo.damage != 0) {
            tmp.text = damageInfo.damage.ToString();

            orange = new Color(1, 0.5f, 0, 1);
            red = new Color(1, 0.1f, 0, 1);
            if (isPlayer)
                tmp.color = Color.red;
            else 
                tmp.color = damageInfo.isCrit ? red : orange;
        } else if (healAmount != 0 && damageInfo.damage == 0) {
            tmp.text = healAmount.ToString();
            tmp.color = Color.green;
        } else {
            tmp.text = staminaAmount.ToString();
            tmp.color = Color.cyan;
        }

        transform.localScale = Vector3.zero;

        Color colorAlphaZero = tmp.color;
        colorAlphaZero.a = 0;

        Sequence mySequence = DOTween.Sequence()
            .Append( transform.DOScale( damageInfo.isCrit ? 1.3f : 1, lifeTime/4) )
            .AppendInterval(lifeTime - lifeTime/2)
            .Append( tmp.DOColor(colorAlphaZero, lifeTime/4) );

        Destroy(gameObject, mySequence.Duration());
    }

    void Update() {
        transform.LookAt(PlayerControlls.instance.playerCamera.transform);
    }
}
