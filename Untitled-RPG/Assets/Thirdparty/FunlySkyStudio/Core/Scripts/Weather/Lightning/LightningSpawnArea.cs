using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio {
  // This object marks areas where lightning bolts can be spawned.
  public class LightningSpawnArea : MonoBehaviour {

    [Tooltip("Dimensions of the lightning area where lightning bolts will be spawned inside randomly.")]
    public Vector3 lightningArea = new Vector3(40.0f, 20.0f, 20.0f);

    public void OnDrawGizmosSelected() {
      Vector3 size = transform.localScale;
      Gizmos.color = Color.yellow;
      Matrix4x4 previousMatrix = Gizmos.matrix;
      Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, lightningArea);
      Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

    void OnEnable()
    {
      LightningRenderer.AddSpawnArea(this);
    }

    private void OnDisable()
    {
      LightningRenderer.RemoveSpawnArea(this);
    }
  }
}
