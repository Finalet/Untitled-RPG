using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem.Wind
{
    public class TreeCreatorWindController : IWindController
    {
        public string WindControlerID { get; }
        private WindControllerSettings _windControllerSettings;
        private int _wind;
        public WindControllerSettings Settings
        {
            get { return _windControllerSettings; }
            set
            {
                _windControllerSettings = value;
                RefreshSettings();
            }
        }

        public TreeCreatorWindController()
        {
            WindControlerID = "TreeCreatorWindController";
            _wind = Shader.PropertyToID("_Wind");
        }
        

        public WindControllerSettings CreateDefaultSettings()
        {
            Settings = new WindControllerSettings
            {
                WindControlerID = WindControlerID,
                Heading = "Tree creator wind"
            };
            return Settings;
        }

        public void RefreshSettings()
        {

        }

        public void UpdateWind(WindZone windZone, float windSpeedFactor)
        {
            Vector3 dir = Vector3.forward;
            Vector4 dir4;

            if (windZone)
            {
                dir = windZone.transform.forward;
                dir4 = new Vector4(dir.x, dir.y, dir.z, Mathf.Abs(windZone.windMain) * windSpeedFactor);
            }
            else
            {
                dir4 = new Vector4(dir.x, dir.y, dir.z, windSpeedFactor);
            }

            Shader.SetGlobalVector(_wind,dir4);
        }
    }
}
