using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

[ExecuteAlways]
public class TimeOfDayManager : MonoBehaviour
{
    [HideInInspector]
    public Light sun;

    public Material skyboxMaterial;

    
    public SkyboxDefinitionScriptableObject skyboxDefinition;
    
    private int periodIndex = 0;
    private int nextPeriodIndex = 0;
    
    private float timeHours = 0f;
    private float sunAngle;
    
    private float targetIntensity;
    
    public void Start()
    {
        sun = GetComponent<Light>();
        if(skyboxDefinition != null)
        {
            InitialSetup();
            Validations(); 
        }
        else
        {
            Debug.Log("Must set Skybox Definition");
        }
        
    }

    public void InitialSetup()
    {
        RenderSettings.skybox = skyboxMaterial;
        UpdateShaderProperties();
    }

    void UpdateShaderProperties()
    {
        skyboxMaterial.SetFloat("_SUNSIZE", skyboxDefinition.sunSize);
        skyboxMaterial.SetColor("_SUNCOLOR", skyboxDefinition.sunColor);
        skyboxMaterial.SetFloat("_SUNINFLUENCESIZE", skyboxDefinition.sunInfluenceSize);
        skyboxMaterial.SetFloat("_SUNINFLUENCEINTENSITY", skyboxDefinition.sunInfluenceIntensity);

        skyboxMaterial.SetTexture("_CLOUDTEXTURE1", skyboxDefinition.cloudTexture1);
        skyboxMaterial.SetTexture("_CLOUDTEXTURE2", skyboxDefinition.cloudTexture2);

        skyboxMaterial.SetVector("_TEXTURE1ZENITHTILING", skyboxDefinition.texture1ZenithTiling);
        skyboxMaterial.SetVector("_TEXTURE2ZENITHTILING", skyboxDefinition.texture2ZenithTiling);
        skyboxMaterial.SetVector("_TEXTURE1HORIZONTILING", skyboxDefinition.texture1HorizonTiling);
        skyboxMaterial.SetVector("_TEXTURE2HORIZONTILING", skyboxDefinition.texture2HorizonTiling);


        skyboxMaterial.SetFloat("_CLOUDSHARPNESS", skyboxDefinition.cloudSharpness);
        skyboxMaterial.SetColor("_CLOUDCOLOR", skyboxDefinition.cloudColor);
        skyboxMaterial.SetFloat("_CLOUDOPACITY", skyboxDefinition.cloudOpacity);
        skyboxMaterial.SetColor("_CLOUDSHADINGCOLOR", skyboxDefinition.cloudShadingColor);
        skyboxMaterial.SetFloat("_CLOUDSHADINGTHRESHOLD", skyboxDefinition.cloudShadingThreshold);
        skyboxMaterial.SetFloat("_CLOUDSHADINGSHARPNESS", skyboxDefinition.cloudShadingSharpness);
        skyboxMaterial.SetFloat("_CLOUDSHADINGSTRENGTH", skyboxDefinition.cloudShadingStrength);

        skyboxMaterial.SetFloat("_ALTERNATEUVATZENITH", skyboxDefinition.alternateUVAtZenith);
        skyboxMaterial.SetFloat("_SUNINFLUENCE", skyboxDefinition.sunCloudInfluence);
        skyboxMaterial.SetFloat("_SKYCOLORINFLUENCE", skyboxDefinition.skyColorCloudInfluence);

        skyboxMaterial.SetColor("_STARCOLOR", skyboxDefinition.starColor);
        skyboxMaterial.SetVector("_STARBRIGHTNESSMINMAX1", skyboxDefinition.starBrightnessMinMax1);
        skyboxMaterial.SetVector("_STARBRIGHTNESSMINMAX2", skyboxDefinition.starBrightnessMinMax2);
        skyboxMaterial.SetFloat("_STARFLICKERFREQUENCY1", skyboxDefinition.starFlickeringFrequency1);
        skyboxMaterial.SetFloat("_STARFLICKERFREQUENCY2", skyboxDefinition.starFlickeringFrequency2);
        skyboxMaterial.SetFloat("_STARFLICKERMASKDENSITY", skyboxDefinition.starFlickerMaskDensity);
        skyboxMaterial.SetFloat("_STARDENSITY", skyboxDefinition.starDensity);
        skyboxMaterial.SetFloat("_STARMASKDENSITY", skyboxDefinition.starMaskDensity);
        skyboxMaterial.SetFloat("_STARPOWERMULTIPLIER", skyboxDefinition.starPowerMultiplier);

        skyboxMaterial.SetFloat("_BLUENOISESTRENGTH", skyboxDefinition.ditherStrength);
    }

