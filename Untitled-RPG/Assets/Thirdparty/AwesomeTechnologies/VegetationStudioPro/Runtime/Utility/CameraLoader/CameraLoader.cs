using System.Collections;
using System.Collections.Generic;
using AwesomeTechnologies.VegetationStudio;
using UnityEngine;

public class CameraLoader : MonoBehaviour
{
	public Camera Camera;
	
	void OnEnable()
	{
		if (Camera == null) return;

		VegetationStudioManager.AddCamera(Camera,false,true);
	}

	void OnDisable()
	{
		if (Camera == null) return;
		VegetationStudioManager.RemoveCamera(Camera);
	}

	private void Reset()
	{
		Camera = this.GetComponent<Camera>();
	}
}
