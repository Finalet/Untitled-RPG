using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicNPC : MonoBehaviour
{
    public bool isTalking;
    int talkID;
    float talkIDRefreshTimer;
    public bool switched;

    Animator animator;

    void Start() {
        animator = GetComponent<Animator>();
    }

    void Update() {
        if (isTalking) { //Every 2 seconds generate new talk ID;
            float normalizedTime = Mathf.Round(animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 * 100) / 100f;
            if (normalizedTime >= 0.9f && !switched) {
                CrossFadeNewTalkAnim();
            }
            if (normalizedTime >= 0.3f && normalizedTime <= 0.8f)
                switched = false;
        }
    }

    void CrossFadeNewTalkAnim () {
        talkID = Random.Range(1, 9);
        switched = true;
        switch (talkID) {
            case 1:
                animator.CrossFade("Talk.Talk1", 0.1f);
                break;
            case 2:
                animator.CrossFade("Talk.Talk2", 0.1f);
                break;
            case 3:
                animator.CrossFade("Talk.Talk3", 0.1f);
                break;
            case 4:
                animator.CrossFade("Talk.Talk4", 0.1f);
                break;
            case 5:
                animator.CrossFade("Talk.Talk5", 0.1f);
                break;
            case 6:
                animator.CrossFade("Talk.Talk6", 0.1f);
                break;
            case 7:
                animator.CrossFade("Talk.Talk7", 0.1f);
                break;
            case 8:
                animator.CrossFade("Talk.Talk8", 0.1f);
                break;
        }
    }

}
