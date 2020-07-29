using AwesomeTechnologies.VegetationStudio;
using UnityEngine;

public class LightLoader : MonoBehaviour {
	public Light Light;
	void OnEnable()
	{
		if (Light == null) return;

		VegetationStudioManager.SetSunDirectionalLight(Light);
	}

	void OnDisable()
	{
		if (Light == null) return;
		VegetationStudioManager.SetSunDirectionalLight(null);
	}

	private void Reset()
	{
		Light = this.GetComponent<Light>();
	}
}
