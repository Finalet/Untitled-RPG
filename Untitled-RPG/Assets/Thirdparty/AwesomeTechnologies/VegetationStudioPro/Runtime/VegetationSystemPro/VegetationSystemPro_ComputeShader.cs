using System;
using System.Collections.Generic;
using System.Reflection;
using AwesomeTechnologies.Utility.Culling;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace AwesomeTechnologies.VegetationSystem
{
    public static class GeometryUtilityAllocFree
    {
        public static Plane[] FrustrumPlanes = new Plane[6];

        private static readonly Action<Plane[], Matrix4x4> InternalExtractPlanes =
            (Action<Plane[], Matrix4x4>)Delegate.CreateDelegate(
                typeof(Action<Plane[], Matrix4x4>),
                // ReSharper disable once AssignNullToNotNullAttribute
                typeof(GeometryUtility).GetMethod("Internal_ExtractPlanes",
                    BindingFlags.Static | BindingFlags.NonPublic));

        public static void CalculateFrustrumPlanes(Camera camera)
        {
            InternalExtractPlanes(FrustrumPlanes, camera.projectionMatrix * camera.worldToCameraMatrix);
        }
    }

    public partial class VegetationSystemPro
    {
        private void SetFrustumCullingPlanes(Camera selectedCamera)
        {
            GeometryUtilityAllocFree.CalculateFrustrumPlanes(selectedCamera);

            Vector4 cameraFrustumPlane0 = new Vector4(GeometryUtilityAllocFree.FrustrumPlanes[0].normal.x, GeometryUtilityAllocFree.FrustrumPlanes[0].normal.y, GeometryUtilityAllocFree.FrustrumPlanes[0].normal.z,
                GeometryUtilityAllocFree.FrustrumPlanes[0].distance);
            Vector4 cameraFrustumPlane1 = new Vector4(GeometryUtilityAllocFree.FrustrumPlanes[1].normal.x, GeometryUtilityAllocFree.FrustrumPlanes[1].normal.y, GeometryUtilityAllocFree.FrustrumPlanes[1].normal.z,
                GeometryUtilityAllocFree.FrustrumPlanes[1].distance);
            Vector4 cameraFrustumPlane2 = new Vector4(GeometryUtilityAllocFree.FrustrumPlanes[2].normal.x, GeometryUtilityAllocFree.FrustrumPlanes[2].normal.y, GeometryUtilityAllocFree.FrustrumPlanes[2].normal.z,
                GeometryUtilityAllocFree.FrustrumPlanes[2].distance);
            Vector4 cameraFrustumPlane3 = new Vector4(GeometryUtilityAllocFree.FrustrumPlanes[3].normal.x, GeometryUtilityAllocFree.FrustrumPlanes[3].normal.y, GeometryUtilityAllocFree.FrustrumPlanes[3].normal.z,
                GeometryUtilityAllocFree.FrustrumPlanes[3].distance);
            Vector4 cameraFrustumPlane4 = new Vector4(GeometryUtilityAllocFree.FrustrumPlanes[4].normal.x, GeometryUtilityAllocFree.FrustrumPlanes[4].normal.y, GeometryUtilityAllocFree.FrustrumPlanes[4].normal.z,
                GeometryUtilityAllocFree.FrustrumPlanes[4].distance);
            Vector4 cameraFrustumPlane5 = new Vector4(GeometryUtilityAllocFree.FrustrumPlanes[5].normal.x, GeometryUtilityAllocFree.FrustrumPlanes[5].normal.y, GeometryUtilityAllocFree.FrustrumPlanes[5].normal.z,
                GeometryUtilityAllocFree.FrustrumPlanes[5].distance);

            FrusumMatrixShader.SetVector(_cameraFrustumPlan0, cameraFrustumPlane0);
            FrusumMatrixShader.SetVector(_cameraFrustumPlan1, cameraFrustumPlane1);
            FrusumMatrixShader.SetVector(_cameraFrustumPlan2, cameraFrustumPlane2);
            FrusumMatrixShader.SetVector(_cameraFrustumPlan3, cameraFrustumPlane3);
            FrusumMatrixShader.SetVector(_cameraFrustumPlan4, cameraFrustumPlane4);
            FrusumMatrixShader.SetVector(_cameraFrustumPlan5, cameraFrustumPlane5);

            Vector3 worldSpaceCameraPosition = selectedCamera.transform.position;
            Vector4 worldSpaceCameraPos = new Vector4(worldSpaceCameraPosition.x, worldSpaceCameraPosition.y, worldSpaceCameraPosition.z, 1);
            //TODO change to ID on setVector
            FrusumMatrixShader.SetVector("_WorldSpaceCameraPos", worldSpaceCameraPos);
        }

        void SetupComputeShaders()
        {
            //_dummyMaskTexture = new Texture2D(2, 2);
            _dummyComputeBuffer = new ComputeBuffer(1, (16 * 4) + 16, ComputeBufferType.Default); // *2

            MergeBufferShader = (ComputeShader)Resources.Load("MergeInstancedIndirectBuffers");
            MergeBufferKernelHandle = MergeBufferShader.FindKernel("MergeInstancedIndirectBuffers");

            FrusumMatrixShader = (ComputeShader)Resources.Load("GPUFrustumCulling");
            FrustumKernelHandle = FrusumMatrixShader.FindKernel("GPUFrustumCulling");
            _mergeBufferID = Shader.PropertyToID("MergeBuffer");

            _floatingOriginOffsetID = Shader.PropertyToID("_FloatingOriginOffset");

            _mergeSourceBuffer0ID = Shader.PropertyToID("MergeSourceBuffer0");
            _mergeSourceBuffer1ID = Shader.PropertyToID("MergeSourceBuffer1");
            _mergeSourceBuffer2ID = Shader.PropertyToID("MergeSourceBuffer2");
            _mergeSourceBuffer3ID = Shader.PropertyToID("MergeSourceBuffer3");
            _mergeSourceBuffer4ID = Shader.PropertyToID("MergeSourceBuffer4");
            _mergeSourceBuffer5ID = Shader.PropertyToID("MergeSourceBuffer5");
            _mergeSourceBuffer6ID = Shader.PropertyToID("MergeSourceBuffer6");
            _mergeSourceBuffer7ID = Shader.PropertyToID("MergeSourceBuffer7");
            _mergeSourceBuffer8ID = Shader.PropertyToID("MergeSourceBuffer8");
            _mergeSourceBuffer9ID = Shader.PropertyToID("MergeSourceBuffer9");
            _mergeSourceBuffer10ID = Shader.PropertyToID("MergeSourceBuffer10");
            _mergeSourceBuffer11ID = Shader.PropertyToID("MergeSourceBuffer11");
            _mergeSourceBuffer12ID = Shader.PropertyToID("MergeSourceBuffer12");
            _mergeSourceBuffer13ID = Shader.PropertyToID("MergeSourceBuffer13");
            _mergeSourceBuffer14ID = Shader.PropertyToID("MergeSourceBuffer14");

            _mergeInstanceCount0ID = Shader.PropertyToID("MergeSourceBufferCount0");
            _mergeInstanceCount1ID = Shader.PropertyToID("MergeSourceBufferCount1");
            _mergeInstanceCount2ID = Shader.PropertyToID("MergeSourceBufferCount2");
            _mergeInstanceCount3ID = Shader.PropertyToID("MergeSourceBufferCount3");
            _mergeInstanceCount4ID = Shader.PropertyToID("MergeSourceBufferCount4");
            _mergeInstanceCount5ID = Shader.PropertyToID("MergeSourceBufferCount5");
            _mergeInstanceCount6ID = Shader.PropertyToID("MergeSourceBufferCount6");
            _mergeInstanceCount7ID = Shader.PropertyToID("MergeSourceBufferCount7");
            _mergeInstanceCount8ID = Shader.PropertyToID("MergeSourceBufferCount8");
            _mergeInstanceCount9ID = Shader.PropertyToID("MergeSourceBufferCount9");
            _mergeInstanceCount10ID = Shader.PropertyToID("MergeSourceBufferCount10");
            _mergeInstanceCount11ID = Shader.PropertyToID("MergeSourceBufferCount11");
            _mergeInstanceCount12ID = Shader.PropertyToID("MergeSourceBufferCount12");
            _mergeInstanceCount13ID = Shader.PropertyToID("MergeSourceBufferCount13");
            _mergeInstanceCount14ID = Shader.PropertyToID("MergeSourceBufferCount14");

            _cameraFrustumPlan0 = Shader.PropertyToID("_VS_CameraFrustumPlane0");
            _cameraFrustumPlan1 = Shader.PropertyToID("_VS_CameraFrustumPlane1");
            _cameraFrustumPlan2 = Shader.PropertyToID("_VS_CameraFrustumPlane2");
            _cameraFrustumPlan3 = Shader.PropertyToID("_VS_CameraFrustumPlane3");
            _cameraFrustumPlan4 = Shader.PropertyToID("_VS_CameraFrustumPlane4");
            _cameraFrustumPlan5 = Shader.PropertyToID("_VS_CameraFrustumPlane5");

            _instanceCountID = Shader.PropertyToID("_InstanceCount");
            _sourceBufferID = Shader.PropertyToID("SourceShaderDataBuffer");
            _visibleBufferLod0ID = Shader.PropertyToID("VisibleBufferLOD0");
            _visibleBufferLod1ID = Shader.PropertyToID("VisibleBufferLOD1");
            _visibleBufferLod2ID = Shader.PropertyToID("VisibleBufferLOD2");
            _visibleBufferLod3ID = Shader.PropertyToID("VisibleBufferLOD3");
            
            _shadowBufferLod0ID = Shader.PropertyToID("ShadowBufferLOD0");
            _shadowBufferLod1ID = Shader.PropertyToID("ShadowBufferLOD1");
            _shadowBufferLod2ID = Shader.PropertyToID("ShadowBufferLOD2");
            _shadowBufferLod3ID = Shader.PropertyToID("ShadowBufferLOD3");
            
            _lightDirection = Shader.PropertyToID("_LightDirection");
            _planeOrigin = Shader.PropertyToID("_PlaneOrigin");
            _boundsSize = Shader.PropertyToID("_BoundsSize");
            
            _cullFarStartID = Shader.PropertyToID("_CullFarStart");
            _visibleShaderDataBufferID = Shader.PropertyToID("VisibleShaderDataBuffer");
            _indirectShaderDataBufferID = Shader.PropertyToID("IndirectShaderDataBuffer");

            _useLodsID = Shader.PropertyToID("UseLODs");
            _noFrustumCullingID = Shader.PropertyToID("NoFrustumCulling");           
            _shadowCullingID = Shader.PropertyToID("ShadowCulling");
            
            _boundingSphereRadiusID = Shader.PropertyToID("_BoundingSphereRadius");

            _lod1Distance = Shader.PropertyToID("_LOD1Distance");
            _lod2Distance = Shader.PropertyToID("_LOD2Distance");
            _lod3Distance = Shader.PropertyToID("_LOD3Distance");

            _lodFactor = Shader.PropertyToID("_LODFactor");
            _lodBias = Shader.PropertyToID("_LODBias");
            _lodFadeDistance = Shader.PropertyToID("_LODFadeDistance");
            _lodCount = Shader.PropertyToID("_LODCount");
        }

        void DisposeComputeShaders()
        {
            _dummyComputeBuffer?.Dispose();
        }

        void DrawCellsIndirectComputeShader()
        {
            Profiler.BeginSample("Draw instanced indirect vegetation");

            float lodBias = QualitySettings.lodBias * VegetationSettings.LODDistanceFactor;

            Vector4 floatingOriginOffsetVector4 = new Vector4(FloatingOriginOffset.x,FloatingOriginOffset.y,FloatingOriginOffset.z,0);

            Vector3 sunLightDirection = SunDirectionalLight ? SunDirectionalLight.transform.forward : new Vector3(0, 0, 0);
            float minBoundsHeight = VegetationSystemBounds.center.y - VegetationSystemBounds.extents.y;
            Vector3 planeOrigin = new Vector3(0, minBoundsHeight, 0);
            bool shadowCulling = (SunDirectionalLight != null);
            
            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                if (!VegetationStudioCameraList[i].Enabled) continue;
                if (VegetationStudioCameraList[i].RenderBillboardsOnly) continue;
                
                SetFrustumCullingPlanes(VegetationStudioCameraList[i].SelectedCamera);

                var targetCamera = VegetationStudioCameraList[i].RenderDirectToCamera
                    ? VegetationStudioCameraList[i].SelectedCamera
                    : null;

                for (int j = 0; j <= VegetationPackageProList.Count - 1; j++)
                {
                    for (int k = 0; k <= VegetationPackageProList[j].VegetationInfoList.Count - 1; k++)
                    {
                        VegetationItemInfoPro vegetationItemInfo = VegetationPackageProList[j].VegetationInfoList[k];
                        if (vegetationItemInfo.VegetationRenderMode != VegetationRenderMode.InstancedIndirect) continue;

                        VegetationItemModelInfo vegetationItemModelInfo =
                            VegetationPackageProModelsList[j].VegetationItemModelList[k];

                      
                        
                        float vegetationItemCullDistance;

                        float renderDistanceFactor =vegetationItemModelInfo.VegetationItemInfo.RenderDistanceFactor;
                        if (VegetationSettings.DisableRenderDistanceFactor) renderDistanceFactor = 1;

                        //if (vegetationItemModelInfo.DistanceBand == 0)
                        if (vegetationItemModelInfo.VegetationItemInfo.VegetationType == VegetationType.Tree || vegetationItemModelInfo.VegetationItemInfo.VegetationType == VegetationType.LargeObjects)
                        {
                            vegetationItemCullDistance = VegetationSettings.GetTreeDistance() * renderDistanceFactor;                            
                        }
                        else
                        {
                            vegetationItemCullDistance = VegetationSettings.GetVegetationDistance() * renderDistanceFactor;
                        }                    

                        ShadowCastingMode shadowCastingMode =
                            VegetationSettings.GetShadowCastingMode(vegetationItemInfo.VegetationType);

                        if (vegetationItemInfo.DisableShadows)
                        {
                            shadowCastingMode = ShadowCastingMode.Off;
                        }
                        
                        bool useShadowCulling = shadowCulling && shadowCastingMode == ShadowCastingMode.On;
                        useShadowCulling = useShadowCulling && vegetationItemModelInfo.DistanceBand == 1;

                        LayerMask layer = VegetationSettings.GetLayer(vegetationItemInfo.VegetationType);

                        int totalInstanceCount = 0;
                        _hasBufferList.Clear();
                        for (int l = 0;
                            l <= VegetationStudioCameraList[i].JobCullingGroup.VisibleCellIndexList.Length - 1;
                            l++)
                        {
                            int potentialVisibleCellIndex =
                                VegetationStudioCameraList[i].JobCullingGroup.VisibleCellIndexList[l];
                            VegetationCell vegetationCell = VegetationStudioCameraList[i]
                                .PotentialVisibleCellList[potentialVisibleCellIndex];

                            BoundingSphereInfo boundingSphereInfo = VegetationStudioCameraList[i]
                                .GetBoundingSphereInfo(potentialVisibleCellIndex);

                            int vegetationItemDistanceBand = vegetationItemModelInfo.DistanceBand;
                            
                            if (boundingSphereInfo.CurrentDistanceBand > vegetationItemDistanceBand)
                            {
                                continue;
                            }

                            if (vegetationCell.VegetationPackageInstancesList[j].VegetationItemMatrixList[k].Length == 0)
                            {
                                continue;
                            }
                            
                            ComputeBufferInfo computeBufferInfo = vegetationCell.VegetationPackageInstancesList[j]
                                .VegetationItemComputeBufferList[k];
                            if (!computeBufferInfo.Created)
                            {
                                continue;
                            }

                            _hasBufferList.Add(vegetationCell);
                        }

                        if (_hasBufferList.Count == 0) continue;

                        int buffercount = 15;
                        for (int m = 0; m <= _hasBufferList.Count - 1; m++)
                        {
                            totalInstanceCount += _hasBufferList[m].VegetationPackageInstancesList[j]
                                .VegetationItemMatrixList[k].Length;
                        }

                        if (totalInstanceCount == 0) continue;

                        CameraComputeBuffers cameraComputeBuffers = vegetationItemModelInfo.CameraComputeBufferList[i];

                        if (totalInstanceCount > cameraComputeBuffers.MergeBuffer.count)
                        {
                            cameraComputeBuffers.UpdateComputeBufferSize(totalInstanceCount + 5000);
                        }

                        cameraComputeBuffers.MergeBuffer.SetCounterValue(0);
                        MergeBufferShader.SetBuffer(MergeBufferKernelHandle, _mergeBufferID,
                            cameraComputeBuffers.MergeBuffer);

                        for (int m = 0; m <= _hasBufferList.Count - 1; m += buffercount)
                        {
                            int instanceCount0 = _hasBufferList[m].VegetationPackageInstancesList[j]
                                .VegetationItemMatrixList[k].Length;

                            for (int n = 1; n <= buffercount - 1; n++)
                            {
                                if (m + n < _hasBufferList.Count)
                                {
                                    int tempInstanceCount = _hasBufferList[m + n].VegetationPackageInstancesList[j]
                                        .VegetationItemMatrixList[k].Length;
                                    if (tempInstanceCount > instanceCount0) instanceCount0 = tempInstanceCount;
                                }
                            }

                            // ReSharper disable once RedundantCast
                            int threadGroups = Mathf.CeilToInt((float)instanceCount0 / 32f);
                            if (threadGroups == 0) continue;

                            SetComputeShaderBuffer(_mergeSourceBuffer0ID, _mergeInstanceCount0ID, m, j, k);
                            SetComputeShaderBuffer(_mergeSourceBuffer1ID, _mergeInstanceCount1ID, m + 1, j, k);
                            SetComputeShaderBuffer(_mergeSourceBuffer2ID, _mergeInstanceCount2ID, m + 2, j, k);
                            SetComputeShaderBuffer(_mergeSourceBuffer3ID, _mergeInstanceCount3ID, m + 3, j, k);
                            SetComputeShaderBuffer(_mergeSourceBuffer4ID, _mergeInstanceCount4ID, m + 4, j, k);
                            SetComputeShaderBuffer(_mergeSourceBuffer5ID, _mergeInstanceCount5ID, m + 5, j, k);
                            SetComputeShaderBuffer(_mergeSourceBuffer6ID, _mergeInstanceCount6ID, m + 6, j, k);
                            SetComputeShaderBuffer(_mergeSourceBuffer7ID, _mergeInstanceCount7ID, m + 7, j, k);
                            SetComputeShaderBuffer(_mergeSourceBuffer8ID, _mergeInstanceCount8ID, m + 8, j, k);
                            SetComputeShaderBuffer(_mergeSourceBuffer9ID, _mergeInstanceCount9ID, m + 9, j, k);
                            SetComputeShaderBuffer(_mergeSourceBuffer10ID, _mergeInstanceCount10ID, m + 10, j, k);
                            SetComputeShaderBuffer(_mergeSourceBuffer11ID, _mergeInstanceCount11ID, m + 11, j, k);
                            SetComputeShaderBuffer(_mergeSourceBuffer12ID, _mergeInstanceCount12ID, m + 12, j, k);
                            SetComputeShaderBuffer(_mergeSourceBuffer13ID, _mergeInstanceCount13ID, m + 13, j, k);
                            SetComputeShaderBuffer(_mergeSourceBuffer14ID, _mergeInstanceCount14ID, m + 14, j, k);

                            MergeBufferShader.Dispatch(MergeBufferKernelHandle, threadGroups, 1, 1);
                        }

                        for (int n = 0; n <= vegetationItemModelInfo.VegetationMeshLod0.subMeshCount - 1; n++)
                        {
                            ComputeBuffer.CopyCount(cameraComputeBuffers.MergeBuffer,
                                cameraComputeBuffers.ArgsBufferMergedLOD0List[n], sizeof(uint) * 1);
                        }

                        int threadGroupsFrustum = Mathf.CeilToInt(totalInstanceCount / 32f);
                        if (threadGroupsFrustum == 0) continue;

                        cameraComputeBuffers.VisibleBufferLOD0.SetCounterValue(0);
                        cameraComputeBuffers.VisibleBufferLOD1.SetCounterValue(0);
                        cameraComputeBuffers.VisibleBufferLOD2.SetCounterValue(0);
                        cameraComputeBuffers.VisibleBufferLOD3.SetCounterValue(0);
                        
                        cameraComputeBuffers.ShadowBufferLOD0.SetCounterValue(0);
                        cameraComputeBuffers.ShadowBufferLOD1.SetCounterValue(0);
                        cameraComputeBuffers.ShadowBufferLOD2.SetCounterValue(0);
                        cameraComputeBuffers.ShadowBufferLOD3.SetCounterValue(0);

                        bool useLODs = true;

                        FrusumMatrixShader.SetFloat(_cullFarStartID, vegetationItemCullDistance);

                        FrusumMatrixShader.SetVector(_floatingOriginOffsetID, floatingOriginOffsetVector4);

                        FrusumMatrixShader.SetBuffer(FrustumKernelHandle, _sourceBufferID,
                            cameraComputeBuffers.MergeBuffer);
                        FrusumMatrixShader.SetBuffer(FrustumKernelHandle, _visibleBufferLod0ID,
                            cameraComputeBuffers.VisibleBufferLOD0);
                        FrusumMatrixShader.SetBuffer(FrustumKernelHandle, _visibleBufferLod1ID,
                            cameraComputeBuffers.VisibleBufferLOD1);
                        FrusumMatrixShader.SetBuffer(FrustumKernelHandle, _visibleBufferLod2ID,
                            cameraComputeBuffers.VisibleBufferLOD2);
                        FrusumMatrixShader.SetBuffer(FrustumKernelHandle, _visibleBufferLod3ID,
                            cameraComputeBuffers.VisibleBufferLOD3);
                        
                        FrusumMatrixShader.SetBuffer(FrustumKernelHandle, _shadowBufferLod0ID,
                            cameraComputeBuffers.ShadowBufferLOD0);
                        FrusumMatrixShader.SetBuffer(FrustumKernelHandle, _shadowBufferLod1ID,
                            cameraComputeBuffers.ShadowBufferLOD1);
                        FrusumMatrixShader.SetBuffer(FrustumKernelHandle, _shadowBufferLod2ID,
                            cameraComputeBuffers.ShadowBufferLOD2);
                        FrusumMatrixShader.SetBuffer(FrustumKernelHandle, _shadowBufferLod3ID,
                            cameraComputeBuffers.ShadowBufferLOD3);
                        
                        FrusumMatrixShader.SetInt(_instanceCountID, totalInstanceCount);
                        FrusumMatrixShader.SetBool(_useLodsID, useLODs);
                        FrusumMatrixShader.SetBool(_noFrustumCullingID, VegetationStudioCameraList[i].CameraCullingMode == CameraCullingMode.Complete360);
                        FrusumMatrixShader.SetBool(_shadowCullingID, useShadowCulling);
                        
                        FrusumMatrixShader.SetFloat(_boundingSphereRadiusID,
                            vegetationItemModelInfo.BoundingSphereRadius);

                        FrusumMatrixShader.SetFloat(_lod1Distance, vegetationItemModelInfo.LOD1Distance);
                        FrusumMatrixShader.SetFloat(_lod2Distance, vegetationItemModelInfo.LOD2Distance);
                        FrusumMatrixShader.SetFloat(_lod3Distance, vegetationItemModelInfo.LOD3Distance);
                        
                        FrusumMatrixShader.SetVector(_lightDirection, sunLightDirection);
                        FrusumMatrixShader.SetVector(_planeOrigin, planeOrigin);
                        FrusumMatrixShader.SetVector(_boundsSize, vegetationItemModelInfo.VegetationItemInfo.Bounds.size);                                                                       

                        FrusumMatrixShader.SetFloat(_lodFactor, vegetationItemInfo.LODFactor);
                        FrusumMatrixShader.SetFloat(_lodBias, lodBias * 2);
                        FrusumMatrixShader.SetFloat(_lodFadeDistance, 10);
                        FrusumMatrixShader.SetInt(_lodCount, vegetationItemModelInfo.LODCount);

                        FrusumMatrixShader.Dispatch(FrustumKernelHandle, threadGroupsFrustum, 1, 1);

                        for (int n = 0; n <= vegetationItemModelInfo.VegetationMeshLod0.subMeshCount - 1; n++)
                        {
                            ComputeBuffer.CopyCount(cameraComputeBuffers.VisibleBufferLOD0,
                                cameraComputeBuffers.ArgsBufferMergedLOD0List[n], sizeof(uint) * 1);
                            
                            ComputeBuffer.CopyCount(cameraComputeBuffers.ShadowBufferLOD0,
                                cameraComputeBuffers.ShadowArgsBufferMergedLOD0List[n], sizeof(uint) * 1);
                        }

                        if (useLODs)
                        {
                            for (int n = 0; n <= vegetationItemModelInfo.VegetationMeshLod1.subMeshCount - 1; n++)
                            {
                                ComputeBuffer.CopyCount(cameraComputeBuffers.VisibleBufferLOD1,
                                    cameraComputeBuffers.ArgsBufferMergedLOD1List[n], sizeof(uint) * 1);
                                
                                ComputeBuffer.CopyCount(cameraComputeBuffers.ShadowBufferLOD1,
                                    cameraComputeBuffers.ShadowArgsBufferMergedLOD1List[n], sizeof(uint) * 1);
                            }

                            for (int n = 0; n <= vegetationItemModelInfo.VegetationMeshLod2.subMeshCount - 1; n++)
                            {                                                            
                                ComputeBuffer.CopyCount(cameraComputeBuffers.VisibleBufferLOD2,
                                    cameraComputeBuffers.ArgsBufferMergedLOD2List[n], sizeof(uint) * 1);
                                
                                ComputeBuffer.CopyCount(cameraComputeBuffers.ShadowBufferLOD2,
                                    cameraComputeBuffers.ShadowArgsBufferMergedLOD2List[n], sizeof(uint) * 1);
                            }

                            for (int n = 0; n <= vegetationItemModelInfo.VegetationMeshLod3.subMeshCount - 1; n++)
                            {
                                ComputeBuffer.CopyCount(cameraComputeBuffers.VisibleBufferLOD3,
                                    cameraComputeBuffers.ArgsBufferMergedLOD3List[n], sizeof(uint) * 1);
                                
                                ComputeBuffer.CopyCount(cameraComputeBuffers.ShadowBufferLOD3,
                                    cameraComputeBuffers.ShadowArgsBufferMergedLOD3List[n], sizeof(uint) * 1);                            
                            }
                        }

                        //TODO calculate bounds one for each LOD
                        float boundsDistance = vegetationItemCullDistance * 2 +
                                               vegetationItemModelInfo.BoundingSphereRadius;
                        Bounds cellBounds = new Bounds(VegetationStudioCameraList[i].SelectedCamera.transform.position,
                            new Vector3(boundsDistance, boundsDistance, boundsDistance));

                        RenderVegetationItemLODIndirect(vegetationItemModelInfo, cellBounds, 0, i,
                            targetCamera, shadowCastingMode, layer,false);

                        if (shadowCastingMode == ShadowCastingMode.On)
                        {
                            RenderVegetationItemLODIndirect(vegetationItemModelInfo, cellBounds, 0, i,
                                targetCamera, ShadowCastingMode.ShadowsOnly, layer,true);
                        }                                          
  
                        if (useLODs)
                        {
                            if (vegetationItemModelInfo.LODCount > 1)
                            {
                                RenderVegetationItemLODIndirect(vegetationItemModelInfo, cellBounds, 1, i,
                                    targetCamera, shadowCastingMode, layer,false);
                                
                                if (shadowCastingMode == ShadowCastingMode.On)
                                {
                                    RenderVegetationItemLODIndirect(vegetationItemModelInfo, cellBounds, 1, i,
                                        targetCamera, ShadowCastingMode.ShadowsOnly, layer,true);
                                }
                            }

                            if (vegetationItemModelInfo.LODCount > 2)
                            {
                                RenderVegetationItemLODIndirect(vegetationItemModelInfo, cellBounds, 2, i,
                                    targetCamera, shadowCastingMode, layer,false);

                                if (shadowCastingMode == ShadowCastingMode.On)
                                {
                                    RenderVegetationItemLODIndirect(vegetationItemModelInfo, cellBounds, 2, i,
                                        targetCamera, ShadowCastingMode.ShadowsOnly, layer,true);
                                }
                            }

                            if (vegetationItemModelInfo.LODCount > 3)
                            {
                                RenderVegetationItemLODIndirect(vegetationItemModelInfo, cellBounds, 3, i,
                                   targetCamera, shadowCastingMode, layer,false);
                                if (shadowCastingMode == ShadowCastingMode.On)
                                {
                                    RenderVegetationItemLODIndirect(vegetationItemModelInfo, cellBounds, 3, i,
                                        targetCamera, ShadowCastingMode.ShadowsOnly, layer,true);
                                }
                            }
                        }
                    }
                }
            }
            Profiler.EndSample();
        }

        void RenderVegetationItemLODIndirect(VegetationItemModelInfo vegetationItemModelInfo, Bounds cellBounds, int lodIndex, int cameraIndex, Camera selectedCamera, ShadowCastingMode shadowCastingMode, int layer, bool shadows)
        {
            MaterialPropertyBlock materialPropertyBlock = vegetationItemModelInfo.GetLODMaterialPropertyBlock(lodIndex);
            materialPropertyBlock.Clear();

            ComputeBuffer visibleBuffer = vegetationItemModelInfo.GetLODVisibleBuffer(lodIndex, cameraIndex,shadows);

            Mesh mesh = vegetationItemModelInfo.GetLODMesh(lodIndex);
            Material[] materials = vegetationItemModelInfo.GetLODMaterials(lodIndex);

            if (vegetationItemModelInfo.ShaderControler != null &&
                vegetationItemModelInfo.ShaderControler.Settings.SampleWind)
            {
                MeshRenderer meshRenderer = vegetationItemModelInfo.WindSamplerMeshRendererList[cameraIndex];
                if (meshRenderer)
                {
                    meshRenderer.GetPropertyBlock(materialPropertyBlock);
                }
            }

            materialPropertyBlock.SetBuffer(_visibleShaderDataBufferID, visibleBuffer);
            materialPropertyBlock.SetBuffer(_indirectShaderDataBufferID, visibleBuffer);

            List<ComputeBuffer> argsBufferList = vegetationItemModelInfo.GetLODArgsBufferList(lodIndex, cameraIndex,shadows);

            //int[] data = new int[5];
            //argsBufferList[0].GetData(data);
            //Debug.Log("Indirect - LOD" + lodIndex + ":" + data[1]);
            int submeshesToRender = Mathf.Min(mesh.subMeshCount, materials.Length);      
            for (int i = 0; i <= submeshesToRender - 1; i++)
            {
                Graphics.DrawMeshInstancedIndirect(mesh, i, materials[i], cellBounds,
                    argsBufferList[i], 0, materialPropertyBlock, shadowCastingMode, true, layer,
                    selectedCamera,LightProbeUsage.Off);
            }
        }

        void SetComputeShaderBuffer(int bufferID, int bufferCountID, int cellIndex, int vegetationPackageIndex, int vegetationItemIndex)
        {
            if (cellIndex < _hasBufferList.Count)
            {
                VegetationCell vegetationCell = _hasBufferList[cellIndex];
                int instanceCount = _hasBufferList[cellIndex].VegetationPackageInstancesList[vegetationPackageIndex].VegetationItemMatrixList[vegetationItemIndex].Length;
                MergeBufferShader.SetBuffer(MergeBufferKernelHandle, bufferID, vegetationCell.VegetationPackageInstancesList[vegetationPackageIndex].VegetationItemComputeBufferList[vegetationItemIndex].ComputeBuffer);
                MergeBufferShader.SetInt(bufferCountID, instanceCount);
            }
            else
            {
                MergeBufferShader.SetBuffer(MergeBufferKernelHandle, bufferID, _dummyComputeBuffer);
                MergeBufferShader.SetInt(bufferCountID, 0);
            }
        }
    }
}

