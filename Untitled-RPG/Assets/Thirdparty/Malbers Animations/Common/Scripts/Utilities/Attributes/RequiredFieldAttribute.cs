using UnityEngine;

namespace MalbersAnimations
{
    public class RequiredFieldAttribute : PropertyAttribute
    {
        public Color color;

        public RequiredFieldAttribute(FieldColor Fieldcolor =  FieldColor.Red)
        {
            switch (Fieldcolor)
            {
                case FieldColor.Red:
                    color = Color.red;
                    break;
                case FieldColor.Green:
                    color = Color.green;
                    break;
                case FieldColor.Blue:
                    color = Color.blue;
                    break;
                case FieldColor.Magenta:
                    color = Color.magenta;
                    break;
                case FieldColor.Cyan:
                    color = Color.cyan;
                    break;
                case FieldColor.Yellow:
                    color = Color.yellow;
                    break;
                case FieldColor.Orange:
                    color = new Color(1, 0.5f, 0);
                    break;
                case FieldColor.Gray:
                    color = Color.gray;
                    break;
                default:
                    color = Color.red;
                    break;
            }
        }

        public RequiredFieldAttribute()
        {
            color = new Color(1,0,0,0.7f);
        }
    }
}