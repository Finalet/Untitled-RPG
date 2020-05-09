using UnityEngine;

namespace MalbersAnimations
{
    public interface IWayPoint : IAITarget
    {
        /// <summary>Returns the Next Target associated to the Current Waypoint</summary>
        Transform NextTarget();

        /// <summary>Transform associated to the WayPoint</summary>
        Transform WPTransform { get; }

        /// <summary>Wait time to go to the next Waypoint</summary>
        float WaitTime { get; }

        /// <summary>Call this method when someones arrives to the Waypoint</summary>
        void TargetArrived(GameObject target);
    }
}