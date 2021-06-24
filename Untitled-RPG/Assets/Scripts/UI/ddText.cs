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

    public void Init (DamageInfo damageInfo) {
        int x = Random.value < 0.5f ? 1 : -1;

        Vector3 dir = (PlayerControlls.instance.transform.right * x  + Vector3.up) * Random.Range(speed*0.5f, speed*1.3f) * (1+(float)damageInfo.damage/4000f) * (damageInfo.isCrit ? 1.4f : 1);
        dir = Vector3.ClampMagnitude(dir, 10);
        GetComponent<Rigidbody>().AddForce(dir, ForceMode.Impulse);

        TextMeshPro tmp = transform.GetChild(0).GetComponent<TextMeshPro>();

        if (damageInfo.damage != 0) {
            string crit = damageInfo.isCrit ? "CRITICAL " : "";
            tmp.text = crit + damageInfo.damage.ToString();
            
            orange = new Color(1, 0.5f, 0, 1);
            red = new Color(1, 0.1f, 0, 1);
            if (damageInfo.damageType == DamageType.Enemy)
                tmp.color = Color.red;
            else 
                tmp.color = damageInfo.isCrit ? red : orange;
        } else {
            tmp.text = "No damage";
            tmp.color = Color.white;
        }

        transform.localScale = Vector3.zero;

        Color colorAlphaZero = tmp.color;
        colorAlphaZero.a = 0;

        Sequence mySequence = DOTween.Sequence()
            .Append( transform.DOScale( damageInfo.isCrit ? 1.2f : 1, lifeTime/4) )
            .AppendInterval(lifeTime - lifeTime/2)
            .Append( tmp.DOColor(colorAlphaZero, lifeTime/4) );

        Destroy(gameObject, mySequence.Duration());
    }

    public void Init (int healAmount, bool isStamina = false) {
        int x = Random.value < 0.5f ? 1 : -1;

        Vector3 dir = (PlayerControlls.instance.transform.right * x  + Vector3.up) * Random.Range(speed*0.5f, speed*1.3f);
        dir = Vector3.ClampMagnitude(dir, 10);
        GetComponent<Rigidbody>().AddForce(dir, ForceMode.Impulse);

        TextMeshPro tmp = transform.GetChild(0).GetComponent<TextMeshPro>();

        if (healAmount != 0) {
            tmp.text = healAmount.ToString();
            tmp.color = isStamina ? Color.cyan : Color.green;
        } else {
            tmp.text = "No effect";
            tmp.color = Color.white;
        }

        transform.localScale = Vector3.zero;

        Color colorAlphaZero = tmp.color;
        colorAlphaZero.a = 0;

        Sequence mySequence = DOTween.Sequence()
            .Append( transform.DOScale(1, lifeTime/4) )
            .AppendInterval(lifeTime - lifeTime/2)
            .Append( tmp.DOColor(colorAlphaZero, lifeTime/4) );

        Destroy(gameObject, mySequence.Duration());
    }

    void Update() {
        transform.LookAt(PlayerControlls.instance.playerCamera.transform);
    }
}
