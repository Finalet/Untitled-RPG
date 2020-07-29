using System;
using AwesomeTechnologies.Common;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem.Wind
{
    public interface IWindController
    {
        String WindControlerID
        {
            get;
        }

        WindControllerSettings Settings
        {
            get;
            set;
        }

        WindControllerSettings CreateDefaultSettings();

        void RefreshSettings();

        void UpdateWind(WindZone windZone, float windSpeedFactor);       
    }

    [Serializable]
    public class WindControllerSettings : BaseControllerSettings 
    {
        public string Heading;
        public string Description;
        public string WindControlerID;
    }   

   
}