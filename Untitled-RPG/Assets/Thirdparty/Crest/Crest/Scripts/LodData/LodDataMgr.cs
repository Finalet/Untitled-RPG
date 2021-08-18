﻿// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Crest
{
    /// <summary>
    /// Base class for data/behaviours created on each LOD.
    /// </summary>
    public abstract class LodDataMgr
    {
        public abstract string SimName { get; }

        // This is the texture format we want to use.
        protected abstract GraphicsFormat RequestedTextureFormat { get; }

        // This is the platform compatible texture format we will use.
        public GraphicsFormat CompatibleTextureFormat { get; private set; }

        // NOTE: This MUST match the value in OceanConstants.hlsl, as it
        // determines the size of the texture arrays in the shaders.
        public const int MAX_LOD_COUNT = 15;

        // NOTE: these MUST match the values in OceanConstants.hlsl
        // 64 recommended as a good common minimum: https://www.reddit.com/r/GraphicsProgramming/comments/aeyfkh/for_compute_shaders_is_there_an_ideal_numthreads/
        public const int THREAD_GROUP_SIZE_X = 8;
        public const int THREAD_GROUP_SIZE_Y = 8;

        // NOTE: This is a temporary solution to keywords having prefixes downstream.
        internal static readonly string MATERIAL_KEYWORD_PREFIX = RenderPipelineHelper.IsHighDefinition ? "CREST" : "";

        protected abstract int GetParamIdSampler(bool sourceLod = false);

        protected abstract bool NeedToReadWriteTextureData { get; }

        protected RenderTexture _targets;

        public RenderTexture DataTexture { get { return _targets; } }

        protected virtual Texture2DArray NullTexture => TextureArrayHelpers.BlackTextureArray;

        public static int sp_LD_SliceIndex = Shader.PropertyToID("_LD_SliceIndex");
        protected static int sp_LODChange = Shader.PropertyToID("_LODChange");

        // shape texture resolution
        int _shapeRes = -1;

        // ocean scale last frame - used to detect scale changes
        float _oceanLocalScalePrev = -1f;

        int _scaleDifferencePow2 = 0;
        protected int ScaleDifferencePow2 { get { return _scaleDifferencePow2; } }

        public bool enabled { get; protected set; }

        protected OceanRenderer _ocean;

        public LodDataMgr(OceanRenderer ocean)
        {
            _ocean = ocean;
        }

        public virtual void Start()
        {
            InitData();
            enabled = true;
        }

        public static RenderTexture CreateLodDataTextures(RenderTextureDescriptor desc, string name, bool needToReadWriteTextureData)
        {
            RenderTexture result = new RenderTexture(desc);
            result.wrapMode = TextureWrapMode.Clamp;
            result.antiAliasing = 1;
            result.filterMode = FilterMode.Bilinear;
            result.anisoLevel = 0;
            result.useMipMap = false;
            result.name = name;
            result.dimension = TextureDimension.Tex2DArray;
            result.volumeDepth = OceanRenderer.Instance.CurrentLodCount;
            result.enableRandomWrite = needToReadWriteTextureData;
            result.Create();
            return result;
        }

        protected virtual void InitData()
        {
            // Find a compatible texture format.
            var formatUsage = NeedToReadWriteTextureData ? FormatUsage.LoadStore : FormatUsage.Sample;
            CompatibleTextureFormat = SystemInfo.GetCompatibleFormat(RequestedTextureFormat, formatUsage);
            if (CompatibleTextureFormat != RequestedTextureFormat)
            {
                Debug.Log($"Using render texture format {CompatibleTextureFormat} instead of {RequestedTextureFormat}");
            }
            Debug.Assert(CompatibleTextureFormat != GraphicsFormat.None, $"The graphics device does not support the render texture format {RequestedTextureFormat}");

            Debug.Assert(OceanRenderer.Instance.CurrentLodCount <= MAX_LOD_COUNT);

            var resolution = OceanRenderer.Instance.LodDataResolution;
            var desc = new RenderTextureDescriptor(resolution, resolution, CompatibleTextureFormat, 0);
            _targets = CreateLodDataTextures(desc, SimName, NeedToReadWriteTextureData);

            // Bind globally once here on init, which will bind to all graphics shaders (not compute)
            Shader.SetGlobalTexture(GetParamIdSampler(), _targets);
        }

        public virtual void UpdateLodData()
        {
            int width = OceanRenderer.Instance.LodDataResolution;
            // debug functionality to resize RT if different size was specified.
            if (_shapeRes == -1)
            {
                _shapeRes = width;
            }
            else if (width != _shapeRes)
            {
                _targets.Release();
                _targets.width = _targets.height = _shapeRes;
                _targets.Create();

                _shapeRes = width;
            }

            // determine if this LOD has changed scale and by how much (in exponent of 2)
            float oceanLocalScale = OceanRenderer.Instance.Root.localScale.x;
            if (_oceanLocalScalePrev == -1f) _oceanLocalScalePrev = oceanLocalScale;
            float ratio = oceanLocalScale / _oceanLocalScalePrev;
            _oceanLocalScalePrev = oceanLocalScale;
            float ratio_l2 = Mathf.Log(ratio) / Mathf.Log(2f);
            _scaleDifferencePow2 = Mathf.RoundToInt(ratio_l2);
        }


        public virtual void BuildCommandBuffer(OceanRenderer ocean, CommandBuffer buf)
        {
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            var temp = b;
            b = a;
            a = temp;
        }

        public interface IDrawFilter
        {
            float Filter(ILodDataInput data, out int isTransition);
        }

        protected void SubmitDraws(int lodIdx, CommandBuffer buf)
        {
            var lt = OceanRenderer.Instance._lodTransform;
            lt._renderData[lodIdx].Validate(0, SimName);

            lt.SetViewProjectionMatrices(lodIdx, buf);

            var drawList = RegisterLodDataInputBase.GetRegistrar(GetType());
            foreach (var draw in drawList)
            {
                if (!draw.Value.Enabled)
                {
                    continue;
                }

                draw.Value.Draw(buf, 1f, 0, lodIdx);
            }
        }

        protected void SubmitDrawsFiltered(int lodIdx, CommandBuffer buf, IDrawFilter filter)
        {
            var lt = OceanRenderer.Instance._lodTransform;
            lt._renderData[lodIdx].Validate(0, SimName);

            lt.SetViewProjectionMatrices(lodIdx, buf);

            var drawList = RegisterLodDataInputBase.GetRegistrar(GetType());
            foreach (var draw in drawList)
            {
                if (!draw.Value.Enabled)
                {
                    continue;
                }

                int isTransition;
                float weight = filter.Filter(draw.Value, out isTransition);
                if (weight > 0f)
                {
                    draw.Value.Draw(buf, weight, isTransition, lodIdx);
                }
            }
        }

        protected struct TextureArrayParamIds
        {
            private int _paramId;
            private int _paramId_Source;
            public TextureArrayParamIds(string textureArrayName)
            {
                _paramId = Shader.PropertyToID(textureArrayName);
                // Note: string concatonation does generate a small amount of
                // garbage. However, this is called on initialisation so should
                // be ok for now? Something worth considering for the future if
                // we want to go garbage-free.
                _paramId_Source = Shader.PropertyToID(textureArrayName + "_Source");
            }
            public int GetId(bool sourceLod) { return sourceLod ? _paramId_Source : _paramId; }
        }

        internal virtual void OnEnable()
        {
        }
        internal virtual void OnDisable()
        {
            // Unbind from all graphics shaders (not compute)
            Shader.SetGlobalTexture(GetParamIdSampler(), NullTexture);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStatics()
        {
            // Init here from 2019.3 onwards
            sp_LD_SliceIndex = Shader.PropertyToID("_LD_SliceIndex");
            sp_LODChange = Shader.PropertyToID("_LODChange");
        }
    }
}
