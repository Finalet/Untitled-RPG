// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Crest
{
    using SettingsType = SimSettingsShadow;

    /// <summary>
    /// Stores shadowing data to use during ocean shading. Shadowing is persistent and supports sampling across
    /// many frames and jittered sampling for (very) soft shadows.
    /// </summary>
    [ExecuteAlways]
    public class LodDataMgrShadow : LodDataMgr
    {
        public override string SimName { get { return "Shadow"; } }
        public override RenderTextureFormat TextureFormat { get { return RenderTextureFormat.RG16; } }
        protected override bool NeedToReadWriteTextureData { get { return true; } }

        public static bool s_processData = true;

        Light _mainLight;
        Camera _cameraMain;

        // URP version needs access to this externally, hence public get
        public CommandBuffer BufCopyShadowMap { get; private set; }

        RenderTexture _sources;
        PropertyWrapperMaterial[] _renderMaterial;

        readonly int sp_CenterPos = Shader.PropertyToID("_CenterPos");
        readonly int sp_Scale = Shader.PropertyToID("_Scale");
        readonly int sp_CamPos = Shader.PropertyToID("_CamPos");
        readonly int sp_CamForward = Shader.PropertyToID("_CamForward");
        readonly int sp_JitterDiameters_CurrentFrameWeights = Shader.PropertyToID("_JitterDiameters_CurrentFrameWeights");
        readonly int sp_MainCameraProjectionMatrix = Shader.PropertyToID("_MainCameraProjectionMatrix");
        readonly int sp_SimDeltaTime = Shader.PropertyToID("_SimDeltaTime");
        readonly int sp_LD_SliceIndex_Source = Shader.PropertyToID("_LD_SliceIndex_Source");
        readonly int sp_LD_TexArray_Target = Shader.PropertyToID("_LD_TexArray_Target");

        SettingsType _defaultSettings;
        public SettingsType Settings
        {
            get
            {
                if (_ocean._simSettingsShadow != null) return _ocean._simSettingsShadow;

                if (_defaultSettings == null)
                {
                    _defaultSettings = ScriptableObject.CreateInstance<SettingsType>();
                    _defaultSettings.name = SimName + " Auto-generated Settings";
                }
                return _defaultSettings;
            }
        }

        public LodDataMgrShadow(OceanRenderer ocean) : base(ocean)
        {
            Start();
        }

        public override void Start()
        {
            base.Start();

#if UNITY_2018
            Debug.LogError("Shadowing not enabled on preview versions of URP. Upgrade to 2019 is required.", this);
#endif
            {
                _renderMaterial = new PropertyWrapperMaterial[OceanRenderer.Instance.CurrentLodCount];
                var shader = Shader.Find("Hidden/Crest/Simulation/Update Shadow");
                for (int i = 0; i < _renderMaterial.Length; i++)
                {
                    _renderMaterial[i] = new PropertyWrapperMaterial(shader);
                }
            }

            if (!SampleShadows.Created
#if UNITY_EDITOR
                // Not excited about this but it seems that the SampleShadows may not be immediately created when in edit mode. TODO - detect directly on render
                // pipeline renderer asset?
                && UnityEditor.EditorApplication.isPlaying
#endif
                )
            {
                Debug.LogError("To support shadowing, a Custom renderer must be configured on the pipeline asset, and this custom renderer data must have the Sample Shadows feature added.", GraphicsSettings.renderPipelineAsset);
            }

            _cameraMain = Camera.main;
            if (_cameraMain == null)
            {
                var viewpoint = OceanRenderer.Instance.Viewpoint;
                _cameraMain = viewpoint != null ? viewpoint.GetComponent<Camera>() : null;

                if (_cameraMain == null)
                {
                    Debug.LogError("Could not find main camera, disabling shadow data", _ocean);
                    enabled = false;
                    return;
                }
            }

#if UNITY_EDITOR
            if (!OceanRenderer.Instance.OceanMaterial.IsKeywordEnabled("_SHADOWS_ON"))
            {
                Debug.LogWarning("Shadowing is not enabled on the current ocean material and will not be visible.", _ocean);
            }
#endif

            var asset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            if (asset && asset.shadowCascadeOption == ShadowCascadesOption.NoCascades)
            {
                Debug.LogError("Crest shadowing requires shadow cascades to be enabled on the pipeline asset.", asset);
                enabled = false;
                return;
            }
        }

        protected override void InitData()
        {
            base.InitData();

            int resolution = OceanRenderer.Instance.LodDataResolution;
            var desc = new RenderTextureDescriptor(resolution, resolution, TextureFormat, 0);
            _sources = CreateLodDataTextures(desc, SimName + "_1", NeedToReadWriteTextureData);

            TextureArrayHelpers.ClearToBlack(_sources);
            TextureArrayHelpers.ClearToBlack(_targets);
        }

        bool StartInitLight()
        {
            if (_mainLight == null)
            {
                _mainLight = OceanRenderer.Instance._primaryLight;

                if (_mainLight == null)
                {
                    if (!Settings._allowNullLight)
                    {
                        Debug.LogWarning("Primary light must be specified on OceanRenderer script to enable shadows.", OceanRenderer.Instance);
                    }
                    return false;
                }

                if (_mainLight.type != LightType.Directional)
                {
                    Debug.LogError("Primary light must be of type Directional.", OceanRenderer.Instance);
                    return false;
                }

                if (_mainLight.shadows == LightShadows.None)
                {
                    Debug.LogError("Shadows must be enabled on primary light to enable ocean shadowing (types Hard and Soft are equivalent for the ocean system).", OceanRenderer.Instance);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// May happen if scenes change etc
        /// </summary>
        void ClearBufferIfLightChanged()
        {
            if (_mainLight != OceanRenderer.Instance._primaryLight)
            {
                if (_mainLight)
                {
                    BufCopyShadowMap = null;
                    TextureArrayHelpers.ClearToBlack(_sources);
                    TextureArrayHelpers.ClearToBlack(_targets);
                }
                _mainLight = null;
            }
        }

        public override void UpdateLodData()
        {
            if (!enabled)
            {
                return;
            }

            base.UpdateLodData();

            ClearBufferIfLightChanged();

            if (!StartInitLight())
            {
                enabled = false;
                return;
            }

            if (!s_processData)
            {
                return;
            }

            if (BufCopyShadowMap == null)
            {
                BufCopyShadowMap = new CommandBuffer();
                BufCopyShadowMap.name = "Shadow data";
            }

            if (!s_processData)
            {
                return;
            }

            Swap(ref _sources, ref _targets);

            BufCopyShadowMap.Clear();

            ValidateSourceData();

            // clear the shadow collection. it will be overwritten with shadow values IF the shadows render,
            // which only happens if there are (nontransparent) shadow receivers around. this is only reliable
            // in play mode, so don't do it in edit mode.
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
#endif
            {
                TextureArrayHelpers.ClearToBlack(_targets);
            }

            // TODO - this is in SRP, so i can't ifdef it? what is a good plan here - wait for it to be removed completely?
#pragma warning disable 618
            using (new ProfilingSample(BufCopyShadowMap, "CrestSampleShadows"))
#pragma warning restore 618
            {
                var lt = OceanRenderer.Instance._lodTransform;
                for (var lodIdx = lt.LodCount - 1; lodIdx >= 0; lodIdx--)
                {
                    lt._renderData[lodIdx].Validate(0, SimName);
                    _renderMaterial[lodIdx].SetVector(sp_CenterPos, lt._renderData[lodIdx]._posSnapped);
                    var scale = OceanRenderer.Instance.CalcLodScale(lodIdx);
                    _renderMaterial[lodIdx].SetVector(sp_Scale, new Vector3(scale, 1f, scale));
                    _renderMaterial[lodIdx].SetVector(sp_JitterDiameters_CurrentFrameWeights, new Vector4(Settings._jitterDiameterSoft, Settings._jitterDiameterHard, Settings._currentFrameWeightSoft, Settings._currentFrameWeightHard));
                    _renderMaterial[lodIdx].SetMatrix(sp_MainCameraProjectionMatrix, _cameraMain.projectionMatrix * _cameraMain.worldToCameraMatrix);
                    _renderMaterial[lodIdx].SetFloat(sp_SimDeltaTime, Time.deltaTime);

                    // compute which lod data we are sampling previous frame shadows from. if a scale change has happened this can be any lod up or down the chain.
                    var srcDataIdx = lodIdx + ScaleDifferencePow2;
                    srcDataIdx = Mathf.Clamp(srcDataIdx, 0, lt.LodCount - 1);
                    _renderMaterial[lodIdx].SetInt(sp_LD_SliceIndex, lodIdx);
                    _renderMaterial[lodIdx].SetInt(sp_LD_SliceIndex_Source, srcDataIdx);
                    BindSourceData(_renderMaterial[lodIdx], false);
                    BufCopyShadowMap.Blit(Texture2D.blackTexture, _targets, _renderMaterial[lodIdx].material, -1, lodIdx);
                }
            }
        }

        public void ValidateSourceData()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                // Don't validate when not in play mode in editor as shadows won't be updating.
                return;
            }
#endif

            foreach (var renderData in OceanRenderer.Instance._lodTransform._renderDataSource)
            {
                renderData.Validate(BuildCommandBufferBase._lastUpdateFrame - OceanRenderer.FrameCount, SimName);
            }
        }

        public void BindSourceData(IPropertyWrapper simMaterial, bool paramsOnly)
        {
            var rd = OceanRenderer.Instance._lodTransform._renderDataSource;
            BindData(simMaterial, paramsOnly ? Texture2D.blackTexture : _sources as Texture, true, ref rd, true);
        }

        internal override void OnEnable()
        {
            base.OnEnable();

            RemoveCommandBuffers();
        }

        internal override void OnDisable()
        {
            base.OnDisable();

            RemoveCommandBuffers();
        }

        void RemoveCommandBuffers()
        {
            if (BufCopyShadowMap != null)
            {
                if (_mainLight)
                {
                    _mainLight.RemoveCommandBuffer(LightEvent.BeforeScreenspaceMask, BufCopyShadowMap);
                }
                BufCopyShadowMap = null;
            }
        }

        readonly static string s_textureArrayName = "_LD_TexArray_Shadow";
        private static TextureArrayParamIds s_textureArrayParamIds = new TextureArrayParamIds(s_textureArrayName);
        public static int ParamIdSampler(bool sourceLod = false) { return s_textureArrayParamIds.GetId(sourceLod); }

        protected override int GetParamIdSampler(bool sourceLod = false)
        {
            return ParamIdSampler(sourceLod);
        }
        public static void BindNull(IPropertyWrapper properties, bool sourceLod = false)
        {
            properties.SetTexture(ParamIdSampler(sourceLod), TextureArrayHelpers.BlackTextureArray);
        }

#if UNITY_2019_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        static void InitStatics()
        {
            // Init here from 2019.3 onwards
            s_textureArrayParamIds = new TextureArrayParamIds(s_textureArrayName);
        }
    }
}
