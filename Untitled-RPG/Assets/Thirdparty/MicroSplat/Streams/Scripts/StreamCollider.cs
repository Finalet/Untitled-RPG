//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if __MICROSPLAT__ 
public class StreamCollider : MonoBehaviour 
{
   StreamManager streamMgr;

   public enum ColliderType
   {
      Water,
      Lava,
      Both
   }

   public ColliderType colliderType = ColliderType.Both;

   void OnDrawGizmos()
   {
      Gizmos.color = Color.yellow;
      Gizmos.DrawWireSphere(this.transform.position, this.transform.lossyScale.x);
   }

   void OnEnable()
   {
      var hits = Physics.RaycastAll(new Ray(transform.position + Vector3.up * 50, Vector3.down));
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