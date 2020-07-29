using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AwesomeTechnologies.VegetationSystem
{
    [Serializable]
    public class EnvironmentSettings
    {
        public float SnowAmount = 0;
        public float SnowMinHeight = 0;
        public float RainAmount = 0;
        public Color SnowColor = new Color(0.75f,0.75f,0.75f,1);
        public Color SnowSpecularColor = new Color(0.2f,0.2f,0.2f,0.25f);
        public Color BillboardSnowColor = new Color(0.75f,0.75f,0.75f,1);
        public float SnowBlendFactor = 2.75f;
        public float SnowBrightness = 1;
    }
}
