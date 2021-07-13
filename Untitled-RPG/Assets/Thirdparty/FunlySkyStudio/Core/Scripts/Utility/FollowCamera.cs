using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // Moves the game component so it follows the main camera or a specific camera.
  [ExecuteInEditMode]
  public class FollowCamera : MonoBehaviour
  {
    public Camera followCamera;
    public Vector3 offset = Vector3.zero;

    void Update()
    {
      Camera followCamera;

      if (this.followCamera != null) {
        followCamera = this.followCamera;
      } else {
        followCamera = Camera.main;
      }

      if (followCamera == null) {
        return;
      }

      transform.position = followCamera.transform.TransformPoint(offset);
    }
  }
}

