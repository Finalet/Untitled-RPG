using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius = 20f;
	[Range(0,360)] public float viewAngle = 120f;
    public float eyeLevel = 1.6f;

	[Space]
    public Transform target;
	[Space]
	public LayerMask targetMask;
	public LayerMask raycastIgnoreMask;

	[Space]
    [DisplayWithoutEdit()] public float distanceToTarget;
    [DisplayWithoutEdit()] public bool isTargetVisible;

	void Start() {    
		StartCoroutine ("FindTargetsWithDelay", .2f);
	}

	public void InitForEnemy() {
		targetMask = LayerMask.GetMask("Player");
        raycastIgnoreMask = ~LayerMask.GetMask("Enemy", "Player"); //ignoring player since we are checking IF DID NOT HIT so we dont want raycast to hit
		StartCoroutine(WaitForPlayer());
	}

	IEnumerator WaitForPlayer() {
		while(PlayerControlls.instance == null) {
			yield return null;
		}
        target = PlayerControlls.instance.chestTransform;
	}

	IEnumerator FindTargetsWithDelay(float delay) {
		while (true) {
			yield return new WaitForSeconds (delay);
			isTargetVisible = targetVisible();
		}
	}

	bool targetVisible() {
		if (target == null)
            return false;

        distanceToTarget = Vector3.Distance(transform.position + Vector3.up * eyeLevel, target.transform.position);
        
        if (distanceToTarget > viewRadius)
            return false;
        
		Vector3 dirToTarget = (target.position - (transform.position + Vector3.up * eyeLevel) ).normalized;

        if (Vector3.Angle (transform.forward, dirToTarget) > viewAngle / 2) 
            return false;
            
        if (!Physics.Raycast (transform.position + Vector3.up * eyeLevel, dirToTarget, distanceToTarget, raycastIgnoreMask)) {
            return true;
        }
        return false;
	}


	public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal) {
		if (!angleIsGlobal) {
			angleInDegrees += transform.eulerAngles.y;
		}
		return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),0,Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
	}
}
