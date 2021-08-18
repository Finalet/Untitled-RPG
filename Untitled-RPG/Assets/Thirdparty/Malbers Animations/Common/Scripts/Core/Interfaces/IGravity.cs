using UnityEngine;
namespace MalbersAnimations
{
    /// <summary>Interface used to set Game Objects with a Local Gravity Direction</summary>
    public interface IGravity
    {
        Vector3 Gravity { get; set; }
        Vector3 UpVector { get; }
    }
}