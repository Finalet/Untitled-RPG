using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // Subclass to remove templating, so we can serialize this class.
  [Serializable]
  public class ColorGroupDictionary : SerializableDictionary<string, ColorKeyframeGroup>
  {
  }
}

