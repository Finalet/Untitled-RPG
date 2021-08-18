using UnityEngine;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Utilities/Tools/Forward Direction to Vector3")]

    public class ForwardDirToV3 : MonoBehaviour
    {
        [RequiredField] [Header("Tranform.Forward is the Direction")]
        public Vector3Var Direction;

        private void OnEnable()
        {
            if (Direction == null) enabled = false; //disable if it does not have a Vector3Var
        }
        void Update()
        {
          if (Direction.Value != transform.forward)
                Direction.SetValue(transform.forward);
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.blue;
            UnityEditor.Handles.ArrowHandleCap(0, transform.position, transform.rotation, 0.5f, EventType.Repaint);
#endif  
        }
    }
}
