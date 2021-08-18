using UnityEngine;

namespace MalbersAnimations
{
    public class FreeLockCameraManager : ScriptableObject
    {
        [Header("Aim States")]
        public FreeLookCameraState AimRight;
        public FreeLookCameraState AimLeft;

        internal MFreeLookCamera mCamera;

        public void SetCamera(MFreeLookCamera Freecamera) => mCamera = Freecamera;

        public void SetAimLeft(FreeLookCameraState state) => AimLeft = state;

        public void SetAimRight(FreeLookCameraState state) => AimRight = state;

        public void Target_Set(Transform tranform) => mCamera?.Target_Set(tranform);
        public void Target_Set_Temporal(Transform tranform) => mCamera?.Target_Set_Temporal(tranform);
        public void Target_Restore() => mCamera?.Target_Restore();

        public void ChangeFOV(float newFOV) => mCamera?.ChangeFOV(newFOV);

        public void ToggleFOV(bool val) => mCamera?.ToggleSprintFOV(val);


        /// <summary> When the Rider is Aiming is necesary to change the Update Mode to Late Update</summary>
        public virtual void ForceUpdateMode(bool val)
        {
           if (mCamera)
                mCamera.updateType = val ? UpdateType.LateUpdate : mCamera.defaultUpdate;
        }

        public virtual void SetAim(int ID)
        {
            if (mCamera)
            {
                if (ID == -1)
                    SetTemporalState(AimLeft);
                else if (ID == 1)
                    SetTemporalState(AimRight);
                else
                    SetState(mCamera.DefaultState);
            }
        }

        public virtual void SetState(FreeLookCameraState state) => SetState(state, false);

        public virtual void SetStateInstant(FreeLookCameraState state) => mCamera?.SetState_Instant(state, false);

        public virtual void SetTemporalState(FreeLookCameraState state) => SetState(state, true);

        public virtual void MobileMovement(Vector2 input) { if (mCamera) mCamera.MovementAxis.Value = input; }

        public virtual void Camera_EnableInput(bool enabled) => mCamera?.EnableInput(enabled);

        private void SetState(FreeLookCameraState state, bool temporal) => mCamera?.SetState(state, temporal);
    }
}