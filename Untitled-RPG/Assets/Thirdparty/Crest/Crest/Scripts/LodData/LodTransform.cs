﻿// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;
using UnityEngine.Rendering;

namespace Crest
{
    /// <summary>
    /// This script is attached to the parent GameObject of each LOD. It provides helper functionality related to each LOD.
    /// </summary>
    public class LodTransform : IFloatingOrigin
    {
        protected int[] _transformUpdateFrame;

        static int s_paramsPosScale = Shader.PropertyToID("_LD_Pos_Scale");
        static int s_paramsPosScaleSource = Shader.PropertyToID("_LD_Pos_Scale_Source");
        static int s_paramsOcean = Shader.PropertyToID("_LD_Params");
        static int s_paramsOceanSource = Shader.PropertyToID("_LD_Params_Source");

        [System.Serializable]
        public struct RenderData
        {
            public float _texelWidth;
            public float _textureRes;
            public Vector3 _posSnapped;
            public int _frame;

            public RenderData Validate(int frameOffset, string context)
            {
                // ignore first frame - this patches errors when using edit & continue in editor
                if (_frame > 0 && _frame != OceanRenderer.FrameCount + frameOffset)
                {
                    Debug.LogWarning($"RenderData validation failed - {context} - _frame of data ({_frame}) != expected ({OceanRenderer.FrameCount + frameOffset}), which may indicate some update functions are being called out of order, or script execution order is broken.", OceanRenderer.Instance);
                }
                return this;
            }

            public Rect RectXZ
            {
                get
                {
                    float w = _texelWidth * _textureRes;
                    return new Rect(_posSnapped.x - w / 2f, _posSnapped.z - w / 2f, w, w);
                }
            }
        }

        public RenderData[] _renderData = null;
        public RenderData[] _renderDataSource = null;

        public int LodCount { get; private set; }

        Matrix4x4[] _worldToCameraMatrix;
        Matrix4x4[] _projectionMatrix;
        public Matrix4x4 GetWorldToCameraMatrix(int lodIdx) { return _worldToCameraMatrix[lodIdx]; }
        public Matrix4x4 GetProjectionMatrix(int lodIdx) { return _projectionMatrix[lodIdx]; }

        public void InitLODData(int lodCount)
        {
            LodCount = lodCount;

            _renderData = new RenderData[lodCount];
            _renderDataSource = new RenderData[lodCount];
            _worldToCameraMatrix = new Matrix4x4[lodCount];
            _projectionMatrix = new Matrix4x4[lodCount];

            _transformUpdateFrame = new int[lodCount];
            for (int i = 0; i < _transformUpdateFrame.Length; i++)
            {
                _transformUpdateFrame[i] = -1;
            }
        }

        public void UpdateTransforms()
        {
            for (int lodIdx = 0; lodIdx < LodCount; lodIdx++)
            {
                if (_transformUpdateFrame[lodIdx] == OceanRenderer.FrameCount) continue;

                _transformUpdateFrame[lodIdx] = OceanRenderer.FrameCount;

                _renderDataSource[lodIdx] = _renderData[lodIdx];

                var lodScale = OceanRenderer.Instance.CalcLodScale(lodIdx);
                var camOrthSize = 2f * lodScale;

                // find snap period
                _renderData[lodIdx]._textureRes = OceanRenderer.Instance.LodDataResolution;
                _renderData[lodIdx]._texelWidth = 2f * camOrthSize / _renderData[lodIdx]._textureRes;
                // snap so that shape texels are stationary
                _renderData[lodIdx]._posSnapped = OceanRenderer.Instance.Root.position
                    - new Vector3(Mathf.Repeat(OceanRenderer.Instance.Root.position.x, _renderData[lodIdx]._texelWidth), 0f, Mathf.Repeat(OceanRenderer.Instance.Root.position.z, _renderData[lodIdx]._texelWidth));

                _renderData[lodIdx]._frame = OceanRenderer.FrameCount;

                // detect first update and populate the render data if so - otherwise it can give divide by 0s and other nastiness
                if (_renderDataSource[lodIdx]._textureRes == 0f)
                {
                    _renderDataSource[lodIdx]._posSnapped = _renderData[lodIdx]._posSnapped;
                    _renderDataSource[lodIdx]._texelWidth = _renderData[lodIdx]._texelWidth;
                    _renderDataSource[lodIdx]._textureRes = _renderData[lodIdx]._textureRes;
                }

                _worldToCameraMatrix[lodIdx] = CalculateWorldToCameraMatrixRHS(_renderData[lodIdx]._posSnapped + Vector3.up * 100f, Quaternion.AngleAxis(90f, Vector3.right));

                _projectionMatrix[lodIdx] = Matrix4x4.Ortho(-2f * lodScale, 2f * lodScale, -2f * lodScale, 2f * lodScale, 1f, 500f);
            }
        }

        // Borrowed from SRP code: https://github.com/Unity-Technologies/ScriptableRenderPipeline/blob/2a68d8073c4eeef7af3be9e4811327a522434d5f/com.unity.render-pipelines.high-definition/Runtime/Core/Utilities/GeometryUtils.cs
        public static Matrix4x4 CalculateWorldToCameraMatrixRHS(Vector3 position, Quaternion rotation)
        {
            return Matrix4x4.Scale(new Vector3(1, 1, -1)) * Matrix4x4.TRS(position, rotation, Vector3.one).inverse;
        }

        public void SetViewProjectionMatrices(int lodIdx, CommandBuffer buf)
        {
            buf.SetViewProjectionMatrices(GetWorldToCameraMatrix(lodIdx), GetProjectionMatrix(lodIdx));
        }

        public float MaxWavelength(int lodIdx)
        {
            float oceanBaseScale = OceanRenderer.Instance.Scale;
            float maxDiameter = 4f * oceanBaseScale * Mathf.Pow(2f, lodIdx);
            float maxTexelSize = maxDiameter / OceanRenderer.Instance.LodDataResolution;
            return 2f * maxTexelSize * OceanRenderer.Instance.MinTexelsPerWave;
        }

        public static int ParamIdPosScale(bool sourceLod = false)
        {
            if (sourceLod)
            {
                return s_paramsPosScaleSource;
            }
            else
            {
                return s_paramsPosScale;
            }
        }

        public static int ParamIdOcean(bool sourceLod = false)
        {
            if (sourceLod)
            {
                return s_paramsOceanSource;
            }
            else
            {
                return s_paramsOcean;
            }
        }

        public void SetOrigin(Vector3 newOrigin)
        {
            for (int lodIdx = 0; lodIdx < LodCount; lodIdx++)
            {
                _renderData[lodIdx]._posSnapped -= newOrigin;
                _renderDataSource[lodIdx]._posSnapped -= newOrigin;
            }
        }

#if UNITY_2019_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        static void InitStatics()
        {
            // Init here from 2019.3 onwards
            s_paramsPosScale = Shader.PropertyToID("_LD_Pos_Scale");
            s_paramsPosScaleSource = Shader.PropertyToID("_LD_Pos_Scale_Source");
            s_paramsOcean = Shader.PropertyToID("_LD_Params");
            s_paramsOceanSource = Shader.PropertyToID("_LD_Params_Source");
        }
    }
}
