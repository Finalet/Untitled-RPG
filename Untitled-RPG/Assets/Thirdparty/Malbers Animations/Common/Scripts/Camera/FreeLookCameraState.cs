using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>Used To change States from a camera to another</summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Camera/FreeLook Camera State")]
    public class FreeLookCameraState : ScriptableObject
    {
        public Vector3 PivotPos;
        public Vector3 CamPos;
        public float CamFOV = 45;
        public float transition = 1f;

        public FreeLookCameraState()
        {
            CamFOV = 45;
            PivotPos = new Vector3(0, 1f, 0);
            CamPos = new Vector3(0, 0, -5f);
            transition = 1f;
        }

        public FreeLookCameraState(float CamFOV, Vector3 PivotPos, Vector3 CamPos)
        {
            this.CamFOV = CamFOV;
            this.PivotPos = PivotPos;
            this.CamPos = CamPos;
            transition = 1f;
        }
    }
}
