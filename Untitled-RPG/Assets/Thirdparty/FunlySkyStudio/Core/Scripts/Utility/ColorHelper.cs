using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  public abstract class ColorHelper
  {
    public static Color ColorWithHex(uint hex)
    {
      return ColorWithHexAlpha(hex << 8 | 0x000000FF);
    }

    public static Color ColorWithHexAlpha(uint hex)
    {
      float r = ((hex >> 24) & 0xFF) / 255.0f;
      float g = ((hex >> 16) & 0xFF) / 255.0f;
      float b = ((hex >> 8) & 0xFF) / 255.0f;
      float a = ((hex) & 0xFF) / 255.0f;

      return new Color(r, g, b, a);
    }
  }

  
}

