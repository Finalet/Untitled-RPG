using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public enum DamageTextColor {Regular, White, LightBlue, Green, Cyan};

public class ddText : MonoBehaviour
{
    public float speed = 1;
    public float lifeTime = 2;
    public TextMeshPro tmp;
    public Rigidbody rb;
    
    [Header("Materials")]
    public Material regular;
    public Material white;
    public Material lightBlue;
    public Material green;
    public Material cyan;

    Color orangeCol = new Color (1, 0.5f, 0);
    Color redCol = new Color(1, 0.1f, 0);

    public void Init (string text, DamageTextColor dtc = DamageTextColor.Regular) {
        int x = Random.value < 0.5f ? 1 : -1;

        Vector3 dir = (PlayerControlls.instance.transform.right * x  + Vector3.up * 1.5f) * Random.Range(speed*0.5f, speed*1.3f) * 1.5f;
        dir = Vector3.ClampMagnitude(dir, 10);
        rb.AddForce(dir, ForceMode.Impulse);

        tmp.text = text;
        tmp.fontSharedMaterial = getMaterial(dtc);

        transform.localScale = Vector3.zero;

        LaunchSequence();
    }

    public void Init (DamageInfo damageInfo) {
        int x = Random.value < 0.5f ? 1 : -1;

        Vector3 dir = (PlayerControlls.instance.transform.right * x  + Vector3.up) * Random.Range(speed*0.5f, speed*1.3f) * (1+(float)damageInfo.damage/4000f) * (damageInfo.isCrit ? 1.4f : 1);
        dir = Vector3.ClampMagnitude(dir, 10);
        rb.AddForce(dir, ForceMode.Impulse);

        if (damageInfo.damage != 0) {
            tmp.text = (damageInfo.isCrit ? "CRITICAL " : "") + damageInfo.damage.ToString();
            tmp.fontSharedMaterial = regular;
            if (damageInfo.damageType == DamageType.Enemy)
                tmp.color = redCol;
            else 
                tmp.color = damageInfo.isCrit ? redCol : orangeCol;
        } else {
            tmp.text = "No damage";
            tmp.fontSharedMaterial = white;
        }

        transform.localScale = Vector3.zero;

       LaunchSequence(damageInfo.isCrit ? 1.2f : 1);
    }

    public void Init (int effectAmount, DamageTextColor dtc) {
        int x = Random.value < 0.5f ? 1 : -1;

        Vector3 dir = (PlayerControlls.instance.transform.right * x  + Vector3.up) * Random.Range(speed*0.5f, speed*1.3f);
        dir = Vector3.ClampMagnitude(dir, 10);
        rb.AddForce(dir, ForceMode.Impulse);

        if (effectAmount != 0) {
            tmp.text = effectAmount.ToString();
            tmp.fontSharedMaterial = getMaterial(dtc);
        } else {
            tmp.text = "No effect";
            tmp.fontSharedMaterial = white;
        }

        transform.localScale = Vector3.zero;

        LaunchSequence();
    }

    void Update() {
        transform.LookAt(PlayerControlls.instance.playerCamera.transform);
    }

    void LaunchSequence (float scale = 1) {
        Color colorAlphaZero = tmp.color;
        colorAlphaZero.a = 0;

        Sequence mySequence = DOTween.Sequence()
            .Append( transform.DOScale(scale, lifeTime/4) )
            .AppendInterval(lifeTime - lifeTime/2)
            .Append( tmp.DOColor(colorAlphaZero, lifeTime/4) );

        Destroy(gameObject, mySequence.Duration());
    }

    Material getMaterial (DamageTextColor dtc) {
        switch (dtc) {
            case DamageTextColor.Regular: return regular;
            case DamageTextColor.White: return white;
            case DamageTextColor.LightBlue: return lightBlue;
            case DamageTextColor.Green: return green;
            case DamageTextColor.Cyan: return cyan;
            default: return regular;
        }
    }
}
