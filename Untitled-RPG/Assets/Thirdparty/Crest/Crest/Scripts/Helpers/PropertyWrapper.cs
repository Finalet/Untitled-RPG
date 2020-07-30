﻿// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;
using UnityEngine.Rendering;

namespace Crest
{
    /// <summary>
    /// Unified interface for setting properties on both materials and material property blocks
    /// </summary>
    public interface IPropertyWrapper
    {
        void SetFloat(int param, float value);
        void SetFloatArray(int param, float[] value);
        void SetVector(int param, Vector4 value);
        void SetVectorArray(int param, Vector4[] value);
        void SetTexture(int param, Texture value);
        void SetMatrix(int param, Matrix4x4 matrix);
        void SetInt(int param, int value);
    }

    [System.Serializable]
    public class PropertyWrapperMaterial : IPropertyWrapper
    {
        public PropertyWrapperMaterial(Material target) { material = target; }
        public PropertyWrapperMaterial(Shader shader) { material = new Material(shader); }
        public void SetFloat(int param, float value) { material.SetFloat(param, value); }
        public void SetFloatArray(int param, float[] value) { material.SetFloatArray(param, value); }
        public void SetTexture(int param, Texture value) { material.SetTexture(param, value); }
        public void SetVector(int param, Vector4 value) { material.SetVector(param, value); }
        public void SetVectorArray(int param, Vector4[] value) { material.SetVectorArray(param, value); }
        public void SetMatrix(int param, Matrix4x4 value) { material.SetMatrix(param, value); }
        public void SetInt(int param, int value) { material.SetInt(param, value); }

        public Material material { get; private set; }
    }

    public class PropertyWrapperMPB : IPropertyWrapper
    {
        public PropertyWrapperMPB() { materialPropertyBlock = new MaterialPropertyBlock(); }
        public void SetFloat(int param, float value) { materialPropertyBlock.SetFloat(param, value); }
        public void SetFloatArray(int param, float[] value) { materialPropertyBlock.SetFloatArray(param, value); }
        public void SetTexture(int param, Texture value) { materialPropertyBlock.SetTexture(param, value); }
        public void SetVector(int param, Vector4 value) { materialPropertyBlock.SetVector(param, value); }
        public void SetVectorArray(int param, Vector4[] value) { materialPropertyBlock.SetVectorArray(param, value); }
        public void SetMatrix(int param, Matrix4x4 value) { materialPropertyBlock.SetMatrix(param, value); }
        public void SetInt(int param, int value) { materialPropertyBlock.SetInt(param, value); }

        public MaterialPropertyBlock materialPropertyBlock { get; private set; }
    }

    [System.Serializable]
    public class PropertyWrapperCompute : IPropertyWrapper
    {
        private CommandBuffer _commandBuffer = null;
        ComputeShader _computeShader = null;
        int _computeKernel = -1;

        public void Initialise(
            CommandBuffer commandBuffer,
            ComputeShader computeShader, int computeKernel
        )
        {
            _commandBuffer = commandBuffer;
            _computeShader = computeShader;
            _computeKernel = computeKernel;
        }

        public void SetFloat(int param, float value) { _commandBuffer.SetComputeFloatParam(_computeShader, param, value); }
        public void SetFloatArray(int param, float[] value) { _commandBuffer.SetGlobalFloatArray(param, value); }
        public void SetInt(int param, int value) { _commandBuffer.SetComputeIntParam(_computeShader, param, value); }
        public void SetTexture(int param, Texture value) { _commandBuffer.SetComputeTextureParam(_computeShader, _computeKernel, param, value); }
        public void SetVector(int param, Vector4 value) { _commandBuffer.SetComputeVectorParam(_computeShader, param, value); }
        public void SetVectorArray(int param, Vector4[] value) { _commandBuffer.SetComputeVectorArrayParam(_computeShader, param, value); }
        public void SetMatrix(int param, Matrix4x4 value) { _commandBuffer.SetComputeMatrixParam(_computeShader, param, value); }

        // NOTE: these MUST match the values in OceanLODData.hlsl
        // 64 recommended as a good common minimum: https://www.reddit.com/r/GraphicsProgramming/comments/aeyfkh/for_compute_shaders_is_there_an_ideal_numthreads/
        public const int THREAD_GROUP_SIZE_X = 8;
        public const int THREAD_GROUP_SIZE_Y = 8;
        public void DispatchShader()
        {
            _commandBuffer.DispatchCompute(
                _computeShader, _computeKernel,
                OceanRenderer.Instance.LodDataResolution / THREAD_GROUP_SIZE_X,
                OceanRenderer.Instance.LodDataResolution / THREAD_GROUP_SIZE_Y,
                1
            );

            _commandBuffer = null;
            _computeShader = null;
            _computeKernel = -1;
        }

        public void DispatchShaderMultiLOD()
        {
            _commandBuffer.DispatchCompute(
                _computeShader, _computeKernel,
                OceanRenderer.Instance.LodDataResolution / THREAD_GROUP_SIZE_X,
                OceanRenderer.Instance.LodDataResolution / THREAD_GROUP_SIZE_Y,
                OceanRenderer.Instance.CurrentLodCount
            );

            _commandBuffer = null;
            _computeShader = null;
            _computeKernel = -1;
        }
    }

    [System.Serializable]
    public class PropertyWrapperComputeStandalone : IPropertyWrapper
    {
        ComputeShader _computeShader = null;
        int _computeKernel = -1;

        public PropertyWrapperComputeStandalone(
            ComputeShader computeShader, int computeKernel
        )
        {
            _computeShader = computeShader;
            _computeKernel = computeKernel;
        }

        public void SetFloat(int param, float value) { _computeShader.SetFloat(param, value); }
        public void SetFloatArray(int param, float[] value) { _computeShader.SetFloats(param, value); }
        public void SetInt(int param, int value) { _computeShader.SetInt(param, value); }
        public void SetTexture(int param, Texture value) { _computeShader.SetTexture(_computeKernel, param, value); }
        public void SetVector(int param, Vector4 value) { _computeShader.SetVector(param, value); }
        public void SetVectorArray(int param, Vector4[] value) { _computeShader.SetVectorArray(param, value); }
        public void SetMatrix(int param, Matrix4x4 value) { _computeShader.SetMatrix(param, value); }
    }
}
