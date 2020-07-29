using System;
using System.Collections.Generic;
using UnityEngine;


namespace AwesomeTechnologies.VegetationSystem
{
    [Serializable]
    public enum SerializedControlerPropertyType
    {
        Integer,
        Float,
        RgbaSelector,
        ColorSelector,
        Boolean,
        DropDownStringList,
        Label,
        Texture2D
    }

    [Serializable]
    public class SerializedControllerProperty
    {
        public SerializedControlerPropertyType SerializedControlerPropertyType;
        public string PropertyName;
        public string PropertyDescription;
        public string PropertyInfo;
        public int IntValue;
        public int IntMinValue;
        public int IntMaxValue;
        public float FloatValue;
        public float FloatMinValue;
        public float FloatMaxValue;
        public Color ColorValue;
        public bool BoolValue;
        public Texture2D TextureValue;
        public List<string> StringList = new List<string>();

        public SerializedControllerProperty(SerializedControllerProperty original)
        {
            SerializedControlerPropertyType = original.SerializedControlerPropertyType;
            PropertyName = original.PropertyName;
            PropertyDescription = original.PropertyDescription;
            PropertyInfo = original.PropertyInfo;
            IntValue = original.IntValue;
            IntMinValue = original.IntMinValue;
            IntMaxValue = original.IntMaxValue;
            FloatValue = original.FloatValue;
            FloatMinValue = original.FloatMinValue;
            FloatMaxValue = original.FloatMaxValue;
            ColorValue = original.ColorValue;
            BoolValue = original.BoolValue;
            TextureValue = original.TextureValue;
            StringList.AddRange(original.StringList);
        }

        public SerializedControllerProperty()
        {

        }
    }
}