    private void Validations()
    {
        // Reorder the Periods of Day list so that they occur sorted by Start Time ascending.
        // We change the input (in the inspector) so that it is easy for the user to see if a change has taken place.
        // We also adjust the Start Time of all of the periods so that they are not too close to the subsequent period of day.
        // This code has some exceptions/use cases when you stack up a bunch of time periods close to each other, I'm not solving for it right now. Just avoid it.
        skyboxDefinition.periodsOfDay = skyboxDefinition.periodsOfDay.OrderBy(x => x.startTime).ToList();
        
        // Repair startTime based on hoursForTransition
        for(int i = 0; i < skyboxDefinition.periodsOfDay.Count - 1; i++)
        {
            if(skyboxDefinition.periodsOfDay[i].startTime > skyboxDefinition.periodsOfDay[i + 1].startTime - skyboxDefinition.hoursForTransition)
            {
                skyboxDefinition.periodsOfDay[i].startTime = skyboxDefinition.periodsOfDay[i + 1].startTime - skyboxDefinition.hoursForTransition;
            }
        }


        // Fix Minimum Cloud Threshold so that we don't get weird shading artifacts from the Cloud Threshold Smoothstep.
        if (skyboxDefinition.cloudThresholdRange.x > skyboxDefinition.cloudThresholdRange.y)
        {
            skyboxDefinition.cloudThresholdRange.x = skyboxDefinition.cloudThresholdRange.y;
        }
    }
    

    // Update is called once per frame
    public void Update()
    {
        Profiler.BeginSample("Skybox");
        if(skyboxDefinition != null)
        {

            if (Application.isEditor)
            {
                UpdateShaderProperties();
            }
            if (Application.isPlaying)
            {
                SetTimeOfDay();
            }

            SetLightDirection();
            SetLightIntensity();
            SetPeriod();
            SetSkyColors();
            SetCloudDensity();
            SetCloudSpeed();
        }
        else
        {
            Debug.Log("Must set Skybox Definition");
        }
        

        Profiler.EndSample();
    }


    private void SetTimeOfDay()
    {
        if (skyboxDefinition.timeOfDay > 24f)
        {
            skyboxDefinition.timeOfDay = skyboxDefinition.timeOfDay - 24f;
        }
        float t = Time.deltaTime * skyboxDefinition.realSecondsToGameHours; // One second in real life is one hour in-game. This can be modified with the realSecondsToGameHours variable (e.g., if realSecondsToGameHours is set to 2, then 1 second in real life corresponds to 2 hours in-game). 
        skyboxDefinition.timeOfDay += t;
        timeHours += t; // Tracking timehours separately for sampling perlin noise (we don't reset it at 24).
    }


    // Basically, the light direction is derived from the current time of day such that the sun is perfectly horizontal at 6am and 6pm for a 12hr "day" cycle and 12hr "night" cycle.
    // Not currently configurable.
    private void SetLightDirection()
    {
        float remappedTimeOfDay = skyboxDefinition.timeOfDay / 24f;
        sunAngle = (remappedTimeOfDay * 360f) - 90f;
        transform.rotation = Quaternion.Euler(sunAngle, skyboxDefinition.sunBaseRotationY, 0f); // Sun rotation is based on time of day
        skyboxMaterial.SetVector("_LIGHTDIRECTION", transform.forward);
    }


    // Prevents the light from clipping through the ground when it should be clipped by the horizon by setting intensity to 0. 
    // We define the "floor" here as 10 degrees below the horizon.
    private void SetLightIntensity()
    {
        targetIntensity = skyboxDefinition.lightIntensity;
        
        float sunAngleAdj = sunAngle + 90f; // Remaps sunangle from -90 -> 270f to 0 -> 360f (where 0 -> 90 is before sunrise and 270 -> 360f is after sunset)

        if(sunAngleAdj > 0f && sunAngleAdj < 80f)
        {
            sun.intensity = 0f;
        }
        else if (sunAngleAdj >= 80f && sunAngleAdj < 90f)
        {
            float t = Remap(sunAngleAdj, 80f, 90f, 0f, 1f);
            sun.intensity = Mathf.Lerp(0, targetIntensity, t);
        }
        else if (sunAngleAdj >= 90f && sunAngleAdj <= 260f)
        {
            sun.intensity = targetIntensity;
        }
        else if (sunAngleAdj >= 260f && sunAngleAdj < 270f)
        {
            float t = Remap(sunAngleAdj, 260f, 270f, 0f, 1f);
            sun.intensity = Mathf.Lerp(targetIntensity, 0f, t);
        }
        else
        {
            sun.intensity = 0f;
        }
    }


