using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem.Wind
{
    // ReSharper disable once UnusedMember.Global
    public class VSWindController : IWindController
    {
        public string WindControlerID { get; }
        private WindControllerSettings _windControllerSettings;

        public Texture2D WindWaveTexture;
        public float WindWaveSize;
        public float WindSpeed;

        private readonly int _awDir;
        private readonly int _awWavesTex;

public WindControllerSettings Settings
        {
            get { return _windControllerSettings; }
            set
            {
                _windControllerSettings = value;
                RefreshSettings();
            }
        }

        public VSWindController()
        {
            WindControlerID = "VSWindController";
            _awDir = Shader.PropertyToID("_AW_DIR");
            _awWavesTex = Shader.PropertyToID("_AW_WavesTex");
        }
        

        public WindControllerSettings CreateDefaultSettings()
        {
            Settings = new WindControllerSettings
            {
                WindControlerID = WindControlerID,
                Heading = "Vegetation Studio Grass Wind Settings"
            };

            Settings.AddTextureProperty("AW_WavesTex", "Wind Waves", "", Resources.Load("PerlinSeamless") as Texture2D);
            Settings.AddFloatProperty("WindWaveSize", "Wind Wave Size", "", 10, 0, 30);
            Settings.AddFloatProperty("WindSpeed", "Wind Speed", "", 1f, 0, 3);
            RefreshSettings();
            return Settings;
        }

        public void RefreshSettings()
        {
            WindSpeed = Settings.GetFloatPropertyValue("WindSpeed");
            WindWaveSize = Settings.GetFloatPropertyValue("WindWaveSize");
            WindWaveTexture = Settings.GetTexturePropertyValue("AW_WavesTex");
        }

        public void UpdateWind(WindZone windZone, float windSpeedFactor)
        {
            Vector3 dir = Vector3.forward;
            Vector4 dir4;

            if (windZone)
            {
                dir = windZone.transform.forward;
                dir4 = new Vector4(dir.x, Mathf.Abs(windZone.windMain) * WindSpeed * windSpeedFactor, dir.z, WindWaveSize);
            }
            else
            {
                dir4 = new Vector4(dir.x, 1 * WindSpeed * windSpeedFactor, dir.z, WindWaveSize);
            }           
            Shader.SetGlobalVector(_awDir, dir4);

            if (WindWaveTexture)
            {
                Shader.SetGlobalTexture(_awWavesTex, WindWaveTexture);
            }


        }
    }
}
