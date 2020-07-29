using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {      
        public void AddCamera(Camera aCamera, bool noFrustumCulling = false, bool renderDirectToCamera = false, bool renderBillboardsOnly = false)
        {
            _prepareVegetationHandle.Complete();

            VegetationStudioCamera vegetationStudioCamera = GetVegetationStudioCamera(aCamera);
            if (vegetationStudioCamera == null)
            {
                vegetationStudioCamera = new VegetationStudioCamera(aCamera)
                {
                    CameraCullingMode = CameraCullingMode.Frustum,
                    RenderDirectToCamera = renderDirectToCamera,
                    RenderBillboardsOnly = renderBillboardsOnly,
                    VegetationSystemPro = this
                };

                AddVegetationStudioCamera(vegetationStudioCamera);
            }

            SetupWindSamplers();
            SetupVegetationItemModelsPerCameraBuffers();
            //PrepareRenderLists();
        }

        private void AddVegetationStudioCamera(VegetationStudioCamera vegetationStudioCamera)
        {
            VegetationStudioCameraList.Add(vegetationStudioCamera);
            OnAddCameraDelegate?.Invoke(vegetationStudioCamera);

            RefreshColliderSystem();
            RefreshRuntimePrefabSpawner();
        }

        public void RemoveCamera(Camera aCamera)
        {
            _prepareVegetationHandle.Complete();

            VegetationStudioCamera vegetationStudioCamera = GetVegetationStudioCamera(aCamera);
            if (vegetationStudioCamera != null)
            {
                RemoveVegetationStudioCamera(vegetationStudioCamera);

            }
            
            SetupWindSamplers();
            SetupVegetationItemModelsPerCameraBuffers();
            
            RefreshColliderSystem();
            RefreshRuntimePrefabSpawner();
        }

        private void RemoveVegetationStudioCamera(VegetationStudioCamera vegetationStudioCamera)
        {
            vegetationStudioCamera.Dispose();
            VegetationStudioCameraList.Remove(vegetationStudioCamera);                
            OnRemoveCameraDelegate?.Invoke(vegetationStudioCamera);
        }
        
        public VegetationStudioCamera GetVegetationStudioCamera(Camera aCamera)
        {
            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                if (VegetationStudioCameraList[i].SelectedCamera == aCamera)
                {
                    return VegetationStudioCameraList[i];
                }
            }
            return null;
        }

        public VegetationStudioCamera GetSceneViewVegetationStudioCamera()
        {
            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                if (VegetationStudioCameraList[i].VegetationStudioCameraType == VegetationStudioCameraType.SceneView)
                {
                    return VegetationStudioCameraList[i];
                }
            }
            
            return null;
        }
        
        public void DisposeVegetationStudioCameras()
        {
            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                VegetationStudioCameraList[i].Dispose();
            }
        }

        public void RemoveVegetationStudioCameraDelegates()
        {
            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                VegetationStudioCameraList[i].Dispose();
            }
        }

        private void SetVegetationStudioCamerasDirty()
        {
            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                VegetationStudioCameraList[i].SetDirty();
            }
        }
    }
}