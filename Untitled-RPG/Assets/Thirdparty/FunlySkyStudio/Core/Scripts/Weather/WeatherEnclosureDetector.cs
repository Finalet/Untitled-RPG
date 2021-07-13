using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // Detect collisions with enclosures to change the weather enclosure as player moves around.
  public class WeatherEnclosureDetector : MonoBehaviour
  {
    [Tooltip("Default enclosure used when not inside the trigger of another enclosure area.")]
    public WeatherEnclosure mainEnclosure;

    List<WeatherEnclosure> triggeredEnclosures = new List<WeatherEnclosure>();

    public RainDownfallController rainController;

    // Callback for when the weather enclosure changes.
    public Action<WeatherEnclosure> enclosureChangedCallback;

    private void Start()
    {
      ApplyEnclosure();
    }

    private void OnEnable()
    {
      ApplyEnclosure();
    }

    private void OnTriggerEnter(Collider other)
    {
      WeatherEnclosure enclosure = other.gameObject.GetComponentInChildren<WeatherEnclosure>();
      if (!enclosure) {
        return;
      }

      if (triggeredEnclosures.Contains(enclosure)) {
        triggeredEnclosures.Remove(enclosure);
      }

      triggeredEnclosures.Add(enclosure);

      ApplyEnclosure();
    }

    private void OnTriggerExit(Collider other)
    {
      WeatherEnclosure enclosure = other.gameObject.GetComponentInChildren<WeatherEnclosure>();
      if (!enclosure) {
        return;
      }

      if (!triggeredEnclosures.Contains(enclosure)) {
        return;
      }

      triggeredEnclosures.Remove(enclosure);

      ApplyEnclosure();
    }

    public void ApplyEnclosure()
    {
      WeatherEnclosure enclosure;
      if (triggeredEnclosures.Count > 0) {
        enclosure = triggeredEnclosures[triggeredEnclosures.Count - 1];

        if (!enclosure) {
          Debug.LogError("Failed to find mesh renderer on weather enclosure, using main enclosure instead.");
          enclosure = mainEnclosure;
        }
      } else {
        enclosure = mainEnclosure;
      }

      if (enclosureChangedCallback != null) {
        enclosureChangedCallback(enclosure);
      }
    }

  }
}

