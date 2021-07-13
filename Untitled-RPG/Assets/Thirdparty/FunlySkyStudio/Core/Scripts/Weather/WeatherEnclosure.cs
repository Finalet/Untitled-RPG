using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  [RequireComponent(typeof(MeshRenderer))]
  public class WeatherEnclosure : MonoBehaviour
  {
    public Vector2 nearTextureTiling = new Vector3(1, 1);
    public Vector2 farTextureTiling = new Vector2(1, 1);
  }
}

