using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skybox Definition", menuName = "Skies/Skybox Definition")]
public class SkyboxDefinitionScriptableObject : ScriptableObject
{
    public List<PeriodOfDay> periodsOfDay = new List<PeriodOfDay>();

    public float timeOfDay = 0f;

    public float realSecondsToGameHours = 0.1f;

    public float hoursForTransition = 1f;

    public float sunBaseRotationY = 0f;

    public float lightIntensity = 1f;

    public Vector2 cloudThresholdRange = new Vector2(0.1f, 0.5f);
    public float cloudSpeed = 1f;
    public float sunSize = 0.04f;

    public Color sunColor = Color.white;

    public float sunInfluenceSize = 1.2f;
    public float sunInfluenceIntensity = 0.005f;

    public float cloudSharpness = 0.2f;

    public Color cloudColor = Color.white;
    public Color cloudShadingColor = Color.white;

    public float cloudShadingThreshold = 0.1f;

    public float cloudShadingSharpness = 0.2f;

    public float cloudShadingStrength = 0.5f;

    public float sunCloudInfluence = 0.01f;
    public float skyColorCloudInfluence = 0.5f;

    public float cloudOpacity = 0.8f;

    public float alternateUVAtZenith = 0.2f;

    public Texture2D cloudTexture1 = null;
    public Texture2D cloudTexture2 = null;

    public Color starColor = Color.white;
    public Vector2 starBrightnessMinMax1 = new Vector2(0.8f, 1.2f);
    public Vector2 starBrightnessMinMax2 = new Vector2(0.9f, 1.1f);
    public float starFlickeringFrequency1 = 8f;
    public float starFlickeringFrequency2 = 12f;
    public float starFlickerMaskDensity = 50f;
    public float starDensity = 30f;
    public float starMaskDensity = 15f;
    public float starPowerMultiplier = 60f;

    public float ditherStrength = 0.3f;

    public Vector2 texture1ZenithTiling = new Vector2(0.5f, 0.5f);
    public Vector2 texture2ZenithTiling = new Vector2(0.25f, 0.25f);
    public Vector2 texture1HorizonTiling = new Vector2(1f, 1f);
    public Vector2 texture2HorizonTiling = new Vector2(2f, 1f);

    public bool useHorizonColorForFog = true;
    public float fogColorBlend = 0.3f;
    public Color baseFogColor = new Color(0.8f, 0.8f, 0.8f);
}
