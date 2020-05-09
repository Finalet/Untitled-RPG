using UnityEngine;

namespace MalbersAnimations
{
    public class UseTransform : MonoBehaviour
    {
        public enum UpdateMode                                          // The available methods of updating are:
        {
            Update = 1,
            FixedUpdate = 2,                                            // Update in FixedUpdate (for tracking rigidbodies).
            LateUpdate = 4,                                             // Update in LateUpdate. (for tracking objects that are moved in Update)
        }


        [Tooltip("Transform to use the Position as Reference")]
        public Transform Reference;
        [Tooltip("Use the Reference's Rotation")]
        public bool rotation = true;  
        [Tooltip("Use the Reference's Position")]
        public bool position = true;


        public UpdateMode updateMode = UpdateMode.LateUpdate;

        // Update is called once per frame
        void Update()
        {
            if (updateMode == UpdateMode.Update) SetTransformReference();
        }

        void LateUpdate()
        {
            if (updateMode == UpdateMode.LateUpdate) SetTransformReference();
        }

        void FixedUpdate()
        {
            if (updateMode == UpdateMode.FixedUpdate) SetTransformReference();
        }

        private void SetTransformReference()
        {
            if (!Reference) return;
            if (position) transform.position = Reference.position;
            if (rotation) transform.rotation = Reference.rotation;
        }
    }
}