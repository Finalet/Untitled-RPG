#if VEGETATION_STUDIO_PRO
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem.Wind
{
    // ReSharper disable once InconsistentNaming
    public class HDWindController : IWindController {
        public string WindControlerID { get; }
        private WindControllerSettings _windControllerSettings;

        public float WindSpeed = 30;
        public float Turbulence = 0.25f;
        public Texture2D NoiseTexture;
        public float FlexNoiseWorldSize = 175.0f;
        public float ShiverNoiseWorldSize = 10.0f;
        public Texture2D GustMaskTexture;
        public float GustWorldSize = 600.0f;
        public float GustSpeed = 50;
        public float GustScale = 1.0f;

        private readonly int _windSettingsTexNoise;
        private readonly int _windSettingsTexGust;

        private readonly int _windSettingsWorldDirectionAndSpeed;
        private readonly int _windSettingsFlexNoiseScale;
        private readonly int _windSettingsShiverNoiseScale;
        private readonly int _windSettingsTurbulence;
        private readonly int _windSettingsGustSpeed;
        private readonly int _windSettingsGustScale;
        private readonly int _windSettingsGustWorldScale;

        public WindControllerSettings Settings
        {
            get { return _windControllerSettings; }
            set
            {
                _windControllerSettings = value;
                RefreshSettings();
            }
        }

        public HDWindController()
        {
            WindControlerID = "HDWindController";

            _windSettingsTexNoise = Shader.PropertyToID("WIND_SETTINGS_TexNoise");
            _windSettingsTexGust = Shader.PropertyToID("WIND_SETTINGS_TexGust");

            _windSettingsWorldDirectionAndSpeed = Shader.PropertyToID("WIND_SETTINGS_WorldDirectionAndSpeed");
            _windSettingsFlexNoiseScale = Shader.PropertyToID("WIND_SETTINGS_FlexNoiseScale");
            _windSettingsShiverNoiseScale = Shader.PropertyToID("WIND_SETTINGS_ShiverNoiseScale");
            _windSettingsTurbulence = Shader.PropertyToID("WIND_SETTINGS_Turbulence");
            _windSettingsGustSpeed = Shader.PropertyToID("WIND_SETTINGS_GustSpeed");
            _windSettingsGustScale = Shader.PropertyToID("WIND_SETTINGS_GustScale");
            _windSettingsGustWorldScale = Shader.PropertyToID("WIND_SETTINGS_GustWorldScale");
    }

        public WindControllerSettings CreateDefaultSettings()
        {
            Settings = new WindControllerSettings
            {
                WindControlerID = WindControlerID,
                Heading = "HD Wind Settings"
            };

            Settings.AddFloatProperty("WindSpeed", "Base Wind Speed (km/h)","", 45, 0, 120);
            Settings.AddFloatProperty("Turbulence", "Turbulence","", 0.4f, 0, 2);
            Settings.AddLabelProperty(" ");
            Settings.AddTextureProperty("3DNoise", "3D Noise", "",Resources.Load("3DNoise") as Texture2D);
            Settings.AddFloatProperty("FlexNoiseWorldSize", "Flex Noise World Size","", 150f, 0, 1000);
            Settings.AddFloatProperty("ShiverNoiseWorldSize", "Shiver Noise World Size","", 60, 0, 300);
            Settings.AddLabelProperty(" ");
            Settings.AddTextureProperty("GustNoise", "Gust Noise","", Resources.Load("GustNoise") as Texture2D);
            Settings.AddFloatProperty("GustWorldSize", "Gust World Size","", 600, 0, 2000);
            Settings.AddFloatProperty("GustSpeed", "Gust Speed","", 20, 0, 100);
            Settings.AddFloatProperty("GustScale", "Gust Scale","", 0.35f, 0, 5);

            RefreshSettings();
            return Settings;
        }

        public void RefreshSettings()
        {
            WindSpeed = Settings.GetFloatPropertyValue("WindSpeed");
            Turbulence = Settings.GetFloatPropertyValue("Turbulence");

            NoiseTexture = Settings.GetTexturePropertyValue("3DNoise");
            FlexNoiseWorldSize = Settings.GetFloatPropertyValue("FlexNoiseWorldSize");
            ShiverNoiseWorldSize = Settings.GetFloatPropertyValue("ShiverNoiseWorldSize");
            GustMaskTexture = Settings.GetTexturePropertyValue("GustNoise");
            GustWorldSize = Settings.GetFloatPropertyValue("GustWorldSize");
            GustSpeed = Settings.GetFloatPropertyValue("GustSpeed");
            GustScale = Settings.GetFloatPropertyValue("GustScale");
    }

        public void UpdateWind(WindZone windZone, float windSpeedFactor)
        {
            float speed = 1;
            if (windZone)
            {
                speed = windZone.windMain;
            }

            Shader.SetGlobalTexture(_windSettingsTexNoise, NoiseTexture);
            Shader.SetGlobalTexture(_windSettingsTexGust, GustMaskTexture);
            Shader.SetGlobalVector(_windSettingsWorldDirectionAndSpeed, GetDirectionAndSpeed(windZone ,windSpeedFactor));
            Shader.SetGlobalFloat(_windSettingsFlexNoiseScale, 1.0f / Mathf.Max(0.01f, FlexNoiseWorldSize));
            Shader.SetGlobalFloat(_windSettingsShiverNoiseScale, 1.0f / Mathf.Max(0.01f, ShiverNoiseWorldSize));
            Shader.SetGlobalFloat(_windSettingsTurbulence, WindSpeed * Turbulence * speed * windSpeedFactor);
            Shader.SetGlobalFloat(_windSettingsGustSpeed, GustSpeed);
            Shader.SetGlobalFloat(_windSettingsGustScale, GustScale);
            Shader.SetGlobalFloat(_windSettingsGustWorldScale, 1.0f / Mathf.Max(0.01f, GustWorldSize));
        }


        Vector4 GetDirectionAndSpeed(WindZone windZone, float windSpeedFactor)
        {
            Vector3 dir;
            float speed = 1;
            if (windZone)
            {
                dir = windZone.transform.forward.normalized;
                speed = windZone.windMain;
            }
            else
            {
                dir = Vector3.forward;
            }
            return new Vector4(dir.x, dir.y, dir.z, WindSpeed * 0.2777f * speed * windSpeedFactor);
        }
    }
}
#endif