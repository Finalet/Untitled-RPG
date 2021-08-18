using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>  Uses a transform as the UpVector  </summary>
    [AddComponentMenu("Malbers/Utilities/Tools/UpVector Transform")]

    public class UpVectorTransform : MonoBehaviour
    {
        public GameObjectReference source;
        private IGravity upVector;

        void Start()
        {
          if (source != null)  upVector = source.Value.GetComponentInChildren<IGravity>();
        }
        void Update()
        {
            if (upVector != null) transform.up = upVector.UpVector;
        }
    }
}
