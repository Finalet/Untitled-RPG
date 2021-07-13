using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // Generic interface for plugins that need to update as the sky profile animates (rain, lightning, etc).
  public interface ISkyModule
  {
    void UpdateForTimeOfDay(SkyProfile skyProfile, float timeOfDay);
  }
}