    // The SetSkyColors function depends on this to set the correct period.
    // Normally, this is straightforward. Transitioning from one day to the next is the tricky bit.
    private void SetPeriod()
    {
        if(skyboxDefinition.timeOfDay < skyboxDefinition.periodsOfDay[0].startTime)
        {
            periodIndex = skyboxDefinition.periodsOfDay.Count - 1;
        }
        else
        {
            for(int i = 0; i < skyboxDefinition.periodsOfDay.Count; i++)
            {
                if(skyboxDefinition.timeOfDay >= skyboxDefinition.periodsOfDay[i].startTime)
                {
                    periodIndex = i;
                }
            }
        }

        nextPeriodIndex = periodIndex + 1;
        if (nextPeriodIndex >= skyboxDefinition.periodsOfDay.Count)
        {
            nextPeriodIndex = 0;
        }
    }


    // Sets the Horizon and Zenith colors based on the current time of day.
    // As we approach the next time of day, we transition smoothly to the Horizon and Zenith colors for that time of day.
    private void SetSkyColors()
    {
        Color fogColor;

        if(skyboxDefinition.timeOfDay >= skyboxDefinition.periodsOfDay[nextPeriodIndex].startTime - skyboxDefinition.hoursForTransition && skyboxDefinition.timeOfDay < skyboxDefinition.periodsOfDay[nextPeriodIndex].startTime)
        {
            float t = Remap(skyboxDefinition.timeOfDay, skyboxDefinition.periodsOfDay[nextPeriodIndex].startTime - skyboxDefinition.hoursForTransition, skyboxDefinition.periodsOfDay[nextPeriodIndex].startTime, 0, 1);
            Color tempHorizon = Color.Lerp(skyboxDefinition.periodsOfDay[periodIndex].horizonColor, skyboxDefinition.periodsOfDay[nextPeriodIndex].horizonColor, t);
            skyboxMaterial.SetColor("_HORIZONCOLOR", tempHorizon);

            Color tempZenith = Color.Lerp(skyboxDefinition.periodsOfDay[periodIndex].zenithColor, skyboxDefinition.periodsOfDay[nextPeriodIndex].zenithColor, t);
            skyboxMaterial.SetColor("_ZENITHCOLOR", tempZenith);

            fogColor = tempHorizon;
        }
        else
        {
            skyboxMaterial.SetColor("_HORIZONCOLOR", skyboxDefinition.periodsOfDay[periodIndex].horizonColor);
            skyboxMaterial.SetColor("_ZENITHCOLOR", skyboxDefinition.periodsOfDay[periodIndex].zenithColor);

            fogColor = skyboxDefinition.periodsOfDay[periodIndex].horizonColor;
        }

        if (skyboxDefinition.useHorizonColorForFog)
        {
            RenderSettings.fogColor = Color.Lerp(skyboxDefinition.baseFogColor, fogColor, skyboxDefinition.fogColorBlend);
        }
        
        
    }


    private void SetCloudDensity()
    {
        float threshold = Remap(Mathf.Clamp01(Mathf.PerlinNoise(timeHours / 24f, 0)), 0f, 1f, skyboxDefinition.cloudThresholdRange.x, skyboxDefinition.cloudThresholdRange.y);
        skyboxMaterial.SetFloat("_CLOUDTHRESHOLD", threshold);
    }
    private void SetCloudSpeed()
    {
        skyboxMaterial.SetFloat("_CLOUDTEXTURESPEED", skyboxDefinition.cloudSpeed);
    }
    

    private float Remap(float value, float low1, float high1, float low2, float high2)
    {
        return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
    }
}


// Internal class used to include all PeriodOfDay information.
// I plan to replace this later with ScriptableObjects for ease of use.
[System.Serializable]
public class PeriodOfDay
{
    [SerializeField]
    [Tooltip("(Optional) Descriptive Name")]
    public string description;
    [SerializeField, Range(0f, 24f)]
    [Tooltip("Set the Start Time for this Period of Day")]
    public float startTime;
    [SerializeField, ColorUsage(false, true)]
    [Tooltip("Set the Horizon Color for this Period of Day")]
    public Color horizonColor;
    [SerializeField, ColorUsage(false, true)]
    [Tooltip("Set the Zenith Color for this Period of Day")]
    public Color zenithColor;

    public PeriodOfDay(string desc, float start, Color horizon, Color zenith)
    {
        description = desc;
        startTime = start;
        horizonColor = horizon;
        zenithColor = zenith;
    }
}
