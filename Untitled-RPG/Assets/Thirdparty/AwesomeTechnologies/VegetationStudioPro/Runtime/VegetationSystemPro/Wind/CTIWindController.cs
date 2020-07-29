using UnityEngine;


namespace AwesomeTechnologies.VegetationSystem.Wind
{
    // ReSharper disable once UnusedMember.Global
    public class CTIWindController : IWindController
    {
        public string WindControlerID { get; }
        private WindControllerSettings _windControllerSettings;
        public float WindSpeed;

        private readonly int _terrainLODWind = Shader.PropertyToID("_TerrainLODWind");

        public WindControllerSettings Settings
        {
            get { return _windControllerSettings; }
            set
            {
                _windControllerSettings = value;
                RefreshSettings();
            }
        }

        public CTIWindController()
        {
            WindControlerID = "CTIWindController";
        }

        public WindControllerSettings CreateDefaultSettings()
        {
            Settings = new WindControllerSettings
            {
                WindControlerID = WindControlerID,
                Heading = "CTI Wind Settings"
            };
            Settings.AddFloatProperty("WindSpeed", "Wind Speed", "", 1f, 0, 3);
            RefreshSettings();
            return Settings;
        }

        public void RefreshSettings()
        {
            WindSpeed = Settings.GetFloatPropertyValue("WindSpeed");
        }

        public void UpdateWind(WindZone windZone, float windSpeedFactor)
        {
            Vector3 windDirection;
            float windStrength;
            float windTurbulence;

            if (windZone)
            {
                windDirection = windZone.transform.forward;
                windStrength = windZone.windMain * windSpeedFactor * WindSpeed;
                windStrength += windZone.windPulseMagnitude * (1.0f + Mathf.Sin(Time.time * windZone.windPulseFrequency) + 1.0f + Mathf.Sin(Time.time * windZone.windPulseFrequency * 3.0f)) * 0.5f;
                windTurbulence = windZone.windTurbulence * windZone.windMain * windSpeedFactor * WindSpeed;
            }
            else
            {
                windDirection =Vector3.forward;
                windStrength = 1 * windSpeedFactor * WindSpeed;
                windStrength += 1 * (1.0f + Mathf.Sin(Time.time) + 1.0f + Mathf.Sin(Time.time * 3.0f)) * 0.5f;
                windTurbulence = windSpeedFactor * WindSpeed;
            }

            windDirection.x *= windStrength;
            windDirection.y *= windStrength;
            windDirection.z *= windStrength;
            Shader.SetGlobalVector(_terrainLODWind, new Vector4(windDirection.x, windDirection.y, windDirection.z, windTurbulence));
        }
    }
}
