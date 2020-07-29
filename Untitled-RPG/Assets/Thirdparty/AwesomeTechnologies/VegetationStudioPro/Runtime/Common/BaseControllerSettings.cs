using System;
using System.Collections.Generic;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Common
{
    [Serializable]
    public class BaseControllerSettings
    {
        [SerializeField]
        public List<SerializedControllerProperty> ControlerPropertyList = new List<SerializedControllerProperty>();

        public void AddFloatProperty(string id, string description, string info, float defaultValue, float minValue, float maxValue)
        {
            SerializedControllerProperty serializedControllerProperty =
                new SerializedControllerProperty
                {
                    SerializedControlerPropertyType = SerializedControlerPropertyType.Float,
                    FloatValue = defaultValue,
                    FloatMinValue = minValue,
                    FloatMaxValue = maxValue,
                    PropertyName = id,
                    PropertyDescription = description,
                    PropertyInfo = info
                };

            ControlerPropertyList.Add(serializedControllerProperty);
        }

        public void AddLabelProperty(string text)
        {
            SerializedControllerProperty labelSerializedControllerProperty =
                new SerializedControllerProperty
                {
                    SerializedControlerPropertyType = SerializedControlerPropertyType.Label,
                    PropertyName = text,
                    PropertyDescription = text
                };

            ControlerPropertyList.Add(labelSerializedControllerProperty);
        }

        public void AddTextureProperty(string id, string description,string info, Texture2D defaultValue)
        {
            SerializedControllerProperty serializedControllerProperty =
                new SerializedControllerProperty
                {
                    SerializedControlerPropertyType = SerializedControlerPropertyType.Texture2D,
                    TextureValue = defaultValue,
                    PropertyName = id,
                    PropertyDescription = description,
                    PropertyInfo = info
                
                };

            ControlerPropertyList.Add(serializedControllerProperty);
        }

        public void AddRgbaSelectorProperty(string id, string description, string info, int defaultValue)
        {
            SerializedControllerProperty serializedControllerProperty =
                new SerializedControllerProperty
                {
                    SerializedControlerPropertyType = SerializedControlerPropertyType.RgbaSelector,
                    IntValue = defaultValue,
                    PropertyName = id,
                    PropertyDescription = description,
                    PropertyInfo = info
                };
            ControlerPropertyList.Add(serializedControllerProperty);
        }

        public void AddColorProperty(string id, string description, string info, Color defaultValue)
        {
            SerializedControllerProperty serializedControllerProperty =
                new SerializedControllerProperty
                {
                    SerializedControlerPropertyType = SerializedControlerPropertyType.ColorSelector,
                    ColorValue = defaultValue,
                    PropertyName = id,
                    PropertyInfo = "",
                    PropertyDescription = description
                };

            ControlerPropertyList.Add(serializedControllerProperty);
        }

        public void AddBooleanProperty(string id, string description, string info, bool defaultValue)
        {
            SerializedControllerProperty serializedControllerProperty =
                new SerializedControllerProperty
                {
                    SerializedControlerPropertyType = SerializedControlerPropertyType.Boolean,
                    BoolValue = defaultValue,
                    PropertyName = id,
                    PropertyInfo = info,
                    PropertyDescription = description
                };

            ControlerPropertyList.Add(serializedControllerProperty);
        }

        public int GetIntPropertyValue(string propertyName)
        {
            for (int i = 0;
                i <= ControlerPropertyList.Count - 1; i++)
            {
                if (ControlerPropertyList[i].PropertyName == propertyName)
                {
                    return ControlerPropertyList[i].IntValue;
                }
            }

            return 0;
        }

        public float GetFloatPropertyValue(string propertyName)
        {
            for (int i = 0;
                i <= ControlerPropertyList.Count - 1; i++)
            {
                if (ControlerPropertyList[i].PropertyName == propertyName)
                {
                    return ControlerPropertyList[i].FloatValue;
                }
            }

            return 0;
        }

        public Color GetColorPropertyValue(string propertyName)
        {
            for (int i = 0;
                i <= ControlerPropertyList.Count - 1; i++)
            {
                if (ControlerPropertyList[i].PropertyName == propertyName)
                {
                    return ControlerPropertyList[i].ColorValue;
                }
            }
            return Color.white;
        }

        public bool GetBooleanPropertyValue(string propertyName)
        {
            for (int i = 0;
                i <= ControlerPropertyList.Count - 1; i++)
            {
                if (ControlerPropertyList[i].PropertyName == propertyName)
                {
                    return ControlerPropertyList[i].BoolValue;
                }
            }
            return false;
        }

        public Texture2D GetTexturePropertyValue(string propertyName)
        {
            for (int i = 0;
                i <= ControlerPropertyList.Count - 1; i++)
            {
                if (ControlerPropertyList[i].PropertyName == propertyName)
                {
                    return ControlerPropertyList[i].TextureValue;
                }
            }
            return null;
        }
    }
}
