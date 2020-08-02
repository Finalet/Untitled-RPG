//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if __MICROSPLAT__ 
public class StreamEmitter : MonoBehaviour 
{
   public enum EmitterType
   {
      Water,
      Lava
   }

   public EmitterType emitterType = EmitterType.Water;

   [Range(0, 1)]
   public float strength = 1;

   void OnDrawGizmos()
   {
      Gizmos.color = (emitterType == EmitterType.Water) ? Color.blue : Color.red;
      Gizmos.DrawWireSphere(this.transform.position, this.transform.lossyScale.x);
   }

   StreamManager streamMgr;
   void OnEnable()
   {
      var hits = Physics.RaycastAll(new Ray(transform.position + Vector3.up * 10, Vector3.down));
      for (int i = 0; i < hits.Length; ++i)
      {
         var sm = hits[i].collider.GetComponent<StreamManager>();
#if __MICROSPLAT_MESHTERRAIN__
         if (sm == null && hits [i].collider.transform.parent != null)
         {
            sm = hits[i].collider.transform.parent.GetComponent<StreamManager>();
         }
#endif
         if (sm != null)
         {
            streamMgr = sm;
            sm.Register(this);
            break;
         }

      }
   }

   void OnDisable()
   {
      if (streamMgr != null)
      {
         streamMgr.Unregister(this);
         streamMgr = null;
      }
   }

}
#endif