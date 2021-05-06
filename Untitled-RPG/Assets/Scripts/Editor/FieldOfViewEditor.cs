using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (FieldOfView))]
public class FieldOfViewEditor : Editor
{
    void OnSceneGUI() {
		FieldOfView fow = (FieldOfView)target;
		Handles.color = Color.white;
		Handles.DrawWireArc (fow.transform.position + Vector3.up * fow.eyeLevel, Vector3.up, Vector3.forward, 360, fow.viewRadius);
		Vector3 viewAngleA = fow.DirFromAngle (-fow.viewAngle / 2, false);
		Vector3 viewAngleB = fow.DirFromAngle (fow.viewAngle / 2, false);

		Handles.DrawLine (fow.transform.position + Vector3.up * fow.eyeLevel, fow.transform.position + Vector3.up * fow.eyeLevel + viewAngleA * fow.viewRadius);
		Handles.DrawLine (fow.transform.position + Vector3.up * fow.eyeLevel, fow.transform.position + Vector3.up * fow.eyeLevel + viewAngleB * fow.viewRadius);

		Handles.color = Color.red;
        if (fow.isTargetVisible)
            Handles.DrawLine (fow.transform.position + Vector3.up * fow.eyeLevel, fow.target.position);
	}
}
