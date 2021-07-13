using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  [ExecuteInEditMode]
  public class OrbitingBody : MonoBehaviour
  {

    private Transform m_PositionTransform;
    public Transform positionTransform {
      get { 
        if (m_PositionTransform == null) {
          m_PositionTransform = this.transform.Find("Position");
        }
        return m_PositionTransform;
      }
    }

    private RotateBody m_RotateBody;
    public RotateBody rotateBody {
      get {
        if (m_RotateBody == null) {
          Transform t = positionTransform;
          if (!t) {
            Debug.LogError("Can't return rotation body without a position transform game object");
            return null;
          }

          m_RotateBody = t.GetComponent<RotateBody>();
        }
        
        return m_RotateBody;
      }
    }

    // Position of the orbiting body.
    private SpherePoint m_SpherePoint = new SpherePoint(0, 0);
    public SpherePoint Point
    {
      get { return m_SpherePoint; }
      set
      {
        if (m_SpherePoint == null)
        {
          m_SpherePoint = new SpherePoint(0, 0);
        }
        else
        {
          m_SpherePoint = value;
        }

        m_CachedWorldDirection = m_SpherePoint.GetWorldDirection();
        LayoutOribit();
      }
    }

    // Direction to orbiting body.
    private Vector3 m_CachedWorldDirection = Vector3.right;
    public Vector3 BodyGlobalDirection { get { return m_CachedWorldDirection; } }

    private Light m_BodyLight;
    public Light BodyLight
    {
      get {
        if (m_BodyLight == null) {
          m_BodyLight = transform.GetComponentInChildren<Light>();
          if (m_BodyLight != null)
          {
            // Reset in case it was rotated from older prefab or developer.
            m_BodyLight.transform.localRotation = Quaternion.identity;
          }
        }

        return m_BodyLight;
      }
    }
    
    public void ResetOrbit() {
      LayoutOribit();
      m_PositionTransform = null;
    }

    public void LayoutOribit()
    {
      transform.position = Vector3.zero;
      transform.rotation = Quaternion.identity;
      transform.forward = BodyGlobalDirection * -1.0f;
    }

    void OnValidate()
    {
      LayoutOribit();
    }
  }
}
