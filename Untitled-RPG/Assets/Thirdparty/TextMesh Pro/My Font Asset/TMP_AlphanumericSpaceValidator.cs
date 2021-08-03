using UnityEngine;
using System;


namespace TMPro
{
    [Serializable]
    [CreateAssetMenu(fileName = "InputValidator - Alphanumeric Space.asset", menuName = "TextMeshPro/Input Validators/Alphanumeric Space", order = 100)]
    public class TMP_AlphanumericSpaceValidator : TMP_InputValidator
    {
        // Custom text input validation function
        public override char Validate(ref string text, ref int pos, char ch)
        {
           if ( (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'z')  || (ch >= 'A' && ch <= 'Z') || (ch == ' ' && text.Length != 0) )
            {
                text = text.Insert(pos, ch.ToString());
                pos += 1;
                return ch;
            }
            return (char)0;
        }
    }
}

