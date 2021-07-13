using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Funly.SkyStudio;

/**
	This script is simple example showing how to modify a "Sky Profile" at runtime.
	This script modifies the color of the "sky middle color" in the background gradient.
	Note, that the TimeOfDayController has enabled "Copy Sky Profile" so that our modifications
	do not change the original "Sky Profile" that's in your project.
 */
public class ModifySkyAtRuntime : MonoBehaviour {
	[Range(0, 1)]
	public float speed = .15f;

	void Update () {
		// Grab the SkyProfile from the current TimeOfDayController in your scene.
		SkyProfile profile = TimeOfDayController.instance.skyProfile;

		// Get the "group" that manages all the animation keyframes for the property you want to modify.
		// This method is templated since there are a few different types of keyframes supported. You
		// can also fetch groups using these group class types:
		// 		GetGroup<NumberKeyframeGroup>(...)
		// 		GetGroup<TextureKeyframeGroup>(...)
		// 		GetGroup<BoolKeyframeGroup>(...)
		// 		GetGroup<SpherePointKeyframeGroup>(...) // Used to position sun and moon
		ColorKeyframeGroup group = profile.GetGroup<ColorKeyframeGroup>(
			ProfilePropertyKeys.SkyMiddleColorKey);
		
		// A group contains an array of keyframs. If you're not using the Sky Timeline
		// just grab the first keyframe (there is always at least 1).
		ColorKeyframe keyframe = group.keyframes[0];

		// You can modify the keyframe at runtime by accessing it's properties.
		float rollingHue = (Time.timeSinceLevelLoad * speed) % 1.0f;
		keyframe.color = Color.HSVToRGB(rollingHue, .8f, .8f);
		
		// You can do this all in 1 line if you like, let's update the upper sky to match now.
		profile.GetGroup<ColorKeyframeGroup>(
			ProfilePropertyKeys.SkyUpperColorKey).keyframes[0].color = keyframe.color;

		// After you modify a SkyProfile, you'll need to update the sky so it refreshes
		// (unless you have automatic time increment enabled).
		TimeOfDayController.instance.UpdateSkyForCurrentTime();
	}
}
