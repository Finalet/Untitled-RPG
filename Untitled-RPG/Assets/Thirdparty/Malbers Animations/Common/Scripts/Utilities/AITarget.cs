using UnityEngine;

namespace MalbersAnimations
{
    [HelpURL("https://docs.google.com/document/d/1QBLQVWcDSyyWBDrrcS2PthhsToWkOayU0HUtiiSWTF8/edit#heading=h.21xyj464v85j")]
    /// <summary>
    /// This Script allows to set a gameObject as a Target for the Ai Logic. 
    /// So when an AI Animal sets a gameObject holding this script  as a target, 
    /// it can have a stopping distance and it can stop on a properly distance.
    /// </summary>
    public class AITarget : MonoBehaviour, IAITarget
    {
        [Tooltip("Distance for AI driven animals to stop when it has arrived to this gameObject as a Target.")]
        public float stoppingDistance = 1f;
        [Tooltip("Offset to correct the Position of the Target")]
        [SerializeField] private Vector3 center;
        
        /// <summary>Center of the Animal to be used for AI and Targeting  </summary>
        public Vector3 Center
        {
            private set => center = value; 
            get => transform.TransformPoint(center);
        }

        public void SetLocalCenter(Vector3 localCenter)
        {
            center = localCenter;
        }

        public Vector3 GetPosition()
        {
            return transform.TransformPoint(center);
        }

        public float StopDistance()
        {
            return stoppingDistance;
        }

        public WayPointType pointType = WayPointType.Ground;

        public WayPointType TargetType => pointType;



#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.color = Gizmos.color = Color.red;
            UnityEditor.Handles.DrawWireDisc(Center, transform.up, stoppingDistance);
            UnityEditor.Handles.DrawSolidDisc(Center, transform.up, stoppingDistance*0.066f);
        }
#endif
    }
}