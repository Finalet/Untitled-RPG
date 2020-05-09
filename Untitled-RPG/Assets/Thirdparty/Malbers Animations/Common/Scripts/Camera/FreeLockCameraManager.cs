using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Utilities;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations
{
    [CreateAssetMenu(menuName = "Malbers Animations/Camera/FreeLook Camera Manager")]
    public class FreeLockCameraManager : ScriptableObject
    { 
        [Header("Rider Aim States")]
        public FreeLookCameraState AimRight;
        public FreeLookCameraState AimLeft;

        internal MFreeLookCamera mCamera;

        public void SetCamera(MFreeLookCamera Freecamera) { mCamera = Freecamera; }

        public void SetAimLeft(FreeLookCameraState state) { AimLeft = state; }
        public void SetAimRight(FreeLookCameraState state) { AimRight = state; }

        public void ChangeTarget(Transform tranform)
        {
            mCamera.SetTarget(tranform);
        }  


        public void ChangeFOV(float newFOV)
        {
            mCamera.ChangeFOV(newFOV); 
        } 

        public void ToggleFOV(bool val)
        {
            mCamera.ToggleSprintFOV(val);
        } 

       
        /// <summary> When the Rider is Aiming is necesary to change the Update Mode to Late Update</summary>
        public virtual void ForceUpdateMode(bool val)
        {
            mCamera.updateType = val ? MFreeLookCamera.UpdateType.LateUpdate : mCamera.defaultUpdate;
        }

        public virtual void SetAim(int ID)
        {
            if (ID == -1)
                SetTemporalState(AimLeft);
            else if (ID == 1)
                SetTemporalState(AimRight);
            else
                SetState(mCamera.DefaultState);
        }

        public virtual void SetState(FreeLookCameraState state) { SetState(state, false); }

        public virtual void SetTemporalState(FreeLookCameraState state) { SetState(state, true); }

        public virtual void MobileMovement(Vector2 input)
        {
            mCamera.XCam = input.x;
            mCamera.YCam = input.y;
        }

        public virtual void CameraDefaultInput(bool enabled)
        {
            mCamera.Vertical.active = enabled;
            mCamera.Horizontal.active = enabled;
        }

        private void SetState(FreeLookCameraState state, bool temporal)
        {
            if (state == null) return;
            if (mCamera == null) return;
            mCamera.SetState(state, temporal);
        }
    }
}